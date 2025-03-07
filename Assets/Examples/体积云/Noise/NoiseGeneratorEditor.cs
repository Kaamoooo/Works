using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NoiseGenerator), true)]
public class NoiseGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NoiseGenerator myScript = (NoiseGenerator) target;
        if (GUILayout.Button("Generate Textures"))
        {
            myScript.Generate();
        }
    }
}
#endif