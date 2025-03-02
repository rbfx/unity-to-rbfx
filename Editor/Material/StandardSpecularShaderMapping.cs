using System;
using UnityEngine;
using UnityToRebelFork.Editor.Shaders;

namespace UnityToRebelFork.Editor
{
    public class StandardSpecularShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public StandardSpecularShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == Standard_Specularsetup_ShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            var shaderArgs = new Shaders.Standard_Specularsetup_ShaderAdapter(material);

            MapCommonParameters(material, model, shaderArgs._BumpMap != null);
            MapDefaultTechnique(material, model);

            model.MatDiffColor = shaderArgs._Color;
            model.MatEmissiveColor = (Vector4)shaderArgs._EmissionColor;
            model.NormalScale = shaderArgs._BumpScale;
            model.AlphaCutoff = shaderArgs._Cutoff;
            //model.Metallic = shaderArgs._SpecColor;
            model.Roughness = 1.0f - shaderArgs._Glossiness;

            if (shaderArgs._BumpMap != null)
                model.Normal = orchestrator.Value.ScheduleExport(shaderArgs._BumpMap);

            if (shaderArgs._EmissionMap != null)
                model.Emission = orchestrator.Value.ScheduleExport(shaderArgs._EmissionMap);

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            if (shaderArgs._OcclusionMap != null || shaderArgs._SpecGlossMap != null)
            {
            }

            return model;
        }
    }
}