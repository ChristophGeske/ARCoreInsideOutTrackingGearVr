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

namespace daydreamrenderer
{
    public class DREditorUtility
    {
        public enum PreviewType
        {
            kStatic,
            kInteractive,
            kIcon
        }

        public struct RadioButtonOutput
        {
            public int m_selectedIndex;
            public int m_dropDownSelected;
        }

        public static class Styles
        {
            // Lighting system advanced settings
            public static GUIStyle dropDownStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            public static GUIStyle dropDownSelectedStyle = new GUIStyle(EditorStyles.toolbarDropDown);
            static Styles()
            {
                dropDownStyle.fixedWidth = 16f;
            }
        }

        public static void DrawPreview(PreviewRenderUtility previewUtil, Material mat, Mesh mesh, PreviewType previewType, Vector2 scroll, ref Quaternion camRot)
        {
            const string uPosition = "dr_LightPosition";
            const string uColor = "dr_LightColor";
            const string uAtten = "dr_LightAtten";

            // reset position and rotation
            previewUtil.camera.transform.position = -Vector3.forward * 4.25f;
            previewUtil.camera.transform.rotation = Quaternion.identity;

            Quaternion orientation = Quaternion.identity;

            if (previewType == PreviewType.kInteractive)
            {
                orientation = Quaternion.AngleAxis(scroll.y, Vector3.right)*Quaternion.AngleAxis(scroll.x, Vector3.up);

                if (Quaternion.Dot(camRot, orientation) < 0f)
                {
                    orientation = Quaternion.Inverse(orientation);
                    orientation.w *= -1;
                }

                camRot = Quaternion.Lerp(camRot, orientation, 0.25f);
                orientation = Quaternion.Inverse(camRot);
            }

            // apply camera orientation and position
            previewUtil.camera.transform.position = orientation * previewUtil.camera.transform.position;
            previewUtil.camera.transform.LookAt(Vector3.zero, orientation * Vector3.up);

            bool staticLit = mat.IsKeywordEnabled("STATIC_LIGHTING");

            Vector4 posSave = Vector4.zero;
            Vector4 colorSave = Vector4.zero;
            Vector4 attenSave = Vector4.zero;

            Debug.unityLogger.logEnabled = false;
            if (!staticLit)
            {
                if (mat.HasProperty(uPosition))
                {
                    posSave = mat.GetVector(uPosition);
                    colorSave = mat.GetColor(uColor);
                    attenSave = mat.GetVector(uAtten);
                }

                Vector4 dir = new Vector4(-0.707f, 0.0f, 0.707f, 0.0f);
                Vector4 atten = new Vector4(-1f, 1f, 1f, 625f);
                Vector3 color = new Vector4(1f, 1f, 1f, 1f);

                mat.SetVector(uPosition, dir);
                mat.SetVector(uColor, color);
                mat.SetVector(uAtten, atten);
            }

            Debug.unityLogger.logEnabled = true;


            if (mesh != null)
            {
                previewUtil.DrawMesh(mesh, Vector3.zero, Quaternion.identity, mat, 0);
            }

            previewUtil.camera.Render();

            if (!staticLit)
            {
                mat.SetVector(uPosition, posSave);
                mat.SetVector(uColor, colorSave);
                mat.SetVector(uAtten, attenSave);
            }
        }

        public static void SetPreviewProperties(Material mat)
        {
            const string uPosition = "dr_LightPosition";
            const string uColor = "dr_LightColor";
            const string uAtten = "dr_LightAtten";
            
            if (mat)
            {
                Vector4 dir = new Vector4(-0.707f, 0.0f, 0.707f, 0.0f);
                Vector4 atten = new Vector4(-1f, 1f, 1f, 625f);
                Vector3 color = new Vector4(1f, 1f, 1f, 1f);

                mat.SetVector(uPosition, dir);
                mat.SetVector(uColor, color);
                mat.SetVector(uAtten, atten);
            }
        }

        public static RadioButtonOutput DrawRadioButton(int currentIndex, GUIContent[] tabNames, GUIStyle unselectedStyle, GUIStyle selectedStyle, int indent = 20, int ellipisCutoff = 16, int buttonWidth = 125, bool[] dropDown = null)
        {
            RadioButtonOutput selections;
            selections.m_selectedIndex = currentIndex;
            selections.m_dropDownSelected = -1;

            ellipisCutoff = Mathf.Max(6, ellipisCutoff);

            for (int i = 0; i < tabNames.Length; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indent);

                GUIContent tabNameContent = tabNames[i];
                string name = tabNames[i].text;
                if (name.Length > ellipisCutoff)
                {
                    name = tabNames[i].text.Substring(0, ellipisCutoff-3);
                    name += "...";
                    tabNameContent = new GUIContent(name, tabNames[i].tooltip);
                }

                if (GUILayout.Button(tabNameContent, currentIndex == i ? selectedStyle : unselectedStyle, GUILayout.Width(buttonWidth)))
                {
                    selections.m_selectedIndex = i;
                }

                // add drop down arrow
                if(dropDown != null && dropDown.Length > i && dropDown[i])
                {
                    if (GUILayout.Button("", Styles.dropDownStyle))
                    {
                        selections.m_dropDownSelected = i;
                    }
                }

                GUILayout.Space(3);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();


            return selections;
        }

        public static bool FlexibleHorizButton(string text, params GUILayoutOption[] options)
        {
            return FlexibleHorizButton(new GUIContent(text), GUI.skin.button, options);
        }

        public static bool FlexibleHorizButton(string text, GUIStyle style, params GUILayoutOption[] options)
        {
            return FlexibleHorizButton(new GUIContent(text), style, options);
        }

        public static bool FlexibleHorizButton(GUIContent text, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool result = GUILayout.Button(text, style, options);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            return result;
        }

    }
}

