#ifndef CUSTOM_LIT_PASS_GPU_INSTANCING_INCLUDED
#define CUSTOM_LIT_PASS_GPU_INSTANCING_INCLUDED

#pragma shader_feature _CLIPPING
#pragma shader_feature _PREMULTIPLY_ALPHA
#pragma multi_compile_instancing

#include "../ShaderLibrary/SurfaceProperties.hlsl"
#include "../ShaderLibrary/Common.GPUInstancing.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

Texture2D _MainTex;
SamplerState sampler_MainTex;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4, _MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
    UNITY_DEFINE_INSTANCED_PROP(float, _Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float, _Smoothness)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 uv: TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varings
{
    float4 positionCS : SV_Position;
    float3 normalWS : VAR_NORMAL_WS;
    float3 positionWS : VAR_POSITION_WS;
    float2 uv : VAR_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varings vertLitPass(Attributes input)
{
    Varings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    float4 mainTexST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.uv = input.uv * mainTexST.xy + mainTexST.zw;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionWS = TransformObjectToWorld(input.positionOS);
    return output;
}

float4 fragLitPass(Varings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 texColor = _MainTex.Sample(sampler_MainTex, input.uv);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 finalColor = texColor * baseColor;

    #if defined(_CLIPPING)
    clip(finalColor.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
    #endif

    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = finalColor.rgb;
    surface.alpha = finalColor.a;
    surface.metallic = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Metallic);
    surface.smoothness = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Smoothness);
    surface.viewDirection = normalize(_WorldSpaceCameraPos - input.positionWS);
    #if defined(_PREMULTIPLY_ALPHA)
    BRDF brdf = GetBRDF(surface,true);
    #else
    BRDF brdf = GetBRDF(surface);
    #endif
    return float4(GetLighting(surface, brdf).xyz, surface.alpha);
}


#endif
