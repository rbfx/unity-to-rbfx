using System;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    /// <summary>
    /// The mapping of shaders nothing else maps (custom shaders, such as Synty's): a lit material with the textures and colour found under the
    /// usual property names, so models keep their look.
    /// </summary>
    public class DefaultShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public static readonly string[] AlbedoNames = { "_Albedo_Map", "_BaseMap", "_MainTex", "_BaseColorMap", "_Albedo" };
        public static readonly string[] NormalNames = { "_Normal_Map", "_BumpMap", "_NormalMap" };
        public static readonly string[] EmissionNames = { "_Emission_Map", "_EmissionMap", "_EmissiveColorMap" };
        public static readonly string[] ColorNames = { "_BaseColor", "_Color", "_Albedo_Tint" };
        public static readonly string[] MetallicNames = { "_Metallic", "_Metalic", "_Metallic_Amount" };
        public static readonly string[] SmoothnessNames = { "_Smoothness", "_Glossiness", "_Smoothness_Amount" };
        public static readonly string[] RoughnessNames = { "_Roughness" };

        /// <summary>The first float property set under one of <paramref name="names"/>, clamped to 0..1.</summary>
        public static float? FindFloat(UnityEngine.Material material, string[] names)
        {
            foreach (var name in names)
                if (material.HasProperty(name)) return UnityEngine.Mathf.Clamp01(material.GetFloat(name));
            return null;
        }

        public int Priority { get; } = int.MaxValue;

        public DefaultShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            return true;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            var normal = FindTexture(material, NormalNames);
            MapCommonParameters(material, model, normal != null);
            MapDefaultTechnique(material, model);

            if (FindTexture(material, AlbedoNames) is { } albedo)
                model.Albedo = orchestrator.Value.ScheduleExport(albedo);
            if (normal != null)
                model.Normal = orchestrator.Value.ScheduleExport(normal);
            if (FindTexture(material, EmissionNames) is { } emission)
                model.Emission = orchestrator.Value.ScheduleExport(emission);
            foreach (var name in ColorNames)
            {
                if (!material.HasProperty(name)) continue;
                model.MatDiffColor = material.GetColor(name);
                break;
            }
            // rbfx's defaults (a material that does not say) are metallic 1 and roughness 1: a rough metal, which ambient and sky light
            // hardly light. Unity's are dielectric; take the material's own values where it has them.
            model.Metallic = FindFloat(material, MetallicNames) ?? 0f;
            model.Roughness = FindFloat(material, SmoothnessNames) is { } smoothness ? 1f - smoothness : FindFloat(material, RoughnessNames) ?? 0.8f;

            return model;
        }

        /// <summary>The first texture set under one of <paramref name="names"/>.</summary>
        public static UnityEngine.Texture FindTexture(UnityEngine.Material material, string[] names)
        {
            if (material == null) return null;
            foreach (var name in names)
            {
                if (material.HasProperty(name) && material.GetTexture(name) is UnityEngine.Texture texture && texture != null)
                    return texture;
            }
            return null;
        }
    }
}
