#ifndef CUSTOM_UNLIT_PASS_SRP_BATCHING_INCLUDED
#define CUSTOM_UNLIT_PASS_SRP_BATCHING_INCLUDED

#include "./ShaderLibrary/Common.SRPBatching.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseColor;
CBUFFER_END

struct Atrributes
{
    float4 positionOS : POSITION;
};

float4 vertUnlitPass(Atrributes input): SV_Position
{
    //Compatible with GPU instancing
    return TransformObjectToHClip(input.positionOS);
}

float4 fragUnlitPass() : SV_Target
{
    return _BaseColor;
}


#endif
