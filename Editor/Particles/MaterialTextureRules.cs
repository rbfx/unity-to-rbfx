#nullable disable
using System;
using System.Collections.Generic;

namespace UnityToRebelFork.Editor.Particles
{
    /// <summary>One texture slot of a material that could be the color texture of its particles (what <see cref="MaterialTextureRules"/> chooses from).</summary>
    public struct TextureCandidate
    {
        /// <summary>The reference name of the property (<c>_MainTex</c>, <c>Texture2D_F593E37E</c>).</summary>
        public string Property;

        /// <summary>What the inspector shows (<c>MainTex</c>); empty when the shader does not declare the property.</summary>
        public string DisplayName;

        /// <summary>The shader declares the property (otherwise it is a value the material kept from another shader).</summary>
        public bool Declared;

        /// <summary>Hidden in the inspector.</summary>
        public bool Hidden;

        /// <summary>The texture is a normal map: its importer says so, or the property has the [Normal] flag.</summary>
        public bool IsNormalMap;
    }

    /// <summary>
    /// Which texture of a particle material is its color. Unity-free so that it can be tested: the exporter lists the candidates (the slots the material has a texture
    /// in) and takes the answer. A normal map or another data texture (mask, noise, lightmap) is never the color: when only such textures are left, the answer is none.
    /// </summary>
    public static class MaterialTextureRules
    {
        /// <summary>The reference names of the main texture, in the order of preference.</summary>
        public static readonly string[] MainNames =
        {
            "_BaseMap", "_MainTex", "_BaseColorMap", "_Texture", "_MainTexture", "_Tex", "_ParticleTexture", "_Diffuse", "_Albedo",
        };

        // What the inspector calls the main texture of a Shader Graph (its reference names are made up).
        private static readonly string[] MainDisplayNames =
        {
            "maintex", "maintexture", "basemap", "basetexture", "albedo", "particletexture", "texture", "flipbook", "emissionflipbook", "diffuse",
        };

        private static readonly string[] DataWords = { "normal", "bump", "distort", "noise", "flow", "lightmap", "height", "detail" };

        private static string Normalize(string text) => (text ?? "").Replace(" ", "").Replace("_", "").ToLowerInvariant();

        /// <summary>
        /// Whether a property name says that its texture holds data, not color: normals, distortion, noise, flow, lightmaps, heights, details, and masks that are not
        /// also named as color (a packed "color mask" can be the color of a particle).
        /// </summary>
        public static bool IsDataName(string name)
        {
            string text = Normalize(name);
            foreach (var word in DataWords)
                if (text.Contains(word)) return true;
            if (text.Contains("mask") && !text.Contains("color") && !text.Contains("albedo") && !text.Contains("base")) return true;
            return false;
        }

        /// <summary>The index of the candidate to use as the color texture, or -1 when there is none.</summary>
        public static int ChooseMain(IReadOnlyList<TextureCandidate> candidates)
        {
            // 1. A usual reference name that the shader declares, in the order of preference.
            foreach (var name in MainNames)
                for (int i = 0; i < candidates.Count; i++)
                    if (candidates[i].Declared && !candidates[i].IsNormalMap && candidates[i].Property == name) return i;

            // 2. The same names among what the material kept from other shaders (a shader that was replaced leaves its values behind).
            foreach (var name in MainNames)
                for (int i = 0; i < candidates.Count; i++)
                    if (!candidates[i].Declared && !candidates[i].IsNormalMap && candidates[i].Property == name) return i;

            // 3. A visible property of the shader whose inspector name says it is the main texture (Shader Graph).
            int first = -1;
            for (int i = 0; i < candidates.Count; i++)
            {
                var c = candidates[i];
                if (!c.Declared || c.Hidden || c.IsNormalMap || IsDataName(c.Property) || IsDataName(c.DisplayName)) continue;
                if (first < 0) first = i;
                if (Array.IndexOf(MainDisplayNames, Normalize(c.DisplayName)) >= 0) return i;
            }

            // 4. The first visible texture of the shader that is not a data texture.
            return first;
        }

        /// <summary>
        /// Whether the material does not show a color but bends what is behind it (Hovl's distortion, refraction shaders): they have only a normal map, which is not
        /// something to draw. Judged by the names of the shader and the material.
        /// </summary>
        public static bool IsDistortion(string shaderName, string materialName)
        {
            foreach (var name in new[] { shaderName, materialName })
            {
                string text = (name ?? "").ToLowerInvariant();
                if (text.Contains("distort") || text.Contains("refract")) return true;
            }
            return false;
        }
    }
}
