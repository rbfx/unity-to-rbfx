namespace UnityToRebelFork.Editor.Shaders.UniversalRenderPipeline
{
    public class SimpleLitShaderAdapter
    {
        public static readonly string ShaderName = "Universal Render Pipeline/Simple Lit";

        UnityEngine.Material material;

        public SimpleLitShaderAdapter(UnityEngine.Material material)
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


        public float _Smoothness
        {
            get { return material.GetFloat("_Smoothness"); }
        }


        public UnityEngine.Color _SpecColor
        {
            get { return material.GetColor("_SpecColor"); }
        }


        public UnityEngine.Texture _SpecGlossMap
        {
            get { return material.GetTexture("_SpecGlossMap"); }
        }


        public float _SmoothnessSource
        {
            get { return material.GetFloat("_SmoothnessSource"); }
        }


        public float _SpecularHighlights
        {
            get { return material.GetFloat("_SpecularHighlights"); }
        }


        public float _BumpScale
        {
            get { return material.GetFloat("_BumpScale"); }
        }


        public UnityEngine.Texture _BumpMap
        {
            get { return material.GetTexture("_BumpMap"); }
        }


        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
        }


        public UnityEngine.Texture _EmissionMap
        {
            get { return material.GetTexture("_EmissionMap"); }
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


        public float _ReceiveShadows
        {
            get { return material.GetFloat("_ReceiveShadows"); }
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


        public float _Shininess
        {
            get { return material.GetFloat("_Shininess"); }
        }


        public float _GlossinessSource
        {
            get { return material.GetFloat("_GlossinessSource"); }
        }


        public float _SpecSource
        {
            get { return material.GetFloat("_SpecSource"); }
        }


        public UnityEngine.Texture unity_Lightmaps
        {
            get { return material.GetTexture("unity_Lightmaps"); }
        }


        public UnityEngine.Texture unity_LightmapsInd
        {
            get { return material.GetTexture("unity_LightmapsInd"); }
        }


        public UnityEngine.Texture unity_ShadowMasks
        {
            get { return material.GetTexture("unity_ShadowMasks"); }
        }
    }
}
