using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GPUInstancing : MonoBehaviour
{
    public Mesh instancedMesh;
    public Material instancedMaterial;
    
    const int InstancedNumber = 1023;

    private Matrix4x4[] _matrices = new Matrix4x4[InstancedNumber];
    private Vector4[] _baseColors = new Vector4[InstancedNumber];
    private float[] _metallic = new float[InstancedNumber];
    private float[] _smoothness = new float[InstancedNumber];

    MaterialPropertyBlock _propertyBlock;

    private void Awake()
    {
        for (int i = 0; i < _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10,
                Quaternion.Euler(Random.value * 360.0f, Random.value * 360.0f, Random.value * 360.0f), Vector3.one);
            _baseColors[i] = new Color(Random.value, Random.value, Random.value, Random.Range(0.8f, 1));
            _metallic[i] = Random.value;
            _smoothness[i] = Random.value;
        }
    }

    private void Update()
    {
        if (_propertyBlock == null)
        {
            _propertyBlock = new MaterialPropertyBlock();
            _propertyBlock.SetVectorArray(PerObjectMaterialProperties._colorID, _baseColors);
            _propertyBlock.SetFloatArray(PerObjectMaterialProperties._metallicID, _metallic);
            _propertyBlock.SetFloatArray(PerObjectMaterialProperties._smoothnessID, _smoothness);
        }

        Graphics.DrawMeshInstanced(instancedMesh, 0, instancedMaterial, _matrices, InstancedNumber, _propertyBlock);
    }
}