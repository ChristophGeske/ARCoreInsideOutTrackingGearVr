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
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace daydreamrenderer
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class DaydreamVertexLighting : MonoBehaviour
    {
        const int kMaxVerts = (1 << 16)-1;
        public string m_sourceMeshPath;
        public Mesh m_sourceMesh;
        public Mesh m_untessellatedMesh;
        public bool m_wasTessellated = false;
        public BakeSets m_bakeSets;
        public MeshContainer m_currentContainer;
        public Mesh m_originalMesh;
        public string m_lightingMeshName;
        public string m_bakeId;
        protected MeshRenderer m_meshRenderer;

        // control initial light data loading
        private static int s_assignedLighting = 0;
        private int s_assignCount = s_assignedLighting + 1;

        // the light data
        private Mesh m_vertexLighting;
        public Mesh VertexLighting {
            get {
                if(m_vertexLighting == null)
                {
                    m_vertexLighting = GetLightinMeshFromContainer(m_lightingMeshName);
                }
                return m_vertexLighting;
            }
            set {
                m_vertexLighting = value;
                m_lightingMeshName = value.name;
            }
        }

        public Mesh GetSourcMesh()
        {
            return GetComponent<MeshFilter>().sharedMesh;
        }

        public Mesh GetUntessellatedMesh()
        {
            if(m_untessellatedMesh != null && m_untessellatedMesh.vertexCount > 0)
            {
                return m_untessellatedMesh;
            }
            return GetComponent<MeshFilter>().sharedMesh;
        }


        public void Awake()
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
#if UNITY_EDITOR
            m_sourceMesh = GetSourcMesh();
#endif
            LoadLightingMesh();

        }

        public void Start()
        {
            if(s_assignedLighting < s_assignCount && m_currentContainer != null)
            {
                s_assignedLighting++;
                UpdateAllVertexLighting(m_currentContainer.m_bakeSetId, true);
            }
        }
        
        #if UNITY_EDITOR
        public void Update()
        {
            if (!Application.isPlaying 
                && (m_meshRenderer == null || m_meshRenderer.additionalVertexStreams == null) 
                && m_currentContainer != null)
            {
                m_sourceMesh = GetSourcMesh();

                LoadLightingMesh();
            }
        }
        #endif

        [ContextMenu("ActivateLighting")]
        public void ActivateVertexLighting()
        {
            m_bakeSets.SetActiveBakeSet(m_currentContainer.m_bakeSetId);
            SetVertexLightingContainer(m_bakeSets.GetActiveContainer());
        }

        public bool SetVertexLightingContainer(MeshContainer activeContainer, bool force = false)
        {
            bool result = false;
            if (force || m_currentContainer.GetInstanceID() != activeContainer.GetInstanceID())
            {
                m_currentContainer = activeContainer;

                // get lighting mesh from the container
                Mesh newMesh = GetLightinMeshFromContainer(m_bakeId, true);

                if (newMesh != null)
                {
                    VertexLighting = newMesh;
                    LoadLightingMesh();
                    result = true;
                }

            }

            return result;
        }

        private Mesh GetLightinMeshFromContainer(string meshName, bool isSuffix = false)
        {
            if (m_currentContainer == null) return null;

            // get lighting mesh from the container
            Mesh lightinMesh = null;
            if(m_currentContainer != null)
            {
                lightinMesh = m_currentContainer.m_list.Find(delegate (Mesh m)
                {
                    if (m != null && m.name != null)
                    {
                        if (isSuffix)
                        {
                            return m.name.EndsWith(meshName);
                        }
                        else
                        {
                            return m.name == meshName;
                        }
                    }

                    return false;
                });
            }

            return lightinMesh;
        }


        // Update all vertex lighting in the scene
        public static void UpdateAllVertexLighting(string lightSetId, bool force = false)
        {
            List<GameObject> roots = Utilities.GetAllRoots();

            MeshContainer activeContainer = null;

            List<GameObject> changedGOs = new List<GameObject>();
            for (int i = 0; i < roots.Count; ++i)
            {
                DaydreamVertexLighting[] dvls = roots[i].GetComponentsInChildren<DaydreamVertexLighting>();
                foreach (DaydreamVertexLighting dvl in dvls)
                {
                    // find the mesh container based on the name, all objects share the same container asset so only need to find it once
                    if (activeContainer == null)
                    {
                        dvl.m_bakeSets.SetActiveBakeSet(lightSetId);
                        activeContainer = dvl.m_bakeSets.GetActiveContainer();
                    }

                    if (activeContainer != null)
                    {
                        if(dvl.SetVertexLightingContainer(activeContainer, force))
                        {
                            Renderer renderer = dvl.GetComponent<Renderer>();

                            // re-assign source mesh in order to re-batch
                            dvl.GetComponent<MeshFilter>().mesh = dvl.m_sourceMesh;

                            if (renderer.isPartOfStaticBatch)
                            {
                                changedGOs.Add(dvl.gameObject);
                            }
                        }
                    }
                }
            }

            // re-batch geometry if it was statically batched
            if (changedGOs.Count > 0)
            {
                StaticBatchingUtility.Combine(changedGOs.ToArray(), null);
                Resources.UnloadUnusedAssets();
            }

        }

    public void LoadLightingMesh()
    {
        if (m_meshRenderer == null)
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }
		if(m_sourceMesh == null)
		{
			m_sourceMesh = GetComponent<MeshFilter>().sharedMesh;
            if(m_sourceMesh == null)
            {
                Debug.LogWarning(gameObject.GetPath() + " is missing its mesh");
            }
        }
        if (m_meshRenderer.enabled && m_sourceMesh != null)
        {
			if (m_bakeSets != null && VertexLighting != null && m_sourceMesh.vertexCount == VertexLighting.vertexCount)
            {
                m_meshRenderer.additionalVertexStreams = VertexLighting;
            }
#if UNITY_EDITOR
                else
                {
                    Mesh mesh = new Mesh();
                    mesh.name = "temp";
                    Vector3[] uvs = new Vector3[m_sourceMesh.vertexCount];
                    Color[] colors = new Color[m_sourceMesh.vertexCount];
                    for (int i = 0; i < m_sourceMesh.vertexCount; ++i)
                    {
                        colors[i] = Color.white * 0.5f;
                        uvs[i] = Vector3.one * 0.5f;
                    }

                    mesh.vertices = m_sourceMesh.vertices;
                    mesh.SetColors(new List<Color>(colors));
                    mesh.SetUVs(2, new List<Vector3>(uvs));
                    mesh.SetUVs(3, new List<Vector3>(uvs));
                    mesh.UploadMeshData(true);
                    m_meshRenderer.additionalVertexStreams = mesh;
                }
#endif
            }

        }

    }
}