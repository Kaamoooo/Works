Shader "Unlit/CloudDepth"
{
    Properties
    {
    }
    SubShader
    {
        ZWrite On
        ZTest LEqual
        Cull Off
        Tags
        {
            "RenderType"="Opaque" "Queue"="Geometry"
        }
        LOD 100

        Pass
        {
//            ColorMask 0
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(1, 1, 0, 0);
            }
            ENDHLSL
        }
    }
}