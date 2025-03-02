using System;

namespace UnityToRebelFork.Editor.Unlit
{
    public class TransparentShaderMapping : ShaderMappingBase, IShaderMapping
    {

        public int Priority { get; } = 0;

        public TransparentShaderMapping(Lazy<ExportOrchestrator> orchestrator, ExportSettings settings) : base(orchestrator, settings)
        {
        }

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == Shaders.Unlit.TransparentShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            var shaderArgs = new Shaders.Unlit.TransparentShaderAdapter(material);

            MapCommonParameters(material, model, false);
            model.Techniques.Add(new TechniqueModel { Name = "Techniques/UnlitTransparent.xml" });

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.Value.ScheduleExport(shaderArgs._MainTex);

            return model;
        }
    }
}