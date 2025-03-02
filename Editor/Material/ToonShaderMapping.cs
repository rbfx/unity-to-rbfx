using System;
using UnityToRebelFork.Editor.Shaders;

namespace UnityToRebelFork.Editor
{
    public class ToonShaderMapping : ShaderMappingBase, IShaderMapping
    {

        public int Priority { get; } = 0;

        public ToonShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == ToonShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            var shaderArgs = new Shaders.ToonShaderAdapter(material);

            MapCommonParameters(material, model, false);
            model.Techniques.Add(new TechniqueModel { Name = "Techniques/CelOpaque.xml" });

            model.MatDiffColor = shaderArgs._Color;
            model.NormalScale = shaderArgs._BumpScale;

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            return model;
        }
    }
}