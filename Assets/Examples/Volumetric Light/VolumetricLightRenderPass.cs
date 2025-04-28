using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;

public class VolumetricLightRenderPass : ScriptableRenderPass
{
    public string m_profilerTag;
    public Material m_VolumetricLightMaterial;
    public int m_FrameIndex = 0;
    public RTHandle[] m_VolumetricLightColorRTs = null;
    public RTHandle[] m_VolumetricLightWorldPosRTs = null;

    public GameObject[] m_SpotLights;

    private Matrix4x4[] m_lastFrameWorldToCameraMatrix = new Matrix4x4[8];
    private bool m_isFirstFrame = true;

    private RTHandle m_cameraOpaqueColorRT = null;
    private Material m_worldPosMaterial = new Material(Shader.Find("Custom/WorldPos"));

    public void Setup(ScriptableRenderer renderer)
    {
        m_cameraOpaqueColorRT = renderer.cameraColorTargetHandle;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer _cmd = CommandBufferPool.Get(m_profilerTag);

        Vector3 _cameraPosition = renderingData.cameraData.camera.transform.position;
        Array.Sort(m_SpotLights, (a, b) =>
        {
            BoxCollider _colliderA = a.GetComponent<BoxCollider>();
            BoxCollider _colliderB = b.GetComponent<BoxCollider>();
            float distA = Vector3.Distance(_colliderA.center + a.transform.position, _cameraPosition);
            float distB = Vector3.Distance(_colliderB.center + b.transform.position, _cameraPosition);
            return distB.CompareTo(distA);
        });
        
        _cmd.SetRenderTarget(m_VolumetricLightWorldPosRTs[m_FrameIndex]);
        _cmd.ClearRenderTarget(RTClearFlags.All, Color.black);
        for (int i = 0; i < m_SpotLights.Length; i++)
        {
            var _volumetricLight = m_SpotLights[i];
            Mesh _mesh = _volumetricLight.GetComponent<MeshFilter>().sharedMesh;
            var localToWorldMatrix = _volumetricLight.transform.localToWorldMatrix;
            _cmd.DrawMesh(_mesh, localToWorldMatrix, m_worldPosMaterial, 0, -1);
        }
        
        _cmd.SetRenderTarget(m_VolumetricLightColorRTs[m_FrameIndex]);
        _cmd.ClearRenderTarget(RTClearFlags.Color, Color.black);
        for (int i = 0; i < m_SpotLights.Length; i++)
        {
            var _spotLightGameObject = m_SpotLights[i];
            Light _spotLight = _spotLightGameObject.GetComponentInChildren<Light>();
            Transform _transform = _spotLightGameObject.transform;
            Camera _depthCamera = _spotLightGameObject.GetComponentInChildren<Camera>();

            MaterialPropertyBlock _materialPropertyBlock = new MaterialPropertyBlock();

            _materialPropertyBlock.SetTexture("_LastFrameColorTexture", m_VolumetricLightColorRTs[(m_FrameIndex + 1) % 2]);
            _materialPropertyBlock.SetTexture("_LastFrameDepthTexture", m_VolumetricLightWorldPosRTs[(m_FrameIndex + 1) % 2]);
            _materialPropertyBlock.SetTexture("_SpotLightDepthTexture", _depthCamera.targetTexture);
            _materialPropertyBlock.SetVector("_WorldSpaceSpotLightPos", _transform.position);
            _materialPropertyBlock.SetVector("_ForwardDirection", -_transform.up);
            _materialPropertyBlock.SetVector("_RightDirection", _transform.right);
            _materialPropertyBlock.SetVector("_SpotLightColor", _spotLight.color);
            _materialPropertyBlock.SetFloat("_OrthographicCameraSize", _depthCamera.orthographicSize);
            _materialPropertyBlock.SetFloat("_SpotLightOuterAngle", _spotLight.spotAngle);
            _materialPropertyBlock.SetFloat("_SpotLightCameraNearPlane", _depthCamera.nearClipPlane);
            _materialPropertyBlock.SetFloat("_SpotLightCameraFarPlane", _depthCamera.farClipPlane);
            // _materialPropertyBlock.SetMatrix("_LastFrameWorldToCameraMatrix", m_lastFrameWorldToCameraMatrix[i]);
            _materialPropertyBlock.SetMatrix("_SpotLightCameraViewProjectionMatrix", _depthCamera.projectionMatrix * _depthCamera.worldToCameraMatrix);

            // m_lastFrameWorldToCameraMatrix[i] = renderingData.cameraData.camera.worldToCameraMatrix;

            var _volumetricLight = m_SpotLights[i];
            Mesh _mesh = _volumetricLight.GetComponent<MeshFilter>().sharedMesh;

            var localToWorldMatrix = _volumetricLight.transform.localToWorldMatrix;
            _cmd.DrawMesh(_mesh, localToWorldMatrix, m_VolumetricLightMaterial, 0, -1, _materialPropertyBlock);
        }

        context.ExecuteCommandBuffer(_cmd);
        CommandBufferPool.Release(_cmd);
    }

    public void Dispose()
    {
    }
}