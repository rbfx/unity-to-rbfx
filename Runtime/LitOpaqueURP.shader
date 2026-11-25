// The Universal Render Pipeline version of RBFX/LitOpaque: the same properties (so a material can switch between the two, and the exporter maps both the same way), written as
// a URP shader instead of a built-in surface shader, which URP draws magenta. The PBR map is packed the way the rbfx "Properties" texture is: R roughness, G metallic, A occlusion.
Shader "RBFX/URP/LitOpaque"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _PBRMap ("PBR Map (R: Roughness, G: Metallic, A: AO)", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "white" {}
        [HDR] _EmissionColor ("Emission Color (HDR: above 1 glows)", Color) = (0,0,0,1)
        _Color ("Albedo", Color) = (1,1,1,1)
        _BumpScale ("Normal Map Scale", Range(0, 1)) = 1
        _Metallic ("Metallic Multiplier", Range(0, 1)) = 1
        _Roughness ("Roughness Multiplier", Range(0, 1)) = 1
        _Cutoff ("Alpha Mask Threshold", Range(0, 1)) = 0.5
        [Toggle] _IgnoreVertexColor ("Ignore Vertex Color (IGNOREVERTEXCOLOR in rbfx)", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "UniversalMaterialType" = "Lit" }
        LOD 200

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
        TEXTURE2D(_BumpMap);      SAMPLER(sampler_BumpMap);
        TEXTURE2D(_PBRMap);       SAMPLER(sampler_PBRMap);
        TEXTURE2D(_EmissionMap);  SAMPLER(sampler_EmissionMap);

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            half4 _EmissionColor;
            half _BumpScale;
            half _Metallic;
            half _Roughness;
            half _Cutoff;
            half _IgnoreVertexColor;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                half4 fogAndVertexLight : TEXCOORD4;
                half4 color : COLOR;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = normals.normalWS;
                output.tangentWS = half4(normals.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                half3 vertexLight = 0;
            #ifdef _ADDITIONAL_LIGHTS_VERTEX
                vertexLight = VertexLighting(positions.positionWS, normals.normalWS);
            #endif
                output.fogAndVertexLight = half4(ComputeFogFactor(positions.positionCS.z), vertexLight);
                output.color = input.color;
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                clip(baseColor.a - _Cutoff);

                half4 pbr = SAMPLE_TEXTURE2D(_PBRMap, sampler_PBRMap, input.uv);
                // A plain RGB normal map (x in R, y in G), as rbfx reads it. UnpackNormalScale would multiply x by the alpha of the texture, which is 0 in some packs.
                half4 packedNormal = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                half3 normalTS;
                normalTS.xy = (packedNormal.rg * 2.0h - 1.0h) * _BumpScale;
                normalTS.z = sqrt(1.0h - saturate(dot(normalTS.xy, normalTS.xy)));
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, input.uv).rgb * _EmissionColor.rgb;

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = baseColor.rgb * lerp(input.color.rgb, half3(1, 1, 1), _IgnoreVertexColor);
                surface.metallic = pbr.g * _Metallic;
                surface.smoothness = 1.0h - pbr.r * _Roughness;
                surface.occlusion = pbr.a;
                surface.normalTS = normalTS;
                surface.emission = emission;
                surface.alpha = 1.0h;
                surface.specular = half3(0, 0, 0);

                InputData data = (InputData)0;
                data.positionWS = input.positionWS;
                half3 bitangent = input.tangentWS.w * cross(input.normalWS, input.tangentWS.xyz);
                data.normalWS = NormalizeNormalPerPixel(TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent, input.normalWS)));
                data.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                data.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                data.fogCoord = input.fogAndVertexLight.x;
                data.vertexLighting = input.fogAndVertexLight.yzw;
                data.bakedGI = SampleSH(data.normalWS);
                data.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                data.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(data, surface);
                color.rgb = MixFog(color.rgb, data.fogCoord);
                color.a = 1.0h;
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
            #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
            #else
                float3 lightDirectionWS = _LightDirection;
            #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
            #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
            #endif
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex Vertex
            #pragma fragment Fragment

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
            };

            Varyings Vertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 Fragment(Varyings input) : SV_Target
            {
                clip(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a * _Color.a - _Cutoff);
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
