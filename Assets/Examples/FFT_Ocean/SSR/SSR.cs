using System;
using System.Collections;
using System.Collections.Generic;
using Accord;
using UnityEngine;

public class SSR : MonoBehaviour
{
    public string m_DepthBlitRenderFeatureName = "DepthBlitRenderFeature";
    
    void Start()
    {
       // RendererFeatureSetter.SetFeature(m_DepthBlitRenderFeatureName,true);
    }
    

    private void OnDestroy()
    {
        // RendererFeatureSetter.SetFeature(m_DepthBlitRenderFeatureName,false);
    }
}
