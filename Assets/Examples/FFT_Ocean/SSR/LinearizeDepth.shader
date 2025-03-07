Shader "Custom/LinearizeDepth"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "LinearizeDepthPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            #pragma vertex Vert
            #pragma fragment frag

            float frag(Varyings input) : SV_Target
            {
                float depth = SAMPLE_TEXTURE2D(_BlitTexture, sampler_PointClamp, input.texcoord);
                // depth = Linear01Depth(depth,_ZBufferParams);
                return depth;
            }
            ENDHLSL
        }
    }
}