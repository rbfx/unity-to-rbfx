namespace UnityToRebelFork.Editor.Shaders.Mobile
{
    public class VertexLitShaderAdapter
    {
        public static readonly string ShaderName = "Mobile/VertexLit";

        UnityEngine.Material material;

        public VertexLitShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }


        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }
    }
}
