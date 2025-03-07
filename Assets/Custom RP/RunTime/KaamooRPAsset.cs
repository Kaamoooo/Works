using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Custom_RP.RunTime
{
    [CreateAssetMenu(menuName = "Rendering/Kaamoo Render Pipeline")]
    public class KaamooRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] bool _enableInstancing = true, _enableDynamicBatching = true, _enableSRPBatching = true;
        
        [SerializeField] ShadowSettings _shadowSettings = default;
        
        protected override RenderPipeline CreatePipeline()
        {
            return new KaamooRP(_enableSRPBatching,_enableInstancing, _enableDynamicBatching, _shadowSettings);
        }
    }
    
    [Serializable]
    public class ShadowSettings
    {
        [Min(0f)] public float shadowDistance = 100f;
        public enum TextureSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
            _8192 = 8192
        }
        
        [Serializable]
        public struct Directional
        {
            public TextureSize atlasSize;
        }
        
        public Directional directional = new Directional {atlasSize = TextureSize._1024};
    }
}