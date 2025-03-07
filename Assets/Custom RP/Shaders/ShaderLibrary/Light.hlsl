#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_CustomLight)
    int _DirectionalLightCount;
    float3 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float3 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 color;
    float3 direction;
};

Light GetDirectionalLight(int index)
{
    Light light;
    light.color = _DirectionalLightColors[index];
    light.direction = _DirectionalLightDirections[index];
    return light;
}

#endif
