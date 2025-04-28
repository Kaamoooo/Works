Shader "Custom/VolumetricLightBlend"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry"
        }
        LOD 100
        ZWrite Off Cull Off
        
        Pass
        {
            Name "TAABlendPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_VolumetricLightWorldPosTexture);
            SAMPLER(sampler_VolumetricLightWorldPosTexture);
            
            TEXTURE2D(_LastFrameColorTexture);
            SAMPLER(sampler_LastFrameColorTexture);

            float _BlendFactor;
            float4x4 _LastFrameVPMatrix;
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 finalColor = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_PointClamp, input.texcoord, 0);
                float4 worldPos = SAMPLE_TEXTURE2D(_VolumetricLightWorldPosTexture, sampler_VolumetricLightWorldPosTexture, input.texcoord);
                float2 lastFrameScreenUV = ComputeNormalizedDeviceCoordinatesWithZ(worldPos,  _LastFrameVPMatrix).xy;
                lastFrameScreenUV.y = 1 - lastFrameScreenUV.y;
                if(worldPos.a < 0.999 && all(lastFrameScreenUV > 0 && lastFrameScreenUV < 1))
                {
                    float4 lastFrameColor = SAMPLE_TEXTURE2D(_LastFrameColorTexture, sampler_LastFrameColorTexture, lastFrameScreenUV);
                    finalColor = finalColor * (1 - _BlendFactor) + lastFrameColor.xyz * _BlendFactor;
                }

                return float4(finalColor, 1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ColorBlitPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_VolumetricLightColorTexture);
            SAMPLER(sampler_VolumetricLightColorTexture);
            TEXTURE2D(_VolumetricLightWorldPosTexture);
            SAMPLER(sampler_VolumetricLightWorldPosTexture);

            float4x4 _VPMatrix;

            half4 frag(Varyings input) : SV_Target
            {
                float4 sceneColor = SAMPLE_TEXTURE2D_LOD(_BlitTexture, sampler_PointClamp, input.texcoord, 0);
                float4 volumetricLightColor = SAMPLE_TEXTURE2D(_VolumetricLightColorTexture,sampler_VolumetricLightColorTexture, input.texcoord);

                float sceneDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord).r;
                float4 volumetricWorldPos = SAMPLE_TEXTURE2D(_VolumetricLightWorldPosTexture, sampler_VolumetricLightWorldPosTexture, input.texcoord);
                float volumetricLightDepth = volumetricWorldPos.a;
                
                if(volumetricLightDepth > sceneDepth)
                {
                    return sceneColor + volumetricLightColor;
                }

                return sceneColor;
                
                // return float4(color.xyz, 1);
            }
            ENDHLSL
        }
    }
}