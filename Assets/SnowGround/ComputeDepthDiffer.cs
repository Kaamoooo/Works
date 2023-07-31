using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace SnowGround
{
    [ExecuteInEditMode]
    public class ComputeDepthDiffer : MonoBehaviour
    {
        public Material snowGroundMaterial;
        
        public Camera camera1;
        public ComputeShader computeShader;
        public Camera camera2;
        public float edgeSize = 3.0f;
        public float blurSpread = 0.5f;
        public int blurIterations = 3;

        private const string DepthTexture1Name = "_DepthTexture1";
        private const string DepthTexture2Name = "_DepthTexture2";
        private const string DifferDepthTextureName = "_DifferDepthTexture";
        private const string EdgeTextureName = "_EdgeTexture";
        private const string EdgeBlurTextureName = "_EdgeBlurTexture";

        private int _kernelHandle;

        private RenderTexture _depthTexture1;
        private RenderTexture _depthTexture2;
        private RenderTexture _differDepthTexture;
        private RenderTexture _edgeTexture;
        private RenderTexture _edgeBlurTexture;

        private const int Width = 1024;
        private const int Height = 1024;

        private void Start()
        {
            _depthTexture1 = new RenderTexture(Width, Height, 24, RenderTextureFormat.Depth);
            _depthTexture1.Create();
            camera1.depthTextureMode = DepthTextureMode.Depth;
            camera1.targetTexture = _depthTexture1;

            _depthTexture2 = new RenderTexture(Width, Height, 24, RenderTextureFormat.Depth);
            _depthTexture2.Create();
            camera2.depthTextureMode = DepthTextureMode.Depth;
            camera2.targetTexture = _depthTexture2;

            _differDepthTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.RFloat);
            _differDepthTexture.enableRandomWrite = true;
            _differDepthTexture.Create();
            snowGroundMaterial.SetTexture(DifferDepthTextureName,_differDepthTexture);

            _edgeTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.RFloat);
            _edgeTexture.enableRandomWrite = true;
            _edgeTexture.Create();
            snowGroundMaterial.SetTexture(EdgeTextureName,_edgeTexture);
            
            _edgeBlurTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.RFloat);
            _edgeBlurTexture.enableRandomWrite = true;
            _edgeBlurTexture.Create();
            snowGroundMaterial.SetTexture(EdgeBlurTextureName,_edgeBlurTexture);

            _kernelHandle = computeShader.FindKernel("CSMain");
            computeShader.SetTexture(_kernelHandle, "_DepthTexture1", _depthTexture1);
            computeShader.SetTexture(_kernelHandle, "_DepthTexture2", _depthTexture2);
            computeShader.SetTexture(_kernelHandle, "_DifferDepthTexture", _differDepthTexture);
            computeShader.SetTexture(_kernelHandle, "_EdgeTexture", _edgeTexture);
            computeShader.SetTexture(_kernelHandle, "_EdgeBlurTexture", _edgeBlurTexture);
        }

        private void Update()
        {
            computeShader.SetFloat("_BlurSpread", blurSpread);
            computeShader.SetInt("_BlurIterations", blurIterations);
            computeShader.SetFloat("_EdgeSize", edgeSize);
            computeShader.Dispatch(_kernelHandle, Width / 8, Height / 8, 1);
        }

        private void OnDestroy()
        {
            _depthTexture1.Release();
            _depthTexture2.Release();
            _differDepthTexture.Release();
        }
    }
}