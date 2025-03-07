using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthBlitRenderPass : ScriptableRenderPass
{
    private readonly Material m_blitMaterial;
    private readonly Material m_linearizeDepthMaterial;
    private readonly ComputeShader m_mipmapComputeShader;
    private RTHandle m_cameraColorRT;
    private RTHandle m_cameraDepthRT;
    private RTHandle m_depthRT;
    private RTHandle m_opaqueColorRT;

    public DepthBlitRenderPass(DepthBlitRenderFeature.Settings settings)
    {
        m_blitMaterial = settings.blitMaterial;
        m_linearizeDepthMaterial = settings.linearizeDepthMaterial;
        m_mipmapComputeShader = settings.mipmapComputeShader;

        renderPassEvent = settings.renderPassEvent;
        m_depthRT = RTHandles.Alloc(
            scaleFactor: Vector2.one,
            depthBufferBits: DepthBits.None,
            colorFormat: GraphicsFormat.R32_SFloat,
            filterMode: FilterMode.Bilinear,
            wrapMode: TextureWrapMode.Clamp,
            dimension: TextureDimension.Tex2D,
            useDynamicScale: false,
            enableRandomWrite: true,
            useMipMap: true,
            autoGenerateMips: false
        );

        m_opaqueColorRT = RTHandles.Alloc(
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

    public void Setup(RTHandle cameraColorRT, RTHandle cameraDepthRT)
    {
        m_cameraColorRT = cameraColorRT;
        m_cameraDepthRT = cameraDepthRT;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _cmd = CommandBufferPool.Get("DepthBlitRenderPass");
        Blitter.BlitCameraTexture(_cmd, m_cameraColorRT, m_opaqueColorRT, m_blitMaterial, 0);
        Blitter.BlitCameraTexture(_cmd, m_cameraDepthRT, m_depthRT, m_linearizeDepthMaterial, 0);
        int _mipmapMaxLevels = ComputeMipmapLevelCount(m_depthRT.referenceSize.x, m_depthRT.referenceSize.y);
        for (int i = 0; i < _mipmapMaxLevels; i++)
        {
            _cmd.SetComputeTextureParam(m_mipmapComputeShader, 0, "Input", m_depthRT, i);
            _cmd.SetComputeTextureParam(m_mipmapComputeShader, 0, "Output", m_depthRT, i + 1);
            _cmd.DispatchCompute(m_mipmapComputeShader, 0, m_depthRT.rt.width / 8, m_depthRT.rt.height / 8, 1);
        }

        _cmd.SetGlobalTexture("_CameraDepthTextureWithLOD", m_depthRT);
        _cmd.SetGlobalTexture("_CameraColorTexture", m_opaqueColorRT);
        context.ExecuteCommandBuffer(_cmd);
        _cmd.Clear();
        CommandBufferPool.Release(_cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.SetRenderTarget(m_opaqueColorRT);
        cmd.ClearRenderTarget(true, true, Color.black);
    }

    public void Dispose()
    {
        m_depthRT.Release();
        m_opaqueColorRT.Release();
    }

    private int ComputeMipmapLevelCount(int x, int y)
    {
        int maxSide = Mathf.Max(x, y);
        return (int) Mathf.Log(maxSide, 2);
    }
}