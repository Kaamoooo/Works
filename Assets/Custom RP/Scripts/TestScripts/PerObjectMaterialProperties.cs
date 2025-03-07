using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    [SerializeField] private Color _color;
    [SerializeField,Range(0,1)] private float _smoothness;
    [SerializeField,Range(0,1)] private float _metallic;
    [SerializeField,Range(0,1)] private float _cutoff;
    
    public static readonly int _colorID = Shader.PropertyToID("_BaseColor");
    public static readonly int _cutoffID = Shader.PropertyToID("_Cutoff");
    public static readonly int _metallicID = Shader.PropertyToID("_Metallic");
    public static readonly int _smoothnessID = Shader.PropertyToID("_Smoothness");
    
    
    private static MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
        }
        _propertyBlock.SetFloat(_cutoffID,_cutoff);
        _propertyBlock.SetColor(_colorID, _color);
        _propertyBlock.SetFloat(_metallicID, _metallic);
        _propertyBlock.SetFloat(_smoothnessID, _smoothness);
        GetComponent<Renderer>().SetPropertyBlock(_propertyBlock);
    }
}
