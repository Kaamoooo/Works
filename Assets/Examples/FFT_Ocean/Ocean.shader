Shader "Unlit/Ocean"
{
    Properties
    {
        _MainColor ("Color", Color) = (1,1,1,1)
        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _Specular("Specular", Range(0, 1)) = 0.5
        _Glossiness("Glossiness", Range(0, 1)) = 0.5
        _FresnelScale("Frenel Scale", Range(0, 1)) = 0.5
        _AmbientStrength("_AmbientStrength", Range(0, 1)) = 0.5
        _FoamHeight("Foam Height",Float) = 0.1
        _HeightFoamScale("Height Foam Scale",Float) = 1
        _EdgeThreshold("Edge Threshold",Float) = 0.1
        _EdgeScale("Edge Scale", Float) = 1
        _DepthDifferenceThreshold("Depth Difference Threshold", Float) = 0.1
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
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma hull HullProgram
            #pragma domain DomainProgram
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct ControlPoint
            {
                float4 vertex : INTERNALTESSPOS;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 modelPos : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _MainColor;
            float4 _FoamColor;
            float _Specular;
            float _Glossiness;
            float _FresnelScale;
            float _AmbientStrength;

            float _FoamHeight;
            float _HeightFoamScale;
            float _EdgeThreshold;
            float _EdgeScale;
            float _DepthDifferenceThreshold;

            sampler2D _TestTexture;
            sampler2D _DisplacementRT;
            sampler2D _NormalRT;
            sampler2D _BubbleRT;

            TEXTURE2D(_CameraColorTexture);
            SAMPLER(sampler_CameraColorTexture);

            TEXTURE2D(_CameraDepthTextureWithLOD);
            SAMPLER(sampler_CameraDepthTextureWithLOD);

            ControlPoint vert(appdata v)
            {
                ControlPoint o;
                o.vertex = v.vertex;
                o.uv = v.uv;
                return o;
            }

            struct TessFactor
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            TessFactor PatchConstant(InputPatch<ControlPoint, 3> patch)
            {
                TessFactor f;
                f.edge[0] = 16;
                f.edge[1] = 16;
                f.edge[2] = 16;
                f.inside = 16;
                return f;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("fractional_odd")]
            [patchconstantfunc("PatchConstant")]
            ControlPoint HullProgram(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
            {
                return patch[id];
            }

            [domain("tri")]
            v2f DomainProgram(TessFactor factor, OutputPatch<ControlPoint, 3> patch,
                              float3 barycentricCoordinates :SV_DomainLocation)
            {
                v2f o;
                #define DomainInterpolate(fieldName) o.fieldName = \
                        patch[0].fieldName * barycentricCoordinates.x + \
                        patch[1].fieldName * barycentricCoordinates.y + \
                        patch[2].fieldName * barycentricCoordinates.z;

                DomainInterpolate(vertex)
                DomainInterpolate(uv)

                float3 displacement = tex2Dlod(_DisplacementRT, float4(o.uv, 0, 0)).xyz;
                float3 worldPos = TransformObjectToWorld(o.vertex + displacement);
                o.worldPos = worldPos;
                o.modelPos = o.vertex;
                o.vertex = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 worldNormal = tex2D(_NormalRT, i.uv).xyz;
                Light mainLight = GetMainLight();
                float3 lightNormal = normalize(mainLight.direction);
                half3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 reflectDir = normalize(reflect(-viewDir, worldNormal));
                float3 ssrReflectDir = normalize(reflect(-viewDir, lerp(float3(0, 1, 0),worldNormal,0.2)));
                half3 halfVec = normalize(lightNormal + viewDir);
                
                float3 waterDiffuse = mainLight.color * _MainColor.xyz * saturate(dot(lightNormal, worldNormal));
                float3 foamDiffuse = mainLight.color * _FoamColor.xyz * saturate(dot(lightNormal, worldNormal));
                float collisionFoamValue = tex2D(_BubbleRT, i.uv).x;
                float heightFoamValue = saturate(i.worldPos.y - _FoamHeight) * _HeightFoamScale;
                float3 diffuse = waterDiffuse + foamDiffuse * (collisionFoamValue + heightFoamValue);
                float fresnel = saturate(_FresnelScale + (1 - _FresnelScale) * pow(1 - dot(worldNormal, viewDir), 5));
                half4 encodedReflection = SAMPLE_TEXTURECUBE(
                    unity_SpecCube0,
                    samplerunity_SpecCube0,
                    reflectDir
                );
                float3 hdrReflectionColor = _AmbientStrength *
                    DecodeHDREnvironment(encodedReflection, unity_SpecCube0_HDR);
                diffuse = lerp(diffuse,hdrReflectionColor, fresnel);

                half3 specular = saturate(
                    mainLight.color * pow(saturate(dot(halfVec, worldNormal)), _Specular * 128.0f) * _Glossiness);

                float2 screenUV = i.vertex.xy / _ScreenParams.xy;
                float depth = SampleSceneDepth(i.vertex.xy / _ScreenParams.xy);
                float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
                float opaqueDistanceToCamera = length(worldPos - _WorldSpaceCameraPos);
                float waterDistanceToCamera = length(i.worldPos - _WorldSpaceCameraPos);
                float waterEdgeValue = abs(
                    min(_EdgeThreshold, opaqueDistanceToCamera - waterDistanceToCamera) - _EdgeThreshold) * _EdgeScale;
                float3 waterEdgeColor = _FoamColor * waterEdgeValue;
                float4 col = (specular + diffuse + hdrReflectionColor + waterEdgeColor).xyzz;

                const float STEP_SIZE = 0.2f;
                const int MAX_STEP_TIMES = 50;

                float2 reflectUV = float2(0, 0);
                bool reflected = false;
                float3 stepWorldPos = i.worldPos;
                int currentDepthLOD = 0;
                UNITY_LOOP
                for (int j = 1; j <= MAX_STEP_TIMES; j++)
                {
                    float3 _stepVector = ssrReflectDir * STEP_SIZE * pow(2, currentDepthLOD);
                    stepWorldPos += _stepVector;
                    float3 _NDC = ComputeNormalizedDeviceCoordinatesWithZ(stepWorldPos,UNITY_MATRIX_VP);
                    bool _isInScreen = _NDC.x >= -1 && _NDC.x <= 1 && _NDC.y >= -1 && _NDC.y <= 1;
                    if (!_isInScreen)
                    {
                        break;
                    }
                    float _depth = _NDC.z;
                    float _opaqueDepth = SAMPLE_TEXTURE2D_LOD(_CameraDepthTextureWithLOD,
               sampler_CameraDepthTextureWithLOD, _NDC.xy,
               currentDepthLOD);
                    float _eyeDepth = LinearEyeDepth(_depth, _ZBufferParams);
                    float _opaqueEyeDepth = LinearEyeDepth(_opaqueDepth, _ZBufferParams);
                    float _depthDiff = _eyeDepth - _opaqueEyeDepth;
                    if (_eyeDepth > _opaqueEyeDepth && _depthDiff < _DepthDifferenceThreshold)
                    {
                        reflected = true;
                        if (currentDepthLOD != 0)
                        {
                            currentDepthLOD--;
                            stepWorldPos -= _stepVector;
                            reflectUV = _NDC.xy;
                        }
                        else
                        {
                            reflectUV = _NDC.xy;
                            break;
                        }
                    }
                    else
                    {
                        currentDepthLOD++;
                    }
                }
                float3 reflectedColor = float3(0, 0, 0);
                if (reflected)
                {
                    reflectedColor = SAMPLE_TEXTURE2D(_CameraColorTexture, sampler_CameraColorTexture, reflectUV);
                    // reflectedColor = float3(1, 1, 1);
                }
                // return float4(col.xyz, 1);
                // return float4(0,0,0,1);

                // return float4(reflectedColor, 1);
                return float4(col.xyz + reflectedColor, _MainColor.a);
                // return float4(col.xyz + reflectedColor, 1);
            }
            ENDHLSL
        }
    }
}