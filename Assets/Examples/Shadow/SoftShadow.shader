Shader "Unlit/SoftShadow"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _SpecularColor ("Specular Color", Color) = (1, 1, 1, 1)
        _Gloss ("Gloss", Range(8, 256)) = 20
        _BaseMap ("Base Texture", 2D) = "white" {}
        _SoftShadowRange ("Soft Shadow Range", Float) = 1
        _LightWidth ("Light Width", Float) = 0.5
        
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _SpecularColor;
                float _Gloss;
                float4 _BaseMap_ST;
                float _LightWidth;
                float _SoftShadowRange;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 CalculateLight(Light light,float3 normalWS, float3 viewDir, half3 albedo)
            {
                float3 lightDir = normalize(light.direction);

                half NdotL = saturate(dot(normalWS, lightDir));
                half3 diffuse = light.color * albedo * NdotL;

                float3 halfDir = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half3 specular = light.color * _SpecularColor.rgb * pow(NdotH, _Gloss);

                return half4(diffuse + specular, 1.0) * light.distanceAttenuation;
            }

            float DefaultShadow(float3 posWS, int index = -1)
            {
                float4 centerShadowCoord;
                if(index == -1)
                {
                    centerShadowCoord = mul(_MainLightWorldToShadow[0], float4(posWS, 1.0));
                    centerShadowCoord.xyz /= centerShadowCoord.w;
                }else
                {
                    centerShadowCoord = mul(_AdditionalLightsWorldToShadow[index], float4(posWS, 1.0));
                    centerShadowCoord.xyz /= centerShadowCoord.w;
                }
                
                float4 shadowCoord = float4(centerShadowCoord.xy,centerShadowCoord.zw);
                float shadow;
                if(index == -1) {
                    shadow = SAMPLE_TEXTURE2D_SHADOW(
                        _MainLightShadowmapTexture, 
                        sampler_MainLightShadowmapTexture, 
                        shadowCoord.xyz
                    );
                }else{
                    shadow = SAMPLE_TEXTURE2D_SHADOW(
                        _AdditionalLightsShadowmapTexture, 
                        sampler_AdditionalLightsShadowmapTexture, 
                        shadowCoord.xyz
                    );
                }
                return shadow;
            }

            float PCF(float3 posWS, int index = -1)
            {
                float4 centerShadowCoord;
                float2 unitSize;
                float2 shadowOffsets[9] = {float2(-1, -1), float2(0, -1), float2(1, -1), float2(-1, 0), float2(0, 0), float2(1, 0), float2(-1, 1), float2(0, 1), float2(1, 1)};
                float res = 0;
                if(index == -1)
                {
                    centerShadowCoord = mul(_MainLightWorldToShadow[0], float4(posWS, 1.0));
                    centerShadowCoord.xyz /= centerShadowCoord.w;
                    unitSize = _MainLightShadowmapSize.xy;
                }else
                {
                    centerShadowCoord = mul(_AdditionalLightsWorldToShadow[index], float4(posWS, 1.0));
                    centerShadowCoord.xyz /= centerShadowCoord.w;
                    unitSize = _AdditionalShadowmapSize.xy;
                }

                unitSize *= _SoftShadowRange;
                
                for(int i = 0 ; i < 9 ; i++)
                {
                    float2 offset = shadowOffsets[i];
                    float4 shadowCoord = float4(centerShadowCoord.xy + offset * unitSize,centerShadowCoord.zw);
                    float shadow;
                    if(index == -1) {
                        shadow = SAMPLE_TEXTURE2D_SHADOW(
                            _MainLightShadowmapTexture, 
                            sampler_MainLightShadowmapTexture, 
                            shadowCoord.xyz
                        );
                    }else{
                        shadow = SAMPLE_TEXTURE2D_SHADOW(
                            _AdditionalLightsShadowmapTexture, 
                            sampler_AdditionalLightsShadowmapTexture, 
                            shadowCoord.xyz
                        );
                    }
                    res += shadow;
                }
                return res / 9;
            }

            float PCSS(float3 posWS, int index = -1)
            {
                if(index == -1) return PCF(posWS,-1);
                
                float2 unitSize;
                float2 shadowOffsets[9] = {float2(-1, -1), float2(0, -1), float2(1, -1), float2(-1, 0), float2(0, 0), float2(1, 0), float2(-1, 1), float2(0, 1), float2(1, 1)};
                float res = 0;
                
                float4 centerShadowCoord = mul(_AdditionalLightsWorldToShadow[index], float4(posWS, 1.0));
                centerShadowCoord.xyz /= centerShadowCoord.w;

                float4 centerCasterCoord = centerShadowCoord;
                float _shadowDepth = SAMPLE_TEXTURE2D(_AdditionalLightsShadowmapTexture, 
                    sampler_LinearClamp, 
                    centerCasterCoord.xy);
                centerCasterCoord.z = _shadowDepth;
                centerCasterCoord.w = 1;
                float4 _casterWorldPos = mul(Inverse(_AdditionalLightsWorldToShadow[index]), centerCasterCoord);
                _casterWorldPos.xyz /= _casterWorldPos.w;
                
                float d_receiver = distance(posWS, _AdditionalLightsPosition[index]);
                float d_caster = distance(_casterWorldPos.xyz,_AdditionalLightsPosition[index]);
                float w_penumbra = _LightWidth * (d_receiver - d_caster) / d_caster;

                unitSize = _AdditionalShadowmapSize.xy * _SoftShadowRange * (w_penumbra + 1.0f);
                // unitSize = _AdditionalShadowmapSize.xy * _SoftShadowRange;
                // unitSize = 1 / (_AdditionalShadowmapSize.zw + w_penumbra) * _SoftShadowRange;
                // unitSize = 1.0f / (_AdditionalShadowmapSize.zw );
                for(int i = 0 ; i < 9 ; i++)
                {
                    float2 offset = shadowOffsets[i];
                    float4 shadowCoord = float4(centerShadowCoord.xy + offset * unitSize,centerShadowCoord.zw);
                    float shadow = SAMPLE_TEXTURE2D_SHADOW(
                        _AdditionalLightsShadowmapTexture, 
                        sampler_AdditionalLightsShadowmapTexture, 
                        shadowCoord.xyz
                    );
                    res += shadow;
                }
                return res / 9;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                half4 res = 0;
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.positionWS);
                half3 ambient = SampleSH(normalWS) * albedo;

                half4 mainLightColor = CalculateLight(GetMainLight(), normalWS, viewDir, albedo);
                mainLightColor.rgb *= PCSS(IN.positionWS,-1);
                res += mainLightColor;

                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint i = 0; i < pixelLightCount; ++i)
                {
                    Light _additionalLight = GetAdditionalLight(i,IN.positionWS);
                    half4 lightColor = CalculateLight(_additionalLight, normalWS, viewDir, albedo);
                    lightColor.rgb *= DefaultShadow(IN.positionWS,i);
                    // lightColor.rgb *= PCF(IN.positionWS,i);
                    // lightColor.rgb *= PCSS(IN.positionWS,i);
                    res += lightColor;
                }
                
                return half4(res.xyz + ambient,1);

            }
            ENDHLSL
        }
    }
}