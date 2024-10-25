using System;
using UnityEngine;

namespace UnityToRebelFork.Editor
{
    public class LitOpaqueShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public LitOpaqueShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == Shaders.RBFX.LitOpaqueShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            MapCommonParameters(material, model);
            MapDefaultTechnique(material, model);

            var shaderArgs = new Shaders.RBFX.LitOpaqueShaderAdapter(material);

            model.MatDiffColor = shaderArgs._Color;
            model.MatEmissiveColor = (Vector4)shaderArgs._EmissionColor;
            model.NormalScale = shaderArgs._BumpScale;
            model.AlphaCutoff = shaderArgs._Cutoff;
            model.Metallic = shaderArgs._Metallic;
            model.Roughness = shaderArgs._Roughness;

            if (shaderArgs._BumpMap != null)
                model.Normal = orchestrator.Value.ScheduleExport(shaderArgs._BumpMap);

            if (shaderArgs._EmissionMap != null)
                model.Emission = orchestrator.Value.ScheduleExport(shaderArgs._EmissionMap);

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            if (shaderArgs._PBRMap != null)
                model.Properties = orchestrator.Value.ScheduleExport(shaderArgs._PBRMap);

            return model;
        }
    }
}