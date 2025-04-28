Shader "Custom/WorldPos"
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
            ColorMask RGBA
            
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
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.worldPos = mul(UNITY_MATRIX_M, float4(v.vertex.xyz, 1)).xyz;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 ndc = ComputeNormalizedDeviceCoordinatesWithZ(i.vertex);
                return float4(i.worldPos,ndc.z);
            }
            ENDHLSL
        }
    }
}