using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraTextureRendererPass : ScriptableRenderPass
{
    private RTHandle m_colorRT;
    private RTHandle m_cameraColorHandle;
    
    public CameraTextureRendererPass()
    {
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        m_colorRT = RTHandles.Alloc(
            scaleFactor: Vector2.one,
            depthBufferBits: DepthBits.None,
            colorFormat: GraphicsFormat.R16G16B16A16_SFloat,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Clamp,
            dimension: TextureDimension.Tex2D,
            useDynamicScale: true,
            enableRandomWrite: true,
            useMipMap: true,
            autoGenerateMips: true
        );
    }
    public void Setup(RTHandle cameraColorHandle)
    {
        m_cameraColorHandle = cameraColorHandle;
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _commandBuffer = CommandBufferPool.Get("CameraTextureRendererPass");
        _commandBuffer.Blit(m_cameraColorHandle, m_colorRT);
        _commandBuffer.SetGlobalTexture("_CameraColorTexture", m_colorRT);
        context.ExecuteCommandBuffer(_commandBuffer);
    }
}
