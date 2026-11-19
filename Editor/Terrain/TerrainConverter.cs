#nullable disable
using System;
using System.Collections.Generic;

namespace UnityToRebelFork.Editor.Terrains
{
    // The numbers of a Unity Terrain (its TerrainData) as plain arrays, so that the conversion can be run and tested outside of Unity; TerrainExporter (which needs Unity)
    // fills it in. Heights are 0..1 as GetHeights returns them, [z * Resolution + x] with z counted from the south (Unity's row 0).

    /// <summary>What the converter needs of a Unity terrain.</summary>
    public class TerrainSourceData
    {
        /// <summary>Height samples across (heightmapResolution, 2^n + 1).</summary>
        public int Resolution;

        /// <summary>Heights 0..1 of the terrain's size.y, [z * Resolution + x], z from the south.</summary>
        public float[] Heights;

        /// <summary>Terrain size in metres (heightmapScale times the resolution minus 1, and the maximum height).</summary>
        public float SizeX, SizeY, SizeZ;

        /// <summary>Alpha map samples across (alphamapResolution); 0 without layers.</summary>
        public int AlphamapResolution;

        /// <summary>Number of terrain layers.</summary>
        public int LayerCount;

        /// <summary>Layer weights, [(z * AlphamapResolution + x) * LayerCount + layer], z from the south (GetAlphamaps reordered).</summary>
        public float[] Alphamaps;

        /// <summary>Tile size in metres of each layer's textures (TerrainLayer.tileSize), two floats a layer: x then y.</summary>
        public float[] TileSizes;

        /// <summary>Things the terrain has that are not exported (the exporter fills these in so the warnings are in one place).</summary>
        public bool HasHoles, HasTrees, HasDetails;
    }

    /// <summary>The terrain as rbfx's <c>Terrain</c> component takes it.</summary>
    public class TerrainResult
    {
        /// <summary>The height map: RGB, three bytes a pixel, the first row the north edge (highest Z). Red is the high byte of a 16-bit height and green the low byte.</summary>
        public byte[] HeightRgb;
        public int HeightSize;

        /// <summary>The weights picture: RGB, the first row the north edge. Red, green and blue are the first three layers.</summary>
        public byte[] WeightsRgb;
        public int WeightsSize;

        /// <summary>Vertex Spacing of the component.</summary>
        public float SpacingX, SpacingY, SpacingZ;

        public int PatchSize;

        /// <summary>Where the terrain's centre is, from Unity's corner of the terrain (rbfx centres the terrain on its node).</summary>
        public float CenterX, CenterY, CenterZ;

        /// <summary>Repeat of the detail textures over the whole terrain (the material's DetailTiling).</summary>
        public float DetailTilingX, DetailTilingY;

        /// <summary>How many layers the weights hold (1 to 3).</summary>
        public int UsedLayers;

        public List<string> Warnings = new List<string>();
    }

    /// <summary>Converts the numbers of a Unity terrain to rbfx's: a height map, a weights picture and the component's settings.</summary>
    public static class TerrainConverter
    {
        /// <summary>rbfx's patch size: a power of two from 4 to 128 and a divisor of the quads across.</summary>
        public const int DefaultPatchSize = 32;

        public static TerrainResult Convert(TerrainSourceData source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new TerrainResult();
            int n = source.Resolution;
            if (n < 5 || source.Heights == null || source.Heights.Length < n * n)
            {
                result.Warnings.Add($"the height map has {n} samples across; at least 5 are needed");
                return result;
            }

            FillLayout(result, n, source.SizeX, source.SizeY, source.SizeZ);

            // Heights: 16 bits across the terrain's height, red the high byte (rbfx: height = (R + G / 256) * spacing.y).
            result.HeightSize = n;
            result.HeightRgb = new byte[n * n * 3];
            for (int z = 0; z < n; z++)
            {
                int row = n - 1 - z; // The first row of the picture is the north edge.
                for (int x = 0; x < n; x++)
                {
                    int v16 = (int)Math.Round(Clamp01(source.Heights[z * n + x]) * 65535.0);
                    int o = (row * n + x) * 3;
                    result.HeightRgb[o] = (byte)(v16 >> 8);
                    result.HeightRgb[o + 1] = (byte)(v16 & 255);
                }
            }

            ConvertWeights(source, result);

            if (source.HasHoles) result.Warnings.Add("the terrain has holes; they are drawn as ground");
            if (source.HasTrees) result.Warnings.Add("tree instances are not exported");
            if (source.HasDetails) result.Warnings.Add("grass and detail meshes are not exported");
            return result;
        }

        /// <summary>Only the settings of the component (spacing, patch size, centre) for a terrain of this size, without converting its pixels.</summary>
        public static TerrainResult Layout(int resolution, float sizeX, float sizeY, float sizeZ)
        {
            var result = new TerrainResult();
            FillLayout(result, resolution, sizeX, sizeY, sizeZ);
            return result;
        }

        private static void FillLayout(TerrainResult result, int resolution, float sizeX, float sizeY, float sizeZ)
        {
            int quads = Math.Max(1, resolution - 1);
            int patch = ChoosePatchSize(quads);
            if (patch == 0)
                result.Warnings.Add($"{quads} quads across is not a power of two; the terrain may not tile into patches");
            result.PatchSize = patch == 0 ? DefaultPatchSize : patch;
            result.SpacingX = sizeX / quads;
            result.SpacingZ = sizeZ / quads;
            // Height = (R + G / 256) * spacing.y with R and G the bytes of a 16-bit value, so the spacing is the height of one 1/256 step.
            result.SpacingY = (float)(sizeY * 256.0 / 65535.0);
            result.CenterX = sizeX * 0.5f;
            result.CenterZ = sizeZ * 0.5f;
        }

        /// <summary>The patch size for a terrain with <paramref name="quads"/> quads across: 32, or the whole terrain when smaller; 0 if no power of two from 4 divides it.</summary>
        public static int ChoosePatchSize(int quads)
        {
            if (quads < 4 || (quads & (quads - 1)) != 0) return 0;
            return Math.Min(DefaultPatchSize, quads);
        }

        private static void ConvertWeights(TerrainSourceData source, TerrainResult result)
        {
            int layers = source.LayerCount;
            int size = source.AlphamapResolution;
            if (layers <= 0 || size <= 0 || source.Alphamaps == null || source.Alphamaps.Length < size * size * layers)
            {
                result.UsedLayers = 0;
                result.DetailTilingX = result.DetailTilingY = 1f;
                if (layers <= 0) result.Warnings.Add("the terrain has no layers; it is drawn with the first layer's slot empty");
                return;
            }

            int used = Math.Min(3, layers);
            result.UsedLayers = used;
            if (layers > 3) result.Warnings.Add($"the terrain has {layers} layers; the blend material takes three, so only the first three are exported");

            result.WeightsSize = size;
            result.WeightsRgb = new byte[size * size * 3];
            for (int z = 0; z < size; z++)
            {
                int row = size - 1 - z;
                for (int x = 0; x < size; x++)
                {
                    int a = (z * size + x) * layers;
                    float r = source.Alphamaps[a];
                    float g = used > 1 ? source.Alphamaps[a + 1] : 0f;
                    float b = used > 2 ? source.Alphamaps[a + 2] : 0f;
                    float sum = r + g + b;
                    if (sum < 1e-5f) { r = 1f; g = 0f; b = 0f; sum = 1f; } // Only later layers here: show the first.
                    int o = (row * size + x) * 3;
                    int wr = (int)Math.Round(255f * r / sum);
                    int wg = (int)Math.Round(255f * g / sum);
                    result.WeightsRgb[o] = (byte)wr;
                    result.WeightsRgb[o + 1] = (byte)wg;
                    result.WeightsRgb[o + 2] = (byte)(used > 2 ? 255 - wr - wg : 0); // The three add up to 255 (with two layers the blue is 0 and red + green are 255).
                }
            }

            // One DetailTiling for the three detail textures: the first layer's tile size, the others are different only if their size is.
            float tx = TileOf(source, 0, 0), ty = TileOf(source, 0, 1);
            for (int i = 1; i < used; i++)
            {
                if (Math.Abs(TileOf(source, i, 0) - tx) > 1e-3f * tx || Math.Abs(TileOf(source, i, 1) - ty) > 1e-3f * ty)
                {
                    result.Warnings.Add("the layers have different tile sizes; the first layer's is used for all");
                    break;
                }
            }

            result.DetailTilingX = source.SizeX / tx;
            result.DetailTilingY = source.SizeZ / ty;
        }

        private static float TileOf(TerrainSourceData source, int layer, int axis)
        {
            float tile = source.TileSizes != null && source.TileSizes.Length > layer * 2 + axis ? source.TileSizes[layer * 2 + axis] : 0f;
            return tile > 1e-4f ? tile : 1f;
        }

        private static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
    }
}
