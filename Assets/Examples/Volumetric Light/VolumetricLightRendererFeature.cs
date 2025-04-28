using System.Collections.Generic;
using Accord;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Examples.Volumetric_Light
{
    public class VolumetricLightRendererFeature : ScriptableRendererFeature
    {
        public Material m_VolumetricLightMaterial;
        public Material m_VolumetricLightBlendMaterial;
        public Material m_BlitMaterial;

        [Range(0,1)] public float m_BlendFactor;
        
        private VolumetricLightRenderPass _volumetricLightRenderPass;
        private VolumetricLightBlendPass _volumetricLightBlendPass;

        private GameObject[] m_spotLights;
        private RTHandle[] m_volumetricLightColorRTs;
        private RTHandle[] m_volumetricLightWorldPosRTs;
        
        private int m_frameIndex = 0;
        public override void Create()
        {
            m_spotLights = GameObject.FindGameObjectsWithTag("VolumetricSpotLight");
            m_volumetricLightColorRTs = new RTHandle[2];
            m_volumetricLightWorldPosRTs = new RTHandle[2];
            
            _volumetricLightRenderPass = new VolumetricLightRenderPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingTransparents,
                m_profilerTag = "VolumetricLightRenderPass",
                m_VolumetricLightMaterial = m_VolumetricLightMaterial,
                m_SpotLights = m_spotLights
            };

            _volumetricLightBlendPass = new VolumetricLightBlendPass
            {
                renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing,
                m_ProfilerTag = "VolumetricLightBlendPass",
                m_VolumetricLightBlendMaterial = m_VolumetricLightBlendMaterial,
                m_BlitMaterial = m_BlitMaterial,
                m_BlendFactor = m_BlendFactor
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.camera.CompareTag("MainCamera")) return;

            _volumetricLightRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_volumetricLightRenderPass);
            
            _volumetricLightBlendPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(_volumetricLightBlendPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (!renderingData.cameraData.camera.CompareTag("MainCamera")) return;

            var _cameraRtDesc = renderingData.cameraData.cameraTargetDescriptor;
            _cameraRtDesc.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            _cameraRtDesc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref m_volumetricLightColorRTs[0], _cameraRtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref m_volumetricLightColorRTs[1], _cameraRtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);

            _cameraRtDesc.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
            // _cameraRtDesc.depthBufferBits = 32;
            RenderingUtils.ReAllocateIfNeeded(ref m_volumetricLightWorldPosRTs[0], _cameraRtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);
            RenderingUtils.ReAllocateIfNeeded(ref m_volumetricLightWorldPosRTs[1], _cameraRtDesc, FilterMode.Bilinear, TextureWrapMode.Clamp);

            _volumetricLightRenderPass.m_FrameIndex = m_frameIndex;
            _volumetricLightRenderPass.m_VolumetricLightColorRTs = m_volumetricLightColorRTs;
            _volumetricLightRenderPass.m_VolumetricLightWorldPosRTs = m_volumetricLightWorldPosRTs;
            
            _volumetricLightBlendPass.m_FrameIndex = m_frameIndex;
            _volumetricLightBlendPass.m_VolumetricLightColorRTs = m_volumetricLightColorRTs;
            _volumetricLightBlendPass.m_VolumetricLightWorldPosRTs = m_volumetricLightWorldPosRTs;
            
            m_frameIndex = (m_frameIndex + 1) % 2;
            
            _volumetricLightRenderPass.Setup(renderer);
            _volumetricLightBlendPass.Setup(renderer);
        }


        protected override void Dispose(bool disposing)
        {
            _volumetricLightRenderPass.Dispose();
            _volumetricLightBlendPass.Dispose();

            // for (int i = 0; i < m_volumetricLightColorRTs.Length; i++)
            // {
            //     m_volumetricLightColorRTs[i].Release();
            // }
            // m_volumetricLightDepthRT.Release();
        }
    }
}