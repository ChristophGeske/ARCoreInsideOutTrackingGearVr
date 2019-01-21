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
using UnityEditor;

namespace daydreamrenderer
{
    public class BakeSetDialog : EditorWindow
    {

        static class Styles
        {
            public static readonly GUIContent m_bakeSetLabel = new GUIContent("Bake Set Name", "The name used to identify the bake set");
            public static readonly string m_missingDataDialogTitle = "Missing Data!";
            public static readonly string m_missingNameMessage = "You must fill in the 'Bake Set Name'";
            public static readonly string m_nameConflictTitle = "Name Exists!";
            public static readonly string m_conflictingNameMessage = "The name {0} already exists";

        }

        public delegate void ResultCallback(bool result, string name);

        string m_bakeSetName;

        public static bool m_active = false;
        static ResultCallback m_resultCallback = null;

        public static void ShowDialog(ResultCallback resultCallback)
        {
            m_active = true;
            m_resultCallback = resultCallback;
            BakeSetDialog window = ScriptableObject.CreateInstance<BakeSetDialog>();
            Vector2 point = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            window.position = new Rect(point.x, point.y, 250, 100);
            window.ShowPopup();
        }

        void CloseDialog()
        {
            m_active = false;
            this.Close();
        }

        void OnGUI()
        {
            GUILayout.Space(10);
            m_bakeSetName = EditorGUILayout.TextField(Styles.m_bakeSetLabel, m_bakeSetName);
            GUILayout.Space(20);
            if (GUILayout.Button("Create"))
            {
                if (string.IsNullOrEmpty(m_bakeSetName))
                {
                    EditorUtility.DisplayDialog(Styles.m_missingDataDialogTitle, Styles.m_missingNameMessage, "OK");
                }
                else
                {
                    DDRSettings settingsData = BakeData.Instance().GetBakeSettings();

                    string[] ids = new string[settingsData.m_settingsList.Count];
                    for (int i = 0, k = settingsData.m_settingsList.Count; i < k; ++i)
                    {
                        ids[i] = settingsData.m_settingsList[i].m_settingsId;
                    }


                    if (!settingsData.m_settingsList.Exists(delegate (DDRSettings.BakeSettings s)
                        {
                            return s.m_settingsId == m_bakeSetName;
                        }))
                    {
                        // name is good
                        m_resultCallback(true, m_bakeSetName);
                        CloseDialog();
                    }
                    else
                    {
                        // name already exists
                        EditorUtility.DisplayDialog(Styles.m_nameConflictTitle, string.Format(Styles.m_conflictingNameMessage, m_bakeSetName), "OK");
                    }


                }
            }
            if (GUILayout.Button("Cancel"))
            {
                if (m_resultCallback != null)
                {
                    m_resultCallback(false, null);
                }
                CloseDialog();
            }
        }

        void OnLostFocus()
        {
            if (m_resultCallback != null)
            {
                m_resultCallback(false, null);
            }
            CloseDialog();
        }
    }
}