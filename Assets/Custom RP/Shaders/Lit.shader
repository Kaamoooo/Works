Shader "Custom/Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,0,1)
        _Metallic("Metallic",Range(0,1)) = 0.6
        _Smoothness("Smoothness",Range(0,1)) = 0.6
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend ("SrcBlend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("DstBlend", Float) = 0
        [Enum(off,0,on,1)]_ZWrite ("ZWrite", Float) = 0
        _Cutoff("Alpha Cutoff",Range(0,1)) = 0.6
        [Toggle(_CLIPPING)]_Clipping("Clipping",Float) = 0
        [Toggle(_PREMULTIPLY_ALPHA)]_Premultiply_Alpha("Premutiply Alpha",Float) = 0
    }
    SubShader
    {
        LOD 100
        Tags
        {
            "LightMode" = "CustomLit"
        }

        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma vertex vertLitPass
            #pragma fragment fragLitPass
            #include "CustomPass\LitPass.GPUInstancing.hlsl"
            ENDHLSL
        }
    }
    CustomEditor "Custom_RP.Editor.CustomShaderGUI"
}