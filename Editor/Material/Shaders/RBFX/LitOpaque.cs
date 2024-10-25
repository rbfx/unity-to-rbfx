namespace UnityToRebelFork.Editor.Shaders.RBFX
{
    public class LitOpaqueShaderAdapter
    {
        public static readonly string ShaderName = "RBFX/LitOpaque";

        UnityEngine.Material material;

        public LitOpaqueShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }

        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }

        public UnityEngine.Texture _BumpMap
        {
            get { return material.GetTexture("_BumpMap"); }
        }

        public UnityEngine.Texture _PBRMap
        {
            get { return material.GetTexture("_PBRMap"); }
        }

        public UnityEngine.Texture _EmissionMap
        {
            get { return material.GetTexture("_EmissionMap"); }
        }

        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
        }

        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }

        public float _BumpScale
        {
            get { return material.GetFloat("_BumpScale"); }
        }

        public float _Metallic
        {
            get { return material.GetFloat("_Metallic"); }
        }

        public float _Roughness
        {
            get { return material.GetFloat("_Roughness"); }
        }

        public float _Cutoff
        {
            get { return material.GetFloat("_Cutoff"); }
        }
    }
}