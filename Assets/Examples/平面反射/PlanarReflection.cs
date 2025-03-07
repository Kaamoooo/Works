using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class PlanarReflection : MonoBehaviour
{
    public Material reflectionMaterial;
    public Camera mainCamera;
    public Transform reflectionPlaneTransform;
    private Camera _reflectionCamera;
    private Transform _mainCameraTransform;
    private Transform _normalObjectTransform;
    private float3 _planeNormal;
    private float3 _planePosition;
    private RTHandle reflectionTexture;

    // Start is called before the first frame update
    void Awake()
    {
        _mainCameraTransform = mainCamera.transform;

        //创建反射相机
        GameObject reflectionCameraGameObject = new GameObject("ReflectionCamera");
        _reflectionCamera = reflectionCameraGameObject.AddComponent<Camera>();
        _reflectionCamera.enabled = false;
        _reflectionCamera.aspect = mainCamera.aspect;
        _reflectionCamera.fieldOfView = mainCamera.fieldOfView;
        _reflectionCamera.nearClipPlane = mainCamera.nearClipPlane;
        _reflectionCamera.farClipPlane = 50;
        reflectionCameraGameObject.transform.SetParent(_mainCameraTransform);

        //创建反射相机的渲染纹理
        RenderTextureDescriptor renderTextureDescriptor = new RenderTextureDescriptor(1024,1024,GraphicsFormat.R8G8B8A8_SRGB,0);
        RenderingUtils.ReAllocateIfNeeded(ref reflectionTexture, renderTextureDescriptor);
        _reflectionCamera.targetTexture = reflectionTexture;
        _reflectionCamera.cullingMask = ~(1 << (LayerMask.NameToLayer("Reflection")));

        //创建反射平面的法线对象
        GameObject normalObject = new GameObject("NormalObject");
        _normalObjectTransform = normalObject.transform;
        _normalObjectTransform.position = reflectionPlaneTransform.position;
        _normalObjectTransform.rotation = reflectionPlaneTransform.rotation;
        _normalObjectTransform.SetParent(reflectionPlaneTransform);
        _planeNormal = _normalObjectTransform.up;
        _planePosition = _normalObjectTransform.position;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType == CameraType.Reflection || camera.cameraType == CameraType.Preview)
            return;

        Vector3 normal = _normalObjectTransform.up;
        Vector3 position = _normalObjectTransform.position;
        float d = -Vector3.Dot(normal, position);
        Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

        //计算反射矩阵
        Matrix4x4 reflectionMatrix = CalculateReflectionMatrix(reflectionPlane);
        Matrix4x4 worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflectionMatrix;
        _reflectionCamera.worldToCameraMatrix = worldToCameraMatrix;

        GL.invertCulling = true;
        // RenderPipeline.SubmitRenderRequest(_reflectionCamera, context);
#pragma warning disable CS0618 // Type or member is obsolete
        UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera);
#pragma warning restore CS0618 // Type or member is obsolete
        GL.invertCulling = false;

        reflectionMaterial.SetTexture("_ReflectionTexture", _reflectionCamera.targetTexture);
    }


    Matrix4x4 CalculateReflectionMatrix(Vector4 reflectionPlane)
    {
        Matrix4x4 reflectionMatrix = Matrix4x4.identity;
        reflectionMatrix.m00 = 1 - 2 * reflectionPlane[0] * reflectionPlane[0];
        reflectionMatrix.m01 = -2 * reflectionPlane[0] * reflectionPlane[1];
        reflectionMatrix.m02 = -2 * reflectionPlane[0] * reflectionPlane[2];
        reflectionMatrix.m03 = -2 * reflectionPlane[3] * reflectionPlane[0];

        reflectionMatrix.m10 = -2 * reflectionPlane[1] * reflectionPlane[0];
        reflectionMatrix.m11 = 1 - 2 * reflectionPlane[1] * reflectionPlane[1];
        reflectionMatrix.m12 = -2 * reflectionPlane[1] * reflectionPlane[2];
        reflectionMatrix.m13 = -2 * reflectionPlane[3] * reflectionPlane[1];

        reflectionMatrix.m20 = -2 * reflectionPlane[2] * reflectionPlane[0];
        reflectionMatrix.m21 = -2 * reflectionPlane[2] * reflectionPlane[1];
        reflectionMatrix.m22 = 1 - 2 * reflectionPlane[2] * reflectionPlane[2];
        reflectionMatrix.m23 = -2 * reflectionPlane[3] * reflectionPlane[2];

        reflectionMatrix.m30 = 0;
        reflectionMatrix.m31 = 0;
        reflectionMatrix.m32 = 0;
        reflectionMatrix.m33 = 1;

        return reflectionMatrix;
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    private void OnDestroy()
    {
    }
}