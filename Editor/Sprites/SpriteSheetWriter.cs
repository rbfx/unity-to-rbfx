using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;

namespace UnityToRebelFork.Editor
{
    /// <summary>What was written for one texture: where, and what its sprites are called in the sheet.</summary>
    public sealed class WrittenSheet
    {
        /// <summary>Resource path of the sheet file (<c>PixelFantasy/.../BlueWolf.json</c>).</summary>
        public string JsonPath;

        /// <summary>Resource path of the picture.</summary>
        public string PngPath;

        /// <summary>The name of each sprite in the sheet (two sprites with one name in a texture are told apart, see <see cref="NameCollisionResolver"/>).</summary>
        public readonly Dictionary<UnityEngine.Sprite, string> Names = new Dictionary<UnityEngine.Sprite, string>();
    }

    /// <summary>
    /// Writes the Toe sprite sheet of a texture that is imported as sprites: the picture as a PNG (the source file itself when it is one, so nothing is changed on the way) and
    /// a JSON file with the rectangle, pivot and pixels per unit of every sprite. Unity counts rectangles from the bottom left of the texture, Toe from the top left, which
    /// <see cref="SpriteEntry.FromUnity"/> takes care of. The filter of the picture goes into the sheet as <c>pixelArt</c> (point filtering).
    /// </summary>
    public static class SpriteSheetWriter
    {
        /// <summary>True for a texture whose importer makes sprites of it (single or multiple).</summary>
        public static bool IsSpriteTexture(UnityEngine.Texture2D texture)
        {
            if (texture == null) return false;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            return importer != null && importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode != SpriteImportMode.None;
        }

        /// <summary>The path of the asset below Assets/, with forward slashes: where its exported files go.</summary>
        public static string RelativePath(string assetPath)
        {
            var result = assetPath.Replace('\\', '/');
            return result.StartsWith("Assets/", StringComparison.InvariantCultureIgnoreCase) ? result.Substring("Assets/".Length) : result;
        }

        public static string JsonPathOf(UnityEngine.Texture2D texture) => ExporterBase.ReplaceExtension(RelativePath(AssetDatabase.GetAssetPath(texture)), ".json");

        public static string PngPathOf(UnityEngine.Texture2D texture) => ExporterBase.ReplaceExtension(RelativePath(AssetDatabase.GetAssetPath(texture)), ".png");

        /// <summary>Writes the sheet of a texture (once per run: a sheet that was written is returned as it is).</summary>
        public static WrittenSheet Write(ExportSettings settings, ExportContext context, NameCollisionResolver names, UnityEngine.Texture2D texture)
        {
            var assetPath = AssetDatabase.GetAssetPath(texture);
            var jsonPath = JsonPathOf(texture);
            if (context.WrittenSheets.TryGetValue(jsonPath, out var written)) return written;

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter
                ?? throw new InvalidOperationException("'" + assetPath + "' is not a texture with an importer.");
            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<UnityEngine.Sprite>().ToList();
            if (sprites.Count == 0) throw new InvalidOperationException("'" + assetPath + "' has no sprites (is it imported as sprites?).");

            var sheet = new WrittenSheet { JsonPath = jsonPath, PngPath = PngPathOf(texture) };
            var entries = new List<SpriteEntry>();
            var forcedPpu = context.SpritePixelsPerUnitFor(assetPath);
            // In reading order (top row first, left to right), the order of a sheet drawn by hand.
            foreach (var sprite in sprites.OrderByDescending(s => s.rect.y).ThenBy(s => s.rect.x).ThenBy(s => s.name, StringComparer.Ordinal))
            {
                var name = names.GetUniqueName(sprite);
                sheet.Names[sprite] = name;
                entries.Add(SpriteEntry.FromUnity(name, sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height, texture.height,
                    sprite.pivot.x, sprite.pivot.y, forcedPpu ?? sprite.pixelsPerUnit));
            }

            WritePicture(settings, texture, assetPath, sheet.PngPath);
            var json = SpriteSheetJson.Build(Path.GetFileName(sheet.PngPath), importer.filterMode == UnityEngine.FilterMode.Point, entries);
            using (var stream = settings.CreateFile(jsonPath))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(json);

            context.WrittenSheets[jsonPath] = sheet;
            UnityEngine.Debug.Log("[SpriteSheetWriter] " + assetPath + " -> " + jsonPath + " (" + entries.Count + " sprites)");
            return sheet;
        }

        private static void WritePicture(ExportSettings settings, UnityEngine.Texture2D texture, string assetPath, string pngPath)
        {
            var target = settings.ResolveResourceFileName(pngPath);
            if (assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                var source = Path.GetFullPath(assetPath);
                System.IO.File.Copy(source, target, true);
                var size = PngSize(source);
                if (size != null && (size.Item1 != texture.width || size.Item2 != texture.height))
                    UnityEngine.Debug.LogWarning("[SpriteSheetWriter] " + assetPath + " is " + size.Item1 + "x" + size.Item2 + " but is imported as " + texture.width + "x" +
                                                 texture.height + " (max size or NPOT scaling); the sprite rectangles are for the imported size.", texture);
                return;
            }
            // Another format (PSD, TGA...): the imported picture, as a PNG.
            new TextureProcessor().ProcessAndSaveTexture(texture, "Hidden/UnityToRebelFork/Copy", target, hasAlpha: true, shaderArgs: new Dictionary<string, float>
            {
                { "_GammaInput", (UnityEditor.PlayerSettings.colorSpace == UnityEngine.ColorSpace.Linear) ? 0.0f : 1.0f },
                { "_GammaOutput", 1.0f },
            });
        }

        /// <summary>Width and height from the header of a PNG file, or null if it is not one.</summary>
        public static Tuple<int, int> PngSize(string path)
        {
            using (var stream = System.IO.File.OpenRead(path))
            {
                var header = new byte[24];
                if (stream.Read(header, 0, 24) < 24 || header[1] != (byte)'P' || header[2] != (byte)'N' || header[3] != (byte)'G') return null;
                int Int(int offset) => (header[offset] << 24) | (header[offset + 1] << 16) | (header[offset + 2] << 8) | header[offset + 3];
                return Tuple.Create(Int(16), Int(20));
            }
        }
    }
}
