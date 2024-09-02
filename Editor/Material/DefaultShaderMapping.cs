using System;

namespace UnityToRebelFork.Editor
{
    public class DefaultShaderMapping : ShaderMappingBase, IShaderMapping
    {
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

            MapCommonParameters(material, model);
            MapDefaultTechnique(material, model);

            return model;
        }
    }
}