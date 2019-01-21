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
    public class LightSystemDialog : EditorWindow
    {

        public enum Result
        {
            Remove,
            Ok,
            Cancel,
        }

        public class Styles
        {
            public static GUIContent toggleLightingSystem = new GUIContent("Enable Daydream Lighting System", "Daydream replaces the lighting system");
            public static GUIContent toggleComponents = new GUIContent("Manual Components", "Lighting system editor will 'not' automatically add components to the scene");
            public static GUIContent addComponents = new GUIContent("Add to Scene", "Adds Daydream lighting system components to scene objects");
            public static GUIContent addComponentsToProject = new GUIContent("Add to Project Assets", "Adds Daydream lighting components to models and prefabs in the project (assets must have daydream shaders)");
            public static GUIContent removeComponents = new GUIContent("Remove from Scene", "Removes all Daydream lighting system components from the scene objects");
            public static GUIContent removeComponentsFromProject = new GUIContent("Remove from Project Assets", "Removes all Daydream lighting system components from models and prefabs in the project");

            static Styles()
            {
            }
        }

        public delegate void ResultCallback(Result result);
        public static bool m_active = false;
        public static LightSystemDialog m_instance = null;
        static ResultCallback m_resultCallback = null;

        public static LightSystemDialog ShowDialog(ResultCallback resultCallback)
        {
            if(m_instance != null)
            {
                m_instance.CloseDialog();
            }
            m_active = true;
            m_resultCallback = resultCallback;
            m_instance = EditorWindow.CreateInstance<LightSystemDialog>();
            
            Vector2 point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            m_instance.position = new Rect(point.x, point.y, 250, 300);
            m_instance.ShowPopup();
            return m_instance;
        }

        void OnGUI()
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Advanced Settings");
            if (GUILayout.Button("X"))
            {
                if (m_resultCallback != null)
                {
                    m_resultCallback(Result.Cancel);
                }
                CloseDialog();
            }
            EditorGUILayout.EndHorizontal();

            DaydreamRenderer renderer = FindObjectOfType<DaydreamRenderer>();
            if(renderer == null)
            {
                EditorGUILayout.LabelField("Daydream Renderer Not Enabled");
                return;
            }

            // ------------------------------------------------------------------- //
            // Daydream Lighting Advanced settings
            GUILayout.Space(5);
            DaydreamRendererImportManager.DrawSection(240, 1);
            renderer.m_enableManualLightingComponents = EditorGUILayout.BeginToggleGroup(Styles.toggleComponents, renderer.m_enableManualLightingComponents);
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(Styles.addComponents))
                {
                    DaydreamRendererImportManager.ApplyLightingComponents();
                }
                if (GUILayout.Button(Styles.removeComponents))
                {
                    DaydreamRendererImportManager.RemoveAllLightingComponents();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndToggleGroup();

            // Add components to FBX and Prefabs not in the scene
            // TODO - rework this UI
            /*
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(Styles.addComponentsToProject))
            {
                DaydreamRendererImportManager.ApplyLightingToProject();
            }
            if (GUILayout.Button(Styles.removeComponentsFromProject))
            {
                DaydreamRendererImportManager.RemoveAllLightingComponents();
            }
            EditorGUILayout.EndHorizontal();
            //*/
        }

        public void CancelDialog()
        {
            if (m_resultCallback != null)
            {
                m_resultCallback(Result.Cancel);
            }
            CloseDialog();
        }

        void CloseDialog()
        {
            m_active = false;
            this.Close();
        }

        void OnLostFocus()
        {
            CancelDialog();
        }
    }

}
