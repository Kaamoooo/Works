using UnityEngine;

namespace Examples.Volumetric_Light
{
    [ExecuteInEditMode]
    public class LightCamera : MonoBehaviour
    {
        private const int Width = 1024;
        private const int Height = 1024;
        
        private RenderTexture _depthTexture;
        private Camera _camera;
        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _depthTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.Depth);
            _depthTexture.Create();
            _camera.depthTextureMode = DepthTextureMode.Depth;
            _camera.targetTexture = _depthTexture;
        }
    }
}