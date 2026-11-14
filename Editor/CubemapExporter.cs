using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// A cube map to export as an HDR texture (a probe's baked reflection, the scene's default reflection): what the exporter is asked
    /// for and what the orchestrator uses to tell requests apart.
    /// </summary>
    public class CubemapExport : IEquatable<CubemapExport>
    {
        /// <summary>The cube map (a Cubemap asset, or the texture of RenderSettings.defaultReflection).</summary>
        public UnityEngine.Texture Cubemap;

        /// <summary>How the texels are encoded: the probe's textureHDRDecodeValues. The default is plain linear HDR data.</summary>
        public Vector4 HdrDecode = new Vector4(1f, 1f, 0f, 1f);

        /// <summary>Multiplies every texel (RenderSettings.reflectionIntensity of a zone's texture).</summary>
        public float Scale = 1f;

        /// <summary>Keep the mip levels of the cube map instead of averaging them from level 0 (Unity's are filtered for roughness).</summary>
        public bool KeepSourceMips;

        /// <summary>The file name of a cube map that is not an asset, without extension.</summary>
        public string Name;

        public bool Equals(CubemapExport other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Cubemap, other.Cubemap) && HdrDecode == other.HdrDecode && Mathf.Approximately(Scale, other.Scale)
                   && KeepSourceMips == other.KeepSourceMips && Name == other.Name;
        }

        public override bool Equals(object obj) => obj is CubemapExport other && Equals(other);

        public override int GetHashCode()
            => Helpers.CombineHashCodes(Helpers.CombineHashCodes(Cubemap, Scale), Helpers.CombineHashCodes(KeepSourceMips, Name));
    }

    /// <summary>
    /// Writes a cube map as <c>X.dds</c> (RGBA16F, DX10 header, mip chain) and <c>X.xml</c> (rbfx's cube map description with the
    /// ambient light as <c>&lt;sh&gt;</c>). Half floats keep light brighter than 1, are filterable on every GPU that rbfx and Toe run on
    /// (WebGL2 included) and take 8 bytes a texel. The XML is the resource the scene refers to.
    /// </summary>
    public class CubemapExporter : ExporterBase<CubemapExport>
    {
        /// <summary>The level the harmonics are made from: the smallest one that is at least this wide.</summary>
        private const int ShSize = 32;

        public CubemapExporter(NameCollisionResolver nameCollisionResolver, ExportContext context, ExportSettings settings) : base(nameCollisionResolver, context, settings)
        {
        }

        public override string EvaluateResourcePath(CubemapExport asset)
        {
            string suffix = Mathf.Approximately(asset.Scale, 1f) && !asset.KeepSourceMips
                ? ""
                : "." + (asset.GetHashCode() & 0x7fffffff).ToString("x");
            if (!string.IsNullOrWhiteSpace(AssetDatabase.GetAssetPath(asset.Cubemap)))
                return EvaluateResourcePath(asset.Cubemap, suffix + ".xml");
            return CombinePath(context.TempFolder, SafeFileName(asset.Name ?? asset.Cubemap.name) + suffix + ".xml");
        }

        public override IEnumerable Export(CubemapExport asset)
        {
            yield return asset.Cubemap.name;

            var xmlPath = EvaluateResourcePath(asset);
            var ddsPath = ReplaceExtension(xmlPath, ".dds");
            var size = Mathf.Max(1, asset.Cubemap.width);

            // faces[face][level] are RGBA floats, rows top first.
            var faces = new float[6][][];
            var levelCount = LevelCount(size);
            var sourceLevels = asset.KeepSourceMips ? Mathf.Min(asset.Cubemap.mipmapCount, levelCount) : 1;

            var material = new UnityEngine.Material(UnityEngine.Shader.Find("Hidden/UnityToRebelFork/CubeFace"));
            try
            {
                material.SetTexture("_Cube", asset.Cubemap);
                material.SetVector("_DecodeInstructions", asset.HdrDecode);
                for (var face = 0; face < 6; face++)
                {
                    faces[face] = new float[levelCount][];
                    for (var level = 0; level < sourceLevels; level++)
                        faces[face][level] = ReadFace(material, face, level, Mathf.Max(1, size >> level), asset.Scale);
                    for (var level = sourceLevels; level < levelCount; level++)
                        faces[face][level] = Downsample(faces[face][level - 1], Mathf.Max(1, size >> (level - 1)));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }

            yield return "writing " + Path.GetFileName(ddsPath);
            var ddsFile = Settings.ResolveResourceFileName(ddsPath);
            WriteDds(ddsFile, faces, size);

            var shLevel = 0;
            while (shLevel + 1 < levelCount && (size >> (shLevel + 1)) >= ShSize) shLevel++;
            var sh = ProjectSphericalHarmonics(faces, shLevel, Mathf.Max(1, size >> shLevel));
            System.IO.File.WriteAllText(Settings.ResolveResourceFileName(xmlPath), BuildXml(Path.GetFileName(ddsPath), sh), new UTF8Encoding(false));

            // Toe filters a cube map into its roughness chain when a game first uses it; the batch export has the Toe CLI bake that into the file instead.
            if (Settings.Target == ExportTarget.Toe && Settings.PrefilterCubemaps && !context.CubemapsToPrefilter.Contains(xmlPath))
                context.CubemapsToPrefilter.Add(xmlPath);
        }

        private static int LevelCount(int size)
        {
            var count = 1;
            while (size > 1) { size >>= 1; count++; }
            return count;
        }

        /// <summary>One face of one level as linear RGBA floats, row 0 at the top of the face.</summary>
        private static float[] ReadFace(UnityEngine.Material material, int face, int level, int size, float scale)
        {
            var target = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            var previous = RenderTexture.active;
            UnityEngine.Texture2D readback = null;
            try
            {
                material.SetFloat("_Face", face);
                material.SetFloat("_Mip", level);
                UnityEngine.Graphics.Blit(null, target, material);
                RenderTexture.active = target;
                readback = new UnityEngine.Texture2D(size, size, TextureFormat.RGBAFloat, false, true);
                readback.ReadPixels(new Rect(0, 0, size, size), 0, 0);
                var pixels = readback.GetPixels();

                var result = new float[size * size * 4];
                for (var row = 0; row < size; row++)
                {
                    var source = (size - 1 - row) * size; // ReadPixels starts at the bottom row.
                    for (var x = 0; x < size; x++)
                    {
                        var c = pixels[source + x];
                        var o = (row * size + x) * 4;
                        result[o] = Finite(c.r * scale);
                        result[o + 1] = Finite(c.g * scale);
                        result[o + 2] = Finite(c.b * scale);
                        result[o + 3] = 1f;
                    }
                }
                return result;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        private static float Finite(float value) => float.IsNaN(value) || value < 0f ? 0f : Mathf.Min(value, 65504f);

        /// <summary>The next level down: every 2 by 2 texels averaged.</summary>
        private static float[] Downsample(float[] source, int size)
        {
            var half = Mathf.Max(1, size / 2);
            var result = new float[half * half * 4];
            for (var y = 0; y < half; y++)
            {
                var y0 = Mathf.Min(y * 2, size - 1);
                var y1 = Mathf.Min(y * 2 + 1, size - 1);
                for (var x = 0; x < half; x++)
                {
                    var x0 = Mathf.Min(x * 2, size - 1);
                    var x1 = Mathf.Min(x * 2 + 1, size - 1);
                    for (var c = 0; c < 4; c++)
                        result[(y * half + x) * 4 + c] = 0.25f * (source[(y0 * size + x0) * 4 + c] + source[(y0 * size + x1) * 4 + c]
                                                                   + source[(y1 * size + x0) * 4 + c] + source[(y1 * size + x1) * 4 + c]);
                }
            }
            return result;
        }

        // ---- DDS --------------------------------------------------------------------------------------------------------------

        /// <summary>A DX10 DDS cube map of R16G16B16A16_FLOAT: for each face its whole mip chain.</summary>
        private static void WriteDds(string fileName, float[][][] faces, int size)
        {
            var levels = faces[0].Length;
            using (var stream = System.IO.File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var w = new BinaryWriter(stream))
            {
                w.Write(0x20534444u);                 // "DDS "
                w.Write(124);                         // header size
                w.Write(0x1007 | 0x20000 | 0x8);      // caps, height, width, pixel format, mip map count, pitch
                w.Write(size);                        // height
                w.Write(size);                        // width
                w.Write(size * 8);                    // pitch
                w.Write(1);                           // depth
                w.Write(levels);                      // mip map count
                for (var i = 0; i < 11; i++) w.Write(0);
                w.Write(32);                          // pixel format size
                w.Write(0x4);                         // FourCC
                w.Write(0x30315844u);                 // "DX10"
                for (var i = 0; i < 5; i++) w.Write(0);
                w.Write(0x1000 | 0x8 | 0x400000);     // caps: texture, complex, mip map
                w.Write(0x200 | 0xFC00);              // caps2: cube map, all six faces
                w.Write(0);
                w.Write(0);
                w.Write(0);
                // DXT10 header
                w.Write(10);                          // DXGI_FORMAT_R16G16B16A16_FLOAT
                w.Write(3);                           // texture 2D
                w.Write(0x4);                         // texture cube
                w.Write(1);                           // array size
                w.Write(0);

                for (var face = 0; face < 6; face++)
                    for (var level = 0; level < levels; level++)
                    {
                        var data = faces[face][level];
                        var bytes = new byte[data.Length * 2];
                        for (var i = 0; i < data.Length; i++)
                        {
                            var h = ToHalf(data[i]);
                            bytes[i * 2] = (byte)(h & 0xFF);
                            bytes[i * 2 + 1] = (byte)(h >> 8);
                        }
                        w.Write(bytes);
                    }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatBits
        {
            [FieldOffset(0)] public float Float;
            [FieldOffset(0)] public uint Bits;
        }

        /// <summary>A non-negative finite float as IEEE half (rounded to nearest; too large values give the largest half).</summary>
        private static ushort ToHalf(float value)
        {
            if (!(value > 0f)) return 0;
            var x = new FloatBits { Float = value }.Bits;
            var exponent = (int)((x >> 23) & 0xFF) - 127 + 15;
            var mantissa = x & 0x7FFFFF;
            if (exponent >= 31) return 0x7BFF;
            if (exponent <= 0)
            {
                if (exponent < -10) return 0;
                mantissa |= 0x800000;
                var shift = 14 - exponent;
                var subnormal = mantissa >> shift;
                if (((mantissa >> (shift - 1)) & 1) != 0) subnormal++;
                return (ushort)subnormal;
            }
            var half = ((uint)exponent << 10) | (mantissa >> 13);
            if ((mantissa & 0x1000) != 0) half++;
            return half >= 0x7C00 ? (ushort)0x7BFF : (ushort)half;
        }

        // ---- Spherical harmonics ----------------------------------------------------------------------------------------------

        private static readonly float[] ShFactors = { 0.282095f, 0.488603f, 0.488603f, 0.488603f, 1.092548f, 1.092548f, 0.315392f, 1.092548f, 0.546274f };
        private static readonly float[] ShCosines = { 1f, 2f / 3f, 2f / 3f, 2f / 3f, 0.25f, 0.25f, 0.25f, 0.25f, 0.25f };

        /// <summary>Nine RGB coefficients of the light around the probe: the sum of texel colour times basis times solid angle.</summary>
        private static Vector3[] ProjectSphericalHarmonics(float[][][] faces, int level, int size)
        {
            var sh = new Vector3[9];
            var basis = new float[9];
            for (var face = 0; face < 6; face++)
            {
                var data = faces[face][level];
                for (var y = 0; y < size; y++)
                    for (var x = 0; x < size; x++)
                    {
                        float s = 2f * (x + 0.5f) / size - 1f, t = 2f * (y + 0.5f) / size - 1f;
                        Vector3 d;
                        switch (face)
                        {
                            case 0: d = new Vector3(1f, -t, -s); break;
                            case 1: d = new Vector3(-1f, -t, s); break;
                            case 2: d = new Vector3(s, 1f, t); break;
                            case 3: d = new Vector3(s, -1f, -t); break;
                            case 4: d = new Vector3(s, -t, 1f); break;
                            default: d = new Vector3(-s, -t, -1f); break;
                        }
                        var lengthSquared = d.sqrMagnitude;
                        var length = Mathf.Sqrt(lengthSquared);
                        var solidAngle = 4f / (size * size) / (lengthSquared * length);
                        d /= length;

                        basis[0] = ShFactors[0];
                        basis[1] = ShFactors[1] * d.y;
                        basis[2] = ShFactors[2] * d.z;
                        basis[3] = ShFactors[3] * d.x;
                        basis[4] = ShFactors[4] * d.x * d.y;
                        basis[5] = ShFactors[5] * d.y * d.z;
                        basis[6] = ShFactors[6] * (3f * d.z * d.z - 1f);
                        basis[7] = ShFactors[7] * d.x * d.z;
                        basis[8] = ShFactors[8] * (d.x * d.x - d.y * d.y);

                        var i = (y * size + x) * 4;
                        var color = new Vector3(data[i], data[i + 1], data[i + 2]);
                        for (var k = 0; k < 9; k++) sh[k] += color * (basis[k] * solidAngle);
                    }
            }
            return sh;
        }

        /// <summary>The <c>&lt;cubemap&gt;</c> description with the harmonics packed as rbfx's SphericalHarmonicsDot9.</summary>
        private static string BuildXml(string ddsFileName, Vector3[] sh)
        {
            var v = new Vector3[9];
            for (var i = 0; i < 9; i++) v[i] = sh[i] * (ShCosines[i] * ShFactors[i]);
            v[0] -= v[6];
            v[6] *= 3f;

            string V4(float x, float y, float z, float w) => string.Format(CultureInfo.InvariantCulture, "{0:R} {1:R} {2:R} {3:R}", x, y, z, w);

            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\"?>");
            xml.AppendLine("<cubemap>");
            xml.AppendLine("    <image name=\"" + ddsFileName + "\" />");
            xml.AppendLine("    <sh"
                           + " ar=\"" + V4(v[3].x, v[1].x, v[2].x, v[0].x) + "\""
                           + " ag=\"" + V4(v[3].y, v[1].y, v[2].y, v[0].y) + "\""
                           + " ab=\"" + V4(v[3].z, v[1].z, v[2].z, v[0].z) + "\""
                           + " br=\"" + V4(v[4].x, v[5].x, v[6].x, v[7].x) + "\""
                           + " bg=\"" + V4(v[4].y, v[5].y, v[6].y, v[7].y) + "\""
                           + " bb=\"" + V4(v[4].z, v[5].z, v[6].z, v[7].z) + "\""
                           + " c=\"" + V4(v[8].x, v[8].y, v[8].z, 1f) + "\" />");
            xml.AppendLine("</cubemap>");
            return xml.ToString();
        }
    }
}
