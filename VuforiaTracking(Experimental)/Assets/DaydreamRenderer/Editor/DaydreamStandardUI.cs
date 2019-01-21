///////////////////////////////////////////////////////////////////////////////
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

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using daydreamrenderer;
using UnityEditorInternal;
using System.Collections.Generic;

[CanEditMultipleObjects]
public class DaydreamStandardUI : ShaderGUI
{
    static private class Styles
    {
        static public bool m_firstRun = true;
        static public GUIContent m_particleRendererGC   = new GUIContent("Particle Rendering", "Enable a fast rendering path for particle rendering, which may include particle lighting (if Light Count > 0).");
        static public GUIContent m_clampAttenuationGC   = new GUIContent("Clamp Attenuation", "Aggressively clamp light attenuation to avoid popping. This will cause the lighting to look different than the default for Unity.");
        static public GUIContent m_normalMapGC          = new GUIContent("Normal map", "Enable normal mapping when lighting.");
        static public GUIContent m_normalFlipYGC        = new GUIContent("Normal map - flip Y (green)", "Flip the green channel on the normal map since many normal maps use a different orientation than Unity.");
        static public GUIContent m_detailMapGC          = new GUIContent("Detail map", "Multiple the RGB value of the Detail Map with the albedo.");
        static public GUIContent m_specularGC           = new GUIContent("Specular", "Enable dynamic (approximate) specular");
        static public GUIContent m_specularFixedLowGC   = new GUIContent("Fixed Specular Power [LOW]", "Use a fixed specular power in order to speed up shading (16).");
        static public GUIContent m_specularFixedMedGC   = new GUIContent("Fixed Specular Power [MED]", "Use a fixed specular power in order to speed up shading (64).");
        static public GUIContent m_specularColoredGC    = new GUIContent("Colored Specular", "Use full color specular (based on light colors) rather than the default monochromatic.");
        static public GUIContent m_specularAAGC         = new GUIContent("Specular Antialiasing", "Centroid interpolate to limit specular shimmering on the edges of polygons.");
        static public GUIContent m_lightprobeSpecGC     = new GUIContent("Reflection Lightprobe", "Use an artist specified cubemap to approximate secondary specular (reflection) effects.");
        static public GUIContent m_lightprobeColoredGC  = new GUIContent("Colored Specular", "Use full color specular (based on texture colors) rather than the default monochromatic.");
        static public GUIContent m_ambientOcclusionGC   = new GUIContent("Ambient Occlusion", "Use the alpha channel of the Normal/Detail map as the Ambient Occlusion factor.");
        static public GUIContent m_shadowRecieverGC     = new GUIContent("Shadow Reciever", "Allow the surface to recieve dynamic shadows");
        static public GUIContent m_dynamicAmbientGC     = new GUIContent("Dynamic Ambient", "Use the dynamic multi-directional ambient. Only enable if no static lighting is used.");
        static public GUIContent m_lightmapGC           = new GUIContent("Use Unity Lightmaps", "Use Unity Lightmaps for static lighting.");
        static public GUIContent m_renderModeGC         = new GUIContent("Rendering Mode", "Material rendering (blending) mode.");
        static public GUIContent m_lightCountGC         = new GUIContent("Light Count", "The number of lights that can simultaneously affect this material.");
        static public GUIContent m_staticLightingGC     = new GUIContent("Enable Static Lighting", "Use static lighting, either static vertex lighting or static lightmaps.");
        static public GUIContent[] m_lightCountList     = null;
        static public GUIContent[] m_renderModeList     = null;
        //TO-DO: change to the tooltip format.
        static public string[] m_staticLightingList     = new string[] { "Daydream Vertex Lighting", "Light Map" };
    };

    private enum BlendMode
    {
        Opaque,
        AlphaTest,
        AlphaBlend,
        Premultiply,
        Additive,
        Multiply
    };

    private enum StaticLighting
    {
        Vertex,
        Lightmap,
    };
    
    private MaterialProperty blendMode = null;

    // Preview fields
    private PreviewRenderUtility m_previewUtility = new PreviewRenderUtility();
   
    private Vector2 m_scroll = Vector2.zero;
    private Quaternion m_rot = Quaternion.identity;
    private static readonly int s_controlHash = "daydream_material_control".GetHashCode ();
    private static Mesh m_previewMesh = null;
    private string kPreviewSpherePath = "Assets/DaydreamRenderer/Editor/PreviewSphere.asset";

    private bool m_ddObjectFound = false;
    
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        buildPopupLists();
        createDaydreamRendererObject();

        // render the default gui
        base.OnGUI(materialEditor, properties);
        UnityEngine.Object[] targetMats = materialEditor.targets;

        //verify that there is at least one material in this list.
        if (targetMats == null || targetMats.Length < 1)
        {
            return;
        }

        Material firstMat = (Material)targetMats[0];

        //This will add the material to the undo list, at the end of the frame Unity will check if its really changed and if so add it to the undo stack.
        //Since Unity does a binary comparison between the copied data and the original material - it should accurately determine if it has changed.
        //And since we're editing one material at a time, the extra memory (one extra copy of the Material) - the memory cost is reasonable.
        foreach (var obj in targetMats)
        {
            Material target = (Material)obj;
            Undo.RecordObject(target, "Shader UI Target Material");
        }

        EditorGUI.BeginChangeCheck();
        blendMode = FindProperty("_Mode", properties);
        if (blendModePopup())
        {
            foreach (var obj in blendMode.targets)
            {
                MaterialChanged((Material)obj);
            }
        }

        int lightCount = 1;
        if (Array.IndexOf(firstMat.shaderKeywords, "MAX_LIGHT_COUNT_8") != -1)
        {
            lightCount = 2;
        }
        else if (Array.IndexOf(firstMat.shaderKeywords, "DISABLE_LIGHTING") != -1)
        {
            lightCount = 0;
        }

        EditorGUI.BeginChangeCheck();
        lightCount = popup(lightCount, Styles.m_lightCountGC, Styles.m_lightCountList);

        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in targetMats)
            {
                Material target = (Material)obj;
                if (lightCount == 0)
                {
                    target.DisableKeyword("MAX_LIGHT_COUNT_8");
                    target.EnableKeyword("DISABLE_LIGHTING");
                }
                else if (lightCount == 1)
                {
                    target.DisableKeyword("MAX_LIGHT_COUNT_8");
                    target.DisableKeyword("DISABLE_LIGHTING");
                }
                else
                {
                    target.EnableKeyword("MAX_LIGHT_COUNT_8");
                    target.DisableKeyword("DISABLE_LIGHTING");
                }
            }
        }
               
        toggleFeature("PARTICLE_RENDERING", Styles.m_particleRendererGC,   targetMats);
        toggleFeature("ATTEN_CLAMP",        Styles.m_clampAttenuationGC,   targetMats);
        if (toggleFeature("NORMALMAP", Styles.m_normalMapGC, targetMats))
        {
            disableFeature("DETAILMAP", Styles.m_detailMapGC, targetMats);
        }
        toggleFeature("NORMALMAP_FLIP_Y",   Styles.m_normalFlipYGC,        targetMats);
        if (toggleFeature("DETAILMAP", Styles.m_detailMapGC, targetMats))
        {
            disableFeature("NORMALMAP", Styles.m_detailMapGC, targetMats);
        }

        if (toggleFeature("SPECULAR", Styles.m_specularGC, targetMats))
        {
            EditorGUI.indentLevel = 1;
            toggleFeature("SPECULAR_FIXED_LOW", Styles.m_specularFixedLowGC,   targetMats);
            toggleFeature("SPECULAR_FIXED_MED", Styles.m_specularFixedMedGC,   targetMats);
            toggleFeature("SPECULAR_COLORED",   Styles.m_specularColoredGC,    targetMats);
            toggleFeature("SPECULAR_AA",        Styles.m_specularAAGC,         targetMats);
            EditorGUI.indentLevel = 0;
        }
        if (toggleFeature("LIGHTPROBE_SPEC", Styles.m_lightprobeSpecGC, targetMats))
        {
            EditorGUI.indentLevel = 1;
            toggleFeature("SPECULAR_COLORED", Styles.m_lightprobeColoredGC, targetMats);
            EditorGUI.indentLevel = 0;
        }
        toggleFeature("AMBIENT_OCCLUSION", Styles.m_ambientOcclusionGC, targetMats);
        toggleFeature("SHADOWS",         Styles.m_shadowRecieverGC, targetMats);
        toggleFeature("DYNAMIC_AMBIENT", Styles.m_dynamicAmbientGC, targetMats);
        
        bool enabledStaticLighting = toggleFeature("STATIC_LIGHTING", Styles.m_staticLightingGC, targetMats);
        if (enabledStaticLighting)
        {
            EditorGUI.indentLevel = 1;
            int selection = Array.IndexOf(firstMat.shaderKeywords, "LIGHTMAP") != -1 ? (int)StaticLighting.Lightmap : (int)StaticLighting.Vertex;
            selection = GUILayout.SelectionGrid(selection, Styles.m_staticLightingList, 2, EditorStyles.radioButton);

            EditorGUI.indentLevel = 0;
            foreach (var obj in targetMats)
            {
                Material target = (Material)obj;
                if (selection == (int)StaticLighting.Vertex)
                {
                    target.DisableKeyword("LIGHTMAP");
                    target.EnableKeyword("VERTEX_LIGHTING");
                }
                else
                {
                    target.DisableKeyword("VERTEX_LIGHTING");
                    target.EnableKeyword("LIGHTMAP");
                }
            }
        }
        else
        {
            foreach (var obj in targetMats)
            {
                Material target = (Material)obj;
                target.DisableKeyword("LIGHTMAP");
                target.DisableKeyword("VERTEX_LIGHTING");
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            // force icon to rebuild
            string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(firstMat));
            IconCache.IconEntry entry = DaydreamMaterialPostProcessor.CustomIcon.Cache.Find(guid);
            if(entry != null)
            {
                DaydreamMaterialPostProcessor.CustomIcon.MarkForRebuild(entry);
            }
        }

    }

    public override void OnMaterialInteractivePreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
    {
        UnityEngine.Object[] targetMats = materialEditor.targets;

        //verify that there is at least one material in this list.
        if(targetMats == null || targetMats.Length < 1)
        {
            return;
        }

        Material firstMat = (Material)targetMats[0];

        if(m_previewMesh == null)
        {
            m_previewMesh = AssetDatabase.LoadAssetAtPath<Mesh>(kPreviewSpherePath);
        }

        m_scroll = HandleInput(m_scroll, r);

        m_previewUtility.BeginPreview(r, background);
        DREditorUtility.DrawPreview(m_previewUtility, firstMat, m_previewMesh, DREditorUtility.PreviewType.kInteractive, m_scroll, ref m_rot);
        m_previewUtility.EndAndDrawPreview(r);
    }

    public override void OnMaterialPreviewGUI(MaterialEditor materialEditor, Rect r, GUIStyle background)
    {
        UnityEngine.Object[] targetMats = materialEditor.targets;

        //verify that there is at least one material in this list.
        if(targetMats == null || targetMats.Length < 1)
        {
            return;
        }

        Material firstMat = (Material)targetMats[0];
        
        if (m_previewMesh == null)
        {
            m_previewMesh = AssetDatabase.LoadAssetAtPath<Mesh>(kPreviewSpherePath);
        }

        m_previewUtility.BeginPreview(r, background);
        DREditorUtility.DrawPreview(m_previewUtility, firstMat, m_previewMesh, DREditorUtility.PreviewType.kStatic, m_scroll, ref m_rot);
        m_previewUtility.EndAndDrawPreview(r);
        
        // update Imported Object preview materials
        if (Selection.activeObject && AssetDatabase.Contains(Selection.activeObject))
        {
            GameObject go = Selection.activeObject as GameObject;
            if(go != null)
            {
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
                foreach(Renderer rdr in renderers)
                {
                    foreach(Material m in rdr.sharedMaterials)
                    {
                        // if the object is using a daydream material update the lights
                        if (m.shader.name.ToLower().Contains("daydream"))
                        {
                            DREditorUtility.SetPreviewProperties(m);
                        }
                    }
                }
            }
        }

    }

    private bool toggleFeature(string defineName, GUIContent guiContent, UnityEngine.Object[] targetMats)
    {
        Material target = (Material)targetMats[0];

        bool value = Array.IndexOf(target.shaderKeywords, defineName) != -1;
        EditorGUI.BeginChangeCheck();
        value = EditorGUILayout.Toggle(guiContent, value);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in targetMats)
            {
                target = (Material)obj;
                // enable or disable the keyword based on checkbox
                if (value)
                {
                    target.EnableKeyword(defineName);
                }
                else
                {
                    target.DisableKeyword(defineName);
                }
            }
        }

        return value;
    }

    private void disableFeature(string defineName, GUIContent guiContent, UnityEngine.Object[] targetMats)
    {
        foreach (var obj in targetMats)
        {
            Material target = (Material)obj;
            target.DisableKeyword(defineName);
        }
    }

    private int popup(int selectIndex, GUIContent label, GUIContent[] choices)
    {
        return EditorGUILayout.Popup(label, selectIndex, choices);
    }

    private bool blendModePopup()
    {
        bool materialChanged = false;

        EditorGUI.showMixedValue = blendMode.hasMixedValue;
        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)popup((int)mode, Styles.m_renderModeGC, Styles.m_renderModeList);

        if (EditorGUI.EndChangeCheck())
        {
            blendMode.floatValue = (float)mode;
            materialChanged = true;
        }
        EditorGUI.showMixedValue = false;

        return materialChanged;
    }

    private void MaterialChanged(Material material)
    {
        SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
    }

    private void buildPopupLists()
    {
        if (Styles.m_firstRun)
        {
            string[] _choices        = new[] { "0", "4", "8" };
            string[] _choiceTooltips = new[] { "Light is disabled.", "4 Lights.", "8 Lights." };

            string[] _blendNames    = new[] { "Opaque", "Cutout", "Blend", "Premultiply", "Additive", "Multiply" };
            string[] _blendTooltips = new[] { "Opaque (Solid) [One, Zero]", "Binary cutout based on alpha value. [One, Zero + Clip]", "Alpha blended [SrcAlpha, 1-SrcAlpha]", "Assume the incoming material color has already been modulated by the alpha value. [One, 1-SrcAlpha]", "Additive blending. [One, One]", "Multiplicative blending. [srcColor * dstColor * srcAlpha + dstColor * (1-srcAlpha)]" };

            Styles.m_lightCountList = new GUIContent[3];
            Styles.m_renderModeList = new GUIContent[6];

            createPopupList(3, _choices,   _choiceTooltips, Styles.m_lightCountList);
            createPopupList(6, _blendNames, _blendTooltips, Styles.m_renderModeList);

            Styles.m_firstRun = false;
        }
    }

    private void createDaydreamRendererObject()
    {
        if (m_ddObjectFound) { return; }
        m_ddObjectFound = true;

        //Is the Daydream Renderer object already in the scene?
        DaydreamRenderer obj = GameObject.FindObjectOfType<DaydreamRenderer>();
        if (obj != null) { return; }

        //If not then add it now.
        GameObject ddo = new GameObject("DaydreamRenderer");
        ddo.AddComponent<DaydreamRenderer>();
    }

    private void createPopupList(int count, string[] labels, string[] tooltips, GUIContent[] list)
    {
        for (int i = 0; i < count; i++)
        {
            list[i] = new GUIContent(labels[i], tooltips[i]);
        }
    }

    private void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
    {
        switch (blendMode)
        {
            case BlendMode.Opaque:
                material.SetOverrideTag("RenderType", "");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case BlendMode.AlphaTest:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case BlendMode.AlphaBlend:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendMode.Premultiply:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendMode.Additive:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case BlendMode.Multiply:
                material.SetOverrideTag("RenderType", "Transparent");
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

    private static Vector2 HandleInput(Vector2 scroll, Rect position)
    {
        int id = GUIUtility.GetControlID (s_controlHash, FocusType.Passive);
        Event e = Event.current;

        switch (e.GetTypeForControl (id))
        {
            case EventType.MouseDown:
                {
                    if(position.Contains(e.mousePosition) && position.width > 50)
                    {
                        GUIUtility.hotControl = id;
                        e.Use();
                        //EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                }
                break;
            case EventType.MouseDrag:
                {
                    if(GUIUtility.hotControl == id)
                    {
                        scroll -= e.delta;
                        e.Use();
                        GUI.changed = true;
                    }
                }break;
            case EventType.MouseUp:
                {
                    if(GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                    }
                    //EditorGUIUtility.SetWantsMouseJumping(0);
                }break;
        }

        return scroll;
    }
}

#endif
