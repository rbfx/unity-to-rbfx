namespace UnityToRebelFork.Editor.Shaders.UniversalRenderPipeline
{
    public class LitShaderAdapter
    {
        public static readonly string ShaderName = "Universal Render Pipeline/Lit";

        UnityEngine.Material material;

        public LitShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }


        public float _WorkflowMode
        {
            get { return material.GetFloat("_WorkflowMode"); }
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


        public float _SmoothnessTextureChannel
        {
            get { return material.GetFloat("_SmoothnessTextureChannel"); }
        }


        public float _Metallic
        {
            get { return material.GetFloat("_Metallic"); }
        }


        public UnityEngine.Texture _MetallicGlossMap
        {
            get { return material.GetTexture("_MetallicGlossMap"); }
        }


        public UnityEngine.Color _SpecColor
        {
            get { return material.GetColor("_SpecColor"); }
        }


        public UnityEngine.Texture _SpecGlossMap
        {
            get { return material.GetTexture("_SpecGlossMap"); }
        }


        public float _SpecularHighlights
        {
            get { return material.GetFloat("_SpecularHighlights"); }
        }


        public float _EnvironmentReflections
        {
            get { return material.GetFloat("_EnvironmentReflections"); }
        }


        public float _BumpScale
        {
            get { return material.GetFloat("_BumpScale"); }
        }


        public UnityEngine.Texture _BumpMap
        {
            get { return material.GetTexture("_BumpMap"); }
        }


        public float _Parallax
        {
            get { return material.GetFloat("_Parallax"); }
        }


        public UnityEngine.Texture _ParallaxMap
        {
            get { return material.GetTexture("_ParallaxMap"); }
        }


        public float _OcclusionStrength
        {
            get { return material.GetFloat("_OcclusionStrength"); }
        }


        public UnityEngine.Texture _OcclusionMap
        {
            get { return material.GetTexture("_OcclusionMap"); }
        }


        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
        }


        public UnityEngine.Texture _EmissionMap
        {
            get { return material.GetTexture("_EmissionMap"); }
        }


        public UnityEngine.Texture _DetailMask
        {
            get { return material.GetTexture("_DetailMask"); }
        }


        public float _DetailAlbedoMapScale
        {
            get { return material.GetFloat("_DetailAlbedoMapScale"); }
        }


        public UnityEngine.Texture _DetailAlbedoMap
        {
            get { return material.GetTexture("_DetailAlbedoMap"); }
        }


        public float _DetailNormalMapScale
        {
            get { return material.GetFloat("_DetailNormalMapScale"); }
        }


        public UnityEngine.Texture _DetailNormalMap
        {
            get { return material.GetTexture("_DetailNormalMap"); }
        }


        public float _ClearCoatMask
        {
            get { return material.GetFloat("_ClearCoatMask"); }
        }


        public float _ClearCoatSmoothness
        {
            get { return material.GetFloat("_ClearCoatSmoothness"); }
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


        public float _GlossMapScale
        {
            get { return material.GetFloat("_GlossMapScale"); }
        }


        public float _Glossiness
        {
            get { return material.GetFloat("_Glossiness"); }
        }


        public float _GlossyReflections
        {
            get { return material.GetFloat("_GlossyReflections"); }
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
