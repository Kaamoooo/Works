using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ClearCameraColorRenderPass : ScriptableRenderPass
{
    private RTHandle m_cameraColorRT;
    private RTHandle m_cameraDepthRT;
    
    public ClearCameraColorRenderPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
    }

    public void Setup(RTHandle cameraColorRT, RTHandle cameraDepthRT)
    {
        m_cameraColorRT = cameraColorRT;
        m_cameraDepthRT = cameraDepthRT;
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _cmd = CommandBufferPool.Get("ClearCameraColorRenderPass");
        _cmd.SetRenderTarget(m_cameraColorRT,m_cameraDepthRT);
        _cmd.ClearRenderTarget(true, true, Color.black);
        context.ExecuteCommandBuffer(_cmd);
        _cmd.Clear();
        CommandBufferPool.Release(_cmd);
    }

}