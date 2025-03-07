#ifndef CUSTOM_UNLIT_PASS_GPU_INSTANCING_INCLUDED
#define CUSTOM_UNLIT_PASS_GPU_INSTANCING_INCLUDED

#pragma shader_feature _CLIPPING
#pragma multi_compile_instancing

#include "..\ShaderLibrary\Common.GPUInstancing.hlsl"

Texture2D _MainTex;
SamplerState sampler_MainTex;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4,_MainTex_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Atrributes
{
    float4 positionOS : POSITION;
    float2 uv: TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varings
{
    float4 positionCS : SV_Position;
    float2 uv : VAR_UV;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varings vertUnlitPass(Atrributes input)
{
    Varings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    output.positionCS = TransformObjectToHClip(input.positionOS);
    float4 mainTexST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _MainTex_ST);
    output.uv = input.uv*mainTexST.xy + mainTexST.zw;
    return output;
}

float4 fragUnlitPass(Varings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 texColor = _MainTex.Sample(sampler_MainTex, input.uv);
    float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
    float4 finalColor = texColor*baseColor;
    #if defined(_CLIPPING)
    clip(finalColor.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
    #endif
    
    return finalColor;
}


#endif
