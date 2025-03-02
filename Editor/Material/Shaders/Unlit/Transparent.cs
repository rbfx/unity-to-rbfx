namespace UnityToRebelFork.Editor.Shaders.Unlit
{
    public class TransparentShaderAdapter
    {
        public static readonly string ShaderName = "Unlit/Transparent";

        UnityEngine.Material material;

        public TransparentShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }

        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }
    }
}