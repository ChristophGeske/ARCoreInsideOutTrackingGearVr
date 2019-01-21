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
using System.Linq;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Threading;
using System.IO;

namespace daydreamrenderer
{
    public class DaydreamMaterialPostProcessor : AssetPostprocessor {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            try
            {
                foreach (string path in importedAssets)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);

                    Texture2D icon = null;
                    CustomIcon.m_guidToIcon.TryGetValue(guid, out icon);
                    if (icon == null)
                    {
                        if (path.EndsWith("mat"))
                        {
                            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (mat != null)
                            {
                                if (mat.shader.name.ToLower().Contains("daydream"))
                                {
                                    IconCache.IconEntry entry = CustomIcon.Cache.Find(guid);
                                    if (entry == null)
                                    {
                                        entry = new IconCache.IconEntry();
                                        // add to cache
                                        CustomIcon.Cache.m_iconList.Add(entry);
                                    }

                                    CustomIcon.BuildIcon(mat, ref entry.m_icon);
                                    entry.m_dirty = true;
                                    entry.m_guid = guid;

                                    if (CustomIcon.m_guidToIcon.ContainsKey(guid))
                                    {
                                        // update dictionary
                                        CustomIcon.m_guidToIcon[guid] = entry.m_icon;
                                    }
                                    else
                                    {
                                        // add to dictionary
                                        CustomIcon.m_guidToIcon.Add(guid, entry.m_icon);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (string path in deletedAssets)
                {
                    string guid = AssetDatabase.AssetPathToGUID(path);

                    Texture2D icon = null;
                    CustomIcon.m_guidToIcon.TryGetValue(guid, out icon);
                    if (icon != null)
                    {
                        CustomIcon.m_guidToIcon.Remove(guid);
                        Object.DestroyImmediate(icon);
                    }

                    IconCache.IconEntry entry = CustomIcon.Cache.Find(guid);
                    if (entry != null)
                    {
                        if (entry.m_icon != null)
                        {
                            Object.DestroyImmediate(entry.m_icon);
                        }
                        CustomIcon.Cache.m_iconList.Remove(entry);
                        entry.Remove();
                    }
                }

                for (int i = 0; i < movedAssets.Length; i++)
                {
                    //Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
                    string oldGuid = AssetDatabase.AssetPathToGUID(movedFromAssetPaths[i]);
                    string newGuid = AssetDatabase.AssetPathToGUID(movedAssets[i]);

                    Texture2D icon = null;
                    CustomIcon.m_guidToIcon.TryGetValue(oldGuid, out icon);
                    if (icon != null)
                    {
                        CustomIcon.m_guidToIcon.Add(newGuid, icon);
                        CustomIcon.m_guidToIcon.Remove(newGuid);
                    }

                    IconCache.IconEntry entry = CustomIcon.Cache.Find(oldGuid);
                    if (entry != null)
                    {
                        // remove current
                        entry.Remove();
                        // update to new guild
                        entry.m_guid = newGuid;
                        entry.m_dirty = true;
                    }
                }
            }
            finally
            {
                CustomIcon.Cache.Serialize();
            }
        }
        
        [InitializeOnLoad]
        public class CustomIcon
        {
            public const int kSmallHeight = 16;
            public const int kImageSize = 128;

            public static Dictionary<string, Texture2D> m_guidToIcon = new Dictionary<string, Texture2D>();
            public static Dictionary<Texture2D, bool> m_rbldMap = new Dictionary<Texture2D, bool>();
            public static IconCache m_cache = null;

            private static PreviewRenderUtility m_previewUtility;

            public static IconCache Cache {
                get {
                    if (m_cache == null)
                    {
                        m_cache = new IconCache();
                        m_cache.DeSerialize();
                    }

                    return m_cache;
                }
            }

            private static Mesh m_previewMesh = null;
            private static string kPreviewSpherePath = "Assets/DaydreamRenderer/Editor/PreviewSphere.asset";

            static CustomIcon()
            {
                m_previewUtility = new PreviewRenderUtility();
                EditorApplication.projectWindowItemOnGUI += DrawCustomIcon;
            }

            ~CustomIcon()
            {
                if(m_cache != null)
                {
                    m_cache.Serialize();
                }
            }

            public static void MarkForRebuild(IconCache.IconEntry entry)
            {
                if (entry != null && m_guidToIcon.ContainsKey(entry.m_guid))
                {
                    entry.m_rebuild = true;
                    m_guidToIcon.Remove(entry.m_guid);
                }
            }

            public static void DrawCustomIcon(string guid, Rect rect)
            {
                // see if guid is in the map
                Texture2D icon = null;
                m_guidToIcon.TryGetValue(guid, out icon);

                if (icon == null)
                {
                    // search cache
                    IconCache.IconEntry entry = Cache.Find(guid);

                    // if we found an entry add it to the map for fast reference
                    if (entry != null && !entry.m_rebuild && entry.m_icon != null && entry.m_icon.width > 8)
                    {
                        // assign
                        icon = entry.m_icon;

                        if (!m_guidToIcon.ContainsKey(entry.m_guid))
                        {
                            m_guidToIcon.Add(entry.m_guid, entry.m_icon);
                        }
                    }
                    else
                    {
                        // otherwise create a new icon, update the cache, and add it to the map

                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path.EndsWith("mat"))
                        {
                            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (mat != null)
                            {
                                if (mat.shader.name.ToLower().Contains("daydream"))
                                {
                                    if (entry == null)
                                    {
                                        entry = new IconCache.IconEntry();
                                        Cache.m_iconList.Add(entry);
                                    }

                                    BuildIcon(mat, ref entry.m_icon);
                                    entry.m_rebuild = false;
                                    entry.m_guid = guid;
                                    entry.m_dirty = true;
                                    // add to cache
                                    // add to dictionary
                                    if (m_guidToIcon.ContainsKey(guid))
                                    {
                                        m_guidToIcon[guid] = entry.m_icon;
                                    }
                                    else
                                    {
                                        m_guidToIcon.Add(guid, entry.m_icon);
                                    }

                                    // assign
                                    icon = entry.m_icon;
                                }
                            }
                        }
                    }
                }

                // if we found an icon render it
                if (icon != null && rect.height > kSmallHeight)
                {
                    GUI.DrawTexture(new Rect(rect.x, rect.y - 7, rect.width, rect.height), icon, ScaleMode.ScaleToFit, false);
                }
            }

            public static void BuildIcon(Material mat, ref Texture2D icon)
            {
                if (m_previewMesh == null)
                {
                    m_previewMesh = AssetDatabase.LoadAssetAtPath<Mesh>(kPreviewSpherePath);
                }

                // get target buffer
                RenderTexture newIcon = RenderTexture.GetTemporary(kImageSize, kImageSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

                // save old target
                RenderTexture save = m_previewUtility.camera.targetTexture;
                // set new RT target
                m_previewUtility.camera.targetTexture = newIcon;
                // configure camera for icon capture
                m_previewUtility.camera.backgroundColor = new Color(80 / 255f, 80 / 255f, 80 / 255f, 1f);
                m_previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;

                // draw the preview
                Quaternion ignore = Quaternion.identity;
                DREditorUtility.DrawPreview(m_previewUtility, mat, m_previewMesh, DREditorUtility.PreviewType.kIcon, Vector2.zero, ref ignore);

                // restore target
                m_previewUtility.camera.targetTexture = save;

                if (icon != null) GameObject.DestroyImmediate(icon);

                RenderTexture saveActiveRT = RenderTexture.active;
                try
                {
                    RenderTexture.active = newIcon;
                    icon = new Texture2D(newIcon.width, newIcon.height, TextureFormat.RGB24, false);
                    icon.ReadPixels(new Rect(0, 0, newIcon.width, newIcon.height), 0, 0);
                    icon.Apply();
                }
                finally
                {
                    // if  texture creation false always make sure we do these things
                    RenderTexture.ReleaseTemporary(newIcon);
                    RenderTexture.active = saveActiveRT;
                }
            }
        }
        
     
    }
}