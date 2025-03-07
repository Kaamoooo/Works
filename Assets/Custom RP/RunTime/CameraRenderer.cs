using System.Collections;
using System.Collections.Generic;
using Custom_RP.RunTime;
using Custom_RP.RunTime.Lighting;
using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    Camera _camera;
    Lighting _lighting = new Lighting();

    static ScriptableRenderContext _context;
    string _bufferName;
    static CommandBuffer _buffer = new CommandBuffer();

    static CullingResults _cullingResults;

    static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId _litShaderTagId = new ShaderTagId("CustomLit");


    public void Render(ref ScriptableRenderContext context, Camera camera, bool enableInstancing,
        bool enableDynamicBatching, ShadowSettings shadowSettings)
    {
        _context = context;
        _camera = camera;
        shadowSettings.shadowDistance = Mathf.Min(shadowSettings.shadowDistance, _camera.farClipPlane);

        //Gizmos
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.shadowDistance))
        {
            return;
        }

        _buffer.BeginSample(_bufferName);
        ExecuteBuffer();
        _lighting.SetUp(_context, _cullingResults,shadowSettings);
        _buffer.EndSample(_bufferName);
        
        SetUp();
        DrawVisibleGeometry(enableInstancing, enableDynamicBatching);
        DrawUnsupportedGeometry();
        DrawGizmos();
        Submit();
    }

    bool Cull(float shadowDistance)
    {
        if (_camera.TryGetCullingParameters(out ScriptableCullingParameters p))
        {
            _cullingResults = _context.Cull(ref p);
            p.shadowDistance = shadowDistance;
            return true;
        }

        return false;
    }

    void SetUp()
    {
        _bufferName = _camera.name;
        _buffer.name = _bufferName;
        _context.SetupCameraProperties(_camera);
        ClearRenderTarget();
        // ExecuteBuffer();
        _buffer.Clear();
    }

    private void ClearRenderTarget()
    {
        var clearFlags = _camera.clearFlags;
        _buffer.ClearRenderTarget(
            clearFlags <= CameraClearFlags.Depth,
            clearFlags <= CameraClearFlags.Color,
            Color.clear);
        _buffer.BeginSample(_bufferName);
    }

    private void DrawVisibleGeometry(bool enableInstancing, bool enableDynamicBatching)
    {
        //Opaque
        SortingSettings sortingSettings = new SortingSettings(_camera) {criteria = SortingCriteria.CommonOpaque};
        DrawingSettings drawingSettings = new DrawingSettings(_unlitShaderTagId, sortingSettings)
        {
            enableInstancing = enableInstancing,
            enableDynamicBatching = enableDynamicBatching
        };
        drawingSettings.SetShaderPassName(1, _litShaderTagId);
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);

        //Skybox
        _context.DrawSkybox(_camera);

        //Transparent
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        _context.DrawRenderers(_cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void Submit()
    {
        _buffer.EndSample(_bufferName);
        ExecuteBuffer();
        _context.Submit();
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}