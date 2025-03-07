Shader "Unlit/UnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _BaseColor ("Base Color", Color) = (1,1,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertUnlitPass
            #pragma fragment fragUnlitPass
            #include "CustomPass\UnlitPass.GPUInstancing.hlsl"
            ENDHLSL
        }
    }
}
