Shader "Custom/VolumetricCloud"
{
    Properties
    {
        _DensityPower("Density Power", Range(0, 32)) = 2
        _DensityScale("Density Scale",Float) = 1
        _DensityThreshold("Density Threshold",Range(0,1)) = 0.1
        _StepSize ("Step Size", Float) = 0.1
        _LightStepSize("Light Step Size",Float) = 0.1

        _LightScale("Light Scale",Float) = 0.5
        _LightAttenuation("Light Attenuation",Float) = 1

        _ReflectionRatioExponent("Reflection Ratio Exponent",Float) = 0.1
        _ReflectionRatioScale("Reflection Ratio Scale",Float) = 0.1

        _ReflectionTransmissionAttenuationExponent("Reflection Transmission Attenuation Exponent",Float) = 1
        _ReflectionTransmissionScale("Reflection Transmission Scale",Float) = 0.5

        _VisibilityExponent("Visibility Exponent",Float) = 1
        _VisibilityScale("Visibility Scale",Float) = 0.5

        _CloudSpeed("Cloud Speed",Range(0,1)) = 0.1

        _Noise3D ("Noise3D", 3D) = "white" {}
        _Noise2D ("Noise2D", 2D) = "Black" {}
        _NoiseOffsetScale ("NoiseOffsetScale", Float) = 1

        _BlurUVScale("Blur UV Scale",Float) = 0.1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        ENDHLSL
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            sampler2D _Noise2D;
            float _NoiseOffsetScale;
            sampler3D _Noise3D;
            float3 _CloudPosition;
            float3 _CloudScale;
            float _DensityPower;
            float _DensityScale;
            float _DensityThreshold;
            float _StepSize;
            float _LightScale;
            float _LightStepSize;
            float _LightAttenuation;
            float _ReflectionRatioExponent;
            float _ReflectionRatioScale;
            float _ReflectionTransmissionAttenuationExponent;
            float _ReflectionTransmissionScale;
            float _CloudSpeed;
            float _VisibilityExponent;
            float _VisibilityScale;

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            bool isInCloud(float3 tmpPoint)
            {
                return
                    tmpPoint.x < _CloudPosition.x + _CloudScale.x / 2 && tmpPoint.x > _CloudPosition.x - _CloudScale.x /
                    2
                    &&
                    tmpPoint.y < _CloudPosition.y + _CloudScale.y / 2 && tmpPoint.y > _CloudPosition.y - _CloudScale.y /
                    2
                    &&
                    tmpPoint.z < _CloudPosition.z + _CloudScale.z / 2 && tmpPoint.z > _CloudPosition.z - _CloudScale.z /
                    2;
            }

            float4 CloudRayMarching(float3 origin, float3 dir, float3 mainLightDir, float4 baseColor)
            {
                float sum = 0;
                float3 tmpPoint = origin;
                dir = normalize(dir);
                dir *= _StepSize;
                float3 lightDir = normalize(mainLightDir);
                lightDir *= _LightStepSize;
                float totalDensity = 0;
                for (int i = 0; i < 16; i++)
                {
                    float viewDirMarchOffset = tex2D(_Noise2D, tmpPoint.xz)*_NoiseOffsetScale;
                    tmpPoint += dir+viewDirMarchOffset;
                    if (isInCloud(tmpPoint))
                    {
                        float3 uv = float3(tmpPoint.x / _CloudScale.x,
                                           tmpPoint.y / _CloudScale.y * 2,
                                           tmpPoint.z / _CloudScale.z);
                        uv.x += _Time.x * _CloudSpeed;
                        // uv.z += _Time.x * _CloudSpeed;
                        float density = pow(tex3D(_Noise3D, uv).r, _DensityPower);
                        density *= _DensityScale;
                        totalDensity += density;

                        if (density < _DensityThreshold)
                        {
                            continue;
                        }

                        float tmpTotalDensity = 0;
                        float3 tmpLightPos = tmpPoint;
                        for (int j = 0; j < 16; j++)
                        {

                            float lightDirOffset = tex2D(_Noise2D, tmpLightPos.xz)*_NoiseOffsetScale;
                            tmpLightPos += lightDir+lightDirOffset;
                            
                            if (isInCloud(tmpLightPos))
                            {
                                float tmpDensity = pow(tex3D(_Noise3D, float3(tmpLightPos.x / _CloudScale.x,
                                                                              tmpLightPos.y / _CloudScale.y *2,
                                                                              tmpLightPos.z / _CloudScale.z)).r,
                                                       _DensityPower);
                                tmpDensity = tmpDensity * _DensityScale;
                                tmpTotalDensity += tmpDensity;
                            }
                            else
                            {
                                break;
                            }
                        }
                        float lightEnergy = exp(-tmpTotalDensity / 32 * _LightAttenuation);
                        lightEnergy *= _LightScale;

                        float reflectionRatio = exp(-32 / density / _ReflectionRatioExponent);
                        reflectionRatio *= _ReflectionRatioScale;

                        float reflectionTransmission = exp(
                            -totalDensity * _ReflectionTransmissionAttenuationExponent);
                        reflectionTransmission *= _ReflectionTransmissionScale;

                        sum += saturate(lightEnergy * reflectionRatio * reflectionTransmission);
                    }
                    else
                    {
                        break;
                    }
                }
                if (totalDensity == 0)
                {
                    return baseColor;
                }
                else
                {
                    float visibility = exp(-totalDensity * _VisibilityExponent) * _VisibilityScale;
                    visibility = visibility > 1 ? 1 : visibility;
                    return sum + visibility * baseColor;
                }
            }

            real4 frag(v2f i) : SV_Target
            {
                float depth = tex2D(_CameraDepthTexture, i.uv);
                depth = Linear01Depth(depth, _ZBufferParams);
                float3 viewPos = i.viewPos * depth;
                float3 worldPos = mul(unity_CameraToWorld, float4(viewPos.xy, viewPos.z * -1, 1));
                float4 baseColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, i.uv);
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 dir = normalize(worldPos - cameraPos);

                Light mainLight = GetMainLight();

                float4 cloud = saturate(CloudRayMarching(worldPos, dir, mainLight.direction, baseColor));
                return cloud;
                // return baseColor.xyzz + cloud;
            }
            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float _BlurUVScale;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.texcoord = GetFullScreenTriangleTexCoord(v.vertexID);
                return o;
            }

            float GaussianWeight(float d, float sigma)
            {
                return 1.0 / (sigma * sqrt(2 * PI)) * exp(-(d * d) / (2.0 * sigma * sigma));
            }

            float4 GaussianWeight(float4 d, float sigma)
            {
                return 1.0 / (sigma * sqrt(2 * PI)) * exp(-(d * d) / (2.0 * sigma * sigma));
            }

            float BilateralWeight(float2 currentUV, float2 centerUV, float4 currentColor, float4 centerColor)
            {
                float spacialDifference = length(centerUV - currentUV);
                float4 tonalDifference = centerColor - currentColor;
                return GaussianWeight(spacialDifference, 10) * GaussianWeight(tonalDifference, 0.1);
            }

            //frag shader
            float4 frag(Varyings i) : SV_Target
            {
                int2 deltaUV[9] = {
                    int2(-1, 1), int2(0, 1), int2(1, 1),
                    int2(-1, 0), int2(0, 0), int2(1, 0),
                    int2(-1, -1), int2(0, -1), int2(1, -1)
                };
                float4 baseColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, i.texcoord);
                float4 centerColor = baseColor;
                float4 res = 0;
                float weightSum = 0;
                for (int iii = 0; iii < 9; iii++)
                {
                    float2 currentUV = i.texcoord + deltaUV[iii] * _BlurUVScale;
                    float4 currentColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, currentUV);
                    float weight = BilateralWeight(currentUV, i.texcoord, currentColor, centerColor);
                    weightSum += weight;
                    res += weight * currentColor;
                }
                return res / weightSum;
            }
            ENDHLSL

        }

    }
}