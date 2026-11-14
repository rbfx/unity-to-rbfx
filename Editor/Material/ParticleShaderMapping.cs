using System;
using System.Collections.Generic;
using UnityEngine;
using UnityToRebelFork.Editor.Particles;

namespace UnityToRebelFork.Editor
{
    /// <summary>The shaders of the materials that particle system renderers use; the exporter notes them so that their materials come out as particle materials.</summary>
    public static class ParticleShaders
    {
        private static readonly HashSet<UnityEngine.Shader> Shaders = new HashSet<UnityEngine.Shader>();

        public static void Add(UnityEngine.Shader shader)
        {
            if (shader != null) Shaders.Add(shader);
        }

        public static bool Contains(UnityEngine.Shader shader) => shader != null && Shaders.Contains(shader);
    }

    /// <summary>
    /// The mapping of materials that particles are drawn with: an unlit material that is added to the picture (when the shader blends by adding) or
    /// blended by alpha, with the main texture and the tint found under the usual property names. Particle shaders are written in many ways (the legacy
    /// ones, URP's, Shader Graph ones), so the blend is read from the shader's properties and name rather than from one list of shaders.
    /// </summary>
    public class ParticleShaderMapping : ShaderMappingBase, IShaderMapping
    {
        private static readonly string[] TextureNames =
        {
            "_BaseMap", "_MainTex", "_BaseColorMap", "_Texture", "_MainTexture", "_Tex", "_ParticleTexture", "_Diffuse", "_Albedo",
        };

        private static readonly string[] ColorNames = { "_BaseColor", "_Color", "_TintColor", "_MainColor", "_Tint", "_ColorTint" };

        public int Priority { get; } = -10;

        public ParticleShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader == null) return false;
            if (ParticleShaders.Contains(shader)) return true;
            var name = shader.name;
            return name.IndexOf("Particle", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>Whether the material adds its color to the picture rather than blending it with it.</summary>
        public static bool IsAdditive(UnityEngine.Material material)
        {
            var name = material.shader.name;
            if (name.IndexOf("Additive", StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("_Add", StringComparison.OrdinalIgnoreCase) >= 0
                || name.EndsWith("Add", StringComparison.OrdinalIgnoreCase) || name.IndexOf("Glow", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            // Blend factors set on the material: One (1) as the destination adds, OneMinusSrcAlpha (10) blends.
            foreach (var property in new[] { "_DstBlend", "_BlendDst", "_DstBlendAlpha" })
                if (material.HasProperty(property)) return Mathf.RoundToInt(material.GetFloat(property)) == 1;
            if (material.HasProperty("_Blend")) return Mathf.RoundToInt(material.GetFloat("_Blend")) == 2; // URP particles: Alpha, Premultiply, Additive, Multiply
            return false;
        }

        private static readonly string[] ColorDisplayNames = { "color", "tint", "tintcolor", "maincolor", "basecolor" };

        private static string Normalize(string text) => text.Replace(" ", "").Replace("_", "").ToLowerInvariant();

        /// <summary>Whether the texture is a normal map by its import settings.</summary>
        private static bool IsNormalMap(UnityEngine.Texture texture)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(texture);
            return !string.IsNullOrEmpty(path) && UnityEditor.AssetImporter.GetAtPath(path) is UnityEditor.TextureImporter importer
                   && importer.textureType == UnityEditor.TextureImporterType.NormalMap;
        }

        /// <summary>
        /// The texture the particles show, chosen by <see cref="MaterialTextureRules"/> among the textures the material has: those of the properties its shader declares, and
        /// those it kept from other shaders (a replaced shader leaves its values in the material). Normal maps and other data textures are never the color.
        /// </summary>
        public static UnityEngine.Texture2D FindMainTexture(UnityEngine.Material material)
        {
            var candidates = new List<TextureCandidate>();
            var textures = new List<UnityEngine.Texture2D>();
            var declared = new HashSet<string>();

            var shader = material.shader;
            int count = shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Texture) continue;
                if (shader.GetPropertyTextureDimension(i) != UnityEngine.Rendering.TextureDimension.Tex2D) continue;
                string name = shader.GetPropertyName(i);
                declared.Add(name);
                if (!(material.GetTexture(name) is UnityEngine.Texture2D texture)) continue;
                var flags = shader.GetPropertyFlags(i);
                candidates.Add(new TextureCandidate
                {
                    Property = name,
                    DisplayName = shader.GetPropertyDescription(i),
                    Declared = true,
                    Hidden = (flags & UnityEngine.Rendering.ShaderPropertyFlags.HideInInspector) != 0,
                    IsNormalMap = (flags & UnityEngine.Rendering.ShaderPropertyFlags.Normal) != 0 || IsNormalMap(texture),
                });
                textures.Add(texture);
            }

            // The values the material saved for properties its shader does not have (the colors of NatureManufacture's smokes are among them).
            var saved = new UnityEditor.SerializedObject(material).FindProperty("m_SavedProperties.m_TexEnvs");
            if (saved != null)
            {
                for (int i = 0; i < saved.arraySize; i++)
                {
                    var entry = saved.GetArrayElementAtIndex(i);
                    string name = entry.FindPropertyRelative("first").stringValue;
                    if (declared.Contains(name)) continue;
                    if (!(entry.FindPropertyRelative("second.m_Texture").objectReferenceValue is UnityEngine.Texture2D texture)) continue;
                    candidates.Add(new TextureCandidate { Property = name, DisplayName = "", Declared = false, IsNormalMap = IsNormalMap(texture) });
                    textures.Add(texture);
                }
            }

            int chosen = MaterialTextureRules.ChooseMain(candidates);
            var normals = new List<string>();
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i].IsNormalMap) normals.Add(candidates[i].Property + "=" + textures[i].name);
            if (normals.Count > 0)
                Debug.Log("[ParticleShaderMapping] material '" + material.name + "': " + string.Join(", ", normals) + " not used as color; color texture: " + (chosen < 0 ? "none" : textures[chosen].name));
            return chosen < 0 ? null : textures[chosen];
        }

        /// <summary>A float of the material: the property of its shader, or the value it saved for a property its shader does not have.</summary>
        private static bool TryGetFloat(UnityEngine.Material material, string name, out float value)
        {
            if (material.HasProperty(name))
            {
                value = material.GetFloat(name);
                return true;
            }
            value = 0f;
            var saved = new UnityEditor.SerializedObject(material).FindProperty("m_SavedProperties.m_Floats");
            if (saved == null) return false;
            for (int i = 0; i < saved.arraySize; i++)
            {
                var entry = saved.GetArrayElementAtIndex(i);
                if (entry.FindPropertyRelative("first").stringValue != name) continue;
                value = entry.FindPropertyRelative("second").floatValue;
                return true;
            }
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();
            bool additive = IsAdditive(material);
            model.Techniques.Add(new TechniqueModel { Name = additive ? "Techniques/UnlitTransparentAdd.xml" : "Techniques/UnlitTransparent.xml" });
            model.RenderOrder = additive ? 129 : 128;

            // A distortion shader has only a normal map, which bends what is behind the particle: nothing to draw as a color (the exporter does not draw such particles).
            var mainTexture = MaterialTextureRules.IsDistortion(material.shader.name, material.name) ? null : FindMainTexture(material);
            if (mainTexture != null) model.Albedo = orchestrator.Value.ScheduleExport(mainTexture);
            else if (!MaterialTextureRules.IsDistortion(material.shader.name, material.name)) Debug.LogWarning("[ParticleShaderMapping] material '" + material.name + "' has no texture the exporter recognizes (shader '" + material.shader.name + "')");

            // The legacy particle shaders double the product of the tint and the texture.
            bool legacy = material.shader.name.StartsWith("Legacy Shaders/Particles", StringComparison.OrdinalIgnoreCase)
                          || material.shader.name.StartsWith("Particles/", StringComparison.OrdinalIgnoreCase)
                          || material.shader.name.StartsWith("Mobile/Particles", StringComparison.OrdinalIgnoreCase);
            string colorName = null;
            foreach (var name in ColorNames)
                if (material.HasProperty(name)) { colorName = name; break; }
            if (colorName == null)
            {
                // Shader Graph references (Color_XXXX): the property's display name says what it is.
                var shader = material.shader;
                for (int i = 0; i < shader.GetPropertyCount() && colorName == null; i++)
                {
                    if (shader.GetPropertyType(i) != UnityEngine.Rendering.ShaderPropertyType.Color) continue;
                    if (Array.IndexOf(ColorDisplayNames, Normalize(shader.GetPropertyDescription(i))) >= 0) colorName = shader.GetPropertyName(i);
                }
            }
            if (colorName != null)
            {
                var color = material.GetColor(colorName);
                model.MatDiffColor = legacy ? new Vector4(color.r * 2f, color.g * 2f, color.b * 2f, color.a * 2f) : (Vector4)color;
            }

            // Soft particles: the alpha fades where the particle meets the surface behind it. URP says so with a switch and two distances, the built-in shaders with a factor.
            // The values come from the properties of the shader, or else from those the material kept from another shader (Shader Graph materials of packs that were made for the
            // built-in shaders keep their _InvFade there).
            float fadeOffset = 0f, fadeLength = 0f;
            bool soft = TryGetFloat(material, "_SoftParticlesEnabled", out float urpEnabled)
                ? SoftParticleRules.TryFromUrp(urpEnabled > 0.5f,
                    TryGetFloat(material, "_SoftParticlesNearFadeDistance", out float near) ? near : 0f,
                    TryGetFloat(material, "_SoftParticlesFarFadeDistance", out float far) ? far : 1f, out fadeOffset, out fadeLength)
                : TryGetFloat(material, "_InvFade", out float invFade) && SoftParticleRules.TryFromInvFade(invFade, out fadeOffset, out fadeLength);
            if (soft)
            {
                model.VSDefines.Add("SOFTPARTICLES");
                model.PSDefines.Add("SOFTPARTICLES");
                model.ShaderParameters["FadeOffsetScale"] = new Vector4(fadeOffset, fadeLength, 0f, 0f);
            }

            int cull = material.HasProperty("_Cull") ? Mathf.RoundToInt(material.GetFloat("_Cull")) : 0;
            model.Cull = cull == 0 ? CullMode.None : cull == 1 ? CullMode.CW : CullMode.CCW;
            model.ShadowCull = model.Cull;
            return model;
        }
    }
}
