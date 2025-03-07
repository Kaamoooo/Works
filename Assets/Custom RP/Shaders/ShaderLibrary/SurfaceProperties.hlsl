#ifndef CUSTOM_SURFACE_PROPERTIES_INCLUDED
#define CUSTOM_SURFACE_PROPERTIES_INCLUDED

struct Surface
{
    float3 color;
    float alpha;
    float3 normal;
    float metallic;
    float smoothness;
    float3 viewDirection;
};

#endif