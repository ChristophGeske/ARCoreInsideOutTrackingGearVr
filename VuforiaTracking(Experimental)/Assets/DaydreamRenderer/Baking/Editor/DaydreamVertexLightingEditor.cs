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

#if DDR_RUNTIME_DLL_LINKING && !UNITY_EDITOR_OSX
#define DDR_RUNTIME_DLL_LINKING_
#endif

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System;

namespace daydreamrenderer
{
    using BakeSettings = DDRSettings.BakeSettings;
    using Face = DDRSettings.BakeSettings.ColorCubeFaces;
    using GradientIndex = DDRSettings.BakeSettings.GradientColorIndex;
    using LightEntry = DDRSettings.LightEntry;
    using LightEntryComparer = DDRSettings.LightEntryCompare;

    public class DaydreamVertexLightingEditor : EditorWindow
    {

        static public readonly string s_lightDataPath = "Assets/DaydreamRenderer/Baking/Editor/ScriptedAssets";
        static bool s_minSliderActive = false;
        static bool s_maxSliderActive = false;
        static int s_control = -1;
        static bool s_bakeSetsFoldout = false;

        public static string[] s_colorCubeFaces = new string[]{
        "Positive X",
        "Negative X",
        "Positive Y",
        "Negative Y",
        "Positive Z",
        "Negative Z",
        };

        static class Toolbar
        {
            public static int kAdd = -1;
            public static int kRemove = -2;
            public static int kSettings = -3;
        }

        private static bool s_settingsRestored = false;
        private static bool s_settingsDirty = false;
        private static bool s_bakeInProgress = false;
        private static volatile bool s_bakeIsFinished = false;
        private static Queue<int> s_bakeSetQueue = new Queue<int>();
        private static List<MeshRenderer> s_meshRenderers = new List<MeshRenderer>();
        private static int s_progress;
        private static bool m_ignoreNextChange = false;
        private static BakeSetSettingsDialog s_settingsDialog;

        public static class Styles
        {
            public const string kEditorTitle = "Vertex Lighting";
            public const int kIndent = 20;
            public const string kNoStaticMeshes = "No meshes are marked static. \nHint: Enable the game objects static flag or 'Lightmapping static' sub flag to include the object in bake.";
            
            public static readonly GUIContent m_bakeAll = new GUIContent("Bake All Sets");
            public static readonly GUIContent m_clearAllBakeData = new GUIContent("Clear All Baked Data", "Clears baked data for all bake sets");
            public static readonly GUIContent m_addBakeData = new GUIContent("Add Baked Lighting Set");
            public static readonly GUIContent m_defaultBakeData = new GUIContent("Default Lighting Set");
            public static readonly GUIContent m_enableShadows = new GUIContent("Shadows");
            public static readonly GUIContent m_lightblockSamples = new GUIContent("Sample Count");
            public static readonly GUIContent m_lightblockStartOffest = new GUIContent("Start Offset");
            public static readonly GUIContent m_normalBend = new GUIContent("Normal Averaging", "slider: 0 use mesh normal, " +
                "slider: 1 use average of nearby normals. 0 < slider < 1 interpolation between mesh normal and average");
            public static readonly GUIContent m_enableAmbient = new GUIContent("Ambient Occlusion");
            public static readonly GUIContent m_aoForceTwoSided = new GUIContent("Two Sided Geo");
            public static readonly GUIContent m_occluderSamples = new GUIContent("Sample Count");
            public static readonly GUIContent m_occluderOffset = new GUIContent("Start Offset");
            public static readonly GUIContent m_occluderLength = new GUIContent("Test Length");
            public static readonly GUIContent m_ambientHeader = new GUIContent("Ambient Light");
            public static readonly GUIContent m_min = new GUIContent("Min Value");
            public static readonly GUIContent m_max = new GUIContent("Max Value");
            public static readonly GUIContent m_ambientSource = new GUIContent("Ambient Source", "Solid ambient color, or axis aligned color cube");
            public static readonly GUIContent m_ambientColor = new GUIContent("Ambient Color");
            public static readonly GUIContent m_tessellationHeader = new GUIContent("Dynamic Tessellation");
            public static readonly GUIContent m_enableTessellation = new GUIContent("Enable Dynamic Tessellation", "Improve static vertex lighting quality by dynamically adding tessellation during the bake process");
            public static readonly GUIContent m_minTessIterations = new GUIContent("Min Iterations", "This will force an entire mesh to a minimum level of tessellation");
            public static readonly GUIContent m_maxTessIterations = new GUIContent("Max Iterations", "Limit the number of tessellation iterations allowed for a mesh, -1 indicates not limits");
            public static readonly GUIContent m_tessShadowSoftness = new GUIContent("Shadow Softness", "Specifies the tessellation level that represents a 'soft' shadow edge. Softness of a shadow is determined by light shadow settings.");
            public static readonly GUIContent m_tessShadowHardness = new GUIContent("Shadow Hardness", "Specifies the tessellation level that represents a 'hard' shadow edge. Hardness of a shadow is determined by light shadow settings.");
            public static readonly GUIContent m_maxTessVertices = new GUIContent("Max Vertices", "Limit the maximum number of vertices allowed, -1 indicates not limits");
            public static readonly GUIContent m_tessAOPasses = new GUIContent("AO Resolution", "Specifies iteration passes on areas affected by ambient occlusion");
            public static readonly GUIContent m_aoAccessibilityThreshold = new GUIContent("AO Threshold", "If the accessibility of a surface falls below this value it is a target for tessellation.");
            public static readonly GUIContent m_surfaceLightThresholdMin = new GUIContent("Shadow Edge Min", "Amount of light that constitutes the leading edge of a shadow.");
            public static readonly GUIContent m_surfaceLightThresholdMax = new GUIContent("Shadow Edge Max", "Amount of light that constitutes the beginning of the inner shadow 'body'.");
            public static readonly GUIContent m_surfaceLightThreshold = new GUIContent("Shadow Edge Min/Max", "This value controls the range of values that indicate a shadow edge."
                                                                                                                            + " The shadow edge is then tessellated according to the how much hard and soft light is hitting it.");
            public static readonly GUIContent m_lightIntensityThreshold = new GUIContent("Light Intensity Threshold", "If the difference between any of the 3 vertices of a triangle exceeds this threshold it is tessellated");
            public static readonly GUIContent m_avgLightIntensityThreshold = new GUIContent("Avg Light Intensity Threshold", "If the difference between any of the 3 vertices of a triangle exceeds this 'average' threshold it is tessellated."
                                                                                            + "The average intensity is based on a 'light patch' which is a rectangle around a vertex encompassing all triangles that include that vertex.");
            public static readonly GUIContent m_restoreDefaults = new GUIContent("Restore Defaults", "Restore tessellation default values.");

            public static GUIStyle m_toolbarButton = new GUIStyle(EditorStyles.toolbar);
            public static GUIStyle m_toolbarButtonSelected = new GUIStyle(EditorStyles.toolbarButton);
            public static GUIStyle m_toolbarAddButton = new GUIStyle(EditorStyles.toolbar);
            public static GUIStyle m_toolbarRemoveButton = new GUIStyle(EditorStyles.toolbar);
            public static GUIStyle m_toolbarSettingsButton = new GUIStyle(EditorStyles.toolbarDropDown);
            public static GUIStyle m_toolbarSettingsSelected = new GUIStyle(EditorStyles.toolbarDropDown);

            public static GUIStyle m_clearButton = new GUIStyle(EditorStyles.miniButton);


            static Styles()
            {
                m_toolbarButton.alignment = m_toolbarButtonSelected.alignment;
                m_toolbarButtonSelected.normal = m_toolbarButtonSelected.active;

                m_toolbarAddButton.fontSize = 16;
                m_toolbarAddButton.fontStyle = FontStyle.Bold;
                m_toolbarAddButton.margin.left = 4;

                m_toolbarRemoveButton.fontSize = 16;
                m_toolbarRemoveButton.fontStyle = FontStyle.Bold;

                m_toolbarSettingsButton.fixedWidth = 16f;
                m_toolbarSettingsSelected.fixedWidth = 16f;
                
            }
        }

        [MenuItem("Window/Daydream Renderer/" + Styles.kEditorTitle)]
        public static void Init()
        {
            BakeData.Instance().GetBakeSettings();
            s_settingsRestored = true;
            OpenWindow();
        }

        public void OnEnable()
        {
            EditorApplication.hierarchyWindowChanged += OnSceneChange;
            EditorApplication.playmodeStateChanged += OnPlayStateChange;
        }

        public static void OnPlayStateChange()
        {
            if (!Application.isPlaying)
            {
                m_ignoreNextChange = true;
            }
        }

        public static void OnSceneChange()
        {
            if (m_ignoreNextChange)
            {
                m_ignoreNextChange = false;
            }
        }

        public static void OpenWindow()
        {
            DaydreamVertexLightingEditor window = EditorWindow.GetWindow<DaydreamVertexLightingEditor>(Styles.kEditorTitle);
            window.Show();
            window.minSize = new Vector2(300, 500);
        }

        void OnFocus()
        {
            if (s_settingsDialog != null)
            {
                s_settingsDialog.CancelDialog();
            }
        }

        void OnGUI()
        {

            DaydreamRenderer renderer = FindObjectOfType<DaydreamRenderer>();
            if (renderer == null)
            {
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("Enable Daydream Renderer To Start Baking", MessageType.Info);
                if (GUILayout.Button("Launch Daydream Wizard"))
                {
                    DaydreamRendererImportManager.OpenWindow();
                }
                return;
            }

            if (SceneManager.GetActiveScene().name == "")
            {
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("Save the scene to begin baking", MessageType.Info);
                return;
            }

            if (!renderer.m_enableStaticLightingForScene)
            {
                GUILayout.Space(50);
                EditorGUILayout.HelpBox("Enable vertex baking to use Daydream Static Lighting", MessageType.Info);
                if (GUILayout.Button("Enable Vertex Baking For Scene"))
                {
                    renderer.m_enableStaticLightingForScene = true;
                }
                return;
            }

            if (!s_settingsRestored)
            {
                BakeData.Instance().SaveBakeSettings();
                s_settingsRestored = true;
            }

            if (Event.current.rawType == EventType.MouseUp)
            {
                if (s_settingsDirty)
                {
                    s_settingsDirty = false;
                    BakeData.Instance().SaveBakeSettings();
                }
            }

            if (s_bakeInProgress)
            {
                if (EditorUtility.DisplayCancelableProgressBar("Daydream Baker", "Baking meshes", VertexBakerLib.Instance.BakeProgress()))
                {
                    VertexBakerLib.Instance.BakeCancel();
                    if (!s_bakeInProgress)
                    {
                        s_bakeIsFinished = true;
                    }
                }

                if (s_bakeIsFinished)
                {
                    s_bakeIsFinished = false;
                    s_bakeInProgress = false;
                    EditorUtility.ClearProgressBar();

                    VertexBakerLib.Instance.BakeFinish(delegate (string msg, float complete)
                    {
                        EditorUtility.DisplayProgressBar("Daydream Baker", msg, complete);
                    });
                    EditorUtility.ClearProgressBar();

                    // queue up next bake
                    if (s_bakeSetQueue.Count > 0)
                    {
                        int bakeSet = s_bakeSetQueue.Dequeue();
                        BakeData.Instance().GetBakeSettings().SetBakeSetIndex(bakeSet);
                        BakeScene();
                    }
                }
            }

            EditorGUI.BeginChangeCheck();

#if DDR_RUNTIME_DLL_LINKING_
        if(GUILayout.Button("Reload Library"))
        {
            if(VertexBakerLib.Instance.LibLoaded)
            {
                VertexBakerLib.Instance.UnloadLib();
            } else
            {
                VertexBakerLib.Instance.LoadLib();
            }
        }
#endif

            DDRSettings settingsData = BakeData.Instance().GetBakeSettings();

            string[] ids = new string[settingsData.m_settingsList.Count];
            for (int i = 0, k = settingsData.m_settingsList.Count; i < k; ++i)
            {
                ids[i] = settingsData.m_settingsList[i].m_settingsId;
            }

            // update selected data
            GUILayout.Space(20);
            int settingsIndex = -1;
            int selected = DrawToolBar(settingsData.GetBakeSetIndex(), ids, true, out settingsIndex);

            if (selected >= 0)
            {
                int cur = settingsData.GetBakeSetIndex();
                settingsData.SetBakeSetIndex(selected);

                if (cur != selected)
                {
                    DaydreamVertexLighting.UpdateAllVertexLighting(settingsData.SelectedBakeSet.m_settingsId);
                }
            }

            BakeSettings settings = settingsData.SelectedBakeSet;

            if (selected == Toolbar.kAdd)
            {
                if (!BakeSetDialog.m_active)
                {
                    BakeSetDialog.ShowDialog(delegate (bool result, string name)
                    {
                        if (result)
                        {
                            BakeSettings newSettings = new BakeSettings(name);
                            settingsData.AddBakeSettings(newSettings);
                            EditorUtility.SetDirty(settingsData);
                        }

                    });
                }

            }
            else if (selected == Toolbar.kRemove)
            {
                if (settingsData.m_settingsList.Count > 1)
                {
                    int current = settingsData.GetBakeSetIndex();

                    settingsData.RemoveBakeSetting(current);
                    EditorUtility.SetDirty(settingsData);

                    --current;
                    if (current < 0)
                    {
                        current = 0;
                    }

                    settingsData.SetBakeSetIndex(current);
                    DaydreamVertexLighting.UpdateAllVertexLighting(settingsData.SelectedBakeSet.m_settingsId);

                }
            }
            else if (selected == Toolbar.kSettings)
            {
                if (!BakeSetSettingsDialog.m_active)
                {
                    BakeSettings curSettings = settingsData.m_settingsList[settingsIndex];
                    s_settingsDialog = BakeSetSettingsDialog.ShowDialog(curSettings.m_settingsId, curSettings.m_lightList
                        , curSettings.m_activeSet
                        , curSettings.m_forceAllLights
                        , delegate (BakeSetSettingsDialog.Result result, string bakeSetName, List<LightEntry> selectedLights, bool activeSet, bool forceAllLights)
                    {
                        s_settingsDialog = null;
                        if (settingsData.m_settingsList.Count > 1 && result == BakeSetSettingsDialog.Result.Remove)
                        {
                            settingsData.RemoveBakeSetting(settingsIndex);
                            EditorUtility.SetDirty(settingsData);
                            curSettings = settingsData.m_settingsList[settingsIndex];
                        }
                        else if (result == BakeSetSettingsDialog.Result.Ok)
                        {
                            curSettings.m_settingsId = bakeSetName;
                            curSettings.m_activeSet = activeSet;
                            curSettings.m_forceAllLights = forceAllLights;
                            if (selectedLights != null)
                            {
                                    // remove empty or stale entries
                                    var idsInFile = Utilities.LightsByLocalFileId();
                                    selectedLights.RemoveAll(delegate(LightEntry obj){
                                        return string.IsNullOrEmpty(obj.m_group) && obj.m_idInFile == 0
                                            || (!idsInFile.ContainsKey(obj.m_idInFile) 
                                                && (string.IsNullOrEmpty(obj.m_group) || GameObject.Find(obj.m_group) == null));
                                    });

                                    curSettings.m_lightList = selectedLights;
                            }
                            EditorUtility.SetDirty(settingsData);
                        }
                    });
                }
                else if (s_settingsDialog != null)
                {
                    s_settingsDialog.CancelDialog();
                }
            }                       

            DrawShadowAndAOSettings(settings, settingsData);

            //settings.m_diffuseEnergyConservation = EditorGUILayout.Slider("Diffuse Conservation", settings.m_diffuseEnergyConservation, 0f, 1f);

            EditorGUILayout.LabelField(Styles.m_ambientHeader);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Styles.kIndent);
            EditorGUILayout.BeginVertical();

            Color solidColor = settings.GetColorSolid();

            Color gradSky = settings.GetColorGradient(GradientIndex.Sky);
            Color gradEquator = settings.GetColorGradient(GradientIndex.Equator);
            Color gradGround = settings.GetColorGradient(GradientIndex.Ground);

            Color posX = settings.GetColorCubeFace(Face.PosX);
            Color posY = settings.GetColorCubeFace(Face.PosY);
            Color posZ = settings.GetColorCubeFace(Face.PosZ);

            Color negX = settings.GetColorCubeFace(Face.NegX);
            Color negY = settings.GetColorCubeFace(Face.NegY);
            Color negZ = settings.GetColorCubeFace(Face.NegZ);

            settings.m_colorMode = (BakeSettings.AmbientColorMode)EditorGUILayout.EnumPopup(Styles.m_ambientSource, settings.m_colorMode);

            // Draw color
            EditorGUILayout.BeginHorizontal();
            {
                //GUILayout.Space(Styles.kIndent);
                EditorGUILayout.BeginVertical();

                if (settings.m_colorMode == BakeSettings.AmbientColorMode.kColorCube)
                {
                    // Check for color value change
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    EditorGUILayout.BeginVertical();

                    DrawColorCube(posX, negX, posY, negY, posZ, negZ);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                else if (settings.m_colorMode == BakeSettings.AmbientColorMode.kColorGradient)
                {
                    // Check for color value change
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    EditorGUILayout.BeginVertical();

                    DrawColorGradient(gradGround, gradEquator, gradSky);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    Color c = EditorGUILayout.ColorField(Styles.m_ambientColor, solidColor);
                    EditorGUILayout.EndHorizontal();

                    if (c != solidColor)
                    {
                        settings.SetColorSolid(c);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            float max = settings.m_ambientMax;
            settings.m_ambientMax = EditorGUILayout.Slider(Styles.m_max, settings.m_ambientMax, 0f, 1f);
            if (!s_maxSliderActive && max != settings.m_ambientMax)
            {
                s_control = EditorGUIUtility.hotControl;
                s_maxSliderActive = true;
            }
            if (s_maxSliderActive && s_control != EditorGUIUtility.hotControl)
            {
                s_control = -1;
                s_maxSliderActive = false;
            }

            float min = settings.m_ambientMin;
            settings.m_ambientMin = EditorGUILayout.Slider(Styles.m_min, settings.m_ambientMin, 0f, 1f);
            if (!s_minSliderActive && min != settings.m_ambientMin)
            {
                s_control = EditorGUIUtility.hotControl;
                s_minSliderActive = true;
            }
            if (s_minSliderActive && s_control != EditorGUIUtility.hotControl)
            {
                s_control = -1;
                s_minSliderActive = false;
            }

            settings.m_ambientMax = Mathf.Clamp(settings.m_ambientMax, settings.m_ambientMin, 1f);
            settings.m_ambientMin = Mathf.Clamp(settings.m_ambientMin, 0f, settings.m_ambientMax);

            if (s_minSliderActive || s_maxSliderActive)
            {
                Color a = Color.black;
                float t = settings.m_ambientMin;
                if (s_maxSliderActive)
                {
                    t = settings.m_ambientMax;
                }

                if (settings.m_colorMode == BakeSettings.AmbientColorMode.kColorCube)
                {
                    Color px = Color.Lerp(a, posX, t);
                    Color nx = Color.Lerp(a, negX, t);
                    Color py = Color.Lerp(a, posY, t);
                    Color ny = Color.Lerp(a, negY, t);
                    Color pz = Color.Lerp(a, posZ, t);
                    Color nz = Color.Lerp(a, negZ, t);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    EditorGUILayout.BeginVertical();
                    DrawColorCube(px, nx, py, ny, pz, nz, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                else
                if (settings.m_colorMode == BakeSettings.AmbientColorMode.kColorGradient)
                {
                    Color sky = Color.Lerp(a, gradSky, t);
                    Color equator = Color.Lerp(a, gradEquator, t);
                    Color ground = Color.Lerp(a, gradGround, t);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    EditorGUILayout.BeginVertical();
                    DrawColorGradient(ground, equator, sky, false);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    Color solid = Color.Lerp(a, solidColor, t);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(Styles.kIndent);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.ColorField(solid);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(50);

            if (VertexBakerLib.Instance.BakeInProgress())
            {
                if (GUILayout.Button("Cancel"))
                {
                    VertexBakerLib.Instance.BakeCancel();
                }
            }
            else
            {
                settings.m_bakeAllLightSets = EditorGUILayout.ToggleLeft(Styles.m_bakeAll, settings.m_bakeAllLightSets);

                if (GUILayout.Button("Bake Scene"))
                {
                    if (settings.m_bakeAllLightSets)
                    {

                        // enqueue all bake sets
                        for (int i = 0, k = settingsData.m_settingsList.Count; i < k; ++i)
                        {
                            if (settingsData.m_settingsList[i].m_activeSet)
                            {
                                s_bakeSetQueue.Enqueue(i);
                            }
                        }

                        // set the first bake set 
                        if (s_bakeSetQueue.Count > 0)
                        {
                            int bakeSet = s_bakeSetQueue.Dequeue();
                            settingsData.SetBakeSetIndex(bakeSet);
                        }

                    }

                    BakeScene();
                }
            }

            BakeSets bakeSets = BakeData.Instance().GetBakeSets();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(bakeSets.m_containers.Count > 0 && GUILayout.Button(Styles.m_clearAllBakeData, Styles.m_clearButton))
            {
                if(EditorUtility.DisplayDialog(Styles.kEditorTitle, "Clear all data, are you sure?", "Yes", "No"))
                {
                    for(int i = 0, k = bakeSets.m_containers.Count; i < k; ++i)
                    {
                        List<Mesh> meshes = bakeSets.m_containers[i].m_list;
                        for(int j = 0; j < meshes.Count; ++j)
                        {
                            DestroyImmediate(meshes[j], true);
                        }
                    }
                }

                EditorUtility.SetDirty(bakeSets);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();


            if (EditorGUI.EndChangeCheck())
            {
                s_settingsDirty = true;
                Undo.RecordObject(settingsData, "SettingsUndo");
                //VertexBakerLib.Instance.WriteSettings();
                //VertexBakerLib.Instance.SaveSettings();
            }

        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        private static int DrawToolBar(int currentIndex, string[] tabNames, bool drawAddButton, out int settingsIndex)
        {
            s_bakeSetsFoldout = EditorGUILayout.Foldout(s_bakeSetsFoldout, "Bake Sets");

            int selected = currentIndex;
            settingsIndex = -1;

            if (s_bakeSetsFoldout)
            {
                for (int i = 0; i < tabNames.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);

                    string name = tabNames[i];
                    if (tabNames[i].Length > 16)
                    {
                        name = tabNames[i].Substring(0, 13);
                        name += "...";
                    }

                    GUIContent tabNameContent = new GUIContent(currentIndex == i ? tabNames[i] : name, tabNames[i]);

                    if (GUILayout.Button(tabNameContent, currentIndex == i ? Styles.m_toolbarButtonSelected : Styles.m_toolbarButton, GUILayout.Width(125)))
                    {
                        selected = i;
                    }
                    else if (GUILayout.Button("", currentIndex == i ? Styles.m_toolbarSettingsSelected : Styles.m_toolbarSettingsButton))
                    {
                        settingsIndex = i;
                        selected = Toolbar.kSettings;
                    }
                    GUILayout.Space(3);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                if (drawAddButton && GUILayout.Button("+", Styles.m_toolbarAddButton))
                {
                    selected = Toolbar.kAdd;
                }
                if (drawAddButton && GUILayout.Button("-", Styles.m_toolbarRemoveButton))
                {
                    selected = Toolbar.kRemove;
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);

            return selected;
        }

        public static void BakeScene()
        {
            List<GameObject> roots = Utilities.GetAllRoots();

            BakeHelper(roots.ToArray());
        }

        [MenuItem("GameObject/Daydream Baker/Bake", false, 0)]
        public static void BakeLightsNative()
        {
            // current selection
            OpenWindow();
            GameObject[] selection = Selection.gameObjects;
            BakeHelper(selection);
        }

        private static void BakeHelper(GameObject[] bakeRoots)
        {
            VertexBakerLib.Instance.BakeReset();

            DateTime bakeStart = DateTime.Now;

            List<MeshFilter> meshes = new List<MeshFilter>();
            s_meshRenderers = new List<MeshRenderer>();

            // gather meshes in selection
            foreach (GameObject go in bakeRoots)
            {
                MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter filter in filters)
                {
                    MeshRenderer mr = filter.GetComponent<MeshRenderer>();
	                if(filter.sharedMesh == null)
	                {
	                    Debug.LogWarning(filter.gameObject.GetPath() + " has a missing mesh");
	                }

                    bool staticLit = (StaticEditorFlags.LightmapStatic & GameObjectUtility.GetStaticEditorFlags(filter.gameObject)) > 0;
                    if (filter.sharedMesh != null && filter.gameObject.activeSelf && staticLit && mr != null && mr.enabled)
                    {
                        s_meshRenderers.Add(mr);
                        meshes.Add(filter);
                    }
                }
            }
            if (meshes.Count == 0)
            {
                EditorUtility.DisplayDialog(Styles.kEditorTitle, Styles.kNoStaticMeshes, "ok");
                return;
            }

            if (meshes.Count != s_meshRenderers.Count)
            {
                EditorUtility.DisplayDialog(Styles.kEditorTitle, "MeshRenderers are not 1 to 1 with Mesh Filters", "ok");
                return;
            }

            List<Light> lights = new List<Light>();
            DDRSettings settingsData = BakeData.Instance().GetBakeSettings();

            if(settingsData.SelectedBakeSet.m_forceAllLights)
            {
                List<GameObject> sceneRoots = Utilities.GetAllRoots();

                foreach(GameObject go in sceneRoots)
                {
                    Light[] lightList = go.GetComponentsInChildren<Light>();
                    foreach(Light light in lightList)
                    {
                        if(light.IsLightmapLight())
                        {
                            lights.Add(light);
                        }
                    }
                }
            }
            else
            {
                List<LightEntry> lightFilter = settingsData.SelectedBakeSet.m_lightList;
                Dictionary<int, Light> localFileIdToLight = Utilities.LightsByLocalFileId();

                foreach (LightEntry lightEntry in lightFilter)
                {
                    Light light = null;

                    // group if lights
                    if(!string.IsNullOrEmpty(lightEntry.m_group))
                    {
                        // get parent objects for each path that matches the group path
                        List<GameObject> parents = Utilities.FindAll(lightEntry.m_group);

                        // gather all lights under group
                        if(parents.Count > 0)
                        {
                            // add lights to the new group
                            foreach(GameObject parent in parents)
                            {
                                for(int i = 0; i < parent.transform.childCount; ++i)
                                {
                                    GameObject child = parent.transform.GetChild(i).gameObject;
                                    light = child.GetComponent<Light>();
                                    if(light.IsLightmapLight())
                                    {
                                        lights.Add(light);
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        // ungrouped light
                        if(localFileIdToLight.TryGetValue(lightEntry.m_idInFile, out light))
                        {
                            if(light.IsLightmapLight())
                            {
                                lights.Add(light);
                            }
                        }

                    }

                }
            }



            VertexBakerLib.Log("Collect data time: " + (DateTime.Now - bakeStart).TotalSeconds + " seconds");

            ///////////////
            // native bake
            ///////////////
            try
            {
                // stop listening for changes
                m_ignoreNextChange = true;

//                int activeLightCount = DaydreamRendererSceneData.GetActiveLightCount();
//                DaydreamRendererSceneData sceneData = TypeExtensions.FindOrCreateScriptableAsset<DaydreamRendererSceneData>(VertexBakerLib.DataPath, "scenedata");

                s_bakeInProgress = true;
                VertexBakerLib.Instance.Bake(meshes, lights, delegate ()
                {
                    s_bakeIsFinished = true;
                });

            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(e.StackTrace);
            }


        }

        public bool DrawCubeColor(string label, int labelWidth, Color color, out Color newColor)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(labelWidth));
            newColor = EditorGUILayout.ColorField(color);
            EditorGUILayout.EndHorizontal();

            return color != newColor;
        }

        public bool DrawColorGradient(Color ground, Color equator, Color sky, bool updateSettings = true)
        {
            bool changed = false;
            BakeSettings settings = BakeData.Instance().GetBakeSettings().SelectedBakeSet;

            Color newColor = sky;

            int labelWidth = 90;

            if (DrawCubeColor("Sky Color", labelWidth, sky, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorGradient(GradientIndex.Sky, newColor);
                }
            }
            if (DrawCubeColor("Equator Color", labelWidth, equator, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorGradient(GradientIndex.Equator, newColor);
                }
            }
            if (DrawCubeColor("Ground Color", labelWidth, ground, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorGradient(GradientIndex.Ground, newColor);
                }
            }

            return changed;
        }


        public bool DrawColorCube(Color px, Color nx, Color py, Color ny, Color pz, Color nz, bool updateSettings = true)
        {
            bool changed = false;
            BakeSettings settings = BakeData.Instance().GetBakeSettings().SelectedBakeSet;

            Color newColor = px;
            int labelWidth = 25;

            if (DrawCubeColor("+X", labelWidth, px, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.PosX, newColor);
                }
            }
            if (DrawCubeColor("-X", labelWidth, nx, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.NegX, newColor);
                }
            }
            if (DrawCubeColor("+Y", labelWidth, py, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.PosY, newColor);
                }
            }
            if (DrawCubeColor("-Y", labelWidth, ny, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.NegY, newColor);
                }
            }
            if (DrawCubeColor("+Z", labelWidth, pz, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.PosZ, newColor);
                }
            }
            if (DrawCubeColor("-Z", labelWidth, nz, out newColor))
            {
                changed = true;
                if (updateSettings)
                {
                    settings.SetColorCubeFace(Face.NegZ, newColor);
                }
            }

            return changed;
        }

        public static void DrawShadowAndAOSettings(BakeSettings settings, UnityEngine.Object undoObject = null, bool enableMinTessellationLevel = false)
        {

            // Tessellation - temporarily disabled
            /*
            // Tessellation settings
            settings.m_tessEnabled = EditorGUILayout.BeginToggleGroup(Styles.m_enableTessellation, settings.m_tessEnabled);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(Styles.kIndent);
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            settings.m_tessControlLevel = (DDRSettings.TessLevel)EditorGUILayout.EnumPopup("Controls", settings.m_tessControlLevel);
            if(GUILayout.Button(Styles.m_restoreDefaults, EditorStyles.miniButton))
            {
                if(EditorUtility.DisplayDialog("Tessellation Settings", "Restore defaults, are you sure?", "Yes", "No"))
                {
                    settings.RestoreTessellationDefaults();
                }
            }

            EditorGUILayout.EndHorizontal();

            const int maxLevel = 8;
            if(enableMinTessellationLevel)
            {
                settings.m_minTessIterations = EditorGUILayout.IntSlider(Styles.m_minTessIterations, settings.m_minTessIterations, 0, maxLevel);
            }
            settings.m_maxTessIterations = EditorGUILayout.IntSlider(Styles.m_maxTessIterations, settings.m_maxTessIterations, 0, maxLevel);
            //settings.m_maxTessVertices = EditorGUILayout.IntField(Styles.m_maxTessVertices, settings.m_maxTessVertices);

            if (settings.m_tessControlLevel == DDRSettings.TessLevel.Advanced || settings.m_tessControlLevel == DDRSettings.TessLevel.VeryAdvanced)
            {
                settings.m_maxShadowSoftTessIterations = EditorGUILayout.IntSlider(Styles.m_tessShadowSoftness, settings.m_maxShadowSoftTessIterations, 0, maxLevel);
                settings.m_maxShadowHardTessIterations = EditorGUILayout.IntSlider(Styles.m_tessShadowHardness, settings.m_maxShadowHardTessIterations, 0, maxLevel);
                settings.m_maxAOTessIterations = EditorGUILayout.IntSlider(Styles.m_tessAOPasses, settings.m_maxAOTessIterations, 0, maxLevel);
            }
            
            if(settings.m_tessControlLevel == DDRSettings.TessLevel.VeryAdvanced)
            {
                settings.m_intesityThreshold = EditorGUILayout.Slider(Styles.m_lightIntensityThreshold, settings.m_intesityThreshold, 0f, 1f);
                settings.m_avgIntensityThreshold = EditorGUILayout.Slider(Styles.m_avgLightIntensityThreshold, settings.m_avgIntensityThreshold, 0f, 1f);
                settings.m_accessabilityThreshold = EditorGUILayout.Slider(Styles.m_aoAccessibilityThreshold, settings.m_accessabilityThreshold, 0f, 1f);

                EditorGUILayout.MinMaxSlider(Styles.m_surfaceLightThreshold, ref settings.m_surfaceLightThresholdMin, ref settings.m_surfaceLightThresholdMax, 0f, 1f);
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(20);
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    settings.m_surfaceLightThresholdMin = EditorGUILayout.FloatField(Styles.m_surfaceLightThresholdMin, settings.m_surfaceLightThresholdMin);
                    settings.m_surfaceLightThresholdMax = EditorGUILayout.FloatField(Styles.m_surfaceLightThresholdMax, settings.m_surfaceLightThresholdMax);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndToggleGroup();
            //*/

            settings.m_shadowsEnabled = EditorGUILayout.BeginToggleGroup(Styles.m_enableShadows, settings.m_shadowsEnabled);
            {

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(Styles.kIndent);
                EditorGUILayout.BeginVertical();

                settings.m_lightSamples = Math.Max(4, EditorGUILayout.IntField(Styles.m_lightblockSamples, settings.m_lightSamples));
                settings.m_rayStartOffset = EditorGUILayout.Slider(Styles.m_lightblockStartOffest, settings.m_rayStartOffset, 0f, 1f);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();

            settings.m_ambientOcclusion = EditorGUILayout.BeginToggleGroup(Styles.m_enableAmbient, settings.m_ambientOcclusion);
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(Styles.kIndent);
                EditorGUILayout.BeginVertical();

                settings.m_aoForceDoubleSidedGeo = EditorGUILayout.Toggle(Styles.m_aoForceTwoSided, settings.m_aoForceDoubleSidedGeo);
                settings.m_occluderSearchSamples = Math.Max(4, EditorGUILayout.IntField(Styles.m_occluderSamples, settings.m_occluderSearchSamples));
                settings.m_normalBend = EditorGUILayout.Slider(Styles.m_normalBend, settings.m_normalBend, 0f, 1f);
                settings.m_occluderStartOffset = Mathf.Max(0f, EditorGUILayout.FloatField(Styles.m_occluderOffset, settings.m_occluderStartOffset));
                settings.m_occlusionRayLength = Mathf.Max(0f, EditorGUILayout.FloatField(Styles.m_occluderLength, settings.m_occlusionRayLength));

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();

            if(undoObject != null)
            {
                Undo.RecordObject(undoObject, undoObject.name + "_BakingUndo");
            }

        }


#if DAYDREAM_TESTING
        [MenuItem("GameObject/Daydream Baker/Test/FileSystem", false, 0)]
        static void TestFileSystem()
        {
            VertexBakerLib.Instance.TestFileSystem();
        }
#endif

    }
}
