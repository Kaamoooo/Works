using System.Collections;
using System.Collections.Generic; //For list functionality
using UnityEngine;

public partial class NoiseGenerator : MonoBehaviour
{
    public ComputeTexture[] computeTextures2D;
    public ComputeTexture3D[] computeTextures3D;
}

#if UNITY_EDITOR
[AddComponentMenu("Noise/Noise Generator")]
public partial class NoiseGenerator : MonoBehaviour
{
    public void Generate()
    {
        foreach (ComputeTexture ct in computeTextures2D)
        {
            ct.CreateRenderTexture();
            ct.SetParameters();
            ct.SetTexture();
            ct.GenerateTexture();
            ct.SaveAsset();
        }

        foreach (ComputeTexture3D ct in computeTextures3D)
        {
            ct.CreateRenderTexture();
            ct.SetParameters();
            ct.SetTexture();
            ct.GenerateTexture();
            ct.SaveAsset();
        }
    }
}
#endif