Shader "Custom/VolumetricLight"
{
    Properties
    {
        _MaxStepCount("Max Step Count", Range(10, 200)) = 20
        _ColorIntensity("Color Intensity", Range(0, 1)) = 1
        _AlphaIntensity("Alpha Intensity", Range(0, 1)) = 1
        _JitterScale("Jitter Scale", Range(0, 0.1)) = 0
        _BlendFactor("Blend Factor", Range(0, 1)) = 1
    }
    SubShader
    {

        Tags
        {
            "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        }
        Pass
        {

            ZWrite Off
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha

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
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = v.uv;
                o.worldPos = TransformObjectToWorld(v.vertex);
                return o;
            }

            sampler2D _CameraDepthTexture;
            TEXTURE2D(_SpotLightDepthTexture);
            SAMPLER(sampler_SpotLightDepthTexture);
            TEXTURE2D(_CameraAllColorTexture);
            SAMPLER(sampler_CameraAllColorTexture);
            TEXTURE2D(_VolumetricLightColorTexture);
            SAMPLER(sampler_VolumetricLightColorTexture);
            TEXTURE2D(_LastFrameColorTexture);
            SAMPLER(sampler_LastFrameColorTexture);
            TEXTURE2D(_LastFrameDepthTexture);
            SAMPLER(sampler_LastFrameDepthTexture);
            
            float3 _WorldSpaceSpotLightPos;
            float3 _ForwardDirection;
            float3 _RightDirection;
            float3 _SpotLightColor;
            float _SpotLightOuterAngle;
            float _SpotLightCameraNearPlane;
            float _SpotLightCameraFarPlane;
            float _OrthographicCameraSize;
            float4x4 _LastFrameWorldToCameraMatrix;
            float4x4 _SpotLightCameraViewProjectionMatrix;
            
            float _MaxStepCount;
            float _ColorIntensity;
            float _AlphaIntensity;
            float _JitterScale;
            float _BlendFactor;
            
            float4 frag(v2f i) : SV_Target
            {
                // return float4(normalize(_WorldSpaceCameraPos), 0.5);
                float3 upDirection = normalize(cross(_ForwardDirection, _RightDirection));
                float3 stepDirection = normalize(i.worldPos - _WorldSpaceCameraPos);
                float stepLength =max((_SpotLightCameraFarPlane - _SpotLightCameraNearPlane) / _MaxStepCount ,
                    _OrthographicCameraSize * 2  / _MaxStepCount);
                float3 currentPosition = i.worldPos;
                float3 color = 0;
                float steps = 0;
                float validSteps = 0;
                UNITY_LOOP
                for(; steps < _MaxStepCount ; steps++)
                {
                    float jitter = frac((_Time.x + steps * 12.9898 + 78.233) * 43758.5453) * _JitterScale;
                    currentPosition += stepDirection * (stepLength + jitter);
                    float3 a = normalize(currentPosition - _WorldSpaceSpotLightPos);
                    float angle = acos(dot(a, _ForwardDirection)) * 180 / PI;
                    if(angle > _SpotLightOuterAngle * 0.5) break;
                    
                    float2 screenUV = ComputeNormalizedDeviceCoordinatesWithZ(currentPosition, UNITY_MATRIX_VP).xy;
                    float cameraDepth = tex2D(_CameraDepthTexture, screenUV).r;
                    float cameraEyeDepth = LinearEyeDepth(cameraDepth, _ZBufferParams);
                    if(cameraEyeDepth < distance(currentPosition, _WorldSpaceCameraPos)) break;

                    // float3 spotLightToPixel = currentPosition - _WorldSpaceSpotLightPos;
                    // float2 spotLightUV = float2(dot(spotLightToPixel, _RightDirection),dot(spotLightToPixel, upDirection));
                    // spotLightUV = (spotLightUV + _OrthographicCameraSize) / (_OrthographicCameraSize * 2);
                    float4 curStepSpotLightCameraProjPos = mul( _SpotLightCameraViewProjectionMatrix , float4(currentPosition, 1));
                    float3 curStepSpotLightCameraNDCPos = curStepSpotLightCameraProjPos.xyz / curStepSpotLightCameraProjPos.w;
                    float2 spotLightUV = curStepSpotLightCameraNDCPos.xy * 0.5 + 0.5;
                    
                    float spotLightDepth = SAMPLE_TEXTURE2D(_SpotLightDepthTexture, sampler_SpotLightDepthTexture, spotLightUV);
                    float4 _SpotLightCameraZBufferParam =
                        float4((_SpotLightCameraFarPlane - _SpotLightCameraNearPlane) / _SpotLightCameraNearPlane,
                                1,
                                (_SpotLightCameraFarPlane - _SpotLightCameraNearPlane) / ( _SpotLightCameraNearPlane * _SpotLightCameraFarPlane),
                                1 / _SpotLightCameraFarPlane);
                    float spotLightEyeDepth = LinearEyeDepth(spotLightDepth, _SpotLightCameraZBufferParam);
                      
                    float3 spotLightToCurrentPosition = currentPosition - _WorldSpaceSpotLightPos;
                    float spotLightToCurrentPositionDistance = dot(spotLightToCurrentPosition, _ForwardDirection);
                    if(spotLightEyeDepth < spotLightToCurrentPositionDistance) continue;

                    validSteps ++;
                    color += _SpotLightColor;
                    
                }
                float3 finalColor = color * _ColorIntensity;
                float alpha = validSteps / _MaxStepCount * _AlphaIntensity;
                return float4(finalColor, alpha);
                
                float2 lastFrameScreenUV = ComputeNormalizedDeviceCoordinatesWithZ(i.worldPos,  mul(UNITY_MATRIX_P , _LastFrameWorldToCameraMatrix)).xy;
                float lastFrameNearestDepth = SAMPLE_TEXTURE2D(_LastFrameDepthTexture,sampler_LastFrameDepthTexture,lastFrameScreenUV);
                float lastFrameNearestEyeDepth = LinearEyeDepth(lastFrameNearestDepth, _ZBufferParams);
                float lastFrameCurrentPositionEyeDepth = -mul(_LastFrameWorldToCameraMatrix, float4(i.worldPos, 1)).z;
                if(abs(lastFrameNearestEyeDepth - lastFrameCurrentPositionEyeDepth) < 0.001 && all(lastFrameScreenUV > 0 && lastFrameScreenUV < 1))
                {
                    float4 lastFrameColor = SAMPLE_TEXTURE2D(_LastFrameColorTexture, sampler_LastFrameColorTexture, lastFrameScreenUV);
                    finalColor = finalColor * (1 - _BlendFactor) + lastFrameColor.xyz / alpha * _BlendFactor;
                }

                return float4(finalColor, alpha);
 
            }
            ENDHLSL
        }
    }
}