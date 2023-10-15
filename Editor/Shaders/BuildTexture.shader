Shader "Hidden/RebelFork/BuildTexture"
{
    Properties
    {
        _RChannelMap("R Channel Texture", 2D) = "black" {}
        _RChannelRanges("R Channel Ranges", Vector) = (0,1,0,1)
        _RChannelMask("R Channel Mask", Vector) = (1,0,0,0)

        _GChannelMap("G Channel Texture", 2D) = "black" {}
        _GChannelRanges("G Channel Ranges", Vector) = (0,1,0,1)
        _GChannelMask("G Channel Mask", Vector) = (0,1,0,0)

        _BChannelMap("B Channel Texture", 2D) = "black" {}
        _BChannelRanges("B Channel Ranges", Vector) = (0,1,0,1)
        _BChannelMask("B Channel Mask", Vector) = (0,0,1,0)

        _AChannelMap("A Channel Texture", 2D) = "white" {}
        _AChannelRanges("A Channel Ranges", Vector) = (0,1,0,1)
        _AChannelMask("A Channel Mask", Vector) = (0,0,0,1)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            sampler2D _RChannelMap;
            float4 _RChannelRanges;
            float4 _RChannelMask;

            sampler2D _GChannelMap;
            float4 _GChannelRanges;
            float4 _GChannelMask;

            sampler2D _BChannelMap;
            float4 _BChannelRanges;
            float4 _BChannelMask;

            sampler2D _AChannelMap;
            float4 _AChannelRanges;
            float4 _AChannelMask;

            float4 MapColorChannel (float4 src, float4 mask, float4 ranges)
            {
                float value = dot(src, mask);
                float normalized = (value-ranges.x)/(ranges.y-ranges.x);
                return normalized*(ranges.w-ranges.z)+ranges.z;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float r = MapColorChannel(tex2D(_RChannelMap, i.uv), _RChannelMask, _RChannelRanges);
                float g = MapColorChannel(tex2D(_GChannelMap, i.uv), _GChannelMask, _GChannelRanges);
                float b = MapColorChannel(tex2D(_BChannelMap, i.uv), _BChannelMask, _BChannelRanges);
                float a = MapColorChannel(tex2D(_AChannelMap, i.uv), _AChannelMask, _AChannelRanges);
                return fixed4(r,g,b,a);
            }
            ENDCG
        }
    }
}
