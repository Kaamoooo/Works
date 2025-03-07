using UnityEngine;
using UnityEngine.Rendering;

namespace Custom_RP.RunTime.Lighting
{
    public class Shadow
    {
        const string BufferName = "Shadows";
        private ScriptableRenderContext _context;
        private readonly CommandBuffer _buffer = new CommandBuffer {name = BufferName};
        private CullingResults _cullingResults;
        private ShadowSettings _shadowSettings;

        struct ShadowedDirectionalLight
        {
            public int visibleLightIndex;
        }

        const int MaxShadowedDirectionalLightCount = 2;
        int _reservedShadowedDirectionalLightCount = 0;

        private readonly ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[MaxShadowedDirectionalLightCount];
        private readonly int _directionalShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

        public void SetUp(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            _buffer.Clear();
            _context = context;
            _cullingResults = cullingResults;
            _shadowSettings = shadowSettings;
        }

        public void Render()
        {
            if (_reservedShadowedDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                _buffer.GetTemporaryRT(_directionalShadowAtlasId, 1, 1,
                    32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            }
        }

        void RenderDirectionalShadows()
        {
            int atlasSize = (int) _shadowSettings.directional.atlasSize;
            _buffer.GetTemporaryRT(_directionalShadowAtlasId, atlasSize, atlasSize,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            _buffer.SetRenderTarget(_directionalShadowAtlasId, RenderBufferLoadAction.DontCare
                , RenderBufferStoreAction.Store);
            _buffer.ClearRenderTarget(true,false, Color.clear);
            ExecuteBuffer();
        }


        public void ReserveDirectionalShadows(Light shadowLight, int visibleLightIndex)
        {
            if (_reservedShadowedDirectionalLightCount < MaxShadowedDirectionalLightCount &&
                shadowLight.shadows != LightShadows.None && shadowLight.shadowStrength > 0f
                && _cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
            {
                _shadowedDirectionalLights[_reservedShadowedDirectionalLightCount] = new ShadowedDirectionalLight
                {
                    visibleLightIndex = visibleLightIndex
                };
                _reservedShadowedDirectionalLightCount++;
            }
        }


        private void ExecuteBuffer()
        {
            _context.ExecuteCommandBuffer(_buffer);
            _buffer.Clear();
        }

        public void Cleanup()
        {
            _buffer.ReleaseTemporaryRT(_directionalShadowAtlasId);
            ExecuteBuffer();
        }
    }
}