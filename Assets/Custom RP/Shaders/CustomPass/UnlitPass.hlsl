#ifndef CUSTOM_UNLIT_PASS_INCLUDED
#define CUSTOM_UNLIT_PASS_INCLUDED

#include "./ShaderLibrary/Common.hlsl"

float4 _BaseColor;

struct Atrributes
{
    float4 positionOS : POSITION;
};

float4 vertUnlitPass(Atrributes input): SV_Position
{
    return TransformObjectToHClip(input.positionOS);
}

float4 fragUnlitPass() : SV_Target
{
    return _BaseColor;
}


#endif
