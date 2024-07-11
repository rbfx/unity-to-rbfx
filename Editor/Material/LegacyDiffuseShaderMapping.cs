namespace UnityToRebelFork.Editor
{
    public class LegacyDiffuseShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == Shaders.LegacyShaders.DiffuseShaderAdapter.ShaderName)
                return true;
            return false;
        }

        public MaterialModel Map(UnityEngine.Material material)
        {
            var model = new MaterialModel();

            MapCommonParameters(material, model);
            MapDefaultTechnique(material, model);

            var shaderArgs = new Shaders.LegacyShaders.DiffuseShaderAdapter(material);

            model.MatDiffColor = shaderArgs._Color;

            if (shaderArgs._MainTex != null)
                model.Albedo = orchestrator.ScheduleExport(shaderArgs._MainTex);

            return model;
        }

    }
}