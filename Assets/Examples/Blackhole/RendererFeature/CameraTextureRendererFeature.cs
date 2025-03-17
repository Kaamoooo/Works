using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraTextureRendererFeature : ScriptableRendererFeature
{
    private CameraTextureRendererPass m_cameraTextureRendererPass;

    public override void Create()
    {
        m_cameraTextureRendererPass = new CameraTextureRendererPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_cameraTextureRendererPass.ConfigureInput(ScriptableRenderPassInput.Color);
        renderer.EnqueuePass(m_cameraTextureRendererPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        m_cameraTextureRendererPass.Setup(renderer.cameraColorTargetHandle);
    }
}