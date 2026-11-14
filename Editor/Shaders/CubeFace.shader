Shader "Hidden/UnityToRebelFork/CubeFace"
{
    // Draws one face (_Face: 0..5 = +X -X +Y -Y +Z -Z, the order of DDS files) of a cube map at one mip level, decoded to linear float.
    // The texture may be RGBM or any other encoding DecodeHDR knows (_DecodeInstructions = the probe's textureHDRDecodeValues).
    // The face is drawn upside down in texture space (row 0 of the target is the bottom row of the face, as ReadPixels returns it),
    // so the exporter flips the rows when it writes the file.
    Properties
    {
        _Cube ("Cube", CUBE) = "" {}
        _Face ("Face", Float) = 0
        _Mip ("Mip", Float) = 0
        _DecodeInstructions ("Decode instructions", Vector) = (1, 1, 0, 1)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            samplerCUBE _Cube;
            float _Face;
            float _Mip;
            float4 _DecodeInstructions;

            float4 frag (v2f i) : SV_Target
            {
                // s runs right, t runs down the face (the Direct3D cube map convention that DDS files use).
                float s = i.uv.x * 2.0 - 1.0;
                float t = (1.0 - i.uv.y) * 2.0 - 1.0;
                float3 d;
                if (_Face < 0.5) d = float3(1.0, -t, -s);
                else if (_Face < 1.5) d = float3(-1.0, -t, s);
                else if (_Face < 2.5) d = float3(s, 1.0, t);
                else if (_Face < 3.5) d = float3(s, -1.0, -t);
                else if (_Face < 4.5) d = float3(s, -t, 1.0);
                else d = float3(-s, -t, -1.0);

                float4 data = texCUBElod(_Cube, float4(d, _Mip));
                return float4(DecodeHDR(data, _DecodeInstructions), 1.0);
            }
            ENDCG
        }
    }
}
