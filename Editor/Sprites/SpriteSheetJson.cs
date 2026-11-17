using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace UnityToRebelFork.Editor
{
    /// <summary>One sprite of a sheet, in the terms of the Toe sprite sheet file: texels from the TOP-LEFT of the image, pivot 0..1 with y up.</summary>
    public sealed class SpriteEntry
    {
        public string Name;
        public int X, Y, Width, Height;
        public float PivotX = 0.5f, PivotY = 0.5f;
        public float PixelsPerUnit = 100f;

        /// <summary>
        /// Makes an entry from what Unity gives: a rectangle with the origin at the BOTTOM-left of the texture, and a pivot in pixels from the bottom-left of that rectangle.
        /// </summary>
        public static SpriteEntry FromUnity(string name, float x, float y, float width, float height, int textureHeight, float pivotPixelsX, float pivotPixelsY, float pixelsPerUnit)
        {
            int w = (int)Math.Round(width), h = (int)Math.Round(height);
            return new SpriteEntry
            {
                Name = name,
                X = (int)Math.Round(x),
                Y = textureHeight - (int)Math.Round(y) - h,
                Width = w,
                Height = h,
                PivotX = w > 0 ? pivotPixelsX / w : 0.5f,
                PivotY = h > 0 ? pivotPixelsY / h : 0.5f,
                PixelsPerUnit = pixelsPerUnit,
            };
        }
    }

    /// <summary>Writes the sprite sheet file of the Toe engine (<c>SpriteSheet</c>): JSON with the texture, the pixels per unit and the sprites.</summary>
    public static class SpriteSheetJson
    {
        /// <summary>
        /// <code>{ "texture": "Wolf.png", "pixelsPerUnit": 16, "pixelArt": true,
        ///   "sprites": { "Idle_0": { "rect": [0, 0, 32, 32], "pivot": [0.5, 0] }, ... } }</code>
        /// The sheet's <c>pixelsPerUnit</c> is the one most sprites have; a sprite that differs says its own (<c>ppu</c>). A pivot in the middle is left out.
        /// </summary>
        public static string Build(string textureFileName, bool pixelArt, IReadOnlyList<SpriteEntry> sprites)
        {
            float sheetPpu = sprites.Count == 0 ? 100f
                : sprites.GroupBy(s => s.PixelsPerUnit).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key;

            var json = new StringBuilder();
            json.Append("{ \"texture\": ").Append(Quote(textureFileName));
            json.Append(", \"pixelsPerUnit\": ").Append(Number(sheetPpu));
            json.Append(", \"pixelArt\": ").Append(pixelArt ? "true" : "false");
            json.AppendLine(",");
            json.AppendLine("  \"sprites\": {");
            for (int i = 0; i < sprites.Count; i++)
            {
                var s = sprites[i];
                json.Append("    ").Append(Quote(s.Name)).Append(": { \"rect\": [")
                    .Append(s.X.ToString(CultureInfo.InvariantCulture)).Append(", ")
                    .Append(s.Y.ToString(CultureInfo.InvariantCulture)).Append(", ")
                    .Append(s.Width.ToString(CultureInfo.InvariantCulture)).Append(", ")
                    .Append(s.Height.ToString(CultureInfo.InvariantCulture)).Append("]");
                if (Math.Abs(s.PivotX - 0.5f) > 1e-4f || Math.Abs(s.PivotY - 0.5f) > 1e-4f)
                    json.Append(", \"pivot\": [").Append(Number(s.PivotX)).Append(", ").Append(Number(s.PivotY)).Append("]");
                if (Math.Abs(s.PixelsPerUnit - sheetPpu) > 1e-4f)
                    json.Append(", \"ppu\": ").Append(Number(s.PixelsPerUnit));
                json.Append(" }").AppendLine(i + 1 < sprites.Count ? "," : string.Empty);
            }
            json.AppendLine("  }");
            json.AppendLine("}");
            return json.ToString();
        }

        /// <summary>A number the way JSON wants it, whatever the culture: up to six decimals, no trailing zeros.</summary>
        public static string Number(float value) => value.ToString("0.######", CultureInfo.InvariantCulture);

        /// <summary>A JSON string.</summary>
        public static string Quote(string text)
        {
            var sb = new StringBuilder("\"");
            foreach (char c in text ?? string.Empty)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.Append('"').ToString();
        }
    }
}
