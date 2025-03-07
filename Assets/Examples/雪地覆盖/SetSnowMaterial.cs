using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetSnowMaterial : MonoBehaviour
{
    public Texture2D snowTexture;
    public Material[] snowMaterials;
    public float snowHeight=0.5f; 
    void Start()
    {
        Shader.SetGlobalTexture("_SnowTex", snowTexture);
    }

    // Update is called once per frame
    void Update()
    {
        for (var i = 0; i < snowMaterials.Length; i++)
        {
            snowMaterials[i].SetFloat("_SnowHeight",snowHeight);
        }
    }
}
