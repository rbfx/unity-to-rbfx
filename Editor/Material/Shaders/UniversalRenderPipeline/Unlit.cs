namespace UnityToRebelFork.Editor.Shaders.UniversalRenderPipeline
{
    public class UnlitShaderAdapter
    {
        public static readonly string ShaderName = "Universal Render Pipeline/Unlit";

        UnityEngine.Material material;

        public UnlitShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }


        public UnityEngine.Texture _BaseMap
        {
            get { return material.GetTexture("_BaseMap"); }
        }


        public UnityEngine.Color _BaseColor
        {
            get { return material.GetColor("_BaseColor"); }
        }


        public float _Cutoff
        {
            get { return material.GetFloat("_Cutoff"); }
        }


        public float _Surface
        {
            get { return material.GetFloat("_Surface"); }
        }


        public float _Blend
        {
            get { return material.GetFloat("_Blend"); }
        }


        public float _Cull
        {
            get { return material.GetFloat("_Cull"); }
        }


        public float _AlphaClip
        {
            get { return material.GetFloat("_AlphaClip"); }
        }


        public float _BlendOp
        {
            get { return material.GetFloat("_BlendOp"); }
        }


        public float _SrcBlend
        {
            get { return material.GetFloat("_SrcBlend"); }
        }


        public float _DstBlend
        {
            get { return material.GetFloat("_DstBlend"); }
        }


        public float _ZWrite
        {
            get { return material.GetFloat("_ZWrite"); }
        }


        public float _QueueOffset
        {
            get { return material.GetFloat("_QueueOffset"); }
        }


        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }


        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }


        public float _SampleGI
        {
            get { return material.GetFloat("_SampleGI"); }
        }
    }
}
