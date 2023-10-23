using UnityEngine.Rendering;

namespace UnityToRebelFork.Editor
{
    public class DefaultShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = int.MaxValue;

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