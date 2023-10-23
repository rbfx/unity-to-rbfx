namespace UnityToRebelFork.Editor.Shaders.HDRP
{
    public class LitShaderAdapter
    {
        public static readonly string ShaderName = "HDRP/Lit";

        UnityEngine.Material material;

        public LitShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }


        public UnityEngine.Color _BaseColor
        {
            get { return material.GetColor("_BaseColor"); }
        }


        public UnityEngine.Texture _BaseColorMap
        {
            get { return material.GetTexture("_BaseColorMap"); }
        }



        public float _Metallic
        {
            get { return material.GetFloat("_Metallic"); }
        }


        public float _Smoothness
        {
            get { return material.GetFloat("_Smoothness"); }
        }


        public UnityEngine.Texture _MaskMap
        {
            get { return material.GetTexture("_MaskMap"); }
        }


        public float _MetallicRemapMin
        {
            get { return material.GetFloat("_MetallicRemapMin"); }
        }


        public float _MetallicRemapMax
        {
            get { return material.GetFloat("_MetallicRemapMax"); }
        }


        public float _SmoothnessRemapMin
        {
            get { return material.GetFloat("_SmoothnessRemapMin"); }
        }


        public float _SmoothnessRemapMax
        {
            get { return material.GetFloat("_SmoothnessRemapMax"); }
        }


        public float _AORemapMin
        {
            get { return material.GetFloat("_AORemapMin"); }
        }


        public float _AORemapMax
        {
            get { return material.GetFloat("_AORemapMax"); }
        }


        public UnityEngine.Texture _NormalMap
        {
            get { return material.GetTexture("_NormalMap"); }
        }


        public UnityEngine.Texture _NormalMapOS
        {
            get { return material.GetTexture("_NormalMapOS"); }
        }


        public float _NormalScale
        {
            get { return material.GetFloat("_NormalScale"); }
        }


        public UnityEngine.Texture _BentNormalMap
        {
            get { return material.GetTexture("_BentNormalMap"); }
        }


        public UnityEngine.Texture _BentNormalMapOS
        {
            get { return material.GetTexture("_BentNormalMapOS"); }
        }


        public UnityEngine.Texture _HeightMap
        {
            get { return material.GetTexture("_HeightMap"); }
        }


        public float _HeightAmplitude
        {
            get { return material.GetFloat("_HeightAmplitude"); }
        }


        public float _HeightCenter
        {
            get { return material.GetFloat("_HeightCenter"); }
        }


        public float _HeightMapParametrization
        {
            get { return material.GetFloat("_HeightMapParametrization"); }
        }


        public float _HeightOffset
        {
            get { return material.GetFloat("_HeightOffset"); }
        }


        public float _HeightMin
        {
            get { return material.GetFloat("_HeightMin"); }
        }


        public float _HeightMax
        {
            get { return material.GetFloat("_HeightMax"); }
        }


        public float _HeightTessAmplitude
        {
            get { return material.GetFloat("_HeightTessAmplitude"); }
        }


        public float _HeightTessCenter
        {
            get { return material.GetFloat("_HeightTessCenter"); }
        }


        public float _HeightPoMAmplitude
        {
            get { return material.GetFloat("_HeightPoMAmplitude"); }
        }


        public UnityEngine.Texture _DetailMap
        {
            get { return material.GetTexture("_DetailMap"); }
        }


        public float _DetailAlbedoScale
        {
            get { return material.GetFloat("_DetailAlbedoScale"); }
        }


        public float _DetailNormalScale
        {
            get { return material.GetFloat("_DetailNormalScale"); }
        }


        public float _DetailSmoothnessScale
        {
            get { return material.GetFloat("_DetailSmoothnessScale"); }
        }


        public UnityEngine.Texture _TangentMap
        {
            get { return material.GetTexture("_TangentMap"); }
        }


        public UnityEngine.Texture _TangentMapOS
        {
            get { return material.GetTexture("_TangentMapOS"); }
        }


        public float _Anisotropy
        {
            get { return material.GetFloat("_Anisotropy"); }
        }


        public UnityEngine.Texture _AnisotropyMap
        {
            get { return material.GetTexture("_AnisotropyMap"); }
        }


        public float _SubsurfaceMask
        {
            get { return material.GetFloat("_SubsurfaceMask"); }
        }


        public UnityEngine.Texture _SubsurfaceMaskMap
        {
            get { return material.GetTexture("_SubsurfaceMaskMap"); }
        }


        public float _Thickness
        {
            get { return material.GetFloat("_Thickness"); }
        }


        public UnityEngine.Texture _ThicknessMap
        {
            get { return material.GetTexture("_ThicknessMap"); }
        }



        public float _IridescenceThickness
        {
            get { return material.GetFloat("_IridescenceThickness"); }
        }


        public UnityEngine.Texture _IridescenceThicknessMap
        {
            get { return material.GetTexture("_IridescenceThicknessMap"); }
        }



        public float _IridescenceMask
        {
            get { return material.GetFloat("_IridescenceMask"); }
        }


        public UnityEngine.Texture _IridescenceMaskMap
        {
            get { return material.GetTexture("_IridescenceMaskMap"); }
        }


        public float _CoatMask
        {
            get { return material.GetFloat("_CoatMask"); }
        }


        public UnityEngine.Texture _CoatMaskMap
        {
            get { return material.GetTexture("_CoatMaskMap"); }
        }


        public float _EnergyConservingSpecularColor
        {
            get { return material.GetFloat("_EnergyConservingSpecularColor"); }
        }


        public UnityEngine.Color _SpecularColor
        {
            get { return material.GetColor("_SpecularColor"); }
        }


        public UnityEngine.Texture _SpecularColorMap
        {
            get { return material.GetTexture("_SpecularColorMap"); }
        }


        public float _SpecularOcclusionMode
        {
            get { return material.GetFloat("_SpecularOcclusionMode"); }
        }


        public UnityEngine.Color _EmissiveColor
        {
            get { return material.GetColor("_EmissiveColor"); }
        }


        public UnityEngine.Color _EmissiveColorLDR
        {
            get { return material.GetColor("_EmissiveColorLDR"); }
        }


        public UnityEngine.Texture _EmissiveColorMap
        {
            get { return material.GetTexture("_EmissiveColorMap"); }
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


        public float _UseShadowThreshold
        {
            get { return material.GetFloat("_UseShadowThreshold"); }
        }


        public float _AlphaCutoffEnable
        {
            get { return material.GetFloat("_AlphaCutoffEnable"); }
        }


        public float _AlphaCutoff
        {
            get { return material.GetFloat("_AlphaCutoff"); }
        }


        public float _AlphaCutoffShadow
        {
            get { return material.GetFloat("_AlphaCutoffShadow"); }
        }


        public float _AlphaCutoffPrepass
        {
            get { return material.GetFloat("_AlphaCutoffPrepass"); }
        }


        public float _AlphaCutoffPostpass
        {
            get { return material.GetFloat("_AlphaCutoffPostpass"); }
        }


        public float _TransparentDepthPrepassEnable
        {
            get { return material.GetFloat("_TransparentDepthPrepassEnable"); }
        }


        public float _TransparentBackfaceEnable
        {
            get { return material.GetFloat("_TransparentBackfaceEnable"); }
        }


        public float _TransparentDepthPostpassEnable
        {
            get { return material.GetFloat("_TransparentDepthPostpassEnable"); }
        }


        public float _TransparentSortPriority
        {
            get { return material.GetFloat("_TransparentSortPriority"); }
        }


        public float _RefractionModel
        {
            get { return material.GetFloat("_RefractionModel"); }
        }


        public float _Ior
        {
            get { return material.GetFloat("_Ior"); }
        }


        public UnityEngine.Color _TransmittanceColor
        {
            get { return material.GetColor("_TransmittanceColor"); }
        }


        public UnityEngine.Texture _TransmittanceColorMap
        {
            get { return material.GetTexture("_TransmittanceColorMap"); }
        }


        public float _ATDistance
        {
            get { return material.GetFloat("_ATDistance"); }
        }


        public float _TransparentWritingMotionVec
        {
            get { return material.GetFloat("_TransparentWritingMotionVec"); }
        }


        public float _StencilRef
        {
            get { return material.GetFloat("_StencilRef"); }
        }


        public float _StencilWriteMask
        {
            get { return material.GetFloat("_StencilWriteMask"); }
        }


        public float _StencilRefGBuffer
        {
            get { return material.GetFloat("_StencilRefGBuffer"); }
        }


        public float _StencilWriteMaskGBuffer
        {
            get { return material.GetFloat("_StencilWriteMaskGBuffer"); }
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


        public float _CullModeForward
        {
            get { return material.GetFloat("_CullModeForward"); }
        }


        public float _TransparentCullMode
        {
            get { return material.GetFloat("_TransparentCullMode"); }
        }


        public float _OpaqueCullMode
        {
            get { return material.GetFloat("_OpaqueCullMode"); }
        }


        public float _ZTestDepthEqualForOpaque
        {
            get { return material.GetFloat("_ZTestDepthEqualForOpaque"); }
        }


        public float _ZTestGBuffer
        {
            get { return material.GetFloat("_ZTestGBuffer"); }
        }


        public float _ZTestTransparent
        {
            get { return material.GetFloat("_ZTestTransparent"); }
        }


        public float _EnableFogOnTransparent
        {
            get { return material.GetFloat("_EnableFogOnTransparent"); }
        }


        public float _EnableBlendModePreserveSpecularLighting
        {
            get { return material.GetFloat("_EnableBlendModePreserveSpecularLighting"); }
        }


        public float _DoubleSidedEnable
        {
            get { return material.GetFloat("_DoubleSidedEnable"); }
        }


        public float _DoubleSidedNormalMode
        {
            get { return material.GetFloat("_DoubleSidedNormalMode"); }
        }



        public float _DoubleSidedGIMode
        {
            get { return material.GetFloat("_DoubleSidedGIMode"); }
        }


        public float _UVBase
        {
            get { return material.GetFloat("_UVBase"); }
        }


        public float _TexWorldScale
        {
            get { return material.GetFloat("_TexWorldScale"); }
        }


        public float _InvTilingScale
        {
            get { return material.GetFloat("_InvTilingScale"); }
        }


        public UnityEngine.Color _UVMappingMask
        {
            get { return material.GetColor("_UVMappingMask"); }
        }


        public float _NormalMapSpace
        {
            get { return material.GetFloat("_NormalMapSpace"); }
        }


        public float _MaterialID
        {
            get { return material.GetFloat("_MaterialID"); }
        }


        public float _TransmissionEnable
        {
            get { return material.GetFloat("_TransmissionEnable"); }
        }


        public float _DisplacementMode
        {
            get { return material.GetFloat("_DisplacementMode"); }
        }


        public float _DisplacementLockObjectScale
        {
            get { return material.GetFloat("_DisplacementLockObjectScale"); }
        }


        public float _DisplacementLockTilingScale
        {
            get { return material.GetFloat("_DisplacementLockTilingScale"); }
        }


        public float _DepthOffsetEnable
        {
            get { return material.GetFloat("_DepthOffsetEnable"); }
        }


        public float _EnableGeometricSpecularAA
        {
            get { return material.GetFloat("_EnableGeometricSpecularAA"); }
        }


        public float _SpecularAAScreenSpaceVariance
        {
            get { return material.GetFloat("_SpecularAAScreenSpaceVariance"); }
        }


        public float _SpecularAAThreshold
        {
            get { return material.GetFloat("_SpecularAAThreshold"); }
        }


        public float _PPDMinSamples
        {
            get { return material.GetFloat("_PPDMinSamples"); }
        }


        public float _PPDMaxSamples
        {
            get { return material.GetFloat("_PPDMaxSamples"); }
        }


        public float _PPDLodThreshold
        {
            get { return material.GetFloat("_PPDLodThreshold"); }
        }


        public float _PPDPrimitiveLength
        {
            get { return material.GetFloat("_PPDPrimitiveLength"); }
        }


        public float _PPDPrimitiveWidth
        {
            get { return material.GetFloat("_PPDPrimitiveWidth"); }
        }



        public float _UVDetail
        {
            get { return material.GetFloat("_UVDetail"); }
        }


        public UnityEngine.Color _UVDetailsMappingMask
        {
            get { return material.GetColor("_UVDetailsMappingMask"); }
        }


        public float _LinkDetailsWithBase
        {
            get { return material.GetFloat("_LinkDetailsWithBase"); }
        }


        public float _EmissiveColorMode
        {
            get { return material.GetFloat("_EmissiveColorMode"); }
        }


        public float _UVEmissive
        {
            get { return material.GetFloat("_UVEmissive"); }
        }


        public float _TexWorldScaleEmissive
        {
            get { return material.GetFloat("_TexWorldScaleEmissive"); }
        }


        public UnityEngine.Color _UVMappingMaskEmissive
        {
            get { return material.GetColor("_UVMappingMaskEmissive"); }
        }


        public UnityEngine.Color _EmissionColor
        {
            get { return material.GetColor("_EmissionColor"); }
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


        public float _SupportDecals
        {
            get { return material.GetFloat("_SupportDecals"); }
        }


        public float _ReceivesSSR
        {
            get { return material.GetFloat("_ReceivesSSR"); }
        }


        public float _ReceivesSSRTransparent
        {
            get { return material.GetFloat("_ReceivesSSRTransparent"); }
        }


        public float _AddPrecomputedVelocity
        {
            get { return material.GetFloat("_AddPrecomputedVelocity"); }
        }


        public float _RayTracing
        {
            get { return material.GetFloat("_RayTracing"); }
        }


        public float _DiffusionProfile
        {
            get { return material.GetFloat("_DiffusionProfile"); }
        }



        public float _DiffusionProfileHash
        {
            get { return material.GetFloat("_DiffusionProfileHash"); }
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
