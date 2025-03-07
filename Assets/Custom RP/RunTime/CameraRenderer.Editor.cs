using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    partial void DrawUnsupportedGeometry();
    partial void DrawGizmos();
    partial void PrepareForSceneWindow();

#if UNITY_EDITOR

    static ShaderTagId[] _legacyShaderTagIds =
    {
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Always"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

    private static Material _errorMaterial;

    partial void DrawUnsupportedGeometry()
    {
        if (!_errorMaterial)
        {
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        DrawingSettings drawingSettings = new DrawingSettings(_legacyShaderTagIds[0], new SortingSettings(_camera));
        for (int i = 1; i < _legacyShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, _legacyShaderTagIds[i]);
        }

        drawingSettings.overrideMaterial = _errorMaterial;
        FilteringSettings filteringSettings = FilteringSettings.defaultValue;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    partial void DrawGizmos()
    {
        if (UnityEditor.Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
    }

    partial void PrepareForSceneWindow()
    {
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
    }
#endif
}