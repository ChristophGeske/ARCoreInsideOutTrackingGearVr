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
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using daydreamrenderer;

namespace daydreamrenderer
{
    class AssetPostProcess : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject model)
        {
            Renderer[] rs = model.GetComponentsInChildren<Renderer>();
            foreach(Renderer r in rs)
            {
                foreach(Material m in r.sharedMaterials)
                {
                    if(m.shader.name.ToLower().Contains("daydream"))
                    {
                        DaydreamMeshRenderer dmr = r.gameObject.GetComponent<DaydreamMeshRenderer>();
                        if (dmr != null)
                        {
                            GameObject.DestroyImmediate(dmr, true);
                        }
                        r.gameObject.AddComponent<DaydreamMeshRenderer>();

                        break;
                    }
                }
            }
        }

        void OnPreprocessModel()
        {
            Debug.Log(assetPath);
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            List<string> assetNames = new List<string>(importedAssets);
        
            // Filter down to just model data
            assetNames.RemoveAll(delegate (string assetName)
                {
                    return !(assetName.ToLower().EndsWith("fbx") || assetName.ToLower().EndsWith("ma") || assetName.ToLower().EndsWith("mb"));
                });

            // pull out statically lit objects in the scene
            List<DaydreamVertexLighting> ddrList = FindReferencesInScene(assetNames);

            foreach (DaydreamVertexLighting dvl in ddrList)
            {
                // refresh the lighting which will revert to default if there is an inconsistency between source and lighting data
                dvl.LoadLightingMesh();

                if (dvl.m_sourceMesh.vertexCount != dvl.VertexLighting.vertexCount)
                {
                    string assetPath = AssetDatabase.GetAssetPath(dvl.m_sourceMesh);
                    string sourceModel = Path.GetFileName(assetPath);
                    string scenePath = dvl.gameObject.GetPath();

                    Debug.Log("Vertex Lighting for \"" + scenePath + "\" with model \"" + sourceModel + "\" has inconsistent lighting and mesh data, a rebake is needed");
                }
            }
        }

        // traverse through every daydream vertex lighting object in the scene to find objects that reference
        // an asset in assetNames
        static List<DaydreamVertexLighting> FindReferencesInScene(List<string> assetNames)
        {
            List<DaydreamVertexLighting> result = new List<DaydreamVertexLighting>();
            List<GameObject> roots = Utilities.GetAllRoots();

            for (int i = 0; i < roots.Count; ++i)
            {
                List<DaydreamVertexLighting> ddrList = new List<DaydreamVertexLighting>(roots[i].GetComponentsInChildren<DaydreamVertexLighting>());

                // search for objects that reference this source asset
                ddrList.ForEach(delegate (DaydreamVertexLighting obj)
                    {
                        MeshFilter mf = obj.GetComponent<MeshFilter>();
                        if (mf != null)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(mf.sharedMesh);
                        // if the asset matches save it in results
                        if (assetNames.Exists(delegate (string assetname)
                            {
                                return assetPath.EndsWith(assetname);
                            }))
                            {
                                result.Add(obj);
                            }
                        }
                    });

            }

            return result;
        }
    }
}