using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;

public class FFT_Ocean : MonoBehaviour
{
    public Material m_OceanMaterial;
    public ComputeShader m_GaussianComputeShader;
    public ComputeShader m_H0OmegaComputeShader;
    public ComputeShader m_DisplacementSpectrumComputeShader;
    public ComputeShader m_NormalComputeShader;
    public ComputeShader m_IFFTComputeShader;

    private const int TextureSize = 512;
    private RenderTexture m_gaussianTexture;
    private RenderTexture m_h0OmegaTexture;
    private RenderTexture m_verticalSpectrumTexture;
    private RenderTexture m_horizontalXSpectrumTexture;
    private RenderTexture m_horizontalZSpectrumTexture;
    private RenderTexture m_gradientXSpectrumTexture;
    private RenderTexture m_gradientZSpectrumTexture;
    
    private RenderTexture m_verticalDisplacementTexture;
    private RenderTexture m_horizontalXDisplacementTexture;
    private RenderTexture m_horizontalZDisplacementTexture;
    private RenderTexture m_gradientXTexture;
    private RenderTexture m_gradientZTexture;
    private RenderTexture m_displacementTexture;
    private RenderTexture m_normalTexture;
    private RenderTexture m_bubbleTexture;
    private float m_oceanSize;

    public float m_VerticalScale = 1.0f;
    public float m_HorizontalScale = 1.0f;
    public float A = 0.01f;
    public Vector2 m_WindDirection = new Vector2(1, 0);
    public float windSpeed = 10.0f;
    public float BubbleThreshold = 0.5f;
    public float BubbleScale = 0.1f;

    public GameObject m_OceanPlane;

    void Start()
    {
        InitRT();
        InitData();
    }

    void Update()
    {
        float _time = Time.time;
        UpdateSpectrum(_time);
        UpdateHeightField();
    }

    private void InitRT()
    {
        m_gaussianTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_gaussianTexture.enableRandomWrite = true;
        m_gaussianTexture.Create();

        m_h0OmegaTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_h0OmegaTexture.enableRandomWrite = true;
        m_h0OmegaTexture.Create();

        m_verticalSpectrumTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_verticalSpectrumTexture.enableRandomWrite = true;
        m_verticalSpectrumTexture.Create();

        m_horizontalXSpectrumTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_horizontalXSpectrumTexture.enableRandomWrite = true;
        m_horizontalXSpectrumTexture.Create();

        m_horizontalZSpectrumTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_horizontalZSpectrumTexture.enableRandomWrite = true;
        m_horizontalZSpectrumTexture.Create();

        m_verticalDisplacementTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_verticalDisplacementTexture.enableRandomWrite = true;
        m_verticalDisplacementTexture.Create();

        m_horizontalXDisplacementTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_horizontalXDisplacementTexture.enableRandomWrite = true;
        m_horizontalXDisplacementTexture.Create();

        m_horizontalZDisplacementTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_horizontalZDisplacementTexture.enableRandomWrite = true;
        m_horizontalZDisplacementTexture.Create();

        m_normalTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_normalTexture.enableRandomWrite = true;
        m_normalTexture.Create();

        m_bubbleTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_bubbleTexture.enableRandomWrite = true;
        m_bubbleTexture.Create();
        
        m_displacementTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_displacementTexture.enableRandomWrite = true;
        m_displacementTexture.Create();
        
        m_gradientXSpectrumTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_gradientXSpectrumTexture.enableRandomWrite = true;
        m_gradientXSpectrumTexture.Create();
        
        m_gradientZSpectrumTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_gradientZSpectrumTexture.enableRandomWrite = true;
        m_gradientZSpectrumTexture.Create();
        
        m_gradientXTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_gradientXTexture.enableRandomWrite = true;
        m_gradientXTexture.Create();
        
        m_gradientZTexture = new RenderTexture(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
        m_gradientZTexture.enableRandomWrite = true;
        m_gradientZTexture.Create();
    }

    private void InitData()
    {
        m_oceanSize = m_OceanPlane.GetComponent<MeshFilter>().mesh.bounds.size.x * m_OceanPlane.transform.localScale.x;

        m_OceanMaterial.SetTexture("_DisplacementRT", m_displacementTexture);
        m_OceanMaterial.SetTexture("_NormalRT", m_normalTexture);
        m_OceanMaterial.SetTexture("_BubbleRT", m_bubbleTexture);

        m_GaussianComputeShader.SetTexture(0, "GaussianRT", m_gaussianTexture);
        m_GaussianComputeShader.Dispatch(0, TextureSize / 8, TextureSize / 8, 1);

        m_H0OmegaComputeShader.SetTexture(0, "H0OmegaRT", m_h0OmegaTexture);
        m_H0OmegaComputeShader.SetTexture(0, "GaussianRT", m_gaussianTexture);
        m_H0OmegaComputeShader.SetInt("N", TextureSize);
        m_H0OmegaComputeShader.SetFloat("A", A);

        Vector2 _windDirection = new Vector2(m_WindDirection.x, m_WindDirection.y).normalized;
        m_H0OmegaComputeShader.SetVector("Wind", new Vector4(_windDirection.x, _windDirection.y, windSpeed, 0.0f));
        m_H0OmegaComputeShader.Dispatch(0, TextureSize / 8, TextureSize / 8, 1);
    }

    private void UpdateSpectrum(float _time)
    {
        m_DisplacementSpectrumComputeShader.SetFloat("t", _time);
        m_DisplacementSpectrumComputeShader.SetInt("N", TextureSize);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "H0OmegaRT", m_h0OmegaTexture);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "VerticalSpectrumTexture", m_verticalSpectrumTexture);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "HorizontalXSpectrumTexture", m_horizontalXSpectrumTexture);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "HorizontalZSpectrumTexture", m_horizontalZSpectrumTexture);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "GradientXSpectrumTexture", m_gradientXSpectrumTexture);
        m_DisplacementSpectrumComputeShader.SetTexture(0, "GradientZSpectrumTexture", m_gradientZSpectrumTexture);
        m_DisplacementSpectrumComputeShader.Dispatch(0, TextureSize / 8, TextureSize / 8, 1);
    }

    private void UpdateHeightField()
    {
        void IFFT(ref RenderTexture _inputRT, ref RenderTexture _outputRT, float _scale)
        {
            float _fftStages = Mathf.Log(1.0f * TextureSize,2.0f);
            m_IFFTComputeShader.SetInt("N", TextureSize);
            RenderTexture _tempRT = RenderTexture.GetTemporary(TextureSize, TextureSize, 0, GraphicsFormat.R32G32B32A32_SFloat);
            _tempRT.enableRandomWrite = true;
            for (int i = 0; i < Mathf.RoundToInt(_fftStages); i++)
            {
                int _stepLength = 1 << (i + 1) ;
                m_IFFTComputeShader.SetInt("StepLength", _stepLength);
                m_IFFTComputeShader.SetTexture(0,"InputRT",_inputRT);
                m_IFFTComputeShader.SetTexture(0,"OutputRT",_tempRT);
                m_IFFTComputeShader.Dispatch(0, TextureSize / 8, TextureSize / 8, 1);
                (_inputRT, _tempRT) = (_tempRT, _inputRT);
            }
        
            for (int i = 0; i < Mathf.RoundToInt(_fftStages); i++)
            {
                int _stepLength = 1 << (i + 1) ;
                m_IFFTComputeShader.SetInt("StepLength", _stepLength);
                m_IFFTComputeShader.SetTexture(1,"InputRT",_inputRT);
                m_IFFTComputeShader.SetTexture(1,"OutputRT",_tempRT);
                m_IFFTComputeShader.Dispatch(1, TextureSize / 8, TextureSize / 8, 1);
                (_inputRT, _tempRT) = (_tempRT, _inputRT);
            }
            m_IFFTComputeShader.SetTexture(2,"InputRT",_inputRT);
            m_IFFTComputeShader.SetTexture(2, "OutputRT", _outputRT);
            m_IFFTComputeShader.SetFloat("Scale", _scale);
            m_IFFTComputeShader.Dispatch(2, TextureSize / 8, TextureSize / 8, 1);
            RenderTexture.ReleaseTemporary(_tempRT);
        }
       
        IFFT(ref m_verticalSpectrumTexture, ref m_verticalDisplacementTexture,m_VerticalScale);
        IFFT(ref m_horizontalXSpectrumTexture, ref m_horizontalXDisplacementTexture,m_HorizontalScale);
        IFFT(ref m_horizontalZSpectrumTexture, ref m_horizontalZDisplacementTexture,m_HorizontalScale);
        IFFT(ref m_gradientXSpectrumTexture, ref m_gradientXTexture,m_VerticalScale);
        IFFT(ref m_gradientZSpectrumTexture, ref m_gradientZTexture,m_VerticalScale);
        
        m_NormalComputeShader.SetInt("N", TextureSize);
        m_NormalComputeShader.SetFloat("OceanSize", m_oceanSize);
        m_NormalComputeShader.SetFloat("BubbleThreshold", BubbleThreshold);
        m_NormalComputeShader.SetFloat("BubbleScale", BubbleScale);
        m_NormalComputeShader.SetFloat("HorizontalScale", m_HorizontalScale);
        m_NormalComputeShader.SetTexture(0, "VerticalRT", m_verticalDisplacementTexture);
        m_NormalComputeShader.SetTexture(0, "HorizontalXRT", m_horizontalXDisplacementTexture);
        m_NormalComputeShader.SetTexture(0, "HorizontalZRT", m_horizontalZDisplacementTexture);
        m_NormalComputeShader.SetTexture(0, "GradientXRT", m_gradientXTexture);
        m_NormalComputeShader.SetTexture(0, "GradientZRT", m_gradientZTexture);
        
        m_NormalComputeShader.SetTexture(0, "DisplacementRT", m_displacementTexture);
        m_NormalComputeShader.SetTexture(0, "NormalRT", m_normalTexture);
        m_NormalComputeShader.SetTexture(0, "BubbleRT", m_bubbleTexture);
        m_NormalComputeShader.Dispatch(0, TextureSize / 8, TextureSize / 8, 1);

        // {
        //     Texture2D texture2D = new Texture2D(512, 512, TextureFormat.RGBA32_SIGNED, false);
        //     RenderTexture currentRT = RenderTexture.active;
        //     RenderTexture.active = m_verticalDisplacementTexture;
        //
        //     // 读取像素
        //     texture2D.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        //
        //     // 应用读取的像素到Texture2D
        //     texture2D.Apply();
        //
        //     // 恢复渲染目标
        //     RenderTexture.active = currentRT;
        //
        //     // 获取像素数据
        //     Color[] pixels = texture2D.GetPixels();
        //
        //     // 例如，打印第一个像素的值
        //     Debug.Log(pixels[90000]);
        // }
    }
}