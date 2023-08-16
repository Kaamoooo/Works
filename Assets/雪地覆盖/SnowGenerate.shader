Shader "Custom/SnowGenerate"
{
    Properties
    {
        [KeywordEnum(True,False)]_EnableSnow ("EnableSnow", Int) = 0
        _MainTex ("Texture", 2D) = "white" {}
        _AOTex ("AO Texture", 2D) = "white" {}
        _SnowHeight ("Snow Height", Range(0,2)) = 0.1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float3 worldNormal : TEXCOORD1;
            float4 vertex : SV_POSITION;
        };
        ENDHLSL
        LOD 100
        Pass
        {
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ENABLESNOW_TRUE _ENABLESNOW_FALSE


            sampler2D _MainTex;
            sampler2D _AOTex;

            sampler2D _SnowTex;

            float4 _MainTex_ST;
            float _SnowHeight;


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            real4 frag(v2f i) : SV_Target
            {
                real4 col = tex2D(_MainTex, i.uv);

                #ifdef _ENABLESNOW_FALSE
                return col;
                #endif

                real4 snow = tex2D(_SnowTex, i.uv);
                float facingRatio = saturate(dot(i.worldNormal, float3(0, 1, 0) * _SnowHeight));
                real4 ao = tex2D(_AOTex, i.uv);
                return facingRatio * ao * snow + col;
            }
            ENDHLSL
        }
        Pass
        {
            ColorMask 0
            Tags
            {
                "LightMode" = "DepthOnly"
            }
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv=v.uv;
                return o;
            }

            float4 frag(v2f i):SV_Target
            {
                return float4(1,1,0,0);   
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}