﻿///////////////////////////////////////////////////////////////////////////////
//Copyright 2017 Google Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace daydreamrenderer
{
[CustomEditor(typeof(DaydreamRenderer))]
public class DaydreamMenu : Editor
{
    static private class Styles
    {
        public const string kEnableEnlightenString = "Enable Unity Lighting";
        public const string kEnableDaydreamString = "Enable Daydream Lighting";

        static public GUIContent m_shadowSettingsUI     = new GUIContent("Shadow Settings", "Adjust global shadow settings.");
        static public GUIContent m_shadowWidthUI        = new GUIContent("Width ", "Shadow map width in pixels.");
        static public GUIContent m_shadowHeightUI       = new GUIContent("Height", "Shadow map height in pixels.");
        static public GUIContent m_shadowSharpnessUI    = new GUIContent("Sharpness", "Sharpness of the shadow edges - 0 is blurry, 1 is sharp (possibly pixelated)");
        static public GUIContent m_maxShadowCastersUI   = new GUIContent("Shadow Count", "The maximum number of shadow casting objects.");
        static public GUIContent m_ambientSettingsUI    = new GUIContent("Dynamic Ambient Settings", "Adjust global ambient settings.");
        static public GUIContent m_ambientUpUI          = new GUIContent("Ambient Sky (up)  ", "Ambient color of normals pointing up (towards the 'sky') (+y).");
        static public GUIContent m_ambientDownUI        = new GUIContent("Ambient Sky (down)  ", "Ambient color of normals pointing down (towards the 'ground') (-y).");
        static public GUIContent m_showFpsUI            = new GUIContent("Show FPS", "Enable to show average FPS and frame time.");
        static public GUIContent m_convertMtlUI         = new GUIContent("Open Material Wizard", "Convert Standard materials to Daydream materials.");
        static public GUIContent m_enableEnlightenUI    = new GUIContent("Enable Unity Lighting", "Enable Unity Lighting support.");
        static public GUIContent m_enableDaydreamUI     = new GUIContent("Enable Daydream Lighting", "Enable Daydream Lighting support.");

        static public GUIContent m_fogSettingsUI        = new GUIContent("Fog settings", "Adjust the global fog settings.");
        static public GUIContent m_fogEnable            = new GUIContent("Fog enable", "Enables global fog.");
        static public GUIContent m_fogHeightEnable      = new GUIContent("Height fog enable", "Enables height fog - fog that becomes less dense higher from the base.");
        static public GUIContent m_fogModeLabel         = new GUIContent("Mode", "Falloff mode used by the fog.");
        static public GUIContent m_fogNear              = new GUIContent("Near", "The distance at which the fog begins to take effect.");
        static public GUIContent m_fogFar               = new GUIContent("Far",  "The distance at which the fog is at full opacity.");
        static public GUIContent m_fogOpacity           = new GUIContent("Opacity", "The opacity of the fog; 0 = transparent, 1 = opaque.");
        static public GUIContent m_fogColorScale        = new GUIContent("Color scale", "Controls how much fog must be visible before seeing the far color.");
        static public GUIContent m_fogMinHeight         = new GUIContent("Min height", "The lowest point; the point at which fog thickness is maximum.");
        static public GUIContent m_fogMaxHeight         = new GUIContent("Max height", "The highest point; the point at which fog thickness is zero.");
        static public GUIContent m_fogThickness         = new GUIContent("Thickness",  "Controls how quickly the fog thins out with height.");
        static public GUIContent m_fogDensity           = new GUIContent("Density", "The overall fog density, larger values will cause fog to become opaque closer.");

        static public GUIContent m_fogColorNear         = new GUIContent("Near color", "The color of the fog where the fog is thin.");
        static public GUIContent m_fogColorFar          = new GUIContent("Far color",  "The color of the fog where the fog is thick.");

        static public GUIContent[] m_fogModes = new GUIContent[3] { null, null, null };
        static public GUIContent[] m_lightingSystemUI;
        static public GUILayoutOption[] m_convertMtlLayout = new GUILayoutOption[] { GUILayout.Width(200) };
        static public GUILayoutOption[] m_enlightenLayout = new GUILayoutOption[] { GUILayout.Width(120) };
        static public GUILayoutOption[] m_drLightingLayout = new GUILayoutOption[] { GUILayout.Width(175) };

        public static GUIStyle m_buttonUnselected = new GUIStyle(EditorStyles.toolbar);
        public static GUIStyle m_buttonSelected = new GUIStyle(EditorStyles.toolbarButton);
        public static GUIStyle m_sectionLabel;

        static private bool s_initialized = false;

        static public void Init()
        {
            if (s_initialized) { return; }

            string[] _choices = new[] { "Linear", "Exponential", "Exponential Squared" };
            string[] _choiceTooltips = new[] { "Linear fog which is transparent before the start depth and opaque at the end depth.", "Exponential fog which uses density rather than start/end depths.", "Squared exponential fog which uses density rather than start/end depths." };

            for (int i = 0; i < 3; i++)
            {
                m_fogModes[i] = new GUIContent(_choices[i], _choiceTooltips[i]);
            }

            s_initialized = true;
        }

        static Styles()
        {
            m_buttonUnselected.alignment = m_buttonSelected.alignment;
            m_buttonSelected.normal = m_buttonSelected.active;
            m_lightingSystemUI = new GUIContent[] { Styles.m_enableDaydreamUI, Styles.m_enableEnlightenUI };
            m_sectionLabel = new GUIStyle(GUI.skin.label);
            m_sectionLabel.fontSize = 12;
            m_sectionLabel.contentOffset = new Vector2(0f, 2.5f);
        }
    }
        
    const float c_specSmoothnessThreshold = 0.1f;
    const float c_lowSpecPowerThreshold = 0.4f;
    const float c_medSpecPowerThreshold = 0.6f;

    enum BlendModeStandard
    {
        Opaque = 0,
        Cutout = 1,
        Fade = 2,
        Transparent = 3,
        Additive = 4,
        Multiply = 5,
    }

#if UNITY_EDITOR
    public void OnEnable()
    {
        DaydreamRenderer renderer = target as DaydreamRenderer;
        renderer.m_uiEnabled = true;
    }

    public void OnDisable()
    {
        DaydreamRenderer renderer = target as DaydreamRenderer;
        renderer.m_uiEnabled = false;
    }
#endif

    public override void OnInspectorGUI()
    {
        bool settingsChanged = false;
        DaydreamRenderer renderer = target as DaydreamRenderer;
        Styles.Init();

        //This will add the material to the undo list, at the end of the frame Unity will check if its really changed and if so add it to the undo stack.
        //Since Unity does a binary comparison between the copied data and the original material - it should accurately determine if it has changed.
        //And since we're editing one material at a time, the extra memory (one extra copy of the Material) - the memory cost is reasonable.
        Undo.RecordObject(renderer, "Daydream Renderer Settings");
        
        DaydreamRendererImportManager.DrawDaydreamLightingToggle(renderer);
        //DaydreamRendererImportManager.DrawSection(500, 1);
        GUILayout.Space(5);

        // Material Conversion
        DaydreamRendererImportManager.DrawSection(500, 1);
        EditorGUILayout.LabelField(DaydreamRendererImportManager.Styles.kConversionWizardSegment, Styles.m_sectionLabel, GUILayout.Height(25));
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(DaydreamRendererImportManager.Styles.kOpenMaterialWizard, EditorStyles.toolbar))
        {
            MaterialConversionDialog.ShowDialog(null);
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);

        renderer.m_shadowSettings = EditorGUILayout.Foldout(renderer.m_shadowSettings, Styles.m_shadowSettingsUI);
        if (renderer.m_shadowSettings)
        {
            EditorGUI.indentLevel = 1;

            EditorGUI.BeginChangeCheck();
            renderer.m_shadowWidth  = EditorGUILayout.IntField(Styles.m_shadowWidthUI,   renderer.m_shadowWidth);
            renderer.m_shadowHeight = EditorGUILayout.IntField(Styles.m_shadowHeightUI,  renderer.m_shadowHeight);
            renderer.m_sharpness    = EditorGUILayout.Slider(Styles.m_shadowSharpnessUI, renderer.m_sharpness, 0.0f, 1.0f);
            renderer.m_maxShadowCasters = EditorGUILayout.IntSlider(Styles.m_maxShadowCastersUI, renderer.m_maxShadowCasters, 0, 4);
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
            }

            EditorGUI.indentLevel = 0;

            renderer.UpdateFilterParam();
        }

        renderer.m_ambientSettings = EditorGUILayout.Foldout(renderer.m_ambientSettings, Styles.m_ambientSettingsUI);
        if (renderer.m_ambientSettings)
        {
            EditorGUI.indentLevel = 1;

            EditorGUI.BeginChangeCheck();
            renderer.m_globalAmbientUp = EditorGUILayout.ColorField(Styles.m_ambientUpUI,   renderer.m_globalAmbientUp);
            renderer.m_globalAmbientDn = EditorGUILayout.ColorField(Styles.m_ambientDownUI, renderer.m_globalAmbientDn);
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
                renderer.UpdateAmbientParam();
                EditorUtility.SetDirty(renderer);
            }

            EditorGUI.indentLevel = 0;
        }

        renderer.m_fogSettings = EditorGUILayout.Foldout(renderer.m_fogSettings, Styles.m_fogSettingsUI);
        if (renderer.m_fogSettings)
        {
            EditorGUI.indentLevel = 1;

            EditorGUI.BeginChangeCheck();
            renderer.m_fogEnable       = EditorGUILayout.Toggle(Styles.m_fogEnable, renderer.m_fogEnable);
            renderer.m_heightFogEnable = EditorGUILayout.Toggle(Styles.m_fogHeightEnable, renderer.m_heightFogEnable);
            renderer.m_fogMode         = EditorGUILayout.Popup(Styles.m_fogModeLabel, renderer.m_fogMode, Styles.m_fogModes);

            EditorGUILayout.Space();

            if (renderer.m_fogMode == DaydreamRenderer.FogMode.Linear)
            {
                renderer.m_fogLinear.x = EditorGUILayout.Slider(Styles.m_fogNear, renderer.m_fogLinear.x, 0.0f, 1000.0f);
                renderer.m_fogLinear.y = EditorGUILayout.Slider(Styles.m_fogFar, renderer.m_fogLinear.y, 0.0f, 10000.0f);
            }
            if (renderer.m_fogMode != DaydreamRenderer.FogMode.Linear)
            {
                renderer.m_fogHeight.w = EditorGUILayout.Slider(Styles.m_fogDensity, renderer.m_fogHeight.w, 0.0f, 1.0f);
            }

            renderer.m_fogLinear.z = EditorGUILayout.Slider(Styles.m_fogOpacity, renderer.m_fogLinear.z, 0.0f, 1.0f);
            renderer.m_fogLinear.w = EditorGUILayout.Slider(Styles.m_fogColorScale, renderer.m_fogLinear.w, 0.0f, 4.0f);

            if (renderer.m_heightFogEnable)
            {
                renderer.m_fogHeight.x = EditorGUILayout.Slider(Styles.m_fogMinHeight, renderer.m_fogHeight.x, -100.0f, 100.0f);
                renderer.m_fogHeight.y = EditorGUILayout.Slider(Styles.m_fogMaxHeight, renderer.m_fogHeight.y, -100.0f, 100.0f);
                renderer.m_fogHeight.z = EditorGUILayout.Slider(Styles.m_fogThickness, renderer.m_fogHeight.z,    0.0f, 100.0f);
            }

            renderer.m_fogColorNear = EditorGUILayout.ColorField(Styles.m_fogColorNear, renderer.m_fogColorNear);
            renderer.m_fogColorFar  = EditorGUILayout.ColorField(Styles.m_fogColorFar,  renderer.m_fogColorFar);
            if (EditorGUI.EndChangeCheck())
            {
                settingsChanged = true;
                renderer.UpdateFogParam();
                EditorUtility.SetDirty(renderer);
            }

            EditorGUI.indentLevel = 0;
        }

        renderer.m_showFPS = EditorGUILayout.Toggle(Styles.m_showFpsUI, renderer.m_showFPS);

    #if UNITY_EDITOR
        if (settingsChanged && !Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    #endif
    }

    private static void SaveAssetsAndFreeMemory()
    {
        UnityEditor.AssetDatabase.SaveAssets();
        System.GC.Collect();
        UnityEditor.EditorUtility.UnloadUnusedAssetsImmediate();
        UnityEditor.AssetDatabase.Refresh();
    }

    public static bool IsConvertible(Shader shader)
    {
        string name = shader.name;

        if (name.Equals("Standard") || name.Equals("Standard (Specular setup)") || name.Equals("Legacy Shaders/Bumped Diffuse") || name.Equals("Legacy Shaders/Transparent/Diffuse")
            || name.Equals("Legacy Shaders/Self-Illumin/Specular")
            || name.Equals("Reflective/Diffuse Transperant")
            || name.Equals("Reflective/Diffuse Reflection Spec Transp")
            || name.Equals("Legacy Shaders/Bumped Specular")
            || name.Equals("Legacy Shaders/Bumped/Specular")
            || name.Equals("Legacy Shaders/Reflective/Specular")
            || name.Equals("Legacy Shaders/Reflective/Bumped Specular")
            || name.Equals("Mobile/Bumped Diffuse") || name.Equals("Mobile/Bumped Specular") || name.Equals("Mobile/Bumped Specular (1 Directional Light)")
            || name.Equals("Mobile/Bumped Diffuse") || name.Equals("Mobile/Bumped Specular") || name.Equals("Mobile/Bumped Specular (1 Directional Light)")
            || name.Equals("Mobile/Diffuse") || name.Equals("Mobile/VertexLit") || name.Equals("Mobile/VertexLit (Only Directional Lights)")
            || name.Equals("Unlit/Color") || name.Equals("Unlit/Texture") || name.Equals("Unlit/Transparent") || name.Equals("Unlit/Transparent Cutout") || name.Equals("Mobile/Unlit (Supports Lightmap)")
            || name.Equals("Mobile/Particles/Additive") || name.Equals("Mobile/Particles/Alpha Blended") || name.Equals("Mobile/Particles/Multiply") || name.Equals("Mobile/Particles/VertexLit Blended")
            || name.Equals("Particles/Additive") || name.Equals("Particles/Additive (Soft)") || name.Equals("Particles/Alpha Blended") || name.Equals("Particles/Alpha Blended Premultiply")
            || name.Equals("Particles/Multiply") || name.Equals("Particles/VertexLit Blended"))
        {
            return true;
        }

        return false;
    }

    public static bool StandardToDaydreamSingleMaterial(Material m, Shader srcShader, Shader dstShader)
    {
        bool materialModified = false;
        string name = srcShader.name;

        if (name.Equals(dstShader.name))
        {
            //This material is already using the Daydream material, note that we don't have to set 'materialModified' to false here but the intention is a bit more clear.
            materialModified = false;
        }
        else if (name.Equals("Standard") || name.Equals("Standard (Specular setup)") || name.Equals("Legacy Shaders/Bumped Diffuse") || name.Equals("Legacy Shaders/Transparent/Diffuse"))
        {
			Texture albedoMap = m.GetTexture("_MainTex");
			Color baseColor = m.GetColor("_Color");
            Texture normalMap = m.GetTexture("_BumpMap");
            float smoothness = m.GetFloat("_Glossiness");
            float specular = m.GetFloat("_SpecularHighlights");
            float renderMode = m.GetFloat("_Mode");
			Color emissionClr = m.GetColor("_EmissionColor");
			Texture emissionMap = m.GetTexture("_EmissionMap");

            float emissive = Mathf.Max(emissionClr.r, Mathf.Max(emissionClr.g, emissionClr.b));

            m.shader = dstShader;

            // in case emission texture is used in place of main texture
            if(albedoMap == null && emissionMap != null)
            {
                m.SetTexture("_MainTex", emissionMap);
            }

            //copy over parameters.
            m.SetColor("_BaseColor", baseColor);
            m.SetTexture("_NormalTex", normalMap);
            if (normalMap == null)
            {
                m.DisableKeyword("NORMALMAP");
            }
            else
            {
                m.EnableKeyword("NORMALMAP");
            }

            m.SetFloat("_Emissive", emissive);
            m.SetFloat("_SpecSmoothness", smoothness);
            if (smoothness > c_specSmoothnessThreshold && specular > 0.5f)
            {
                m.EnableKeyword("SPECULAR");
                if (smoothness < c_lowSpecPowerThreshold)
                {
                    m.EnableKeyword("SPECULAR_FIXED_LOW");
                    m.DisableKeyword("SPECULAR_FIXED_MED");
                }
                else if (smoothness < c_medSpecPowerThreshold)
                {
                    m.EnableKeyword("SPECULAR_FIXED_MED");
                    m.DisableKeyword("SPECULAR_FIXED_LOW");
                }
                else
                {
                    m.DisableKeyword("SPECULAR_FIXED_LOW");
                    m.DisableKeyword("SPECULAR_FIXED_MED");
                }
            }
            else
            {
                m.DisableKeyword("SPECULAR");
            }

            m.DisableKeyword("DISABLE_LIGHTING");
            m.EnableKeyword("MAX_LIGHT_COUNT_8");
            m.EnableKeyword("DYNAMIC_AMBIENT");
            m.SetTexture("_Cube", null);

            SetupMaterialWithBlendMode(m, (BlendModeStandard)renderMode);

            materialModified = true;
        }
        else if (name.Equals("Legacy Shaders/Self-Illumin/Specular")
            || name.Equals("Reflective/Diffuse Transperant") 
            || name.Equals("Reflective/Diffuse Reflection Spec Transp")
            || name.Equals("Legacy Shaders/Bumped Specular")
            || name.Equals("Legacy Shaders/Bumped/Specular")
            || name.Equals("Legacy Shaders/Reflective/Specular")
            || name.Equals("Legacy Shaders/Reflective/Bumped Specular"))
        {

            Color baseColor = m.GetColor("_Color");
            Texture normalMap = m.GetTexture("_BumpMap");
            Texture cubeMap = m.GetTexture("_Cube");
            float smoothness = m.GetFloat("_Shininess");
            float renderMode = m.GetFloat("_Mode");
            float emission = m.GetFloat("_Emission");
            
            m.shader = dstShader;
            //copy over parameters.
            m.SetColor("_BaseColor", baseColor);
            m.SetTexture("_NormalTex", normalMap);
            if (normalMap == null)
            {
                m.DisableKeyword("NORMALMAP");
            }
            else
            {
                m.EnableKeyword("NORMALMAP");
            }

            if(name.Contains("Self-Illumin"))
            {
                m.SetFloat("_Emissive", Mathf.Min(1.0f, emission));
            }

            m.EnableKeyword("SPECULAR");
            // unity squares the their smoothness daydream does not
            m.SetFloat("_SpecSmoothness", Mathf.Sqrt(Mathf.Clamp(smoothness, 0.001f, 1.0f)));
            m.EnableKeyword("SPECULAR_FIXED_LOW");
            m.DisableKeyword("SPECULAR_FIXED_MED");

            m.DisableKeyword("DISABLE_LIGHTING");
            m.EnableKeyword("MAX_LIGHT_COUNT_8");
            m.EnableKeyword("DYNAMIC_AMBIENT");

            m.SetTexture("_Cube", cubeMap);
            if(cubeMap != null)
            {
                m.EnableKeyword("LIGHTPROBE_SPEC");
            }

            if(name.Contains("Transperant") || name.Contains("Transparent") || name.Contains("Transp"))
            {
                renderMode = (float)BlendModeStandard.Transparent;
            }

            SetupMaterialWithBlendMode(m, (BlendModeStandard)renderMode);

            materialModified = true;
        }
        else if (name.Equals("Mobile/Bumped Diffuse") || name.Equals("Mobile/Bumped Specular") || name.Equals("Mobile/Bumped Specular (1 Directional Light)"))
        {
            Color baseColor = Color.white;
            Texture normalMap = m.GetTexture("_BumpMap");

            m.shader = dstShader;
            m.SetColor("_BaseColor", baseColor);
            m.SetTexture("_NormalTex", normalMap);
            m.SetFloat("_Emissive", 0.0f);

            m.DisableKeyword("DISABLE_LIGHTING");
            if (name.Equals("Mobile/Bumped Diffuse"))
            {
                m.DisableKeyword("SPECULAR");
                m.DisableKeyword("SPECULAR_FIXED_LOW");
                m.DisableKeyword("SPECULAR_FIXED_MED");
            }
            else
            {
                m.EnableKeyword("SPECULAR");
                m.EnableKeyword("SPECULAR_FIXED_LOW");
                m.SetFloat("_SpecSmoothness", 0.5f);
            }

            m.EnableKeyword("NORMALMAP");
            m.EnableKeyword("MAX_LIGHT_COUNT_8");
            m.EnableKeyword("DYNAMIC_AMBIENT");

            m.SetTexture("_Cube", null);

            materialModified = true;
        }
        else if (name.Equals("Mobile/Diffuse") || name.Equals("Mobile/VertexLit") || name.Equals("Mobile/VertexLit (Only Directional Lights)"))
        {
            Color baseColor = Color.white;
            m.shader = dstShader;
            m.SetColor("_BaseColor", baseColor);
            m.SetFloat("_Emissive", 0.0f);

            m.DisableKeyword("DISABLE_LIGHTING");
            m.DisableKeyword("SPECULAR");
            m.DisableKeyword("SPECULAR_FIXED_LOW");
            m.DisableKeyword("SPECULAR_FIXED_MED");

            m.DisableKeyword("NORMALMAP");
            m.EnableKeyword("MAX_LIGHT_COUNT_8");
            m.EnableKeyword("DYNAMIC_AMBIENT");

            m.SetTexture("_NormalTex", null);
            m.SetTexture("_Cube", null);

            materialModified = true;
        }
        else if (name.Equals("Unlit/Color") || name.Equals("Unlit/Texture") || name.Equals("Unlit/Transparent") || name.Equals("Unlit/Transparent Cutout") || name.Equals("Mobile/Unlit (Supports Lightmap)"))
        {
            //copy over parameters.
            Color baseColor;
            if (name.Equals("Unlit/Color"))
            {
                baseColor = m.GetColor("_Color");
            }
            else
            {
                baseColor = Color.white;
            }

            m.shader = dstShader;
            //copy over parameters.
            m.SetColor("_BaseColor", baseColor);
            m.SetFloat("_Emissive", 1.0f);

            if (name.Equals("Unlit/Color"))
            {
                m.SetTexture("_MainTex", null);
            }
            else if (name.Equals("Unlit/Transparent"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Fade);
            }
            else if (name.Equals("Unlit/Transparent"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Cutout);
            }
            m.SetTexture("_NormalTex", null);
            m.SetTexture("_Cube", null);

            m.EnableKeyword("DISABLE_LIGHTING");
            m.DisableKeyword("MAX_LIGHT_COUNT_8");
            m.DisableKeyword("DYNAMIC_AMBIENT");
            m.DisableKeyword("NORMALMAP");
            m.DisableKeyword("SPECULAR");
            m.DisableKeyword("SPECULAR_FIXED_LOW");
            m.DisableKeyword("SPECULAR_FIXED_MED");

            if (name.Equals("Mobile/Unlit (Supports Lightmap)"))
            {
                m.EnableKeyword("DISABLE_LIGHTING");
                m.EnableKeyword("LIGHTMAP");
            }

            materialModified = true;
        }
        else if (name.Equals("Mobile/Particles/Additive") || name.Equals("Mobile/Particles/Alpha Blended") || name.Equals("Mobile/Particles/Multiply") || name.Equals("Mobile/Particles/VertexLit Blended") ||
            name.Equals("Particles/Additive") || name.Equals("Particles/Additive (Soft)") || name.Equals("Particles/Alpha Blended") || name.Equals("Particles/Alpha Blended Premultiply") || 
            name.Equals("Particles/Multiply") || name.Equals("Particles/VertexLit Blended"))
        {
            Color baseColor = Color.white;
            if (name.Equals("Particles/Additive") || name.Equals("Particles/Additive (Soft)") || name.Equals("Particles/Alpha Blended"))
            {
                baseColor = m.GetColor("_TintColor");
            }

            m.shader = dstShader;
            //copy over parameters.
            m.SetColor("_BaseColor", baseColor);
            m.SetFloat("_Emissive", 1.0f);

            m.SetTexture("_NormalTex", null);
            m.SetTexture("_Cube", null);

            m.EnableKeyword("DISABLE_LIGHTING");
            m.DisableKeyword("MAX_LIGHT_COUNT_8");
            m.DisableKeyword("DYNAMIC_AMBIENT");
            m.DisableKeyword("NORMALMAP");
            m.DisableKeyword("SPECULAR");
            m.DisableKeyword("SPECULAR_FIXED_LOW");
            m.DisableKeyword("SPECULAR_FIXED_MED");

            m.EnableKeyword("PARTICLE_RENDERING");

            if (name.Equals("Mobile/Particles/VertexLit Blended") || name.Equals("Particles/VertexLit Blended"))
            {
                m.DisableKeyword("DISABLE_LIGHTING");
                m.EnableKeyword("MAX_LIGHT_COUNT_8");
                m.EnableKeyword("DYNAMIC_AMBIENT");
                SetupMaterialWithBlendMode(m, BlendModeStandard.Fade);
            }
            else if (name.Equals("Mobile/Particles/Alpha Blended") || name.Equals("Particles/Alpha Blended"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Fade);
            }
            else if (name.Equals("Particles/Alpha Blended Premultiply"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Transparent);
            }
            else if (name.Equals("Mobile/Particles/Additive") || name.Equals("Particles/Additive") || name.Equals("Particles/Additive (Soft)"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Additive);
            }
            else if (name.Equals("Mobile/Particles/Multiply") || name.Equals("Particles/Multiply"))
            {
                SetupMaterialWithBlendMode(m, BlendModeStandard.Multiply);
            }
        }

        return materialModified;
    }

    private static void StandardToDaydream()
    {
        Shader destShader = Shader.Find("Daydream/Standard");
        if (destShader == null)
        {
            Debug.LogWarning("Cannot find the \"Daydream/Standard\" shader!" + "\n");
        }

        int count = 0;
        int numMaterialsConverted = 0;
        foreach (string s in UnityEditor.AssetDatabase.GetAllAssetPaths())
        {
            if (s.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                count++;
            }
        }

        int i = 0;
        List<string> alreadyConvertedMaterials = new List<string>();
        foreach (string s in UnityEditor.AssetDatabase.GetAllAssetPaths())
        {
            if (s.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Material Conversion to Daydream Shaders", string.Format("({0} of {1}) {2}", i, count, s), (float)i / (float)count))
                {
                    break;
                }

                Material m = UnityEditor.AssetDatabase.LoadMainAssetAtPath(s) as Material;
                if (m == null || m.shader == null) { continue; }
                if (!m.name.StartsWith("")) { continue; }
                if (alreadyConvertedMaterials.Contains(m.name)) { continue; }

                alreadyConvertedMaterials.Add(m.name);

                if (StandardToDaydreamSingleMaterial(m, m.shader, destShader))
                {
                    numMaterialsConverted++;
                }
            }
        }

        if (numMaterialsConverted > 0)
        {
            EditorSceneManager.MarkSceneDirty( SceneManager.GetActiveScene() );
        }

        SaveAssetsAndFreeMemory();
        UnityEditor.EditorUtility.ClearProgressBar();
    }

    static private void SetupMaterialWithBlendMode(Material material, BlendModeStandard blendMode)
    {
        switch (blendMode)
        {
            case BlendModeStandard.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case BlendModeStandard.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_Mode", 1);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case BlendModeStandard.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendModeStandard.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendModeStandard.Additive:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_Mode", 4);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendModeStandard.Multiply:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_Mode", 5);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }
}
}