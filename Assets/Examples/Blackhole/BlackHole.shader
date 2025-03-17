Shader "Custom/BlackHole"
{
    Properties
    {
        _EventHorizonRatio("Event Horizon Ratio", Range(0, 1)) = 0.5
        _GM("GM", Float) = 0.4
        _dt("dt", Float) = 0.02
        _AccumulationColorScale ("Accumulation Color Scale", Range(0, 1)) = 0.01
        _DiskScale("Disk Scale", Range(0, 10)) = 0.02
        _DiskRange("Disk Range", Range(0, 0.5)) = 0.5
        _DiskHeight("Disk Height", Range(0, 1)) = 0.1
        _DiskMainColor("Disk Main Color", Color) = (1, 1, 1, 1)
        _RotateSpeed("Rotate Speed", Range(0, 1)) = 0.2
        _NoiseTexture("Noise Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Geometry"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _EventHorizonRatio;
                float _GM;
                float _dt;
                float _AccumulationColorScale;
                float _DiskScale;
                float _DiskRange;
                float _DiskHeight;
                float _RotateSpeed;
                float4 _DiskMainColor;
                Texture2D _CameraColorTexture;
                SamplerState sampler_CameraColorTexture;
                Texture2D _NoiseTexture;
                SamplerState sampler_NoiseTexture;
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
                float3 positionOS : TEXCOORD2;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = posInputs.positionCS;
                o.positionWS = posInputs.positionWS;
                o.positionOS = v.positionOS;

                o.uv = v.uv;
                return o;
            }

            bool isEventHorizon(float3 posOS)
            {
                float distance = length(posOS);
                return distance < _EventHorizonRatio * 0.5;
            }

            bool IsOnDisk(float3 posOS)
            {
                float horizontalDistance = length(posOS.xz);
                float distanceToOuterRing = 0.5 - horizontalDistance;
                float2 uv = float2(horizontalDistance * 2,
                                   atan2(posOS.x, posOS.z) / 3.1415926 * 0.5 + _Time.x * _RotateSpeed);
                float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, uv).x;
                return abs(posOS.y) < distanceToOuterRing * (0.1 * _DiskHeight) && distanceToOuterRing > _DiskRange
                    && noise > (horizontalDistance * horizontalDistance + 0.01) ;
            }

            float4 DiskColor(float3 posOS)
            {
                float horizontalDistance = length(posOS.xz);
                float2 uv = float2(horizontalDistance * 2,
                                   atan2(posOS.x, posOS.z) / 3.1415926 * 0.5 + _Time.x * _RotateSpeed);
                float noise = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, uv).x;
                float4 color = lerp(_DiskMainColor, float4(1, 1, 1, 1), noise);
                float distanceToOuterRing = 0.5 - horizontalDistance+0.1;
                float colorFading = distanceToOuterRing * 2;
                return color * (colorFading * colorFading * colorFading ) * _DiskScale;
            }

            float gravityScale(float3 posOS)
            {
                float distance = length(posOS);
                return 1 - smoothstep(0, 0.5, distance);
            }

            float RecalculateDt(float3 posWS)
            {
                float3 blackHolePos = unity_ObjectToWorld._m03_m13_m23;
                float3 cameraToPointDir = normalize(posWS - _WorldSpaceCameraPos);
                float3 pointToBlackHoleDir = normalize(blackHolePos - posWS);
                float theta = acos(dot(cameraToPointDir, pointToBlackHoleDir));
                float radius = length(posWS - blackHolePos);
                float stringLength = radius * cos(theta) * 2;
                return _dt / (2 * radius) * stringLength;
            }

            half4 frag(VertexOutput i) : SV_Target
            {
                // float3 posWS = _WorldSpaceCameraPos;
                float3 posWS = i.positionWS;
                float3 viewDir = normalize(i.positionWS - _WorldSpaceCameraPos);
                float3 blackHolePos = unity_ObjectToWorld._m03_m13_m23;
                float3 holeCenterToCamera = _WorldSpaceCameraPos - blackHolePos;
                float3 closestNDC = ComputeNormalizedDeviceCoordinatesWithZ(
                    blackHolePos + holeCenterToCamera * 2.5f, UNITY_MATRIX_VP);
                float closestLinearDepth = Linear01Depth(closestNDC.z, _ZBufferParams);
                float3 accumulationColor = float3(0, 0, 0);
                float3 res = float3(0, 0, 0);
                float dt = _dt;
                UNITY_LOOP
                for (int j = 0; j < 200; j++)
                {
                    float3 posOS = mul(unity_WorldToObject, float4(posWS, 1)).xyz;
                    float gravityLerp = gravityScale(posOS);

                    float3 r = posWS - blackHolePos;
                    float3 a = -_GM / dot(r, r) * normalize(r);
                    viewDir += (a) * dt * gravityLerp;
                    posWS += viewDir * dt;
                    float3 ndc = ComputeNormalizedDeviceCoordinatesWithZ(posWS,UNITY_MATRIX_VP);
                    float sceneDepth = SampleSceneDepth(ndc.xy);
                    float linearStepDepth = Linear01Depth(ndc.z, _ZBufferParams);
                    float linearSceneDepth = Linear01Depth(sceneDepth, _ZBufferParams);

                    float r2 = dot(r, r);
                    accumulationColor += 0.01 / (r2 * r2) * gravityLerp * _AccumulationColorScale;

                    if (linearStepDepth > linearSceneDepth && linearSceneDepth > closestLinearDepth)
                    {
                        break;
                    }

                    if (isEventHorizon(posOS))
                    {
                        return float4(0, 0, 0, 1);
                    }

                    if (IsOnDisk(posOS))
                    {
                        return float4(DiskColor(posOS) + accumulationColor, 1);
                    }
                }
                float3 ndc = ComputeNormalizedDeviceCoordinatesWithZ(posWS,UNITY_MATRIX_VP);
                res = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, ndc.xy);

                return float4(res + accumulationColor, 1);
                // return float4(1,0,0, 1);
            }
            ENDHLSL
        }
    }
}