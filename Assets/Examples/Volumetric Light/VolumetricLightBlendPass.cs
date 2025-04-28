using System.Collections;
using System.Collections.Generic;
using Accord;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

public class VolumetricLightBlendPass : ScriptableRenderPass
{
    public string m_ProfilerTag;
    public float m_BlendFactor;
    public Material m_VolumetricLightBlendMaterial;
    public Material m_BlitMaterial;
    public int m_FrameIndex = 0;
    public RTHandle[] m_VolumetricLightColorRTs = null;
    public RTHandle[] m_VolumetricLightWorldPosRTs = null;

    private Matrix4x4 m_lastFrameWorldToCameraMatrix = Matrix4x4.zero;
    private RTHandle m_cameraOpaqueColorRT = null;
    private RTHandle m_tmpCameraOpaqueColorRT = null;
    private RTHandle m_tmpVolumetricLightColorRT = null;

    public void Setup(ScriptableRenderer renderer)
    {
        m_cameraOpaqueColorRT = renderer.cameraColorTargetHandle;
        RenderTextureDescriptor _descriptor = renderer.cameraColorTargetHandle.rt.descriptor;
        RenderingUtils.ReAllocateIfNeeded(ref m_tmpCameraOpaqueColorRT, _descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp);
        RenderingUtils.ReAllocateIfNeeded(ref m_tmpVolumetricLightColorRT, _descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        ConfigureTarget(m_cameraOpaqueColorRT);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _cmd = CommandBufferPool.Get(m_ProfilerTag);
        Blitter.BlitCameraTexture(_cmd, m_cameraOpaqueColorRT, m_tmpCameraOpaqueColorRT, m_BlitMaterial, 0);
        Blitter.BlitCameraTexture(_cmd, m_VolumetricLightColorRTs[m_FrameIndex], m_tmpVolumetricLightColorRT, m_BlitMaterial, 0);

        m_VolumetricLightBlendMaterial.SetFloat("_BlendFactor", m_BlendFactor);
        // m_VolumetricLightBlendMaterial.SetMatrix("_LastFrameVPMatrix",renderingData.cameraData.camera.previousViewProjectionMatrix);
        m_VolumetricLightBlendMaterial.SetMatrix("_LastFrameVPMatrix", m_lastFrameWorldToCameraMatrix);
        m_VolumetricLightBlendMaterial.SetMatrix("_VPMatrix", renderingData.cameraData.camera.projectionMatrix * renderingData.cameraData.camera.worldToCameraMatrix);
        m_VolumetricLightBlendMaterial.SetTexture("_VolumetricLightWorldPosTexture", m_VolumetricLightWorldPosRTs[m_FrameIndex]);
        m_VolumetricLightBlendMaterial.SetTexture("_LastFrameColorTexture", m_VolumetricLightColorRTs[(m_FrameIndex + 1) % 2]);
        m_VolumetricLightBlendMaterial.SetTexture("_VolumetricLightWorldPosTexture", m_VolumetricLightWorldPosRTs[m_FrameIndex]);
        m_VolumetricLightBlendMaterial.SetTexture("_VolumetricLightColorTexture", m_VolumetricLightColorRTs[m_FrameIndex]);

        Blitter.BlitCameraTexture(_cmd, m_tmpVolumetricLightColorRT, m_VolumetricLightColorRTs[m_FrameIndex], m_VolumetricLightBlendMaterial, 0);
        Blitter.BlitCameraTexture(_cmd, m_tmpCameraOpaqueColorRT, m_cameraOpaqueColorRT, m_VolumetricLightBlendMaterial, 1);

        context.ExecuteCommandBuffer(_cmd);
        CommandBufferPool.Release(_cmd);

        m_lastFrameWorldToCameraMatrix = renderingData.cameraData.camera.projectionMatrix * renderingData.cameraData.camera.worldToCameraMatrix;
    }

    public void Dispose()
    {
    }
}