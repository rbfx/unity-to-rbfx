Shader "RBFX/LitOpaque"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _PBRMap ("PBR Map (R: Roughness, G: Metallic, A: AO)", 2D) = "white" {}
        _EmissionMap ("Emission Map", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0,0,0,1)
        _Color ("Albedo", Color) = (1,1,1,1)
        _BumpScale ("Normal Map Scale", Range(0, 1)) = 1
        _Metallic ("Metallic Multiplier", Range(0, 1)) = 1
        _Roughness ("Roughness Multiplier", Range(0, 1)) = 1
        _Cutoff ("Alpha Mask Threshold", Range(0, 1)) = 0.5
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

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            clip(c.a - _Cutoff);
            o.Albedo = c.rgb * IN.color.rgb;

            // PBR map unpacking
            half4 pbr = tex2D(_PBRMap, IN.uv_MainTex);
            half roughness = pbr.r * _Roughness;
            half metallic = pbr.g * _Metallic;
            half ao = pbr.a;

            o.Metallic = metallic;
            o.Smoothness = 1.0 - roughness;
            o.Occlusion = ao;

            // Apply the normal map
            o.Normal = UnpackNormalWithScale(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);

            // Apply emission map
            fixed4 emission = tex2D(_EmissionMap, IN.uv_MainTex) * _EmissionColor;
            o.Emission = emission.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
