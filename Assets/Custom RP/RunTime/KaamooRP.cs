using System.Collections;
using System.Collections.Generic;
using Custom_RP.RunTime;
using UnityEngine;
using UnityEngine.Rendering;

public class KaamooRP : RenderPipeline
{
    private readonly CameraRenderer _renderer = new CameraRenderer();
    private bool _enableInstancing;
    private bool _enableDynamicBatching;
    ShadowSettings _shadowSettings;

    public KaamooRP(bool useSRPBatching, bool enableInstancing, bool enableDynamicBatching,ShadowSettings shadowSettings)
    {
        this._enableDynamicBatching = enableDynamicBatching;
        this._enableInstancing = enableInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatching;
        GraphicsSettings.lightsUseLinearIntensity = true;
        _shadowSettings = shadowSettings;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        throw new System.NotImplementedException();
    }

    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        foreach (var t in cameras)
        {
            _renderer.Render(ref context, t, _enableInstancing, _enableDynamicBatching,_shadowSettings);
        }
    }
}