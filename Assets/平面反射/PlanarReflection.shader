Shader "Custom/Lake"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NormalTex("Normal Texture", 2D) = "bump" {}
        _Alpha("Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float2 ndc : TEXCOORD1;
            float4 vertex : SV_POSITION;
        };

        sampler2D _ReflectionTexture;
        sampler2D _MainTex;
        sampler2D _NormalTex;
        float _Alpha;
        ENDHLSL

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
                "IgnoreProjector" = "True"
            }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                float4 ndcPos = GetVertexPositionInputs(v.vertex).positionNDC;
                o.ndc = ndcPos.xy / ndcPos.w;
                return o;
            }

            real4 frag(v2f i) : SV_Target
            {
                // float3 normal = UnpackNormal(tex2D(_NormalTex, i.uv));
                float4 col = tex2D(_ReflectionTexture, i.ndc);
                float4 baseCol = tex2D(_MainTex, i.uv);
                return float4(col.rgb + baseCol.rgb, _Alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Simple Lit"
}