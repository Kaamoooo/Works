Shader "Custom/DecalShader"
{
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _CubeScale;
                Texture2D _DecalColorTexture;
                Texture2D _DecalOpacityTexture;
                SamplerState sampler_DecalColorTexture;
            CBUFFER_END

            struct VertexInput
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;

                VertexNormalInputs normInputs = GetVertexNormalInputs(v.normalOS);
                o.normalWS = normInputs.normalWS;

                o.uv = v.uv;
                return o;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                float3 NDC = ComputeNormalizedDeviceCoordinatesWithZ(i.positionWS,UNITY_MATRIX_VP);
                float depth = SampleSceneDepth(NDC.xy);
                float3 worldPos = ComputeWorldSpacePosition(NDC.xy, depth, UNITY_MATRIX_I_VP);
                float3 objPos = mul(UNITY_MATRIX_I_M, float4(worldPos, 1)).xyz;
                float3 normalizePos = float3(objPos.x + 0.5,
                                             objPos.y + 0.5,
                                             objPos.z + 0.5);
                if (any(saturate(normalizePos) != normalizePos))
                {
                    return float4(0, 0, 0, 0);
                }
                float2 uv = normalizePos.xz;
                half4 decalColor = SAMPLE_TEXTURE2D(_DecalColorTexture, sampler_DecalColorTexture, uv);
                float opacity = SAMPLE_TEXTURE2D(_DecalOpacityTexture, sampler_DecalColorTexture, uv);
                return float4(decalColor.xyz, opacity);
            }
            ENDHLSL
        }
    }
}