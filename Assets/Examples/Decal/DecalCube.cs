using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteAlways]
public class DecalCube : MonoBehaviour
{
    public Texture2D m_DecalOpacityTexture;
    public Texture2D m_DecalColorTexture;
    public Shader m_DecalShader;

    private Material m_DecalMaterial;

    private void Start()
    {
        m_DecalMaterial = new Material(m_DecalShader);
        GetComponent<MeshRenderer>().material = m_DecalMaterial;
        m_DecalMaterial.SetTexture("_DecalColorTexture",m_DecalColorTexture);
        m_DecalMaterial.SetTexture("_DecalOpacityTexture",m_DecalOpacityTexture);
    }

    private void Update()
    {
        m_DecalMaterial.SetVector("_CubeScale",transform.localScale);
    }
}