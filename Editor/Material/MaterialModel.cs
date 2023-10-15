using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class MaterialModel
    {
        public IList<TechniqueModel> Techniques { get; } = new List<TechniqueModel>();
        public IDictionary<string, string> Textures { get; } = new Dictionary<string, string>();
        public HashSet<string> VSDefines { get; set; } = new HashSet<string>();
        public HashSet<string> PSDefines { get; } = new HashSet<string>();

        public string Cull { get; set; } = Editor.CullMode.CCW;

        public string ShadowCull { get; set; } = Editor.CullMode.CCW;

        public string FillMode { get; set; } = Editor.FillMode.Solid;

        public Dictionary<string, Variant> ShaderParameters { get; set; } = new Dictionary<string, Variant>();

        public Vector4 UOffset
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(UOffset), out var value))
                    return value.Vector4;
                return new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
            }
            set
            {
                ShaderParameters[nameof(UOffset)] = value;
            }
        }

        public Vector4 VOffset
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(VOffset), out var value))
                    return value.Vector4;
                return new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
            }
            set
            {
                ShaderParameters[nameof(VOffset)] = (Variant)value;
            }
        }

        public Vector4 MatDiffColor
        {
            get
            {
                if (ShaderParameters.TryGetValue( nameof(MatDiffColor), out var value))
                    return value.Vector4;
                return new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            }
            set
            {
                ShaderParameters[nameof(MatDiffColor)] = value;
            }
        }

        public Vector3 MatEmissiveColor
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(MatEmissiveColor), out var value))
                    return value.Vector3;
                return new Vector3(0.0f, 0.0f, 0.0f);
            }
            set
            {
                ShaderParameters[nameof(MatEmissiveColor)] = value;
            }
        }

        public Vector4 MatEnvMapColor
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(MatEnvMapColor), out var value))
                    return value.Vector3;
                return new Vector3(1.0f, 1.0f, 1.0f);
            }
            set
            {
                ShaderParameters[nameof(MatEnvMapColor)] = value;
            }
        }

        public Vector4 MatSpecColor
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(MatSpecColor), out var value))
                    return value.Vector4;
                return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            set
            {
                ShaderParameters[nameof(MatSpecColor)] = value;
            }
        }

        public float Roughness
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(Roughness), out var value))
                    return value.Float;
                return 1.0f;
            }
            set
            {
                ShaderParameters[nameof(Roughness)] = value;
            }
        }

        public float Metallic
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(Metallic), out var value))
                    return value.Float;
                return 1.0f;
            }
            set
            {
                ShaderParameters[nameof(Metallic)] = value;
            }
        }

        public float DielectricReflectance
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(DielectricReflectance), out var value))
                    return value.Float;
                return 0.5f;
            }
            set
            {
                ShaderParameters[nameof(DielectricReflectance)] = value;
            }
        }

        public float NormalScale
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(NormalScale), out var value))
                    return value.Float;
                return 1.0f;
            }
            set
            {
                ShaderParameters[nameof(NormalScale)] = value;
            }
        }

        public float AlphaCutoff
        {
            get
            {
                if (ShaderParameters.TryGetValue(nameof(AlphaCutoff), out var value))
                    return value.Float;
                return 0.5f;
            }
            set
            {
                ShaderParameters[nameof(AlphaCutoff)] = value;
            }
        }

        public string Albedo
        {
            get
            {
                if (Textures.TryGetValue(nameof(Albedo), out var value))
                    return value;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Textures.Remove(nameof(Albedo));
                else
                    Textures[nameof(Albedo)] = value;
            }
        }

        public string Normal
        {
            get
            {
                if (Textures.TryGetValue(nameof(Normal), out var value))
                    return value;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Textures.Remove(nameof(Normal));
                else
                    Textures[nameof(Normal)] = value;
            }
        }

        public string Properties
        {
            get
            {
                if (Textures.TryGetValue(nameof(Properties), out var value))
                    return value;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Textures.Remove(nameof(Properties));
                else
                    Textures[nameof(Properties)] = value;
            }
        }

        public string Emission
        {
            get
            {
                if (Textures.TryGetValue(nameof(Emission), out var value))
                    return value;
                return null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    Textures.Remove(nameof(Emission));
                else
                    Textures[nameof(Emission)] = value;
            }
        }

        public void Save(StreamWriter writer)
        {
            var doc = new XDocument();
            var xMaterial = new XElement(XName.Get("material"));
            doc.Add(xMaterial);
            foreach (var technique in Techniques)
            {
                xMaterial.Add(new XElement(XName.Get("technique")
                    , new XAttribute(XName.Get("name"), technique.Name)
                    , new XAttribute(XName.Get("quality"), technique.Quality)
                    , new XAttribute(XName.Get("loddistance"), technique.LODDistance)));
            }
            foreach (var texture in Textures)
            {
                xMaterial.Add(new XElement(XName.Get("texture")
                    , new XAttribute(XName.Get("slot"), texture.Key)
                    , new XAttribute(XName.Get("name"), texture.Value)
                    ));
            }

            if (VSDefines.Count > 0 || PSDefines.Count > 0)
            {
                xMaterial.Add(new XElement(XName.Get("shader")
                    , new XAttribute(XName.Get("vsdefines"), string.Join(" ", VSDefines))
                    , new XAttribute(XName.Get("psdefines"), string.Join(" ", PSDefines))));
            }

            foreach (var shaderParameter in ShaderParameters)
            {
                xMaterial.Add(new XElement(XName.Get("parameter")
                    , new XAttribute(XName.Get("name"), shaderParameter.Key)
                    , new XAttribute(XName.Get("type"), shaderParameter.Value.Type.GetName())
                    , new XAttribute(XName.Get("value"), shaderParameter.Value.ToString())));
            }

            xMaterial.Add(new XElement(XName.Get("cull"), new XAttribute(XName.Get("value"), Cull)));
            xMaterial.Add(new XElement(XName.Get("shadowcull"), new XAttribute(XName.Get("value"), ShadowCull)));
            xMaterial.Add(new XElement(XName.Get("fill"), new XAttribute(XName.Get("value"), FillMode)));
            xMaterial.Add(new XElement(XName.Get("depthbias")
                , new XAttribute(XName.Get("constant"), DepthConstantBias)
                , new XAttribute(XName.Get("slopescaled"), DepthSlopeScaledBias)
                , new XAttribute(XName.Get("normaloffset"), DepthNormalOffset)
                ));

            xMaterial.Add(new XElement(XName.Get("alphatocoverage")
                , new XAttribute(XName.Get("enable"), AlphaToCoverage)
            ));
            xMaterial.Add(new XElement(XName.Get("lineantialias")
                , new XAttribute(XName.Get("enable"), LineAntiAlias)
            ));
            xMaterial.Add(new XElement(XName.Get("renderorder")
                , new XAttribute(XName.Get("value"), RenderOrder)
            ));
            xMaterial.Add(new XElement(XName.Get("occlusion")
                , new XAttribute(XName.Get("enable"), Occlusion)
            ));

            doc.Save(writer);
        }

        public bool LineAntiAlias { get; set; }

        public bool Occlusion { get; set; } = true;

        public int RenderOrder { get; set; } = 128;
        public bool AlphaToCoverage { get; set; }

        public float DepthNormalOffset { get; set; }

        public float DepthSlopeScaledBias { get; set; }

        public float DepthConstantBias { get; set; }
    }

    public class TechniqueModel
    {
        public string Name { get; set; }
        public int Quality { get; set; }
        public float LODDistance { get; set; }
    }

    public class TextureModel
    {
        public string Name { get; set; }
        public string Slot { get; set; }
        
    }
}