Shader "Custom/Unlit/UnlitTransparent"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "red" {}
        _BaseColor ("Base Color", Color) = (1,1,0,1)
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("SrcBlend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("DstBlend", Float) = 0
        [Enum(off,0,on,1)]_ZWrite ("ZWrite", Float) = 0
        _Cutoff("Alphat Cutoff",Range(0,1)) = 0.6
        [Toggle(_CLIPPING)]_Clipping("Clipping",Float) = 0
    }
    SubShader
    {
        LOD 100
        Tags
        {
            "RenderType"="Transparent" "RenderQueue"="Transparent"
        }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            #pragma vertex vertUnlitPass
            #pragma fragment fragUnlitPass
            #include "CustomPass\UnlitPass.GPUInstancing.hlsl"
            ENDHLSL
        }
    }
}