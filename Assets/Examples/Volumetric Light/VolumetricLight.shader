Shader "Custom/VolumetricLight/VolumetricLight"
{
    Properties
    {
        _StepLength("Step Length", Range(0.0001, 0.5)) = 0.02
        _MaxLightness("Max Lightness", Range(0, 1)) = 0.7
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
                float4 vertex : SV_POSITION;
                float3 viewPos : TEXCOORD1;
            };

            v2f vert(Attributes v)
            {
                v2f o;
                o.vertex = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.uv = GetFullScreenTriangleTexCoord(v.vertexID);

                float4 screenPos = ComputeScreenPos(o.vertex); // 0...w
                float4 NDCPos = (screenPos / screenPos.w) * 2 - 1; // -1...1
                float far = _ProjectionParams.z;
                float4 clipPos = float4(NDCPos.xy, 1, 1) * far;
                o.viewPos = mul(unity_CameraInvProjection, clipPos).xyz;
                return o;
            }

            sampler2D _CameraDepthTexture;
            float3 _LightDirection;
            float3 _LightPosition;
            float3 _VolumeCenterPosition;
            float3 _VolumeSize;
            float _StepLength;
            float _MaxLightness;

            sampler2D _LightDepthTexture;
            float4x4 _LightViewProjectionMatrix;

            bool IsInVolume(float3 worldPos)
            {
                float3 localPos = worldPos - _VolumeCenterPosition;
                float3 halfSize = _VolumeSize / 2;
                return abs(localPos.x) < halfSize.x && abs(localPos.y) < halfSize.y && abs(localPos.z) < halfSize.z;
            }

            bool LightDetected(float3 worldPos)
            {
                float4 lightClipPos = mul(_LightViewProjectionMatrix, float4(worldPos, 1));
                float2 lightUV = float2(lightClipPos.x / lightClipPos.w, lightClipPos.y / lightClipPos.w) * 0.5 + 0.5;
                float lightPositionDepth = -1 * lightClipPos.z / lightClipPos.w * 0.5 + 0.5;
                float lightTextureDepth = tex2D(_LightDepthTexture, lightUV).r;
                if (lightTextureDepth == 0 || lightUV.x < 0 || lightUV.x > 1 || lightUV.y < 0 || lightUV.y > 1 ||
                    lightPositionDepth < 0 ||
                    lightPositionDepth > 1)
                {
                    lightTextureDepth = 100;
                }
                return lightPositionDepth >= lightTextureDepth;
            }

            real4 frag(v2f i) : SV_Target
            {
                float depth = tex2D(_CameraDepthTexture, i.uv);
                // return depth;
                // return Linear01Depth(depth, _ZBufferParams);
                float3 viewPos = i.viewPos * Linear01Depth(depth, _ZBufferParams);
                float4 worldPos = mul(unity_CameraToWorld, float4(viewPos.xy, viewPos.z * -1, 1));

                // return worldPos;
                
                float4 baseColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, i.uv);
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 cameraStepForwardDir = normalize(worldPos - cameraPos);
                float4 finalColor = baseColor;
                
                float4 lightClipPos = mul(_LightViewProjectionMatrix, float4(worldPos));
                float2 lightUV = float2(lightClipPos.x / lightClipPos.w, lightClipPos.y / lightClipPos.w) * 0.5 + 0.5;
                float lightPositionDepth = Linear01Depth(-1 * lightClipPos.z / lightClipPos.w * 0.5 + 0.5,_ZBufferParams);
                // return lightPositionDepth;
                float lightTextureDepth = Linear01Depth(tex2D(_LightDepthTexture, lightUV).r,_ZBufferParams);
                return lightTextureDepth;
                // return tex2D(_LightDepthTexture, lightUV).r;

                int stepCount = 64;
                float stepLightness = _MaxLightness / stepCount;
                for (int j = 1; j <= stepCount; j++)
                {
                    float3 samplePos = worldPos - cameraStepForwardDir * _StepLength * j;

                    if (!IsInVolume(samplePos))
                    {
                        break;
                    }
                    else if (!LightDetected(samplePos))
                    {
                        continue;
                    }

                    finalColor += stepLightness;
                }
                return finalColor;
            }
            ENDHLSL
        }
    }
}