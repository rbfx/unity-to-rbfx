using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityToRebelFork.Editor.ShaderGraphs
{
    /// <summary>Feeds a Shader Graph evaluation with a Unity material: its property values, its textures (each stands for its average colour) and its keywords.</summary>
    public sealed class MaterialGraphInputs : IShaderGraphInputs
    {
        private readonly UnityEngine.Material material;
        private readonly bool linearColorSpace;
        private readonly Dictionary<string, UnityEngine.Texture> textures = new Dictionary<string, UnityEngine.Texture>();
        private readonly Dictionary<string, bool> normalMaps = new Dictionary<string, bool>();

        public MaterialGraphInputs(UnityEngine.Material material, bool linearColorSpace)
        {
            this.material = material;
            this.linearColorSpace = linearColorSpace;
        }

        public bool LinearColorSpace => linearColorSpace;

        public bool TryGetValue(SgProperty property, out Val value)
        {
            value = default;
            if (!material.HasProperty(property.Reference)) return false;
            switch (property.Kind)
            {
                case SgPropertyKind.Float:
                    value = Val.Splat(material.GetFloat(property.Reference));
                    return true;
                case SgPropertyKind.Boolean:
                    value = Val.Splat(material.GetFloat(property.Reference) > 0.5f ? 1f : 0f);
                    return true;
                case SgPropertyKind.Vector:
                {
                    var v = material.GetVector(property.Reference);
                    value = new Val(v.x, v.y, v.z, v.w);
                    return true;
                }
                case SgPropertyKind.Color:
                {
                    // A colour property is stored as it was entered (gamma space) and reaches the shader converted to linear, unless it is an HDR colour.
                    var c = material.GetColor(property.Reference);
                    value = new Val(c.r, c.g, c.b, c.a);
                    if (!property.Hdr && linearColorSpace)
                    {
                        value.X = ShaderGraphEvaluator.GammaToLinear(value.X);
                        value.Y = ShaderGraphEvaluator.GammaToLinear(value.Y);
                        value.Z = ShaderGraphEvaluator.GammaToLinear(value.Z);
                    }
                    return true;
                }
            }
            return false;
        }

        public string GetTexture(SgProperty property)
        {
            // A property the material has but leaves empty reads the default texture of its type (white, black, grey or bump), not the graph's asset.
            if (!material.HasProperty(property.Reference)) return null;
            var texture = material.GetTexture(property.Reference);
            if (texture == null) return "#none" + property.DefaultType;
            textures[property.Reference] = texture;
            return property.Reference;
        }

        public bool IsKeywordEnabled(string reference)
        {
            return material.IsKeywordEnabled(reference);
        }

        /// <summary>The texture an identity stands for (null for "no texture").</summary>
        public UnityEngine.Texture Resolve(string id)
        {
            if (string.IsNullOrEmpty(id) || id.StartsWith("#none", System.StringComparison.Ordinal)) return null;
            if (textures.TryGetValue(id, out var texture)) return texture;
            if (id.StartsWith("inline:", System.StringComparison.Ordinal))
            {
                var path = AssetDatabase.GUIDToAssetPath(id.Substring("inline:".Length));
                texture = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<UnityEngine.Texture>(path);
                textures[id] = texture;
                return texture;
            }
            return null;
        }

        private bool IsNormalMap(string id, UnityEngine.Texture texture)
        {
            if (normalMaps.TryGetValue(id, out var cached)) return cached;
            var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
            return normalMaps[id] = importer != null && importer.textureType == TextureImporterType.NormalMap;
        }

        public Val Sample(string id, SgSampleKind kind, Val uv)
        {
            if (id.StartsWith("#none", System.StringComparison.Ordinal))
            {
                switch (id[id.Length - 1])
                {
                    case '1': return new Val(0f, 0f, 0f, 1f);
                    case '2': return new Val(0.5f, 0.5f, 0.5f, 1f);
                    case '3': return new Val(0.5f, 0.5f, 1f, 1f);
                    default: return new Val(1f, 1f, 1f, 1f);
                }
            }

            var texture = Resolve(id);
            // A normal map stands for the flat normal, whatever its average is (it is packed in a format of its own).
            if (kind == SgSampleKind.Normal || texture != null && IsNormalMap(id, texture)) return new Val(0.5f, 0.5f, 1f, 1f);
            if (texture == null) return new Val(1f, 1f, 1f, 1f);

            var mean = TextureStatistics.Mean(texture);
            return new Val(mean.x, mean.y, mean.z, mean.w);
        }
    }
}
