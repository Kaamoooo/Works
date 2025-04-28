using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Volumetric_Light
{
    public class CameraTextureBlitRendererFeature : ScriptableRendererFeature
    {
        public Material m_BlitMaterial;
        
        private CameraAllColorBlitRenderPass _cameraAllColorBlitRenderPass;

        private GameObject[] m_spotLights;

        public override void Create()
        {
            _cameraAllColorBlitRenderPass = new CameraAllColorBlitRenderPass
            {
                profilerTag = "CameraAllColorBlitRenderPass",
                m_BlitMaterial = m_BlitMaterial,
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents
            };

            m_spotLights = GameObject.FindGameObjectsWithTag("VolumetricSpotLight");
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.camera.CompareTag("MainCamera")) return;

            _cameraAllColorBlitRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_cameraAllColorBlitRenderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (!renderingData.cameraData.camera.CompareTag("MainCamera")) return;

            _cameraAllColorBlitRenderPass.Setup(renderer);
        }


        protected override void Dispose(bool disposing)
        {
            _cameraAllColorBlitRenderPass.Dispose();
        }
    }
}