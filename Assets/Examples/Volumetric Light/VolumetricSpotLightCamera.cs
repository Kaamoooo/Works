using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace Examples.Volumetric_Light
{
    [ExecuteInEditMode]
    public class VolumetricSpotLightCamera : MonoBehaviour
    {
        private Camera m_camera;
        public RTHandle m_SpotLightShadowMapHandle = null;
        private void Start()
        {
            m_camera = GetComponent<Camera>();
            RenderTextureDescriptor _descriptor = new RenderTextureDescriptor(1024, 1024, GraphicsFormat.None, 32);
            RenderingUtils.ReAllocateIfNeeded(ref m_SpotLightShadowMapHandle, _descriptor);
            m_camera.targetTexture = m_SpotLightShadowMapHandle;
            m_camera.clearFlags = CameraClearFlags.Depth;
        }

    }
}