using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Custom_RP.Editor
{
    public class CustomShaderGUI : ShaderGUI
    {
        MaterialEditor materialEditor;
        Object[] materials;
        MaterialProperty[] properties;

        BlendMode SrcBlend
        {
            set => SetProperty("_SrcBlend", (float) value);
        }

        BlendMode DstBlend
        {
            set => SetProperty("_DstBlend", (float) value);
        }

        bool ZWrite
        {
            set => SetProperty("_ZWrite", value ? 1f : 0f);
        }

        bool Clipping
        {
            set => SetProperty("_Clipping", "_CLIPPING", value);
        }

        bool PremultiplyAlpha
        {
            set => SetProperty("_Premultiply_Alpha", "_PREMULTIPLY_ALPHA", value);
        }

        RenderQueue RenderQueue
        {
            set
            {
                foreach (var o in materials)
                {
                    var m = (Material) o;
                    m.renderQueue = (int) value;
                }
            }
        }

        bool PresetButton(string name)
        {
            if (GUILayout.Button(name))
            {
                materialEditor.RegisterPropertyChangeUndo(name);
                return true;
            }

            return false;
        }

        void OpaquePreset()
        {
            if (PresetButton("Opaque"))
            {
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.Zero;
                ZWrite = true;
                RenderQueue = RenderQueue.Geometry;
            }
        }
        void AlphaBlendPreset()
        {
            if (PresetButton("Alpha Blend"))
            {
                Clipping = false;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.SrcAlpha;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }
        
        void AlphaTestPreset()
        {
            if (PresetButton("Alpha Test"))
            {
                Clipping = true;
                PremultiplyAlpha = false;
                SrcBlend = BlendMode.SrcAlpha;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = true;
                RenderQueue = RenderQueue.AlphaTest;
            }
        }

        void PremultiplyBRDF()
        {
            if (PresetButton("Premultiply BRDF"))
            {
                Clipping = false;
                PremultiplyAlpha = true;
                SrcBlend = BlendMode.One;
                DstBlend = BlendMode.OneMinusSrcAlpha;
                ZWrite = false;
                RenderQueue = RenderQueue.Transparent;
            }
        }
        
        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditor, properties);
            this.materialEditor = materialEditor;
            this.materials = materialEditor.targets;
            this.properties = properties;

            OpaquePreset();
            AlphaBlendPreset();
            AlphaTestPreset();
            PremultiplyBRDF();
        }

        void SetProperty(string name, float value)
        {
            FindProperty(name, properties).floatValue = value;
        }

        //The keyword also needs being set, instead of just the property
        //or even though the property is set, the shader will not work as expected.
        void SetProperty(string name, string keyword, bool value)
        {
            SetProperty(name, value? 1f : 0f);
            SetKeyword(keyword, value);
        }

        void SetKeyword(string name, bool enabled)
        {
            foreach (Object material in materials)
            {
                if (enabled)
                {
                    (material as Material)?.EnableKeyword(name);
                }
                else
                {
                    (material as Material)?.DisableKeyword(name);
                }
            }
        }
    }
}