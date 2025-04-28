using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

namespace Examples.Volumetric_Light
{
    [ExecuteAlways]
    public class VolumetricSpotLight : MonoBehaviour
    {
        public Light m_spotLight;
        public Camera m_depthCamera;

        private Camera m_mainCamera;
        private MaterialPropertyBlock m_materialPropertyBlock;
        private MeshRenderer m_meshRenderer;
        private Matrix4x4 m_lastFrameWorldToCameraMatrix = Matrix4x4.identity;
        private bool m_isFirstFrame = true;
        private static int m_volumetricSpotLightCount = 0;
        
        private void Start()
        {
            m_volumetricSpotLightCount++;
            m_meshRenderer = GetComponent<MeshRenderer>();
            m_materialPropertyBlock = new MaterialPropertyBlock();
            m_mainCamera = Camera.main;
            m_lastFrameWorldToCameraMatrix = Matrix4x4.zero;
        }

        private void Update()
        {
            var localScale = transform.localScale;
            // m_spotLight.range = localScale.x * 2;
            m_spotLight.range = localScale.x * 2 / Mathf.Cos(Mathf.Deg2Rad * m_spotLight.spotAngle / 2);
            // m_depthCamera.orthographicSize = localScale.x;
            m_depthCamera.fieldOfView = m_spotLight.spotAngle;
            m_depthCamera.farClipPlane = 2.2f * localScale.x;
            // m_materialPropertyBlock.SetTexture("_SpotLightDepthTexture", m_depthCamera.targetTexture);
            // m_materialPropertyBlock.SetVector("_WorldSpaceSpotLightPos",new float4(transform.position,1f));
            // m_materialPropertyBlock.SetVector("_ForwardDirection",-transform.up);
            // m_materialPropertyBlock.SetVector("_RightDirection",transform.right);
            // m_materialPropertyBlock.SetVector("_SpotLightColor",m_spotLight.color);
            // m_materialPropertyBlock.SetFloat("_OrthographicCameraSize",m_depthCamera.orthographicSize);
            // m_materialPropertyBlock.SetFloat("_SpotLightOuterAngle",m_spotLight.spotAngle);
            // m_materialPropertyBlock.SetFloat("_SpotLightCameraNearPlane",m_depthCamera.nearClipPlane);
            // m_materialPropertyBlock.SetFloat("_SpotLightCameraFarPlane",m_depthCamera.farClipPlane);
            // m_materialPropertyBlock.SetInt("_IsFirstFrame",m_isFirstFrame ? 1 : 0);
            // m_materialPropertyBlock.SetMatrix("_LastFrameWorldToCameraMatrix",m_lastFrameWorldToCameraMatrix);
            // m_meshRenderer.SetPropertyBlock(m_materialPropertyBlock);
            //
            // m_lastFrameWorldToCameraMatrix = m_mainCamera.worldToCameraMatrix;
            // m_isFirstFrame = false;

        }
    }
}