namespace UnityToRebelFork.Editor.Shaders.LegacyShaders
{
    public class DiffuseShaderAdapter
    {
        public static readonly string ShaderName = "Legacy Shaders/Diffuse";

        UnityEngine.Material material;

        public DiffuseShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }

        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }

        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }
    }
}