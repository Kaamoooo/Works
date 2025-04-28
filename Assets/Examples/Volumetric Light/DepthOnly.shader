Shader "Custom/DepthOnly"
{
    SubShader
    {
        Pass
        {
            Tags
            {
                "Queue"="Opaque" "RenderType"="Geometry"
            }

            ZWrite On
            ZTest Less
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return float4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}