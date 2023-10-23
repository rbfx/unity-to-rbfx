namespace UnityToRebelFork.Editor.Shaders
{
    public class Standard_Specularsetup_ShaderAdapter
    {
        public static readonly string ShaderName = "Standard (Specular setup)";

        UnityEngine.Material material;

        public Standard_Specularsetup_ShaderAdapter(UnityEngine.Material material)
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


        public float _Cutoff
        {
            get { return material.GetFloat("_Cutoff"); }
        }


        public float _Glossiness
        {
            get { return material.GetFloat("_Glossiness"); }
        }


        public float _GlossMapScale
        {
            get { return material.GetFloat("_GlossMapScale"); }
        }


        public float _SmoothnessTextureChannel
        {
            get { return material.GetFloat("_SmoothnessTextureChannel"); }
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


        public float _GlossyReflections
        {
            get { return material.GetFloat("_GlossyReflections"); }
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


        public float _UVSec
        {
            get { return material.GetFloat("_UVSec"); }
        }


        public float _Mode
        {
            get { return material.GetFloat("_Mode"); }
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
    }
}
