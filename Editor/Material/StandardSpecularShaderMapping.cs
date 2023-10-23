using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace UnityToRebelFork.Editor
{
    public class StandardSpecularShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == "Standard (Specular setup)")
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            MapCommonParameters(material, model);
            MapDefaultTechnique(material, model);

            var shaderArgs = new Shaders.Standard_Specularsetup_ShaderAdapter(material);

            model.MatDiffColor = shaderArgs._Color;
            model.MatEmissiveColor = (Vector4)shaderArgs._EmissionColor;
            model.NormalScale = shaderArgs._BumpScale;
            model.AlphaCutoff = shaderArgs._Cutoff;
            //model.Metallic = shaderArgs._SpecColor;
            model.Roughness = 1.0f - shaderArgs._Glossiness;

            if (shaderArgs._BumpMap != null)
                model.Normal = orchestrator.ScheduleExport(shaderArgs._BumpMap);

            if (shaderArgs._EmissionMap != null)
                model.Emission = orchestrator.ScheduleExport(shaderArgs._EmissionMap);

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.ScheduleExport(shaderArgs._MainTex);

            if (shaderArgs._OcclusionMap != null || shaderArgs._SpecGlossMap != null)
            {
            }

            return model;
        }
    }
}