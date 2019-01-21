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

using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace daydreamrenderer
{
    using Constants = DDRSettings.Constants;
    using LightEntry = DDRSettings.LightEntry;

    public class BakeSetSettingsDialog : EditorWindow
    {

        static class Styles
        {
            public static readonly GUIContent m_clearBakeData = new GUIContent("Clear", "Clears baked data for this set");
            public static readonly GUIContent m_activeBake = new GUIContent("Active", "Inactive bake set will be skipped when baking all sets.");
            public static readonly GUIContent m_lightGroups = new GUIContent("Select Lights", "Lights grouped by parent");
            public static readonly GUIContent m_bakeAllLights = new GUIContent("Force Enable All Lights", "Always include all lights during baking");
            public static GUIStyle m_groupedStyle = new GUIStyle(EditorStyles.label);
            public static GUIStyle m_ungroupedStyle = new GUIStyle(EditorStyles.label);
            public static GUIStyle m_clearButton = new GUIStyle(EditorStyles.miniButton);

            static Styles()
            {
                m_groupedStyle.fontStyle = FontStyle.Italic;
            }
        }

        public enum Result
        {
            Remove,
            Ok,
            Cancel,
        }

        public delegate void ResultCallback(Result remove, string bakeSetName, List<LightEntry> selectedLights, bool activeSet, bool forceAllLights);

        string m_bakeSetName;
        List<LightEntry> m_selectedLights;
        public bool m_activeSet = false;
        public bool m_forceAllLights = false;

        Dictionary<string, List<Light>> m_groups;
        Vector2 m_scrollPosition = Vector2.zero;
        public static bool m_active = false;
        static ResultCallback m_resultCallback = null;

        public static BakeSetSettingsDialog ShowDialog(string bakeSetName, List<LightEntry> lightGroups, bool activeSet, bool forceAllLights, ResultCallback resultCallback)
        {
            m_active = true;
            m_resultCallback = resultCallback;
            BakeSetSettingsDialog window = EditorWindow.CreateInstance<BakeSetSettingsDialog>();

            window.m_activeSet = activeSet;
            window.m_bakeSetName = bakeSetName;
            window.m_forceAllLights = forceAllLights;

            Vector2 point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(point.x, point.y, 250, 300);
            window.m_selectedLights = new List<LightEntry>(lightGroups);
            DDRSettings.GatherLightGroups(ref window.m_groups);
            window.ShowPopup();
            return window;
        }

        public void CancelDialog()
        {
            if (m_resultCallback != null)
            {
                m_resultCallback(Result.Cancel, m_bakeSetName, null, m_activeSet, m_forceAllLights);
            }
            CloseDialog();
        }

        void CloseDialog()
        {
            m_active = false;
            this.Close();
        }

        void OnGUI()
        {
            GUILayout.Space(10);

            m_bakeSetName = EditorGUILayout.TextField(m_bakeSetName);

            m_forceAllLights = EditorGUILayout.ToggleLeft(Styles.m_bakeAllLights, m_forceAllLights);

            if (m_groups == null)
            {
                CloseDialog();
                m_resultCallback(Result.Cancel, m_bakeSetName, null, m_activeSet, m_forceAllLights);
            }

            if (!m_forceAllLights)
            {

                EditorGUILayout.LabelField(Styles.m_lightGroups);

                m_scrollPosition = EditorGUILayout.BeginScrollView(m_scrollPosition);
                {
                    // 'ungrouped' lights under root
                    List<Light> rootLights = null;
                    if (m_groups.TryGetValue(Constants.kRoot, out rootLights))
                    {
                        for (int i = 0; i < rootLights.Count; ++i)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(10);
                            EditorGUILayout.LabelField(rootLights[i].name, Styles.m_ungroupedStyle);

                            int idInFile = rootLights[i].GetLocalIDinFile();
                            // search to see if light already exists in the list
                            bool included = m_selectedLights.Exists(delegate(LightEntry obj)
                                {
                                    // find by object reference
                                    return obj.m_idInFile == idInFile;
                                });

                            // Create UI and list for toggle
                            if (EditorGUILayout.Toggle(included))
                            {
                                // added - toggle was checked and the item was not already selected
                                if (!included)
                                {
                                    m_selectedLights.Add(new LightEntry(rootLights[i], null));
                                }
                            }
                            else
                            {
                                // was included - it was unchecked and was previously selected so it needs to be removed
                                if (included)
                                {
                                    m_selectedLights.RemoveAll(delegate(LightEntry obj)
                                        {
                                            // find by object reference
                                            return obj.m_idInFile == idInFile;
                                        });

                                }
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    // Lights contained under a parent group
                    Dictionary<string, List<Light>>.Enumerator iter = m_groups.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        string groupPath = iter.Current.Key;
                        if (groupPath == Constants.kRoot) continue;


                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUIContent groupName = new GUIContent(groupPath, "Lights grouped by parent");
                        EditorGUILayout.LabelField(groupName, Styles.m_groupedStyle);

                        // search for group path
                        bool included = m_selectedLights.Exists(delegate(LightEntry obj) {
                            return obj.m_group == groupPath;
                        });

                        if (EditorGUILayout.Toggle(included))
                        {
                            // added
                            if (!included)
                            {
                                // add new group path
                                m_selectedLights.Add(new LightEntry(null, groupPath));
                            }
                        }
                        else
                        {
                            // was included
                            if (included)
                            {
                                int idx = m_selectedLights.FindIndex(delegate(LightEntry obj)
                                    {
                                        return obj.m_group == groupPath;   
                                    });

                                if(idx >= 0)
                                {
                                    m_selectedLights.RemoveAt(idx);
                                }

                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }

            GUILayout.Space(20);

            m_activeSet = EditorGUILayout.ToggleLeft(Styles.m_activeBake, m_activeSet);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                m_resultCallback(Result.Ok, m_bakeSetName, m_selectedLights, m_activeSet, m_forceAllLights);
                CloseDialog();
            }
            if (GUILayout.Button("Cancel"))
            {
                m_resultCallback(Result.Cancel, m_bakeSetName, m_selectedLights, m_activeSet, m_forceAllLights);
                CloseDialog();
            }
            EditorGUILayout.EndHorizontal();

            BakeSets bakeSets = BakeData.Instance().GetBakeSets();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (bakeSets.m_containers.Count > 0 && GUILayout.Button(Styles.m_clearBakeData, Styles.m_clearButton))
            {
                if (EditorUtility.DisplayDialog(m_bakeSetName, "Clear data, are you sure?", "Yes", "No"))
                {
                    for (int i = 0, k = bakeSets.m_containers.Count; i < k; ++i)
                    {
                        if(bakeSets.m_containers[i].m_bakeSetId == m_bakeSetName)
                        {
                            List<Mesh> meshes = bakeSets.m_containers[i].m_list;
                            for (int j = 0; j < meshes.Count; ++j)
                            {
                                DestroyImmediate(meshes[j], true);
                            }
                            break;
                        }
                    }
                }

                EditorUtility.SetDirty(bakeSets);
                AssetDatabase.SaveAssets();
            }
            GUILayout.EndHorizontal();

        }

        void OnLostFocus()
        {
            CancelDialog();
        }
    }
}