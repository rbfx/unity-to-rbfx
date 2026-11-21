using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityToRebelFork.Editor.Terrains;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Exports a Unity TerrainData for rbfx's <c>Terrain</c> component (and Toe's, which reads the same attributes): the height map as a 16-bit RGB PNG
    /// (red the high byte, green the low byte, the first row the north edge), the weights picture of the three most used layers as an RGB PNG and the blend material
    /// (<c>Techniques/TerrainBlend.xml</c>: the weights in the albedo slot, the layers' textures in the normal, properties and emission slots, <c>DetailTiling</c>).
    /// The numbers are worked out by <see cref="TerrainConverter"/>, which does not use Unity. The three files sit next to each other: <c>Name_height.png</c>,
    /// <c>Name_weights.png</c> and <c>Name.material</c>; <see cref="PrefabVisitor"/> asks for the height map's path and derives the others.
    /// </summary>
    public class TerrainExporter : ExporterBase<UnityEngine.TerrainData>
    {
        private const string HeightSuffix = "_height.png";
        private const string WeightsSuffix = "_weights.png";

        private readonly Lazy<ExportOrchestrator> _orchestrator;

        public TerrainExporter(Lazy<ExportOrchestrator> orchestrator, NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
            _orchestrator = orchestrator;
        }

        /// <summary>The path of the height map, which the component's Height Map attribute names.</summary>
        public override string EvaluateResourcePath(UnityEngine.TerrainData asset)
        {
            return EvaluateResourcePath(asset, HeightSuffix);
        }

        /// <summary>The weights picture next to a height map.</summary>
        public static string WeightsPath(string heightPath) => ReplaceSuffix(heightPath, WeightsSuffix);

        /// <summary>The blend material next to a height map.</summary>
        public static string MaterialPath(string heightPath) => ReplaceSuffix(heightPath, ".material");

        private static string ReplaceSuffix(string heightPath, string suffix)
        {
            if (string.IsNullOrEmpty(heightPath)) return null;
            return heightPath.EndsWith(HeightSuffix, StringComparison.Ordinal)
                ? heightPath.Substring(0, heightPath.Length - HeightSuffix.Length) + suffix
                : heightPath + suffix;
        }

        public override IEnumerable Export(UnityEngine.TerrainData asset)
        {
            yield return asset.name;

            var source = Read(asset);
            var result = TerrainConverter.Convert(source);
            foreach (var warning in result.Warnings)
                UnityEngine.Debug.LogWarning($"[TerrainExporter] '{asset.name}': {warning}.", asset);
            if (result.HeightRgb == null) yield break;

            var heightPath = EvaluateResourcePath(asset);
            WritePng(heightPath, result.HeightRgb, result.HeightSize);
            yield return asset.name;

            if (result.WeightsRgb != null)
                WritePng(WeightsPath(heightPath), result.WeightsRgb, result.WeightsSize);

            WriteMaterial(asset, result, heightPath);
        }

        private void WriteMaterial(UnityEngine.TerrainData asset, TerrainResult result, string heightPath)
        {
            var model = new MaterialModel();
            model.Techniques.Add(new TechniqueModel { Name = "Techniques/TerrainBlend.xml" });
            if (result.WeightsRgb != null)
                model.Albedo = WeightsPath(heightPath);

            var layers = asset.terrainLayers;
            string[] slots = { "Normal", "Properties", "Emission" };
            for (int i = 0; i < result.UsedLayers && layers != null && i < layers.Length; i++)
            {
                int layerIndex = i < result.LayerOrder.Length ? result.LayerOrder[i] : i;
                var layer = layerIndex < layers.Length ? layers[layerIndex] : null;
                var texture = layer != null ? layer.diffuseTexture : null;
                if (texture == null) continue;
                var path = _orchestrator.Value.ScheduleExport(texture);
                if (!string.IsNullOrEmpty(path)) model.Textures[slots[i]] = path;
            }

            // A layer with no texture shows the first one's, so the blend never samples an empty slot.
            string first = null;
            foreach (var slot in slots)
                if (first == null && model.Textures.TryGetValue(slot, out var p)) first = p;
            if (first != null)
                foreach (var slot in slots)
                    if (!model.Textures.ContainsKey(slot)) model.Textures[slot] = first;

            model.ShaderParameters["DetailTiling"] = new Variant(new UnityEngine.Vector2(result.DetailTilingX, result.DetailTilingY));

            using (var stream = _settings.CreateFile(MaterialPath(heightPath)))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                model.Save(writer);
        }

        private void WritePng(string resourcePath, byte[] rgb, int size)
        {
            var texture = new UnityEngine.Texture2D(size, size, UnityEngine.TextureFormat.RGB24, false, true);
            try
            {
                var pixels = new UnityEngine.Color32[size * size];
                // The picture's first row is the top: Unity's pixel rows run from the bottom, so the arrays' first row (north) goes last.
                for (int row = 0; row < size; row++)
                {
                    int y = size - 1 - row;
                    for (int x = 0; x < size; x++)
                    {
                        int o = (row * size + x) * 3;
                        pixels[y * size + x] = new UnityEngine.Color32(rgb[o], rgb[o + 1], rgb[o + 2], 255);
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply(false);
                var png = UnityEngine.ImageConversion.EncodeToPNG(texture);
                using (var stream = _settings.CreateFile(resourcePath))
                    stream.Write(png, 0, png.Length);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        /// <summary>Copies the numbers of a TerrainData into what the converter reads.</summary>
        internal static TerrainSourceData Read(UnityEngine.TerrainData data)
        {
            int n = data.heightmapResolution;
            var heights = data.GetHeights(0, 0, n, n); // [z, x], z from the south.
            var source = new TerrainSourceData
            {
                Resolution = n,
                Heights = new float[n * n],
                SizeX = data.size.x,
                SizeY = data.size.y,
                SizeZ = data.size.z,
                HasTrees = data.treeInstanceCount > 0,
                HasDetails = data.detailPrototypes != null && data.detailPrototypes.Length > 0,
            };
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                    source.Heights[z * n + x] = heights[z, x];

            try
            {
                int h = data.holesResolution;
                if (h > 0)
                {
                    var holes = data.GetHoles(0, 0, h, h);
                    foreach (var solid in holes)
                        if (!solid) { source.HasHoles = true; break; }
                }
            }
            catch (Exception)
            {
                // Older Unity versions have no holes.
            }

            var layers = data.terrainLayers;
            int layerCount = layers != null ? layers.Length : 0;
            int a = data.alphamapResolution;
            if (layerCount > 0 && a > 0)
            {
                var maps = data.GetAlphamaps(0, 0, a, a); // [z, x, layer]
                source.AlphamapResolution = a;
                source.LayerCount = layerCount;
                source.Alphamaps = new float[a * a * layerCount];
                for (int z = 0; z < a; z++)
                    for (int x = 0; x < a; x++)
                        for (int l = 0; l < layerCount; l++)
                            source.Alphamaps[(z * a + x) * layerCount + l] = maps[z, x, l];
                source.TileSizes = new float[layerCount * 2];
                for (int l = 0; l < layerCount; l++)
                {
                    source.TileSizes[l * 2] = layers[l] != null ? layers[l].tileSize.x : 1f;
                    source.TileSizes[l * 2 + 1] = layers[l] != null ? layers[l].tileSize.y : 1f;
                }
            }

            return source;
        }
    }
}
