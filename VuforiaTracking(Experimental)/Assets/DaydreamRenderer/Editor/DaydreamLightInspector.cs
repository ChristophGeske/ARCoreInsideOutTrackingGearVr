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
using System.Linq;

namespace daydreamrenderer
{
    [CustomEditor(typeof(DaydreamLight), true)]
    [CanEditMultipleObjects]
    public class DaydreamLightInspector : Editor
    {
        static System.Type m_annotationUtilType;
        static System.Reflection.PropertyInfo m_iconSizeProp;
        static Material iconMaterial;

        static class Styles
        {
            public static Texture2D daydreamLight = AssetDatabase.LoadAssetAtPath<Texture2D>(BakeData.kDaydreamPath + "Baking/Editor/Images/LightIcon.png");
        }

        static Material IconMaterial
        {
            get{
                if(iconMaterial == null){
                    iconMaterial = new Material(Shader.Find("Hidden/Daydream/Gizmo"));
                }

                return iconMaterial;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Daydream Renderer utilizes a custom lighting system that requires this components "+
                "in order to provide lighting data to shaders. This component is added automatically. This behavior can be disabled under " +
                "Window->Daydream Renderer->Import Wizard by unchecking the 'Auto add daydream lighting system components' toggle", MessageType.Info);

            base.OnInspectorGUI();
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
        static void DrawGizmos(DaydreamLight source, GizmoType gizmoType)
        {
            if (source == null || !source.enabled) return;

            Vector4 pos = source.transform.position;
            pos.w = 1f;

            m_annotationUtilType = typeof(Editor).Assembly.GetTypes().Where(t => t.Name == "AnnotationUtility").FirstOrDefault();
            if (m_annotationUtilType == null)
            {
                Debug.LogWarning("The internal type 'AnnotationUtility' could not be found. Maybe something changed inside Unity");
                return;
            }
            m_iconSizeProp = m_annotationUtilType.GetProperty("iconSize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            if (m_iconSizeProp == null)
            {
                Debug.LogWarning("The internal class 'AnnotationUtility' doesn't have a property called 'iconSize'");
            }

            Vector3 toCam = Camera.current.transform.position - source.transform.position;
            float z = Vector3.Dot(Camera.current.transform.forward, toCam);

            // create rotation towards the camera
            Matrix4x4 rot = Matrix4x4.LookAt(pos, source.transform.position + Camera.current.transform.forward*z, Camera.current.transform.up);
            Matrix4x4 world = new Matrix4x4();
            float scale = m_iconSizeProp == null ? 1f : (float)m_iconSizeProp.GetValue(null, null);

            Vector3 screen = new Vector3(Screen.width, Screen.height);
            Vector3 tr = Camera.current.ScreenToViewportPoint(screen) * 0.5f;
            Vector3 bl = Camera.current.ScreenToViewportPoint(-screen) * 0.5f;

            Rect r = new Rect(bl.x, bl.y, tr.x - bl.x, tr.y - bl.y);

            float multiplier = 1;
            if (Camera.current.orthographic)
                multiplier = (r.height * 0.5f) / (Camera.current.orthographicSize);
            else
                multiplier = (r.height * 0.5f) / Mathf.Tan(Mathf.Deg2Rad*(Camera.current.fieldOfView * 0.5f));

            scale *= multiplier * 20f;

            world.m00 = scale;
            world.m11 = scale;
            world.m22 = scale;
            world *= rot;
            world.SetColumn(3, pos);

            Material m = IconMaterial;
            m.SetMatrix("_world", world);
            m.color = source.m_light.color;
            m.mainTexture = Styles.daydreamLight;
            Gizmos.DrawGUITexture(new Rect(-1f, -1f, 2f, 2f), Styles.daydreamLight, m);


        }
    }
}
