using System;

namespace UnityToRebelFork.Editor
{
    public class LegacyDiffuseShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public LegacyDiffuseShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == Shaders.LegacyShaders.DiffuseShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            var shaderArgs = new Shaders.LegacyShaders.DiffuseShaderAdapter(material);

            MapCommonParameters(material, model, false);
            MapDefaultTechnique(material, model);

            model.MatDiffColor = shaderArgs._Color;

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            return model;
        }
    }
}