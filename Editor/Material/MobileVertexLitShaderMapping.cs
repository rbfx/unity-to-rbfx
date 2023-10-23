using UnityEngine.Rendering;
using UnityToRebelFork.Editor.Shaders.Mobile;

namespace UnityToRebelFork.Editor
{
    public class MobileVertexLitShaderMapping : ShaderMappingBase, IShaderMapping
    {
        public int Priority { get; } = 0;

        public bool CanMap(UnityEngine.Shader shader)
        {
            if (shader.name == VertexLitShaderAdapter.ShaderName)
                return true;
            return false;
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