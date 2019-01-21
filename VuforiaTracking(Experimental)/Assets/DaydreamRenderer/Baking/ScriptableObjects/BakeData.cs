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
using System.Collections.Generic;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine.SceneManagement;
#endif

namespace daydreamrenderer
{
    /// <summary>
    /// Bake data aggregates all data into one place
    /// </summary>
    public class BakeData : ScriptableObject
    {
        public const string kDaydreamPath = "Assets/DaydreamRenderer/";
#if UNITY_EDITOR
        static string s_lastPath = "";
#endif
        static string s_scenePath;
        static string s_sceneName;

        public static string DataPath {
#if UNITY_EDITOR
            get {
                if(string.IsNullOrEmpty(s_scenePath) || s_sceneName != SceneManager.GetActiveScene().name)
                {
                    string sceneName = SceneManager.GetActiveScene().name;

                    if (string.IsNullOrEmpty(sceneName))
                    {
                        // new scene
                        s_scenePath = "Assets";
                    }
                    else
                    {
                        s_scenePath = SceneManager.GetActiveScene().path.Split(new string[] { ".unity" }, System.StringSplitOptions.None)[0];
                    }

                    // if the scene was unsaved but is now saved... copy data to new scene
                    if (!string.IsNullOrEmpty(sceneName) && string.IsNullOrEmpty(s_sceneName) && s_instance != null)
                    {
                        string oldPath = AssetDatabase.GetAssetPath(s_instance);
                        FileInfo fi = new FileInfo(s_scenePath + "/BakeData.asset");
                        if (!fi.Exists)
                        {
                            fi.Directory.Create();
                            // if asset does not exist move temp asset to new location
                            EditorUtility.SetDirty(s_instance);
                            AssetDatabase.SaveAssets();
                            File.Move(oldPath, s_scenePath + "/BakeData.asset");
                            AssetDatabase.Refresh(ImportAssetOptions.Default);
                            s_instance = AssetDatabase.LoadAssetAtPath<BakeData>(s_scenePath + "/BakeData.asset");
                        }
                    }

                    s_sceneName = sceneName;
                }
                return s_scenePath;
            }
#else
            get { return Application.dataPath; }
#endif
        }

        public BakeSets m_bakeSets;
        public DDRSettings m_bakeSettings;
        public List<MeshContainer> m_meshContainerList = new List<MeshContainer>();
        public VertexDebugState m_debugState;

#pragma warning disable

        #region Bake Settings
        public DDRSettings GetBakeSettings()
        {
#if UNITY_EDITOR
            if (m_bakeSettings == null)
            {
                m_bakeSettings = new DDRSettings();
                m_bakeSettings.name = "BakeSettings";
                AssetDatabase.AddObjectToAsset(m_bakeSettings, DataPath + "/BakeData.asset");
                EditorUtility.SetDirty(this);
                SaveBakeSettings();
            }
#endif
            return m_bakeSettings;
        }

        public void SaveBakeSettings()
        {
#if UNITY_EDITOR
            if (m_bakeSettings != null)
            {
                EditorUtility.SetDirty(m_bakeSettings);
                AssetDatabase.SaveAssets();
            }
#endif
        }
        #endregion

        #region Bake Sets
        public BakeSets GetBakeSets()
        {
#if UNITY_EDITOR
            if (m_bakeSets == null)
            {
                m_bakeSets = new BakeSets();
                m_bakeSets.name = "BakeSets";
                AssetDatabase.AddObjectToAsset(m_bakeSets, DataPath + "/BakeData.asset");
                EditorUtility.SetDirty(this);
                SaveBakeSets();
            }
#endif
            return m_bakeSets;
        }

        public void SaveBakeSets()
        {
#if UNITY_EDITOR
            if (m_bakeSets != null)
            {
                EditorUtility.SetDirty(m_bakeSets);
                AssetDatabase.SaveAssets();
            }
#endif
        }
        #endregion

        #region Mesh Container
        public MeshContainer GetMeshContainer(string bakeSetId)
        {
            string name = bakeSetId + "_lighting";
            MeshContainer mc = m_meshContainerList.Find(delegate (MeshContainer _mc) { return _mc.name == name; });
#if UNITY_EDITOR
            if (mc == null)
            {
                mc = new MeshContainer();
                mc.name = name;
                mc.m_bakeSetId = bakeSetId;
                m_meshContainerList.Add(mc);
                AssetDatabase.AddObjectToAsset(mc, DataPath + "/BakeData.asset");
                EditorUtility.SetDirty(this);
                SaveMeshContainer(mc);
            }
#endif
            return mc;
        }

        public void AddToMeshContainer(MeshContainer mc, Mesh mesh)
        {
#if UNITY_EDITOR
            if (mc != null)
            {
                mc.m_list.Add(mesh);
                // add to the container asset
                AssetDatabase.AddObjectToAsset(mesh, DataPath + "/BakeData.asset");
            }
#endif
        }

        public void SaveMeshContainer(MeshContainer mc)
        {
#if UNITY_EDITOR
            if (mc != null)
            {
                EditorUtility.SetDirty(mc);
                AssetDatabase.SaveAssets();
            }
#endif
        }
        #endregion

        #region Debug State

        public VertexDebugState GetDebugState()
        {

#if UNITY_EDITOR
            if (m_debugState == null)
            {
                m_debugState = new VertexDebugState();
                m_debugState.name = "DebugState";
                AssetDatabase.AddObjectToAsset(m_debugState, DataPath + "/BakeData.asset");
                EditorUtility.SetDirty(this);
                SaveDebugState();
            }
#endif
            return m_debugState;
        }

        public void SaveDebugState()
        {
#if UNITY_EDITOR
            if (m_debugState != null)
            {
                EditorUtility.SetDirty(m_debugState);
                AssetDatabase.SaveAssets();
            }
#endif
        }
        #endregion

#pragma warning restore

#if UNITY_EDITOR
        private static BakeData s_instance = null;
        public static BakeData Instance()
        {
            if(s_instance == null || s_lastPath != BakeData.DataPath)
            {
                s_lastPath = BakeData.DataPath;
                if (BakeData.DataPath != "")
                {
                    s_instance = TypeExtensions.FindOrCreateScriptableAsset<BakeData>(BakeData.DataPath, "BakeData");
                    s_instance.SaveBakeData();
                }
                else
                {
                    s_instance = new BakeData();
                }
               
            }
            
            return s_instance;
        }
#endif

        private void SaveBakeData()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }

}
