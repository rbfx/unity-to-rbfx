using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityToRebelFork.Editor.ShaderGraphs
{
    /// <summary>The average colour of textures (linear space, as a shader reads them), which stands in for "the texture" when a Shader Graph is analysed.</summary>
    public static class TextureStatistics
    {
        private const int Size = 8;

        private static readonly Dictionary<int, Vector4> Cache = new Dictionary<int, Vector4>();

        public static Vector4 Mean(UnityEngine.Texture texture)
        {
            if (texture == null) return Vector4.one;
            var key = texture.GetInstanceID();
            if (Cache.TryGetValue(key, out var cached)) return cached;
            return Cache[key] = Compute(texture);
        }

        private static Vector4 Compute(UnityEngine.Texture texture)
        {
            if (texture.dimension != TextureDimension.Tex2D) return new Vector4(0.5f, 0.5f, 0.5f, 1f);

            var previous = RenderTexture.active;
            var target = RenderTexture.GetTemporary(Size, Size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            UnityEngine.Texture2D readback = null;
            try
            {
                // The blit samples the texture (with its mip chain, so a few pixels stand for the whole texture) the way a shader does: sRGB textures arrive decoded.
                UnityEngine.Graphics.Blit(texture, target);
                RenderTexture.active = target;
                readback = new UnityEngine.Texture2D(Size, Size, TextureFormat.RGBAFloat, false, true);
                readback.ReadPixels(new Rect(0, 0, Size, Size), 0, 0, false);
                var sum = Vector4.zero;
                foreach (var pixel in readback.GetPixels())
                    sum += new Vector4(pixel.r, pixel.g, pixel.b, pixel.a);
                return sum / (Size * Size);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
            }
        }
    }
}
