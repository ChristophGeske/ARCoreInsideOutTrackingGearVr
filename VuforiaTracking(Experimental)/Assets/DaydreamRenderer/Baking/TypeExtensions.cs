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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor;
#endif

namespace daydreamrenderer
{
    public static class TypeExtensions
    {

        public class LightModes
        {
            public const int REALTIME = 4;
            public const int BAKED = 2;
            public const int MIXED = 1;
        };

        public static Color ToColor(this Vector3 v, float alpha = 1f)
        {
            return new Color(v.x, v.y, v.z, alpha);
        }
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.r, color.g, color.b);
        }

        public static Vector4 ToVector4(this Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }

        public static Vector4 ToVector4(this Color color, float w)
        {
            return new Vector4(color.r, color.g, color.b, w);
        }

        public static Vector4 ToVector4(this Vector3 v, float w = 0f)
        {
            return new Vector4(v.x, v.y, v.z, w);
        }

        public static void Resize<T>(this List<T> list, int size, T copy)
        {
            int curSize = list.Count;
            if (size < curSize)
            {
                list.RemoveRange(size, curSize - size);
            }
            else if (size > curSize)
            {
                if (size > list.Capacity)
                {
                    list.Capacity = size;
                }
                list.AddRange(Enumerable.Repeat(copy, size - curSize));
            }
        }
        public static void Resize<T>(this List<T> list, int sz) where T : new()
        {
            Resize(list, sz, new T());
        }

#if UNITY_EDITOR
        public static T FindOrCreateScriptableAsset<T>(string assetFilePath, string filename) where T : ScriptableObject
        {
            FileInfo fi = new FileInfo(assetFilePath + "/" + filename + ".asset");

            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            T data = default(T);
            if (fi.Exists)
            {
                data = AssetDatabase.LoadAssetAtPath<T>(assetFilePath + "/" + filename + ".asset");
            }

            if (data == null)
            {
                data = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(data, assetFilePath + "/" + filename + ".asset");
                AssetDatabase.SaveAssets();
            }

            return data;
        }
#endif

        public static string GetPath(this GameObject go)
        {
            List<string> path = new List<string>();

            Transform current = go.transform;
            path.Add(current.name);

            while (current.parent != null)
            {
                path.Insert(0, current.parent.name);
                current = current.parent;
            }

            return string.Join("/", path.ToArray());
        }

        public static int GetUniqueId(this Light light)
        {
            string id = string.Format("{0}{1}{2}{3}"
                , light.type.ToString()
                , light.gameObject.transform.position.x
                , light.gameObject.transform.position.y
                , light.gameObject.transform.position.z);
            return id.GetHashCode();
        }

        public static int GetUniqueId(this MeshFilter filter)
        {
            MeshRenderer renderer = filter.GetComponent<MeshRenderer>();
            Vector3 center = renderer != null ? renderer.bounds.center : Vector3.zero;

            string id = string.Format("{0}|{1}{2}{3}"
                , filter.sharedMesh.name
                , center.x
                , center.y
                , center.z);
            return id.GetHashCode();
        }

        public static int GetUniqueId(this DaydreamVertexLighting comp)
        {
            MeshFilter filter = comp.GetComponent<MeshFilter>();
            MeshRenderer renderer = comp.GetComponent<MeshRenderer>();
            Vector3 center = renderer != null ? renderer.bounds.center : Vector3.zero;
            string id = string.Format("{0}|{1}{2}{3}"
                , filter != null ? filter.sharedMesh.name : "missing_mesh"
                , center.x
                , center.y
                , center.z);
            return id.GetHashCode();
        }


        public static bool IsLightmapLight(this Light light)
        {
#if UNITY_EDITOR
            if (light != null)
            {
                SerializedObject serialObj = new SerializedObject(light);
                SerializedProperty lightmapProp = serialObj.FindProperty("m_Lightmapping");

                if (light.gameObject.activeSelf && lightmapProp.intValue != LightModes.REALTIME)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static int LightMode(this Light light)
        {
#if UNITY_EDITOR
            if (light != null)
            {
                SerializedObject serialObj = new SerializedObject(light);
                SerializedProperty lightmapProp = serialObj.FindProperty("m_Lightmapping");

                return lightmapProp.intValue;
            }
#endif

            return 4;
        }

        public static int GetLocalIDinFile(this Component comp)
        {
            int m_LocalIdentfierInFile = 0;
#if UNITY_EDITOR

            PropertyInfo inspectorModeInfoProperty = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            SerializedObject serializedObject = new SerializedObject(comp);
            inspectorModeInfoProperty.SetValue(serializedObject, InspectorMode.Debug, null);

            SerializedProperty m_LocalIdentfierInFile_Prop = serializedObject.FindProperty("m_LocalIdentfierInFile");
            m_LocalIdentfierInFile = m_LocalIdentfierInFile_Prop.intValue;

            EditorUtility.SetDirty(comp);
#endif

            return m_LocalIdentfierInFile;
        }
    }

}
