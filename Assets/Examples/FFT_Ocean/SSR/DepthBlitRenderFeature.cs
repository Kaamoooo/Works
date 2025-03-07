using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DepthBlitRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        public Material blitMaterial = null;
        public Material linearizeDepthMaterial = null;
        public Material waterMaterial = null;
        public ComputeShader mipmapComputeShader = null;
    }
    public Settings m_Settings = new Settings();

    private DepthBlitRenderPass m_depthBlitRenderPass;
    private ClearCameraColorRenderPass m_clearCameraColorRenderPass;


    public override void Create()
    {
        m_depthBlitRenderPass = new DepthBlitRenderPass(m_Settings);
        m_clearCameraColorRenderPass = new ClearCameraColorRenderPass();
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_depthBlitRenderPass.Setup(renderer.cameraColorTargetHandle,renderer.cameraDepthTargetHandle);
        m_clearCameraColorRenderPass.Setup(renderer.cameraColorTargetHandle,renderer.cameraDepthTargetHandle);
    }
    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_depthBlitRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        renderer.EnqueuePass(m_depthBlitRenderPass);
        m_clearCameraColorRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
        renderer.EnqueuePass(m_clearCameraColorRenderPass);
    }
    
    
    protected override void Dispose(bool disposing)
    {
        m_depthBlitRenderPass.Dispose();
    }
}
