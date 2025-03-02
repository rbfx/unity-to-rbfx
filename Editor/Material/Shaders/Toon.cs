namespace UnityToRebelFork.Editor.Shaders
{
    public class ToonShaderAdapter
    {
        public static readonly string ShaderName = "Toon";

        UnityEngine.Material material;

        public ToonShaderAdapter(UnityEngine.Material material)
        {
            this.material = material;
        }

        public float _simpleUI
        {
            get { return material.GetFloat("_simpleUI"); }
        }

        public float _isUnityToonshader
        {
            get { return material.GetFloat("_isUnityToonshader"); }
        }

        public float _utsVersionX
        {
            get { return material.GetFloat("_utsVersionX"); }
        }

        public float _utsVersionY
        {
            get { return material.GetFloat("_utsVersionY"); }
        }

        public float _utsVersionZ
        {
            get { return material.GetFloat("_utsVersionZ"); }
        }

        public float _utsTechnique
        {
            get { return material.GetFloat("_utsTechnique"); }
        }

        public float _AutoRenderQueue
        {
            get { return material.GetFloat("_AutoRenderQueue"); }
        }

        public float _StencilMode
        {
            get { return material.GetFloat("_StencilMode"); }
        }

        public float _StencilComp
        {
            get { return material.GetFloat("_StencilComp"); }
        }

        public float _StencilNo
        {
            get { return material.GetFloat("_StencilNo"); }
        }

        public float _StencilOpPass
        {
            get { return material.GetFloat("_StencilOpPass"); }
        }

        public float _StencilOpFail
        {
            get { return material.GetFloat("_StencilOpFail"); }
        }

        public float _TransparentEnabled
        {
            get { return material.GetFloat("_TransparentEnabled"); }
        }

        public float _ClippingMode
        {
            get { return material.GetFloat("_ClippingMode"); }
        }

        public float _CullMode
        {
            get { return material.GetFloat("_CullMode"); }
        }

        public float _ZWriteMode
        {
            get { return material.GetFloat("_ZWriteMode"); }
        }

        public float _ZOverDrawMode
        {
            get { return material.GetFloat("_ZOverDrawMode"); }
        }

        public float _SPRDefaultUnlitColorMask
        {
            get { return material.GetFloat("_SPRDefaultUnlitColorMask"); }
        }

        public float _SRPDefaultUnlitColMode
        {
            get { return material.GetFloat("_SRPDefaultUnlitColMode"); }
        }

        public UnityEngine.Texture _ClippingMask
        {
            get { return material.GetTexture("_ClippingMask"); }
        }

        public float _IsBaseMapAlphaAsClippingMask
        {
            get { return material.GetFloat("_IsBaseMapAlphaAsClippingMask"); }
        }

        public float _Inverse_Clipping
        {
            get { return material.GetFloat("_Inverse_Clipping"); }
        }

        public float _Clipping_Level
        {
            get { return material.GetFloat("_Clipping_Level"); }
        }

        public float _Tweak_transparency
        {
            get { return material.GetFloat("_Tweak_transparency"); }
        }

        //public float _CullMode
        //{
        //    get { return material.GetFloat("_CullMode"); }
        //}

        public UnityEngine.Texture _MainTex
        {
            get { return material.GetTexture("_MainTex"); }
        }

        public UnityEngine.Texture _BaseMap
        {
            get { return material.GetTexture("_BaseMap"); }
        }

        public UnityEngine.Color _BaseColor
        {
            get { return material.GetColor("_BaseColor"); }
        }

        public UnityEngine.Color _Color
        {
            get { return material.GetColor("_Color"); }
        }

        public float _Is_LightColor_Base
        {
            get { return material.GetFloat("_Is_LightColor_Base"); }
        }

        public UnityEngine.Texture _1st_ShadeMap
        {
            get { return material.GetTexture("_1st_ShadeMap"); }
        }

        public float _Use_BaseAs1st
        {
            get { return material.GetFloat("_Use_BaseAs1st"); }
        }

        public UnityEngine.Color _1st_ShadeColor
        {
            get { return material.GetColor("_1st_ShadeColor"); }
        }

        public float _Is_LightColor_1st_Shade
        {
            get { return material.GetFloat("_Is_LightColor_1st_Shade"); }
        }

        public UnityEngine.Texture _2nd_ShadeMap
        {
            get { return material.GetTexture("_2nd_ShadeMap"); }
        }

        public float _Use_1stAs2nd
        {
            get { return material.GetFloat("_Use_1stAs2nd"); }
        }

        public UnityEngine.Color _2nd_ShadeColor
        {
            get { return material.GetColor("_2nd_ShadeColor"); }
        }

        public float _Is_LightColor_2nd_Shade
        {
            get { return material.GetFloat("_Is_LightColor_2nd_Shade"); }
        }

        public UnityEngine.Texture _NormalMap
        {
            get { return material.GetTexture("_NormalMap"); }
        }

        public float _BumpScale
        {
            get { return material.GetFloat("_BumpScale"); }
        }

        public float _Is_NormalMapToBase
        {
            get { return material.GetFloat("_Is_NormalMapToBase"); }
        }

        public float _Set_SystemShadowsToBase
        {
            get { return material.GetFloat("_Set_SystemShadowsToBase"); }
        }

        public float _Tweak_SystemShadowsLevel
        {
            get { return material.GetFloat("_Tweak_SystemShadowsLevel"); }
        }

        public float _BaseColor_Step
        {
            get { return material.GetFloat("_BaseColor_Step"); }
        }

        public float _BaseShade_Feather
        {
            get { return material.GetFloat("_BaseShade_Feather"); }
        }

        public float _ShadeColor_Step
        {
            get { return material.GetFloat("_ShadeColor_Step"); }
        }

        public float _1st2nd_Shades_Feather
        {
            get { return material.GetFloat("_1st2nd_Shades_Feather"); }
        }

        public float _1st_ShadeColor_Step
        {
            get { return material.GetFloat("_1st_ShadeColor_Step"); }
        }

        public float _1st_ShadeColor_Feather
        {
            get { return material.GetFloat("_1st_ShadeColor_Feather"); }
        }

        public float _2nd_ShadeColor_Step
        {
            get { return material.GetFloat("_2nd_ShadeColor_Step"); }
        }

        public float _2nd_ShadeColor_Feather
        {
            get { return material.GetFloat("_2nd_ShadeColor_Feather"); }
        }

        public float _StepOffset
        {
            get { return material.GetFloat("_StepOffset"); }
        }

        public float _Is_Filter_HiCutPointLightColor
        {
            get { return material.GetFloat("_Is_Filter_HiCutPointLightColor"); }
        }

        public UnityEngine.Texture _Set_1st_ShadePosition
        {
            get { return material.GetTexture("_Set_1st_ShadePosition"); }
        }

        public UnityEngine.Texture _Set_2nd_ShadePosition
        {
            get { return material.GetTexture("_Set_2nd_ShadePosition"); }
        }

        public UnityEngine.Texture _ShadingGradeMap
        {
            get { return material.GetTexture("_ShadingGradeMap"); }
        }

        public float _Tweak_ShadingGradeMapLevel
        {
            get { return material.GetFloat("_Tweak_ShadingGradeMapLevel"); }
        }

        public float _BlurLevelSGM
        {
            get { return material.GetFloat("_BlurLevelSGM"); }
        }

        public UnityEngine.Color _HighColor
        {
            get { return material.GetColor("_HighColor"); }
        }

        public UnityEngine.Texture _HighColor_Tex
        {
            get { return material.GetTexture("_HighColor_Tex"); }
        }

        public float _Is_LightColor_HighColor
        {
            get { return material.GetFloat("_Is_LightColor_HighColor"); }
        }

        public float _Is_NormalMapToHighColor
        {
            get { return material.GetFloat("_Is_NormalMapToHighColor"); }
        }

        public float _HighColor_Power
        {
            get { return material.GetFloat("_HighColor_Power"); }
        }

        public float _Is_SpecularToHighColor
        {
            get { return material.GetFloat("_Is_SpecularToHighColor"); }
        }

        public float _Is_BlendAddToHiColor
        {
            get { return material.GetFloat("_Is_BlendAddToHiColor"); }
        }

        public float _Is_UseTweakHighColorOnShadow
        {
            get { return material.GetFloat("_Is_UseTweakHighColorOnShadow"); }
        }

        public float _TweakHighColorOnShadow
        {
            get { return material.GetFloat("_TweakHighColorOnShadow"); }
        }

        public UnityEngine.Texture _Set_HighColorMask
        {
            get { return material.GetTexture("_Set_HighColorMask"); }
        }

        public float _Tweak_HighColorMaskLevel
        {
            get { return material.GetFloat("_Tweak_HighColorMaskLevel"); }
        }

        public float _RimLight
        {
            get { return material.GetFloat("_RimLight"); }
        }

        public UnityEngine.Color _RimLightColor
        {
            get { return material.GetColor("_RimLightColor"); }
        }

        public float _Is_LightColor_RimLight
        {
            get { return material.GetFloat("_Is_LightColor_RimLight"); }
        }

        public float _Is_NormalMapToRimLight
        {
            get { return material.GetFloat("_Is_NormalMapToRimLight"); }
        }

        public float _RimLight_Power
        {
            get { return material.GetFloat("_RimLight_Power"); }
        }

        public float _RimLight_InsideMask
        {
            get { return material.GetFloat("_RimLight_InsideMask"); }
        }

        public float _RimLight_FeatherOff
        {
            get { return material.GetFloat("_RimLight_FeatherOff"); }
        }

        public float _LightDirection_MaskOn
        {
            get { return material.GetFloat("_LightDirection_MaskOn"); }
        }

        public float _Tweak_LightDirection_MaskLevel
        {
            get { return material.GetFloat("_Tweak_LightDirection_MaskLevel"); }
        }

        public float _Add_Antipodean_RimLight
        {
            get { return material.GetFloat("_Add_Antipodean_RimLight"); }
        }

        public UnityEngine.Color _Ap_RimLightColor
        {
            get { return material.GetColor("_Ap_RimLightColor"); }
        }

        public float _Is_LightColor_Ap_RimLight
        {
            get { return material.GetFloat("_Is_LightColor_Ap_RimLight"); }
        }

        public float _Ap_RimLight_Power
        {
            get { return material.GetFloat("_Ap_RimLight_Power"); }
        }

        public float _Ap_RimLight_FeatherOff
        {
            get { return material.GetFloat("_Ap_RimLight_FeatherOff"); }
        }

        public UnityEngine.Texture _Set_RimLightMask
        {
            get { return material.GetTexture("_Set_RimLightMask"); }
        }

        public float _Tweak_RimLightMaskLevel
        {
            get { return material.GetFloat("_Tweak_RimLightMaskLevel"); }
        }

        public float _MatCap
        {
            get { return material.GetFloat("_MatCap"); }
        }

        public UnityEngine.Texture _MatCap_Sampler
        {
            get { return material.GetTexture("_MatCap_Sampler"); }
        }

        public float _BlurLevelMatcap
        {
            get { return material.GetFloat("_BlurLevelMatcap"); }
        }

        public UnityEngine.Color _MatCapColor
        {
            get { return material.GetColor("_MatCapColor"); }
        }

        public float _Is_LightColor_MatCap
        {
            get { return material.GetFloat("_Is_LightColor_MatCap"); }
        }

        public float _Is_BlendAddToMatCap
        {
            get { return material.GetFloat("_Is_BlendAddToMatCap"); }
        }

        public float _Tweak_MatCapUV
        {
            get { return material.GetFloat("_Tweak_MatCapUV"); }
        }

        public float _Rotate_MatCapUV
        {
            get { return material.GetFloat("_Rotate_MatCapUV"); }
        }

        public float _CameraRolling_Stabilizer
        {
            get { return material.GetFloat("_CameraRolling_Stabilizer"); }
        }

        public float _Is_NormalMapForMatCap
        {
            get { return material.GetFloat("_Is_NormalMapForMatCap"); }
        }

        public UnityEngine.Texture _NormalMapForMatCap
        {
            get { return material.GetTexture("_NormalMapForMatCap"); }
        }

        public float _BumpScaleMatcap
        {
            get { return material.GetFloat("_BumpScaleMatcap"); }
        }

        public float _Rotate_NormalMapForMatCapUV
        {
            get { return material.GetFloat("_Rotate_NormalMapForMatCapUV"); }
        }

        public float _Is_UseTweakMatCapOnShadow
        {
            get { return material.GetFloat("_Is_UseTweakMatCapOnShadow"); }
        }

        public float _TweakMatCapOnShadow
        {
            get { return material.GetFloat("_TweakMatCapOnShadow"); }
        }

        public UnityEngine.Texture _Set_MatcapMask
        {
            get { return material.GetTexture("_Set_MatcapMask"); }
        }

        public float _Tweak_MatcapMaskLevel
        {
            get { return material.GetFloat("_Tweak_MatcapMaskLevel"); }
        }

        public float _Inverse_MatcapMask
        {
            get { return material.GetFloat("_Inverse_MatcapMask"); }
        }

        public float _Is_Ortho
        {
            get { return material.GetFloat("_Is_Ortho"); }
        }

        public float _AngelRing
        {
            get { return material.GetFloat("_AngelRing"); }
        }

        public UnityEngine.Texture _AngelRing_Sampler
        {
            get { return material.GetTexture("_AngelRing_Sampler"); }
        }

        public UnityEngine.Color _AngelRing_Color
        {
            get { return material.GetColor("_AngelRing_Color"); }
        }

        public float _Is_LightColor_AR
        {
            get { return material.GetFloat("_Is_LightColor_AR"); }
        }

        public float _AR_OffsetU
        {
            get { return material.GetFloat("_AR_OffsetU"); }
        }

        public float _AR_OffsetV
        {
            get { return material.GetFloat("_AR_OffsetV"); }
        }

        public float _ARSampler_AlphaOn
        {
            get { return material.GetFloat("_ARSampler_AlphaOn"); }
        }

        public float _EMISSIVE
        {
            get { return material.GetFloat("_EMISSIVE"); }
        }

        public UnityEngine.Texture _Emissive_Tex
        {
            get { return material.GetTexture("_Emissive_Tex"); }
        }

        public UnityEngine.Color _Emissive_Color
        {
            get { return material.GetColor("_Emissive_Color"); }
        }

        public float _Base_Speed
        {
            get { return material.GetFloat("_Base_Speed"); }
        }

        public float _Scroll_EmissiveU
        {
            get { return material.GetFloat("_Scroll_EmissiveU"); }
        }

        public float _Scroll_EmissiveV
        {
            get { return material.GetFloat("_Scroll_EmissiveV"); }
        }

        public float _Rotate_EmissiveUV
        {
            get { return material.GetFloat("_Rotate_EmissiveUV"); }
        }

        public float _Is_PingPong_Base
        {
            get { return material.GetFloat("_Is_PingPong_Base"); }
        }

        public float _Is_ColorShift
        {
            get { return material.GetFloat("_Is_ColorShift"); }
        }

        public UnityEngine.Color _ColorShift
        {
            get { return material.GetColor("_ColorShift"); }
        }

        public float _ColorShift_Speed
        {
            get { return material.GetFloat("_ColorShift_Speed"); }
        }

        public float _Is_ViewShift
        {
            get { return material.GetFloat("_Is_ViewShift"); }
        }

        public UnityEngine.Color _ViewShift
        {
            get { return material.GetColor("_ViewShift"); }
        }

        public float _Is_ViewCoord_Scroll
        {
            get { return material.GetFloat("_Is_ViewCoord_Scroll"); }
        }

        public float _OUTLINE
        {
            get { return material.GetFloat("_OUTLINE"); }
        }

        public float _Outline_Width
        {
            get { return material.GetFloat("_Outline_Width"); }
        }

        public float _Farthest_Distance
        {
            get { return material.GetFloat("_Farthest_Distance"); }
        }

        public float _Nearest_Distance
        {
            get { return material.GetFloat("_Nearest_Distance"); }
        }

        public UnityEngine.Texture _Outline_Sampler
        {
            get { return material.GetTexture("_Outline_Sampler"); }
        }

        public UnityEngine.Color _Outline_Color
        {
            get { return material.GetColor("_Outline_Color"); }
        }

        public float _Is_BlendBaseColor
        {
            get { return material.GetFloat("_Is_BlendBaseColor"); }
        }

        public float _Is_LightColor_Outline
        {
            get { return material.GetFloat("_Is_LightColor_Outline"); }
        }

        public float _Is_OutlineTex
        {
            get { return material.GetFloat("_Is_OutlineTex"); }
        }

        public UnityEngine.Texture _OutlineTex
        {
            get { return material.GetTexture("_OutlineTex"); }
        }

        public float _Offset_Z
        {
            get { return material.GetFloat("_Offset_Z"); }
        }

        public float _Is_BakedNormal
        {
            get { return material.GetFloat("_Is_BakedNormal"); }
        }

        public UnityEngine.Texture _BakedNormal
        {
            get { return material.GetTexture("_BakedNormal"); }
        }

        public float _GI_Intensity
        {
            get { return material.GetFloat("_GI_Intensity"); }
        }

        public float _Unlit_Intensity
        {
            get { return material.GetFloat("_Unlit_Intensity"); }
        }

        public float _Is_Filter_LightColor
        {
            get { return material.GetFloat("_Is_Filter_LightColor"); }
        }

        public float _Is_BLD
        {
            get { return material.GetFloat("_Is_BLD"); }
        }

        public float _Offset_X_Axis_BLD
        {
            get { return material.GetFloat("_Offset_X_Axis_BLD"); }
        }

        public float _Offset_Y_Axis_BLD
        {
            get { return material.GetFloat("_Offset_Y_Axis_BLD"); }
        }

        public float _Inverse_Z_Axis_BLD
        {
            get { return material.GetFloat("_Inverse_Z_Axis_BLD"); }
        }

        public float _BaseColorVisible
        {
            get { return material.GetFloat("_BaseColorVisible"); }
        }

        public float _BaseColorOverridden
        {
            get { return material.GetFloat("_BaseColorOverridden"); }
        }

        public UnityEngine.Color _BaseColorMaskColor
        {
            get { return material.GetColor("_BaseColorMaskColor"); }
        }

        public float _FirstShadeVisible
        {
            get { return material.GetFloat("_FirstShadeVisible"); }
        }

        public float _FirstShadeOverridden
        {
            get { return material.GetFloat("_FirstShadeOverridden"); }
        }

        public UnityEngine.Color _FirstShadeMaskColor
        {
            get { return material.GetColor("_FirstShadeMaskColor"); }
        }

        public float _SecondShadeVisible
        {
            get { return material.GetFloat("_SecondShadeVisible"); }
        }

        public float _SecondShadeOverridden
        {
            get { return material.GetFloat("_SecondShadeOverridden"); }
        }

        public UnityEngine.Color _SecondShadeMaskColor
        {
            get { return material.GetColor("_SecondShadeMaskColor"); }
        }

        public float _HighlightVisible
        {
            get { return material.GetFloat("_HighlightVisible"); }
        }

        public float _HighlightOverridden
        {
            get { return material.GetFloat("_HighlightOverridden"); }
        }

        public UnityEngine.Color _HighlightMaskColor
        {
            get { return material.GetColor("_HighlightMaskColor"); }
        }

        public float _AngelRingVisible
        {
            get { return material.GetFloat("_AngelRingVisible"); }
        }

        public float _AngelRingOverridden
        {
            get { return material.GetFloat("_AngelRingOverridden"); }
        }

        public UnityEngine.Color _AngelRingMaskColor
        {
            get { return material.GetColor("_AngelRingMaskColor"); }
        }

        public float _RimLightVisible
        {
            get { return material.GetFloat("_RimLightVisible"); }
        }

        public float _RimLightOverridden
        {
            get { return material.GetFloat("_RimLightOverridden"); }
        }

        public UnityEngine.Color _RimLightMaskColor
        {
            get { return material.GetColor("_RimLightMaskColor"); }
        }

        public float _OutlineVisible
        {
            get { return material.GetFloat("_OutlineVisible"); }
        }

        public float _OutlineOverridden
        {
            get { return material.GetFloat("_OutlineOverridden"); }
        }

        public UnityEngine.Color _OutlineMaskColor
        {
            get { return material.GetColor("_OutlineMaskColor"); }
        }

        public float _ComposerMaskMode
        {
            get { return material.GetFloat("_ComposerMaskMode"); }
        }

        public float _ClippingMatteMode
        {
            get { return material.GetFloat("_ClippingMatteMode"); }
        }

        public UnityEngine.Color emissive
        {
            get { return material.GetColor("emissive"); }
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

        public float _SmoothnessRemapMin
        {
            get { return material.GetFloat("_SmoothnessRemapMin"); }
        }

        public float _SmoothnessRemapMax
        {
            get { return material.GetFloat("_SmoothnessRemapMax"); }
        }

        public float _AlphaRemapMin
        {
            get { return material.GetFloat("_AlphaRemapMin"); }
        }

        public float _AlphaRemapMax
        {
            get { return material.GetFloat("_AlphaRemapMax"); }
        }

        public float _AORemapMin
        {
            get { return material.GetFloat("_AORemapMin"); }
        }

        public float _AORemapMax
        {
            get { return material.GetFloat("_AORemapMax"); }
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

        public float _DiffusionProfile
        {
            get { return material.GetFloat("_DiffusionProfile"); }
        }

        public float _DiffusionProfileHash
        {
            get { return material.GetFloat("_DiffusionProfileHash"); }
        }

        public float _SubsurfaceMask
        {
            get { return material.GetFloat("_SubsurfaceMask"); }
        }

        public UnityEngine.Texture _SubsurfaceMaskMap
        {
            get { return material.GetTexture("_SubsurfaceMaskMap"); }
        }

        public float _TransmissionMask
        {
            get { return material.GetFloat("_TransmissionMask"); }
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

        public UnityEngine.Texture _DistortionVectorMap
        {
            get { return material.GetTexture("_DistortionVectorMap"); }
        }

        public float _DistortionEnable
        {
            get { return material.GetFloat("_DistortionEnable"); }
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

        public float _SSRefractionProjectionModel
        {
            get { return material.GetFloat("_SSRefractionProjectionModel"); }
        }

        public float _Ior
        {
            get { return material.GetFloat("_Ior"); }
        }

        public float _ThicknessMultiplier
        {
            get { return material.GetFloat("_ThicknessMultiplier"); }
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

        public float _StencilRefDistortionVec
        {
            get { return material.GetFloat("_StencilRefDistortionVec"); }
        }

        public float _StencilWriteMaskDistortionVec
        {
            get { return material.GetFloat("_StencilWriteMaskDistortionVec"); }
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

        public float _ZWrite
        {
            get { return material.GetFloat("_ZWrite"); }
        }

        public float _TransparentZWrite
        {
            get { return material.GetFloat("_TransparentZWrite"); }
        }

        //public float _CullMode
        //{
        //    get { return material.GetFloat("_CullMode"); }
        //}

        public float _CullModeForward
        {
            get { return material.GetFloat("_CullModeForward"); }
        }

        public float _TransparentCullMode
        {
            get { return material.GetFloat("_TransparentCullMode"); }
        }

        public float _ZTestDepthEqualForOpaque
        {
            get { return material.GetFloat("_ZTestDepthEqualForOpaque"); }
        }

        public float _ZTestModeDistortion
        {
            get { return material.GetFloat("_ZTestModeDistortion"); }
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

        public float _UVBase
        {
            get { return material.GetFloat("_UVBase"); }
        }

        public float _ObjectSpaceUVMapping
        {
            get { return material.GetFloat("_ObjectSpaceUVMapping"); }
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

        //public UnityEngine.Color _Color
        //{
        //    get { return material.GetColor("_Color"); }
        //}

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

        public float _AddPrecomputedVelocity
        {
            get { return material.GetFloat("_AddPrecomputedVelocity"); }
        }
    }
}
