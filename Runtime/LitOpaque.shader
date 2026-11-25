Shader "RBFX/LitOpaque"
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
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _PBRMap;
        sampler2D _EmissionMap;
        fixed4 _Color;
        fixed4 _EmissionColor;
        float _BumpScale;
        float _Metallic;
        float _Roughness;
        float _Cutoff;
        float _IgnoreVertexColor;

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            clip(c.a - _Cutoff);
            o.Albedo = c.rgb * lerp(IN.color.rgb, fixed3(1, 1, 1), _IgnoreVertexColor);

            // PBR map unpacking
            half4 pbr = tex2D(_PBRMap, IN.uv_MainTex);
            half roughness = pbr.r * _Roughness;
            half metallic = pbr.g * _Metallic;
            half ao = pbr.a;

            o.Metallic = metallic;
            o.Smoothness = 1.0 - roughness;
            o.Occlusion = ao;

            // Apply the normal map
            // A plain RGB normal map (x in R, y in G), as rbfx reads it. UnpackNormal would multiply x by the alpha of the texture, which is 0 in some packs.
            fixed4 packedNormal = tex2D(_BumpMap, IN.uv_MainTex);
            fixed3 tangentNormal;
            tangentNormal.xy = (packedNormal.rg * 2 - 1) * _BumpScale;
            tangentNormal.z = sqrt(1 - saturate(dot(tangentNormal.xy, tangentNormal.xy)));
            o.Normal = tangentNormal;

            // Apply emission map
            fixed4 emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;
            o.Emission = emission.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
