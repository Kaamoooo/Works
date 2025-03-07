using Unity.Mathematics;
using UnityEngine;

namespace SnowGround
{
    [ExecuteInEditMode]
    public class SnowGroundManager : MonoBehaviour
    {
        public Material snowGroundMaterial;
        public float snowHeight = 1f;
        public Camera camera1;

        // Update is called once per frame
        void Update()
        {
            snowGroundMaterial.SetFloat("_SnowHeight", snowHeight);
            float x = -1 + camera1.farClipPlane / camera1.nearClipPlane;
            float farClipPlane = camera1.farClipPlane;
            float4 cameraParams;
            cameraParams.x = x;
            cameraParams.y = 1;
            cameraParams.z = x / farClipPlane;
            cameraParams.w = 1 / farClipPlane;
            snowGroundMaterial.SetVector("_DepthCameraParams", cameraParams);
        }
    }
}