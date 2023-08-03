using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SetSnowMaterial : MonoBehaviour
{
    public Texture2D snowTexture;
    void Start()
    {
        Shader.SetGlobalTexture("_SnowTex", snowTexture);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
