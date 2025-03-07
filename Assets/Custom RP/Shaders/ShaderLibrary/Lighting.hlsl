#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#ifndef CUSTOM_SURFACE_PROPERTIES_INCLUDED
#include "SurfaceProperties.hlsl"
#endif

#ifndef CUSTOM_LIGHT_INCLUDED
#include "Light.hlsl"
#endif

float3 GetLighting(Surface surface, BRDF brdf);
float3 GetSingleLighting(Surface surface, BRDF brdf, Light light);
float3 IncomingLight(Surface surface, Light light);

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(float3(dot(surface.normal, -light.direction) * light.color));
}

float3 GetLighting(Surface surface, BRDF brdf)
{
    float3 finalLightColor = float3(0.0, 0.0, 0.0);
    for (int i = 0; i < _DirectionalLightCount; i++)
    {
        finalLightColor += GetSingleLighting(surface, brdf, GetDirectionalLight(i));
    }
    return finalLightColor;
}

float3 GetSingleLighting(Surface surface, BRDF brdf, Light light)
{
    return IncomingLight(surface, light).xyz * DirectBRDF(surface, brdf, light);
}

#endif
