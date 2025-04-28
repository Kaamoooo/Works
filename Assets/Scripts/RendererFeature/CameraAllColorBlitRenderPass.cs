using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraAllColorBlitRenderPass : ScriptableRenderPass
{
    public Material m_BlitMaterial;
    public string profilerTag;

    private RTHandle m_opaqueColorRT = null;
    private RTHandle m_cameraOpaqueColorRT = null;
    private bool m_isAllocated = false;
    
    public void Setup(ScriptableRenderer renderer)
    {
        m_cameraOpaqueColorRT = renderer.cameraColorTargetHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        if (!m_isAllocated)
        {
            m_isAllocated = true;
            m_opaqueColorRT = RTHandles.Alloc(Screen.width, Screen.height, 1, DepthBits.None, GraphicsFormat.R16G16B16A16_SFloat, FilterMode.Bilinear, TextureWrapMode.Clamp);
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _cmd = CommandBufferPool.Get(profilerTag);

        Blitter.BlitCameraTexture(_cmd, m_cameraOpaqueColorRT, m_opaqueColorRT, m_BlitMaterial, 0);
        _cmd.SetGlobalTexture("_CameraAllColorTexture", m_opaqueColorRT);
        context.ExecuteCommandBuffer(_cmd);
        _cmd.Clear();
        CommandBufferPool.Release(_cmd);
    }

    public void Dispose()
    {
        if (m_opaqueColorRT != null)
        {
            m_opaqueColorRT.Release();
        }
    }
}