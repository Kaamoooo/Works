using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RendererFeatureSetter
{
    public const string RENDERER_ASSET_PATH = "Assets/Settings/URP-Balanced-Renderer.asset";
    public static void SetFeature(string name,bool active)
    {
        UniversalRendererData _rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RENDERER_ASSET_PATH);
        // if (scriptableRendererFeature == null)
        // {
            // Debug.Log("Invalid renderer feature");
            // return;
        // }

        List<ScriptableRendererFeature> _rendererFeatures = _rendererData.rendererFeatures;
        foreach (ScriptableRendererFeature _feature in _rendererFeatures)
        {
            if (_feature.GetType().Name == name)
            {
                _feature.SetActive(active);
                EditorUtility.SetDirty(_rendererData);
                AssetDatabase.SaveAssets();
                break;
            }
        }
    }
}
