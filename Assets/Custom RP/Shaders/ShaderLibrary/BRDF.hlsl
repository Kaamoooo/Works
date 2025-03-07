#ifndef CUSTOM_BRDF_INCLUDED
#define CUSTOM_BRDF_INCLUDED

#ifndef CUSTOM_SURFACE_PROPERTIES_INCLUDED
#include "SurfaceProperties.hlsl"
#endif

#ifndef CUSTOM_COMMON_GPU_INSTANCING_INCLUDED
#include "Common.GPUInstancing.hlsl"
#endif

#ifndef CUSTOM_LIGHT_INCLUDED
#include "Light.hlsl"
#endif

#define MIN_REFLECTANCE 0.04

struct BRDF
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

float Square(float x)
{
    return x * x;
}

float SpecularStrength(Surface surface, BRDF brdf, Light light)
{
    float3 h = SafeNormalize(-light.direction + surface.viewDirection);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(-light.direction, h)));
    float r2 = Square(brdf.roughness);
    float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
    float normalization = brdf.roughness * 4.0 + 2.0;
    return r2 / (d2 * max(0.1, lh2) * normalization);
}

float GetOneMinusReflectance(Surface surface)
{
    return 1 - MIN_REFLECTANCE - surface.metallic;
}

BRDF GetBRDF(Surface surface, bool premultiplyAlpha = false)
{
    BRDF brdf;
    if (premultiplyAlpha)
    {
        brdf.diffuse = surface.color * GetOneMinusReflectance(surface) * surface.alpha;
    }
    else
    {
        brdf.diffuse = surface.color * GetOneMinusReflectance(surface);
    }
    brdf.specular = lerp(MIN_REFLECTANCE * surface.color, surface.color, surface.metallic);
    brdf.roughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
    return brdf;
}

float3 DirectBRDF(Surface surface, BRDF brdf, Light light)
{
    return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}

#endif
