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

#if DAYDREAM_STATIC_LIGHTING_DEBUG
#define _DAYDREAM_STATIC_LIGHTING_DEBUG
#endif

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace daydreamrenderer
{
    public class DaydreamInspector : Editor
    {
        // flatbuffer data
        protected static BVHNode_FBWrapper s_bvhWrapper = new BVHNode_FBWrapper();
        protected static MeshCacheWrapper s_cacheWrapper = new MeshCacheWrapper();
        protected static VertexBakerLib.BVHHandle s_bvhHandle = null;
        protected static int s_lastInstanceId = 0;

        // hold debug state information
        protected static VertexDebugState s_debugState = null;

        protected static void Init(MeshFilter meshFilter, Mesh untessellatedMesh)
        {

            if (!Directory.Exists(FBSConstants.BasePath + "/Cache/"))
            {
                Directory.CreateDirectory(FBSConstants.BasePath + "/Cache/");
            }
            if (!Directory.Exists(FBSConstants.BasePath + "/BVHCache/"))
            {
                Directory.CreateDirectory(FBSConstants.BasePath + "/BVHCache/");
            }

            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning(meshFilter.gameObject.GetPath() + " is missing its source mesh");
                return;
            }
            s_debugState = BakeData.Instance().GetDebugState();

#if _DAYDREAM_STATIC_LIGHTING_DEBUG
            DateTime start = DateTime.Now;

            // if we still have a handle for some reason try to free it
            if (s_lastInstanceId != meshFilter.GetUniqueId())
            {
                if (s_bvhHandle != null)
                {
                    VertexBakerLib.Instance.FreeHandle(s_bvhHandle.Ptr());
                }
                s_bvhHandle = null;

                BuildWorldVertices(meshFilter);

                s_debugState.m_tessFaces = null;
            }
            
            TryLoadBVH(meshFilter);
            
            s_lastInstanceId = meshFilter.GetUniqueId();

            if (s_bvhWrapper == null)
            {
                s_bvhWrapper = new BVHNode_FBWrapper();
            }

            string sourceAssetPath = AssetDatabase.GetAssetPath(untessellatedMesh);
            if (!string.IsNullOrEmpty(sourceAssetPath) && !Application.isPlaying)
            {
                Debug.LogWarning("Could not find asset " + untessellatedMesh.name + " the asset may be an instance. Some debug data may not be available.");
            }

            string path = BVH.ConvertMeshIdToBVHPath(s_lastInstanceId);
            s_bvhWrapper.SetPath(path);
            s_bvhWrapper.Validate();

            s_cacheWrapper.SetPath("" + s_lastInstanceId);
            s_cacheWrapper.Validate();

            VertexBakerLib.Log("Debug setup time: " + (DateTime.Now - start).TotalSeconds + " seconds");
#endif
        }

        protected static void TryLoadBVH(MeshFilter meshFilter)
        {
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                if (!VertexBakerLib.Instance.LoadBVH(meshFilter, ref s_bvhHandle))
                {
                    // Load from scratch
                    VertexBakerLib.Log("Could not load BVH data, building from scratch");
                    VertexBakerLib.BVHHandle[] bvhHandles = new VertexBakerLib.BVHHandle[1];
                    // build BVH and get handle
                    VertexBakerLib.Instance.BuildBVH(new MeshFilter[] { meshFilter }, ref bvhHandles);
                    // make sure its good
                    if (bvhHandles != null && VertexBakerLib.Instance.ValidHandle(bvhHandles[0].Ptr()))
                    {
                        s_bvhHandle = bvhHandles[0];
                        if (VertexBakerLib.s_logging == VertexBakerLib.Logging.kVerbose)
                        {
                            VertexBakerLib.Log("BVH build success!");
                        }
                    }
                    else
                    {
                        s_bvhHandle = null;
                        VertexBakerLib.LogError("Invalid BVH Handle, BVH not built");
                    }
                }
            }
        }

        protected static void BuildWorldVertices(MeshFilter sourceMesh)
        {
            s_debugState.m_triangles = sourceMesh.sharedMesh.triangles;
            s_debugState.m_worldVerPos = sourceMesh.sharedMesh.vertices;
            s_debugState.m_worldNormals = sourceMesh.sharedMesh.normals;
            for (int i = 0; i < s_debugState.m_worldNormals.Length; ++i)
            {
                s_debugState.m_worldVerPos[i] = sourceMesh.transform.TransformPoint(s_debugState.m_worldVerPos[i]);
                s_debugState.m_worldNormals[i] = sourceMesh.transform.TransformVector(s_debugState.m_worldNormals[i]).normalized;
            }
        }

        protected static void BuildTessFaces(VertexBakerLib.BVHHandle bvh, MeshFilter sourceMesh, Mesh bakeData)
        {
            VertexBakerLib.Instance.FindTessellationTriangles(bvh, sourceMesh, bakeData, out s_debugState.m_tessFaces);
        }

    }
}