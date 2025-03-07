using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Custom_RP.RunTime.Lighting
{
    public class Lighting
    {
        const string BufferName = "Lighting";
        const int MaxVisibleDirectionalLights = 4;

        CullingResults _cullingResults;

        private readonly CommandBuffer _buffer = new CommandBuffer
        {
            name = BufferName
        };


        static readonly int DirectionalLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
        static readonly int DirectionalLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
        static readonly int DirectionalLightCountId = Shader.PropertyToID("_DirectionalLightCount");

        private readonly Vector4[] _visibleDirectionalLightColors = new Vector4[MaxVisibleDirectionalLights];
        private readonly Vector4[] _visibleDirectionalLightDirections = new Vector4[MaxVisibleDirectionalLights];

        Shadow _shadow = new Shadow();
        


        public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            _cullingResults = cullingResults;
            _buffer.BeginSample(BufferName);
            _shadow.SetUp(context, cullingResults, shadowSettings);
            SetUpLights();
            _shadow.Render();
            _buffer.EndSample(BufferName);
            context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        private void SetUpLights()
        {
            NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
            for (int i = 0; i < visibleLights.Length; i++)
            {
                var visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    SetUpDirectionalLight(i, ref visibleLight);
                }
            }

            _buffer.SetGlobalInt(DirectionalLightCountId,
                Mathf.Min(visibleLights.Length, MaxVisibleDirectionalLights));
            _buffer.SetGlobalVectorArray(DirectionalLightDirectionsId, _visibleDirectionalLightDirections);
            _buffer.SetGlobalVectorArray(DirectionalLightColorsId, _visibleDirectionalLightColors);
        }

        void SetUpDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            _visibleDirectionalLightColors[index] = visibleLight.finalColor;
            _visibleDirectionalLightDirections[index] = visibleLight.localToWorldMatrix.GetColumn(2);
            _shadow.ReserveDirectionalShadows(visibleLight.light, index);
        }

        public void CleanUp()
        {
            _shadow.Cleanup();
        }
    }
}