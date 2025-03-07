using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Volumetric_Light
{
    public class VolumetricLightFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class RendererFeatureSettings
        {
            public Material material = null;
            public RenderPassEvent whenToInsert = RenderPassEvent.AfterRenderingOpaques;
        }

        public RendererFeatureSettings settings = new RendererFeatureSettings();

        class CustomRenderPass : ScriptableRenderPass
        {
            public Material material;
            public string profilerTag;

            private RTHandle _source;
            private RTHandle _tmpTex;

            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
                var renderer = renderingData.cameraData.renderer;
                _source = renderer.cameraColorTargetHandle;
                RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
                opaqueDesc.depthBufferBits = 0;
                RenderingUtils.ReAllocateIfNeeded(ref _tmpTex, opaqueDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                ConfigureTarget(_source);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                // Debug.Log(renderingData.cameraData.camera.cameraType);
                if (renderingData.cameraData.isPreviewCamera) return;

                CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

                if (material != null)
                {
                    Blitter.BlitCameraTexture(cmd, _source, _tmpTex, material, 0);
                    Blitter.BlitCameraTexture(cmd, _tmpTex, _source, material, 0);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public void Dispose()
            {
                _tmpTex?.Release();
            }
        }

        CustomRenderPass _scriptablePass;

        public override void Create()
        {
            _scriptablePass = new CustomRenderPass
            {
                profilerTag = "Volumetric Light",
                material = settings.material,
                renderPassEvent = settings.whenToInsert
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            _scriptablePass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_scriptablePass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer,
            in RenderingData renderingData)
        {
        }

        protected override void Dispose(bool disposing)
        {
            _scriptablePass.Dispose();
        }
    }
}