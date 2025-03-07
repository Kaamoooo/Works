Shader "Custom/InteractiveSnowGround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _DisplacementTex ("Displacement Tex",2D) = "black"{}
        _NormalTex ("Normal Tex",2D) = "bump"{}
        _RoughnessTex ("Roughness Tex",2D) = "white"{}
        _DisplacementScale ("Displacement Scale",Float ) = 0.1
        _TessScale("Tess Scale",Float) = 1
        _EdgeHeight("Edge Height",Float) = 1


    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        ZWrite On
        ZTest LEqual
        ColorMask RGB
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float3 worldPos:TEXCOORD1;
            float3 worldNormal : TEXCOORD2;
            float4 vertex : SV_POSITION;
            float4 tangent : TEXCOORD3;
            float3x3 TBN : TEXCOORD4;
        };

        struct TessControlPoint
        {
            float4 vertex : INTERALTESSPOS;
            float3 worldPos : TEXCOORD1;
            float3 worldNormal : TEXCOORD2;
            float2 uv : TEXCOORD0;
            float4 tangent : TEXCOORD3;
        };

        sampler2D _DifferDepthTexture;
        sampler2D _EdgeTexture;

        sampler2D _MainTex;
        float4 _MainTex_TexelSize;
        sampler2D _DisplacementTex;
        sampler2D _NormalTex;
        sampler2D _RoughnessTex;

        float _EdgeWidth;
        float _SnowHeight;
        float _DisplacementScale;
        float _EdgeHeight;

        float4 _DepthCameraParams;
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0

            #pragma vertex vert
            #pragma hull hs
            #pragma domain ds
            #pragma fragment frag
            #pragma geometry geo


            float4 _MainTex_ST;
            float _TessScale;


            TessControlPoint vert(appdata v)
            {
                TessControlPoint o;
                o.vertex = v.vertex;
                o.worldPos = TransformObjectToWorld(v.vertex);
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.tangent = v.tangent;
                return o;
            }

            struct TessFactors
            {
                float tessFactors[3] : SV_TESSFACTOR;
                float insideTessFactor : SV_INSIDETESSFACTOR;
            };

            TessFactors ConstantHullShader(InputPatch<TessControlPoint, 3> patch, uint patchID : SV_PrimitiveID)
            {
                TessFactors o;

                float sum = 0;
                for (uint j = 0; j < 3; j++)
                {
                    // float l = length(patch[j].worldPos - patch[(j + 1) % 3].worldPos);
                    // float d = length(_WorldSpaceCameraPos - (patch[j].worldPos + patch[(j + 1) % 3].worldPos) / 2);
                    // o.tessFactors[j] = _TessScale * l / d;
                    // sum += o.tessFactors[j];

                    o.tessFactors[j] = _TessScale;
                }
                o.insideTessFactor = _TessScale;
                return o;
            }

            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(3)]
            [patchconstantfunc("ConstantHullShader")]
            [maxtessfactor(32.0)]
            TessControlPoint hs(InputPatch<TessControlPoint, 3> patch, uint pointID : SV_OutputControlPointID,
                                uint patchID : SV_PrimitiveID)
            {
                TessControlPoint o;
                o.vertex = patch[pointID].vertex;
                o.uv = patch[pointID].uv;
                o.worldPos = patch[pointID].worldPos;
                o.worldNormal = patch[pointID].worldNormal;
                o.tangent = patch[pointID].tangent;
                return o;
            }

            #define BARYCENTRIC_INTERPOLATE(bary,a,b,c) ((a)*(bary.x) + (b)*(bary.y) + (c)*(bary.z))


            [domain("tri")]
            v2f ds(TessFactors factors, OutputPatch<TessControlPoint, 3> patch, float3 bary:SV_DomainLocation)
            {
                v2f o;
                o.vertex = BARYCENTRIC_INTERPOLATE(bary, patch[0].vertex, patch[1].vertex, patch[2].vertex);
                o.worldPos = BARYCENTRIC_INTERPOLATE(bary, patch[0].worldPos, patch[1].worldPos, patch[2].worldPos);
                o.uv = BARYCENTRIC_INTERPOLATE(bary, patch[0].uv, patch[1].uv, patch[2].uv);
                o.worldNormal = BARYCENTRIC_INTERPOLATE(bary, patch[0].worldNormal, patch[1].worldNormal,
                                                        patch[2].worldNormal);
                o.tangent = BARYCENTRIC_INTERPOLATE(bary, patch[0].tangent, patch[1].tangent,
                                                    patch[2].tangent);
                return o;
            }

            [maxvertexcount(3)]
            void geo(triangle v2f i[3], inout TriangleStream<v2f> stream)
            {
                v2f o[3];
                for (int j = 0; j < 3; j++)
                {
                    o[j].uv = i[j].uv;
                    float height = tex2Dlod(_DisplacementTex, float4(1 - i[j].uv.x, i[j].uv.y, 0, 0)).x *
                        _DisplacementScale;
                    float depth = tex2Dlod(_DifferDepthTexture, float4(1 - i[j].uv.x, i[j].uv.y, 0, 0)).x /
                        _DepthCameraParams.w;
                    float edge = tex2Dlod(_EdgeTexture, float4(1 - i[j].uv.x, i[j].uv.y, 0, 0)).x;
                    depth = depth > 0 ? depth : 0;
                    int depthMask = depth > 0 ? 1 : 0;
                    float4 objPos = i[j].vertex + float4(0, -1, 0, 0) * (depth * saturate(1 - edge)) 
                        + float4(0, 1, 0, 0) * (height + saturate(edge) * _EdgeHeight);
                    o[j].worldPos = TransformObjectToWorld(objPos);
                    o[j].vertex = TransformObjectToHClip(objPos);
                    o[j].worldNormal = normalize(i[j].worldNormal);
                    o[j].tangent = i[j].tangent;

                    float3 T = normalize(TransformObjectToWorld(i[j].tangent.xyz));
                    float3 B = normalize(cross(T, o[j].worldNormal.xyz)) * i[j].tangent.w;
                    o[j].TBN = transpose(float3x3(T, B, o[j].worldNormal));
                    stream.Append(o[j]);
                }
                stream.RestartStrip();
            }


            real4 frag(v2f i) : SV_Target
            {
                float3 sampleNormal = UnpackNormal(tex2D(_NormalTex, float2(1 - i.uv.x, i.uv.y)));
                float3 N = normalize(mul(i.TBN, sampleNormal));
                float3 V = GetWorldSpaceViewDir(i.worldPos);

                real3 col = 0;
                Light mainLight = GetMainLight();
                float3 L = mainLight.direction;
                float3 H = normalize(V + L);
                float diffuse = pow(max(0, dot(N, L)), 0.3) * 0.7;
                float specular = pow(max(0, dot(N, H)), 15);
                float ambient = 0.04;
                float roughness = tex2D(_RoughnessTex, float2(1 - i.uv.x, i.uv.y)).x;

                col += ambient;

                col += (diffuse + specular) * mainLight.distanceAttenuation * mainLight.color;


                int lightsCount = GetAdditionalLightsCount();
                for (int j = 0; j < lightsCount; j++)
                {
                    Light light = GetAdditionalLight(j, i.worldPos);
                    L = light.direction;
                    H = normalize(V + L);
                    diffuse = pow(max(0, dot(N, L)), 0.3) * 0.7;
                    specular = pow(max(0, dot(N, H)), 15) * roughness;

                    col += (diffuse + specular) * light.shadowAttenuation * light.distanceAttenuation * light.color;
                }
                // return tex2D(_EdgeTexture, float2(1 - i.uv.x, i.uv.y));
                return col.xyzz * tex2D(_MainTex, float2(1 - i.uv.x, i.uv.y));
            }
            ENDHLSL
        }
    }
}