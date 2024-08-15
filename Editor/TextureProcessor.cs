using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityToRebelFork.Editor.StbSharpDxt;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// Texture processor.
    /// Runs texture mip maps through shader and saves the result.
    /// </summary>
    public class TextureProcessor
    {
        public void ProcessAndSaveTexture(UnityEngine.Texture sourceTexture, string shaderName, string fullOutputPath,
            bool hasAlpha = true, bool compress = true, Dictionary<string, float> shaderArgs = null)
        {
            ProcessAndSaveTexture(sourceTexture, UnityEngine.Shader.Find(shaderName), fullOutputPath, hasAlpha, compress, shaderArgs);
        }
        public void ProcessTexture(UnityEngine.Texture sourceTexture, string shaderName, Action<UnityEngine.Texture2D> callback)
        {
            ProcessTexture(sourceTexture, UnityEngine.Shader.Find(shaderName), callback);
        }
        public void ProcessAndSaveTexture(UnityEngine.Texture sourceTexture, UnityEngine.Shader shader, string fullOutputPath,
            bool hasAlpha = true, bool compress = true, Dictionary<string, float> shaderArgs = null)
        {
            UnityEngine.Material material = null;

            try
            {
                material = new UnityEngine.Material(shader);
                if (shaderArgs != null)
                {
                    foreach (var arg in shaderArgs)
                    {
                        if (material.HasProperty(arg.Key))
                        {
                            material.SetFloat(arg.Key, arg.Value);
                        }
                    }
                }
                ProcessAndSaveTexture(sourceTexture, material, fullOutputPath, hasAlpha: hasAlpha, compress: compress);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        public void ProcessTexture(UnityEngine.Texture sourceTexture, UnityEngine.Shader shader, Action<UnityEngine.Texture2D> callback)
        {
            UnityEngine.Material material = null;

            try
            {
                material = new UnityEngine.Material(shader);
                ProcessTexture(sourceTexture, material, callback);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }

        public void ProcessAndSaveTexture(UnityEngine.Texture sourceTexture, UnityEngine.Material material, string fullOutputPath,
            bool hasAlpha = true, bool compress = true)
        {
            ProcessAndSaveTexture(sourceTexture, sourceTexture.width, sourceTexture.height, material, fullOutputPath,
                hasAlpha: hasAlpha, compress: compress);
        }

        public void ProcessTexture(UnityEngine.Texture sourceTexture, UnityEngine.Material material, Action<UnityEngine.Texture2D> callback)
        {
            ProcessTexture(sourceTexture, sourceTexture.width, sourceTexture.height, material, callback);
        }

        public void ProcessTexture(UnityEngine.Texture sourceTexture, int width, int height, UnityEngine.Material material,
            Action<UnityEngine.Texture2D> callback)
        {
            UnityEngine.RenderTexture renderTex = null;
            UnityEngine.Texture2D texture = null;
            var lastActiveRenderTexture = UnityEngine.RenderTexture.active;

            try
            {
                var mips = sourceTexture.mipmapCount > 1;
                var descriptor = new RenderTextureDescriptor
                {
                    width = width,
                    height = height,
                    colorFormat = PickRenderTextureFormat(sourceTexture),
                    autoGenerateMips = mips,
                    depthBufferBits = 16,
                    dimension = TextureDimension.Tex2D,
                    enableRandomWrite = false,
                    memoryless = RenderTextureMemoryless.None,
                    sRGB = false,
                    useMipMap = false,
                    volumeDepth = 1,
                    msaaSamples = 1,
                };
                renderTex = RenderTexture.GetTemporary(descriptor);
                UnityEngine.Graphics.Blit(sourceTexture, renderTex, material);


                UnityEngine.RenderTexture.active = renderTex;
                texture = new UnityEngine.Texture2D(width, height, PickTextureFormat(descriptor.colorFormat), mips /* mipmap */, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0, mips);
                texture.Apply();

                callback(texture);
            }
            finally
            {
                UnityEngine.RenderTexture.active = lastActiveRenderTexture;
                if (renderTex != null)
                    UnityEngine.RenderTexture.ReleaseTemporary(renderTex);
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
            }
        }

        private TextureFormat PickTextureFormat(RenderTextureFormat format)
        {
            switch (format)
            {
                case RenderTextureFormat.ARGBHalf:
                    return TextureFormat.RGBAHalf;
                case RenderTextureFormat.RGHalf:
                    return TextureFormat.RGHalf;
                case RenderTextureFormat.RHalf:
                    return TextureFormat.RHalf;
                case RenderTextureFormat.ARGBFloat:
                    return TextureFormat.RGBAFloat;
                case RenderTextureFormat.RFloat:
                    return TextureFormat.RFloat;
                case RenderTextureFormat.RGFloat:
                    return TextureFormat.RGFloat;
            }
            return TextureFormat.ARGB32;
        }

        private RenderTextureFormat PickRenderTextureFormat(UnityEngine.Texture sourceTexture)
        {
            if (sourceTexture is UnityEngine.Texture2D texture2D)
            {
                switch (texture2D.format)
                {
                    case TextureFormat.RGBAHalf:
                    case TextureFormat.RGHalf:
                    case TextureFormat.RHalf:
                        return RenderTextureFormat.ARGBHalf;
                    case TextureFormat.RGB9e5Float:
                    case TextureFormat.RGBAFloat:
                    case TextureFormat.RGFloat:
                    case TextureFormat.RFloat:
                        return RenderTextureFormat.ARGBFloat;
                }
            }
            return RenderTextureFormat.ARGB32;
        }

        public void ProcessAndSaveTexture(UnityEngine.Texture sourceTexture, int width, int height, UnityEngine.Material material,
            string fullOutputPath, bool hasAlpha = true, bool compress = true)
        {
            ProcessTexture(sourceTexture, width, height, material,
                texture => SaveTexture(texture, fullOutputPath, hasAlpha:hasAlpha, compress:compress));
        }

        public void SaveTexture(UnityEngine.Texture2D texture, string fullOutputPath, bool hasAlpha = true, bool compress = true)
        {
            if (string.IsNullOrWhiteSpace(fullOutputPath))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));

            var ext = Path.GetExtension(fullOutputPath).ToLower();
            switch (ext)
            {
                case ".png":
                    var png = texture.EncodeToPNG();
                    WriteAllBytes(fullOutputPath, png);
                    break;
                case ".jpg":
                    var jpg = texture.EncodeToJPG();
                    WriteAllBytes(fullOutputPath, jpg);
                    break;
                case ".tga":
                    var tga = texture.EncodeToTGA();
                    WriteAllBytes(fullOutputPath, tga);
                    break;
                case ".exr":
                    var exr = texture.EncodeToEXR();
                    WriteAllBytes(fullOutputPath, exr);
                    break;
                case ".dds":
                    DDS.SaveAsRgbaDds(texture, fullOutputPath, hasAlpha: hasAlpha, compress: compress);
                    break;
                default:
                    throw new NotImplementedException("Not implemented texture file type " + ext);
            }
        }

        private void WriteAllBytes(string fullOutputPath, byte[] buffer)
        {
            using (var fs = System.IO.File.Open(fullOutputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Write(buffer, 0, buffer.Length);
            }
        }
    }

    public class DDS
    {
        public static void SaveAsRgbaDds(UnityEngine.Texture2D texture, string fileName, bool hasAlpha = true, bool compress = true, bool dither = false)
        {
            using (var fileStream = System.IO.File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    compress = compress && (0 == (texture.width % 4)) && (0 == (texture.height % 4));
                    if (compress)
                    {
                        WriteCompressedHeader(binaryWriter, texture.width, texture.height, texture.mipmapCount, false,
                            hasAlpha);
                        var width = texture.width;
                        var height = texture.height;
                        for (var mipIndex = 0; mipIndex < texture.mipmapCount; ++mipIndex)
                        {
                            WriteCompressed(binaryWriter, texture.GetPixels32(mipIndex), width, height, hasAlpha,
                                dither);
                            width = Math.Max(1, width / 2);
                            height = Math.Max(1, height / 2);
                        }
                    }
                    else
                    {
                        WriteHeader(binaryWriter, texture.width, texture.height, texture.mipmapCount, false);
                        for (var mipIndex = 0; mipIndex < texture.mipmapCount; ++mipIndex)
                            WriteAsIs(binaryWriter, texture.GetPixels32(mipIndex), Math.Max(1, texture.width / (1 << mipIndex)), true);
                    }
                }
            }
        }

        public static byte[] Compress(int width, int height, Color32[] pixels, bool hasAlpha, bool dithered = false)
        {
            var mode = CompressionMode.HighQuality;
            if (dithered)
                mode |= CompressionMode.Dithered;
            return StbDxt.CompressDxt(width, height, pixels, hasAlpha, mode);
        }

        public static void SaveAsRgbaDds(Cubemap texture, string fileName, bool convertToSRGB = false)
        {
            int skipMipMaps = 0;
            int mipMapsCount = Math.Max(1, texture.mipmapCount - skipMipMaps);
            skipMipMaps = texture.mipmapCount - mipMapsCount;
            int sizeDivisor = 1 << skipMipMaps;
            using (var fileStream = System.IO.File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var binaryWriter = new BinaryWriter(fileStream))
                {
                    WriteHeader(binaryWriter, texture.width / sizeDivisor, texture.height / sizeDivisor, mipMapsCount, true);
                    var facesInOrder = new[]
                    {
                        CubemapFace.PositiveX,
                        CubemapFace.NegativeX,
                        CubemapFace.PositiveY,
                        CubemapFace.NegativeY,
                        CubemapFace.PositiveZ,
                        CubemapFace.NegativeZ
                    };
                    foreach (var cubemapFace in facesInOrder)
                        for (var mipIndex = skipMipMaps; mipIndex < texture.mipmapCount; ++mipIndex)
                        {
                            sizeDivisor = 1 << mipIndex;
                            var pixels = texture.GetPixels(cubemapFace, mipIndex);

                            if (convertToSRGB)
                                WriteLinearAsSRGB(binaryWriter, pixels, Math.Max(1, texture.width / sizeDivisor));
                            else
                                WriteAsIs(binaryWriter, pixels, Math.Max(1, texture.width / sizeDivisor), false);
                        }
                }
            }
        }

        private static void WriteCompressed(BinaryWriter binaryWriter, Color32[] getPixels32, int width, int height,
            bool hasAlpha, bool dither = false)
        {
            //var data = StbSharpDxt.StbDxt.stb_compress_dxt(image, hasAlpha);
            var data = Compress(width, height, getPixels32, hasAlpha, dither);
            binaryWriter.Write(data);
        }

        private static void WriteCompressedHeader(BinaryWriter binaryWriter, int width, int height, int mipMapCount,
            bool cubemap, bool hasAlpha)
        {
            binaryWriter.Write((uint)0x20534444); // DDS magic
            binaryWriter.Write(124); // Size
            binaryWriter.Write(0x00001007 | 0x000A0000); // Flags
            binaryWriter.Write(height); // Height
            binaryWriter.Write(width); // Width
            if (hasAlpha)
                binaryWriter.Write(height * width); // Pitch
            else
                binaryWriter.Write(height * width / 2); // Pitch

            binaryWriter.Write(1); // Depth
            binaryWriter.Write(mipMapCount); // MipMapCount

            binaryWriter.Write(0); //Reserved1
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);

            // Pixel format
            binaryWriter.Write(32); //Size
            binaryWriter.Write(0x00000004); //compressed
            if (hasAlpha)
                binaryWriter.Write(0x35545844); //DXT5
            else
                binaryWriter.Write(0x31545844); //DXT1
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);

            // Caps
            if (cubemap)
            {
                binaryWriter.Write(0x00001000 | 0x00400008 | 0x00000008);
                binaryWriter.Write(0x00000600 | 0x00000a00 | 0x00001200 | 0x00002200 | 0x00004200 | 0x00008200);
            }
            else
            {
                binaryWriter.Write(0x00001000 | 0x00400008);
                binaryWriter.Write(0);
            }

            binaryWriter.Write(0); // Caps3
            binaryWriter.Write(0); // Caps4
            binaryWriter.Write(0); // Reserved
        }

        private static void WriteHeader(BinaryWriter binaryWriter, int width, int height, int mipMapCount, bool cubemap)
        {
            binaryWriter.Write((uint)0x20534444); // DDS magic
            binaryWriter.Write(124); // Size
            binaryWriter.Write(0x00001007 | 0x00020000 | 0x00000008); // Flags
            binaryWriter.Write(height); // Height
            binaryWriter.Write(width); // Width
            binaryWriter.Write(width * 4); // Pitch

            binaryWriter.Write(1); // Depth
            binaryWriter.Write(mipMapCount); // MipMapCount

            binaryWriter.Write(0); //Reserved1
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);
            binaryWriter.Write(0);

            // Pixel format
            binaryWriter.Write(32); //Size
            binaryWriter.Write(0x00000041); //RGBA
            binaryWriter.Write(0);
            binaryWriter.Write(32);
            binaryWriter.Write((uint)0x000000ff);
            binaryWriter.Write((uint)0x0000ff00);
            binaryWriter.Write((uint)0x00ff0000);
            binaryWriter.Write(0xff000000);

            // Caps
            if (cubemap)
            {
                binaryWriter.Write(0x00001000 | 0x00400008 | 0x00000008);
                binaryWriter.Write(0x00000600 | 0x00000a00 | 0x00001200 | 0x00002200 | 0x00004200 | 0x00008200);
            }
            else
            {
                binaryWriter.Write(0x00001000 | 0x00400008);
                binaryWriter.Write(0);
            }

            binaryWriter.Write(0); // Caps3
            binaryWriter.Write(0); // Caps4
            binaryWriter.Write(0); // Reserved
        }
        private static Color LinearToSRGB(Color rgb)
        {
            Color RGB = rgb;
            var S1 = new Color(Mathf.Sqrt(RGB.r), Mathf.Sqrt(RGB.g), Mathf.Sqrt(RGB.b), RGB.a);
            var S2 = new Color(Mathf.Sqrt(S1.r), Mathf.Sqrt(S1.g), Mathf.Sqrt(S1.b), RGB.a);
            var S3 = new Color(Mathf.Sqrt(S2.r), Mathf.Sqrt(S2.g), Mathf.Sqrt(S2.b), RGB.a);
            var k1 = new Color(0.662002687f, 0.662002687f, 0.662002687f, 1);
            var k2 = new Color(0.684122060f, 0.684122060f, 0.684122060f, 1);
            var k3 = new Color(0.323583601f, 0.323583601f, 0.323583601f, 1);
            var k4 = new Color(0.0225411470f, 0.0225411470f, 0.0225411470f, 1);
            var res = k1 * S1 + k2 * S2 - k3 * S3 - k4 * RGB;
            return new Color(res.r, res.g, res.b, RGB.a);
        }

        private static Color32 LinearToSRGB(Color32 rgb)
        {
            Color RGB = rgb;
            var S1 = new Color(Mathf.Sqrt(RGB.r), Mathf.Sqrt(RGB.g), Mathf.Sqrt(RGB.b), RGB.a);
            var S2 = new Color(Mathf.Sqrt(S1.r), Mathf.Sqrt(S1.g), Mathf.Sqrt(S1.b), RGB.a);
            var S3 = new Color(Mathf.Sqrt(S2.r), Mathf.Sqrt(S2.g), Mathf.Sqrt(S2.b), RGB.a);
            var k1 = new Color(0.662002687f, 0.662002687f, 0.662002687f, 1);
            var k2 = new Color(0.684122060f, 0.684122060f, 0.684122060f, 1);
            var k3 = new Color(0.323583601f, 0.323583601f, 0.323583601f, 1);
            var k4 = new Color(0.0225411470f, 0.0225411470f, 0.0225411470f, 1);
            var res = k1 * S1 + k2 * S2 - k3 * S3 - k4 * RGB;
            return new Color(res.r, res.g, res.b, RGB.a);
        }

        private static void WriteLinearAsSRGB(BinaryWriter binaryWriter, Color[] getPixels, int textureWidth)
        {
            foreach (var c in getPixels)
            {
                Color32 rgb = LinearToSRGB(c);
                binaryWriter.Write(rgb.r);
                binaryWriter.Write(rgb.g);
                binaryWriter.Write(rgb.b);
                binaryWriter.Write(rgb.a);
            }
        }

        private static void WriteAsIs(BinaryWriter binaryWriter, Color32[] getPixels32, int width, bool flipVertically)
        {
            if (flipVertically)
            {
                var height = getPixels32.Length / width;
                for (var y = height - 1; y >= 0; y--)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = x + y * width;
                        var color32 = getPixels32[index];
                        binaryWriter.Write(color32.r);
                        binaryWriter.Write(color32.g);
                        binaryWriter.Write(color32.b);
                        binaryWriter.Write(color32.a);
                    }
                }
            }
            else
            {
                foreach (var c in getPixels32)
                {
                    Color32 rgb = c;
                    binaryWriter.Write(rgb.r);
                    binaryWriter.Write(rgb.g);
                    binaryWriter.Write(rgb.b);
                    binaryWriter.Write(rgb.a);
                }
            }
        }
        private static void WriteAsIs(BinaryWriter binaryWriter, Color[] getPixels32, int width, bool flipVertically)
        {
            if (flipVertically)
            {
                var height = getPixels32.Length / width;
                for (var y = height - 1; y >= 0; y--)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var index = x + y * width;
                        Color32 color32 = getPixels32[index];
                        binaryWriter.Write(color32.r);
                        binaryWriter.Write(color32.g);
                        binaryWriter.Write(color32.b);
                        binaryWriter.Write(color32.a);
                    }
                }
            }
            else
            {
                foreach (var c in getPixels32)
                {
                    Color32 rgb = c;
                    binaryWriter.Write(rgb.r);
                    binaryWriter.Write(rgb.g);
                    binaryWriter.Write(rgb.b);
                    binaryWriter.Write(rgb.a);
                }
            }
        }
    }
}
