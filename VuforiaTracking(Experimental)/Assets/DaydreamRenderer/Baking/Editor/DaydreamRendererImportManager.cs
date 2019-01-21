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
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System;
using UnityEditorInternal;
using UnityEditor.SceneManagement;

namespace daydreamrenderer
{
    using UnityEditor.AnimatedValues;

    class DaydreamRendererImportManager : EditorWindow
    {
        public const string kDaydreamObjectName = "Daydream Renderer";
        public static List<string> m_projectAssetAddDaydream = new List<string>();

        static AnimBool m_UIFade;
        
        public static class Styles
        {
            public const string kEditorTitle = "Daydream Wizard";
            static public GUIStyle s_defaultLabel;
            public static GUIStyle boldCentered;
            public static GUIStyle boldButton;
            public static GUIStyle imageStyle;
            public static GUIStyle daydreamLightingStyle;
            public static GUIStyle unityLightingStyle;
            public static GUIStyle helpText;
            public static GUIStyle sectionLabel;
            public static GUIStyle sectionLineStyle;

            public const int kSectionSpacing = 35;

            public static Texture2D daydreamLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/Daydream_Icon_White_RGB.png");
            public static Texture2D daydreamLight = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/DaydreamLight.png");
            public static Texture2D unityLight = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/NormalLight.png");
            public static Texture2D section = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/Section.png");

            public static GUIContent importStaticLightingLabel = new GUIContent("Import daydream static lighting");
            public static GUIContent importStaticLightingButton = new GUIContent("Convert");
            
            public static GUIContent toggleLightingSystem = new GUIContent("Enable Daydream Lighting System", "Daydream replaces the lighting system");
            public static GUIContent ddrNotEnabled = new GUIContent("Daydream Renderer is not enabled for this scene");

          
            public const string kDaydreamEnabled = "Daydream Renderer is enabled";
            public const string kObjectsWaiting = "{0} dynamic and {1} static objects waiting for conversion to daydream lighting";
            public const string kToggleComponentsHelp = "Daydream Renderer can utilize a custom lighting system to provide lighting data to shaders. " +
                "For the best work-flow experience enable this option when using Daydream Renderer static lighting.";
            public const string kEnlightenHelp = "Daydream Renderer static lighting depends on Unity's light states in order apply lighting to static lit objects correctly. " +
                "In order to have the light states apply to the scene you must bake the scene with the Enlighten at least once. Or, you can enable the Daydream lighting system";
            public const string kOpenMaterialWizard = "Open Conversion Wizard";
            public const string kAddDrToScene = "Add The Daydream Renderer To Your Scene";
            public const string kMaterialWizardInfo = "The Material Wizard assists in converting an existing scene over to Daydream materials. It applies a conversion process to preserve all the feature selections of the original Unity shader but converted to the Daydream standard shader.";
            public const string kOpenDocumentation = "Open Documentation";
            public const string kConversionWizardSegment = "Material Conversion Wizard";

            // content and styles for lighting system toggle 
            static public GUIContent m_enableEnlightenUI = new GUIContent("Enable Unity Lighting", "Enable Unity Lighting support.");
            static public GUIContent m_enableDaydreamUI = new GUIContent("Enable Daydream Lighting", "Daydream Renderer overrides Unity lighting system.");
            static public GUIContent[] m_lightingSystemUI;
            public static GUIStyle m_buttonUnselected = new GUIStyle(EditorStyles.toolbar);
            public static GUIStyle m_buttonSelected = new GUIStyle(EditorStyles.toolbarButton);

            // Lighting system advanced settings
            public static GUIStyle dropDownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            public static GUIStyle dropDownSelectedStyle = new GUIStyle(EditorStyles.toolbarDropDown);

            static Styles()
            {
                boldCentered = new GUIStyle();
                boldCentered.alignment = TextAnchor.MiddleCenter;
                boldCentered.fontSize = 18;
                boldCentered.margin = new RectOffset(10, 10, 10, 10);
                boldCentered.normal.textColor = Color.white;

                boldButton = new GUIStyle(GUI.skin.GetStyle("button"));
                boldButton.alignment = TextAnchor.MiddleCenter;
                boldButton.fontSize = 10;

                s_defaultLabel = new GUIStyle(EditorStyles.label);
                s_defaultLabel.fixedHeight = 18;
                s_defaultLabel.wordWrap = false;
                s_defaultLabel.stretchHeight = false;

                imageStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                imageStyle.normal.background = daydreamLogo;
                imageStyle.alignment = TextAnchor.MiddleCenter;

                helpText = new GUIStyle(GUI.skin.GetStyle("HelpBox"));
                helpText.normal.textColor = Color.white;
                helpText.alignment = TextAnchor.MiddleCenter;
                helpText.fontSize = 18;

                daydreamLightingStyle = new GUIStyle(EditorStyles.boldLabel);
                daydreamLightingStyle.normal.background = daydreamLight;

                unityLightingStyle = new GUIStyle(EditorStyles.boldLabel);
                unityLightingStyle.normal.background = unityLight;

                sectionLineStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                sectionLineStyle.normal.background = section;
                sectionLineStyle.alignment = TextAnchor.MiddleCenter;

                sectionLabel = new GUIStyle(GUI.skin.label);
                //sectionLabel.normal.textColor = Color.white;
                sectionLabel.fontSize = 12;
                sectionLabel.contentOffset = new Vector2(0f, 7f);

                m_buttonUnselected.alignment = m_buttonSelected.alignment;
                m_buttonSelected.normal = m_buttonSelected.active;
                m_lightingSystemUI = new GUIContent[] { Styles.m_enableDaydreamUI, Styles.m_enableEnlightenUI };
            }

        }

        [InitializeOnLoad]
        public class FirstTimeLoader
        {
            static DateTime s_lastUpdate = DateTime.Now;
            static FirstTimeLoader()
            {
                EditorApplication.update += Update;
                AssetDatabase.importPackageCompleted += PackageImport;
            }

            static void Update()
            {
                if (!string.IsNullOrEmpty(SceneManager.GetActiveScene().name) && SceneManager.GetActiveScene().isLoaded)
                {
                    if (DaydreamRendererImportSettings.FirstRun)
                    {
                        DaydreamRendererImportSettings.FirstRun = false;
                        DaydreamRendererImportManager.OpenWindow();
                    }
                }

                DaydreamRenderer renderer = FindObjectOfType<DaydreamRenderer>();
                if (renderer != null && !renderer.m_enableManualLightingComponents && (DateTime.Now - s_lastUpdate).TotalSeconds > 2)
                {
                    ApplyLightingComponents();
                    s_lastUpdate = DateTime.Now;
                }

            }

            static void PackageImport(string packageName)
            {
                Debug.Log("package imported " + packageName);
            }
        }

        

        [MenuItem("Window/Daydream Renderer/" + Styles.kEditorTitle)]
        public static void OpenWindow()
        {
            DaydreamRendererImportManager window = EditorWindow.GetWindow<DaydreamRendererImportManager>(Styles.kEditorTitle);
            window.Show();
            window.minSize = new Vector2(300, 500);
        }


        void OnEnable()
        {
            m_UIFade = new AnimBool(true);
            m_UIFade.valueChanged.AddListener(Repaint);
        }
        
        static void DrawCenteredLogo(int size)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Button("", Styles.imageStyle, GUILayout.Width(size), GUILayout.Height(size));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        static void DrawLightingLogo(int size)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Button("", Styles.daydreamLightingStyle, GUILayout.Width(size), GUILayout.Height(size));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public static void DrawSection(int sizeX, int sizeY)
        {
            GUILayout.Button("", Styles.sectionLineStyle, GUILayout.Height(sizeY));
        }

        void OnGUI()
        {
            DrawCenteredLogo(100);

            DaydreamRenderer renderer = FindObjectOfType<DaydreamRenderer>();
            if (renderer == null)
            {
                m_UIFade.target = false;

                EditorGUILayout.LabelField(Styles.ddrNotEnabled, Styles.helpText);

                GUILayout.Space(10);

                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (DREditorUtility.FlexibleHorizButton(Styles.kAddDrToScene, Styles.boldButton, GUILayout.Width(280), GUILayout.Height(50)))
                {
                    GameObject go = GameObject.Find(kDaydreamObjectName);
                    if (go == null)
                    {
                        go = new GameObject(kDaydreamObjectName);
                    }

                    go.AddComponent<DaydreamRenderer>();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                return;
            }
            else
            {
                m_UIFade.target = true;
            }

            //---------------------------------------------------------------------//
            // Daydream lighting
            if (EditorGUILayout.BeginFadeGroup(m_UIFade.faded))
            {
                EditorGUILayout.LabelField(Styles.kDaydreamEnabled, Styles.helpText);
                
                DrawDaydreamLightingToggle(renderer);
            }
            EditorGUILayout.EndFadeGroup();


            //---------------------------------------------------------------------//
            // Material conversion
            GUILayout.Space(10);
            DaydreamRendererImportManager.DrawSection(500, 1);
            EditorGUILayout.LabelField(Styles.kConversionWizardSegment, DaydreamRendererImportManager.Styles.sectionLabel, GUILayout.Height(25));
            GUILayout.Space(5);

            EditorGUILayout.HelpBox(Styles.kMaterialWizardInfo, MessageType.Info);

            if (GUILayout.Button(Styles.kOpenMaterialWizard))
            {
                MaterialConversionDialog.ShowDialog(null);
            }

            //---------------------------------------------------------------------//
            // Documentation
            GUILayout.Space(10);
            DaydreamRendererImportManager.DrawSection(500, 1);
            EditorGUILayout.LabelField("Documentation", DaydreamRendererImportManager.Styles.sectionLabel, GUILayout.Height(25));
            if (GUILayout.Button(Styles.kOpenDocumentation))
            {
                Application.OpenURL("https://github.com/googlevr/daydream-renderer-for-unity/blob/master/README.md");
            }
        }

        public static void DrawDaydreamLightingToggle(DaydreamRenderer renderer)
        {
            if (renderer == null) return;

            // determine lighting system in use
            const int kDaydreamLighting = 0;
            const int kUnityLighting = 1;
            int selectedIndex = renderer.m_daydreamLighting ? kDaydreamLighting : kUnityLighting;

            EditorGUI.BeginChangeCheck();

            // draw section separator
            DrawSection(500, 1);
            EditorGUILayout.BeginHorizontal();
            if (selectedIndex == kDaydreamLighting)
            {
                EditorGUILayout.LabelField("Lighting System", Styles.sectionLabel, GUILayout.Width(105), GUILayout.Height(25));
                GUILayout.Button("", Styles.daydreamLightingStyle, GUILayout.Width(25), GUILayout.Height(25));
            }
            else
            {
                EditorGUILayout.LabelField("Lighting System", Styles.sectionLabel, GUILayout.Width(105), GUILayout.Height(25));
                GUILayout.Button("", Styles.unityLightingStyle, GUILayout.Width(25), GUILayout.Height(25));
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            DREditorUtility.RadioButtonOutput selected = DREditorUtility.DrawRadioButton(selectedIndex, Styles.m_lightingSystemUI, Styles.m_buttonUnselected, Styles.m_buttonSelected, 20, 32, 150, new bool[] { true, false });
            if (EditorGUI.EndChangeCheck())
            {
                if (selected.m_selectedIndex == kDaydreamLighting)
                {
                    if (renderer.m_daydreamLighting == false)
                    {
                        renderer.m_daydreamLighting = true;
                        if (!renderer.m_enableManualLightingComponents)
                        {
                            DaydreamRendererImportManager.ApplyLightingComponents();
                        }
                        renderer.EnableEnlighten(false);
                    }
                }
                else
                {
                    if (renderer.m_daydreamLighting == true)
                    {
                        renderer.m_daydreamLighting = false;
                        if (!renderer.m_enableManualLightingComponents)
                        {
                            DaydreamRendererImportManager.RemoveAllLightingComponents();
                        }
                        renderer.EnableEnlighten(true);
                    }
                }

                // display advanced settings
                if(selected.m_dropDownSelected == kDaydreamLighting)
                {
                    LightSystemDialog.ShowDialog(null);
                }
            }
            GUILayout.Space(5);
        }

       


        public static void RemoveAllLightingComponents()
        {
            DaydreamLight[] lightComp = GameObject.FindObjectsOfType<DaydreamLight>();
            DaydreamMeshRenderer[] meshComp = GameObject.FindObjectsOfType<DaydreamMeshRenderer>();
            
            foreach (var v in lightComp)
            {
                DestroyImmediate(v);
            }
            foreach (var v in meshComp)
            {
                DestroyImmediate(v);
            }
        }

        public static void RemoveLightingComponentsFromAssets()
        {
            // find models and prefabs
            string[] modelAssets = AssetDatabase.FindAssets("t:Model t:prefab");

            // remove from prefabs and models in the project
            int count = modelAssets.Length;
            int index = 0;
            foreach(string guid in modelAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                DaydreamMeshRenderer[] dmrs = model.GetComponentsInChildren<DaydreamMeshRenderer>();

                EditorUtility.DisplayProgressBar(Styles.kEditorTitle, "Adding Component to Model " + path, index / (float)count);

                foreach(DaydreamMeshRenderer dmr in dmrs)
                {
                    GameObject.DestroyImmediate(dmr, true);
                }

                index++;
            }

            EditorUtility.ClearProgressBar();
        }

        public static void ApplyLightingToProject()
        {
            // find models and prefabs
            string[] modelAssets = AssetDatabase.FindAssets("t:prefab");

            // Add daydream mesh renderer to objects in the project
            int count = modelAssets.Length;
            int index = 0;
            foreach(string guid in modelAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Renderer[] mrs = model.GetComponentsInChildren<Renderer>();

                EditorUtility.DisplayProgressBar(Styles.kEditorTitle, "Adding Component to Prefab " + path, index / (float)count);

                foreach(Renderer mr in mrs)
                {
                    if(mr != null && mr.gameObject != null && mr.gameObject.GetComponent<DaydreamMeshRenderer>() == null)
                    {
                        foreach(Material m in mr.sharedMaterials)
                        {
                            if(m != null && m.shader.name.ToLower().Contains("daydream"))
                            {
                                mr.gameObject.AddComponent<DaydreamMeshRenderer>();
                                break;
                            }
                        }
                    }
                }
            }

            // find models and prefabs
            modelAssets = AssetDatabase.FindAssets("t:Model");

            // Add daydream mesh renderer to objects in the project
            count = modelAssets.Length;
            index = 0;
            foreach(string guid in modelAssets)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Renderer[] mrs = model.GetComponentsInChildren<Renderer>();

                EditorUtility.DisplayProgressBar(Styles.kEditorTitle, "Adding Component to Model " + path, index / (float)count);

                if(mrs.Length > 0)
                {
                    // check if daydream shader is used
                    bool imported = false;
                    foreach(Renderer r in mrs)
                    {
                        if(r != null && r.gameObject.GetComponent<DaydreamMeshRenderer>() == null)
                        {
                            foreach(Material m in r.sharedMaterials)
                            {
                                if(m != null && m.shader.name.ToLower().Contains("daydream"))
                                {
                                    AssetDatabase.ImportAsset(path, ImportAssetOptions.Default);
                                    imported = true;
                                    break;
                                }
                            }
                        }

                        if(imported)
                        {
                            break;
                        }
                    }
                }

                index++;
            }

            EditorUtility.ClearProgressBar();
        }

        public static void ApplyLightingComponents()
        {
            // check renderers
            List<GameObject> roots = Utilities.GetAllRoots();
            List<GameObject> missingRenderer = new List<GameObject>();
            List<GameObject> missingDrLight = new List<GameObject>();

            for (int i = 0, k = roots.Count; i < k; ++i)
            {
                Renderer[] renderers = roots[i].GetComponentsInChildren<Renderer>();
                Light[] lights = roots[i].GetComponentsInChildren<Light>();

                for (int j = 0, n = renderers.Length; j < n; ++j)
                {
                    MeshRenderer mr = renderers[j] as MeshRenderer;
                    SkinnedMeshRenderer smr = renderers[j] as SkinnedMeshRenderer;

                    if (renderers[j] == null || (mr == null && smr == null))
                    {
                        continue;
                    }

                    bool isDaydreamRendered = false;
                    for (int m = 0; m < renderers[j].sharedMaterials.Length; ++m)
                    {
                        if (renderers[j].sharedMaterials[m] != null && renderers[j].sharedMaterials[m].shader.name.ToLower().Contains("daydream"))
                        {
                            isDaydreamRendered = true;
                            break;
                        }
                    }
                    if (renderers[j] != null && isDaydreamRendered)
                    {
                        DaydreamMeshRenderer[] dmrs = renderers[j].GetComponents<DaydreamMeshRenderer>();
                        if(null == dmrs || dmrs.Length == 0)
                        {
                            missingRenderer.Add(renderers[j].gameObject);
                        }
                        else if(dmrs.Length > 1)
                        {
                            // remove extra
                            for(int x = 1; x < dmrs.Length; ++x)
                            {
                                GameObject.DestroyImmediate(dmrs[x], true);
                            }
                        }
                        else
                        {
                            // update disable/enabled
                            dmrs[0].enabled = renderers[j].enabled;
                        }
                    }
                }
                for (int j = 0, n = lights.Length; j < n; ++j)
                {
                    if (lights[j] != null && null == lights[j].GetComponent<DaydreamLight>())
                    {
                        missingDrLight.Add(lights[j].gameObject);
                    }
                }
            }

            if (missingRenderer.Count != 0 || missingDrLight.Count != 0)
            {
                if(!Application.isPlaying)
                {
                    for (int i = 0, k = missingRenderer.Count; i < k; ++i)
                    {
                        if(missingRenderer[i] != null)
                        {
                            missingRenderer[i].AddComponent<DaydreamMeshRenderer>();
                        }
                    }
                    for (int i = 0, k = missingDrLight.Count; i < k; ++i)
                    {
                        if(missingDrLight[i] != null)
                        {
                            missingDrLight[i].AddComponent<DaydreamLight>();
                        }
                    }
                }else
                {
                    for (int i = 0, k = missingRenderer.Count; i < k; ++i)
                    {
                        if(missingRenderer[i] != null)
                        {
                            Debug.LogWarning(missingRenderer[i].GetPath() + ": DaydreamMeshRenderer must be added to MeshRenderer objects in order for Daydream Lighting system to function");
                        }
                    }
                    for (int i = 0, k = missingDrLight.Count; i < k; ++i)
                    {
                        if(missingDrLight[i] != null)
                        {
                            Debug.LogWarning(missingDrLight[i].GetPath() + ": DaydreamLight must be added to Light components \tin order for Daydream Lighting system to function");
                        }
                    }
                }
            }
        }

    }

}