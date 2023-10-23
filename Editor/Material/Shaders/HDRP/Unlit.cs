namespace UnityToRebelFork.Editor.Shaders.HDRP
{
    public class UnlitShaderAdapter
    {
        public static readonly string ShaderName = "HDRP/Unlit";

        UnityEngine.Material material;

        public UnlitShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }


        public UnityEngine.Color _UnlitColor
        {
            get { return material.GetColor("_UnlitColor"); }
        }


        public UnityEngine.Texture _UnlitColorMap
        {
            get { return material.GetTexture("_UnlitColorMap"); }
        }


        public UnityEngine.Color _EmissiveColor
        {
            get { return material.GetColor("_EmissiveColor"); }
        }


        public UnityEngine.Texture _EmissiveColorMap
        {
            get { return material.GetTexture("_EmissiveColorMap"); }
        }


        public UnityEngine.Color _EmissiveColorLDR
        {
            get { return material.GetColor("_EmissiveColorLDR"); }
        }


        public float _AlbedoAffectEmissive
        {
            get { return material.GetFloat("_AlbedoAffectEmissive"); }
        }


        public float _EmissiveIntensityUnit
        {
            get { return material.GetFloat("_EmissiveIntensityUnit"); }
        }


        public float _UseEmissiveIntensity
        {
            get { return material.GetFloat("_UseEmissiveIntensity"); }
        }


        public float _EmissiveIntensity
        {
            get { return material.GetFloat("_EmissiveIntensity"); }
        }


        public float _EmissiveExposureWeight
        {
            get { return material.GetFloat("_EmissiveExposureWeight"); }
        }


        public UnityEngine.Texture _DistortionVectorMap
        {
            get { return material.GetTexture("_DistortionVectorMap"); }
        }


        public float _DistortionEnable
        {
            get { return material.GetFloat("_DistortionEnable"); }
        }


        public float _DistortionOnly
        {
            get { return material.GetFloat("_DistortionOnly"); }
        }


        public float _DistortionDepthTest
        {
            get { return material.GetFloat("_DistortionDepthTest"); }
        }


        public float _DistortionBlendMode
        {
            get { return material.GetFloat("_DistortionBlendMode"); }
        }


        public float _DistortionSrcBlend
        {
            get { return material.GetFloat("_DistortionSrcBlend"); }
        }


        public float _DistortionDstBlend
        {
            get { return material.GetFloat("_DistortionDstBlend"); }
        }


        public float _DistortionBlurSrcBlend
        {
            get { return material.GetFloat("_DistortionBlurSrcBlend"); }
        }


        public float _DistortionBlurDstBlend
        {
            get { return material.GetFloat("_DistortionBlurDstBlend"); }
        }


        public float _DistortionBlurBlendMode
        {
            get { return material.GetFloat("_DistortionBlurBlendMode"); }
        }


        public float _DistortionScale
        {
            get { return material.GetFloat("_DistortionScale"); }
        }


        public float _DistortionVectorScale
        {
            get { return material.GetFloat("_DistortionVectorScale"); }
        }


        public float _DistortionVectorBias
        {
            get { return material.GetFloat("_DistortionVectorBias"); }
        }


        public float _DistortionBlurScale
        {
            get { return material.GetFloat("_DistortionBlurScale"); }
        }


        public float _DistortionBlurRemapMin
        {
            get { return material.GetFloat("_DistortionBlurRemapMin"); }
        }


        public float _DistortionBlurRemapMax
        {
            get { return material.GetFloat("_DistortionBlurRemapMax"); }
        }


        public float _AlphaCutoffEnable
        {
            get { return material.GetFloat("_AlphaCutoffEnable"); }
        }


        public float _AlphaCutoff
        {
            get { return material.GetFloat("_AlphaCutoff"); }
        }


        public float _TransparentSortPriority
        {
            get { return material.GetFloat("_TransparentSortPriority"); }
        }


        public float _SurfaceType
        {
            get { return material.GetFloat("_SurfaceType"); }
        }


        public float _BlendMode
        {
            get { return material.GetFloat("_BlendMode"); }
        }


        public float _SrcBlend
        {
            get { return material.GetFloat("_SrcBlend"); }
        }


        public float _DstBlend
        {
            get { return material.GetFloat("_DstBlend"); }
        }


        public float _AlphaSrcBlend
        {
            get { return material.GetFloat("_AlphaSrcBlend"); }
        }


        public float _AlphaDstBlend
        {
            get { return material.GetFloat("_AlphaDstBlend"); }
        }


        public float _AlphaToMaskInspectorValue
        {
            get { return material.GetFloat("_AlphaToMaskInspectorValue"); }
        }


        public float _AlphaToMask
        {
            get { return material.GetFloat("_AlphaToMask"); }
        }


        public float _ZWrite
        {
            get { return material.GetFloat("_ZWrite"); }
        }


        public float _TransparentZWrite
        {
            get { return material.GetFloat("_TransparentZWrite"); }
        }


        public float _CullMode
        {
            get { return material.GetFloat("_CullMode"); }
        }


        public float _TransparentCullMode
        {
            get { return material.GetFloat("_TransparentCullMode"); }
        }


        public float _OpaqueCullMode
        {
            get { return material.GetFloat("_OpaqueCullMode"); }
        }


        public float _ZTestModeDistortion
        {
            get { return material.GetFloat("_ZTestModeDistortion"); }
        }


        public float _ZTestTransparent
        {
            get { return material.GetFloat("_ZTestTransparent"); }
        }


        public float _ZTestDepthEqualForOpaque
        {
            get { return material.GetFloat("_ZTestDepthEqualForOpaque"); }
        }


        public float _EnableFogOnTransparent
        {
            get { return material.GetFloat("_EnableFogOnTransparent"); }
        }


        public float _DoubleSidedEnable
        {
            get { return material.GetFloat("_DoubleSidedEnable"); }
        }


        public float _DoubleSidedGIMode
        {
            get { return material.GetFloat("_DoubleSidedGIMode"); }
        }


        public float _StencilRef
        {
            get { return material.GetFloat("_StencilRef"); }
        }


        public float _StencilWriteMask
        {
            get { return material.GetFloat("_StencilWriteMask"); }
        }


        public float _StencilRefDepth
        {
            get { return material.GetFloat("_StencilRefDepth"); }
        }


        public float _StencilWriteMaskDepth
        {
            get { return material.GetFloat("_StencilWriteMaskDepth"); }
        }


        public float _StencilRefMV
        {
            get { return material.GetFloat("_StencilRefMV"); }
        }


        public float _StencilWriteMaskMV
        {
            get { return material.GetFloat("_StencilWriteMaskMV"); }
        }


        public float _AddPrecomputedVelocity
        {
            get { return material.GetFloat("_AddPrecomputedVelocity"); }
        }


        public float _StencilRefDistortionVec
        {
            get { return material.GetFloat("_StencilRefDistortionVec"); }
        }


        public float _StencilWriteMaskDistortionVec
        {
            get { return material.GetFloat("_StencilWriteMaskDistortionVec"); }
        }


        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
        }


        public float _IncludeIndirectLighting
        {
            get { return material.GetFloat("_IncludeIndirectLighting"); }
        }


        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }


        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }


        public float _Cutoff
        {
            get { return material.GetFloat("_Cutoff"); }
        }

    }
}
