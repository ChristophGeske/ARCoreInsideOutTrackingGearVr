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
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace daydreamrenderer
{

    [CustomEditor(typeof(DaydreamVertexLighting), true)]
    public class DaydreamVertexLightingInspector : DaydreamInspector
    {
        #if _DAYDREAM_STATIC_LIGHTING_DEBUG
        static int s_controlHash = "DaydreamLightBakerInspectorHash".GetHashCode();
        static Vector2 s_position;
        static bool s_inputUpdate = false;
        static int s_currentIdx = 0;
        static int s_currentFace = 0;
        #endif
        static Stack<Color> s_gizmoColors = new Stack<Color>();
        static SamplePatch s_samplePatch = new SamplePatch();
        
        static OccluderData s_occluderTestData = new OccluderData();
        
        public static class Styles
        {
            public static readonly GUIContent m_vertLightingLabel = new GUIContent("Vertex Lighting Mesh", "Additional vertex stream containing lighting data");
            public static readonly GUIContent m_debugLabel = new GUIContent("Debug Visualizations", "Debug visualizations for tuning vertex lighting");
            public static readonly GUIContent m_vertOffset = new GUIContent("Vertex Offset", "Offset the selected vert in the mesh (allows manually scrolling through verts)");
            public static readonly GUIContent m_lightBlockerLabel = new GUIContent("Light Blockers", "Show light ray samples at the vertex sample patch, affected by the 'Shadow->SampleCount' setting");
            public static readonly GUIContent m_sampleLightLabel = new GUIContent("Light Source", "Light sample source");
            public static readonly GUIContent m_samplePatchLabel = new GUIContent("Sample Patch", "The sample area for the selected vertex");
            public static readonly GUIContent m_adjacenciesLabel = new GUIContent("Adjacencies", "Show vertices listed as adjacent");
            public static readonly GUIContent m_aoSamplesLabel = new GUIContent("AO Samples", "Show sample points used to determine ambient occlusion, samples affected by 'Ambient Occlusion->SampleCount' setting"
                + " Note: Samples use the normals based on the 'Normal Averaging' setting");
            public static readonly GUIContent m_avgNormalsLabel = new GUIContent("Averaged Normals", "Show how 'Normal Averaging' setting is affecting normals");
            public static readonly GUIContent m_bvhLabel = new GUIContent("BVH", "Show the bounding volume hierarchy of the mesh");
            public static readonly GUIContent m_normalsLabel = new GUIContent("Mesh Normals", "Show the normals stored in the vertex");
            public static readonly GUIContent m_tessellationLabel = new GUIContent("Tessellation Faces", "Show the faces that would be flagged for tessellation");
            public static readonly GUIContent m_selectedFaceLabel = new GUIContent("Selected Face", "Show the currently selected face");
        }

        public class OccluderData
        {
            public List<Vector3> m_testPoints = new List<Vector3>();
            public List<Vector3> m_colPoints = new List<Vector3>();
            public float m_accessability = 0f;
            public Vector3 m_accumulatedRayDir = Vector3.zero;
        }

        public void OnEnable()
        {
            DaydreamVertexLighting source = target as DaydreamVertexLighting;
            MeshFilter meshFilter = source.GetComponent<MeshFilter>();
            Mesh untessellatedMesh = source.GetUntessellatedMesh();

            Init(meshFilter, untessellatedMesh);
        }

        public override void OnInspectorGUI()
        {
            DaydreamVertexLighting source = target as DaydreamVertexLighting;

            if (source.m_currentContainer != null)
            {
                EditorGUILayout.ObjectField(Styles.m_vertLightingLabel, source.m_currentContainer, typeof(MeshContainer), false);
            }

            if (source.VertexLighting != null)
            {
                EditorGUILayout.ObjectField(Styles.m_vertLightingLabel, source.VertexLighting, typeof(Mesh), false);
            }

#if _DAYDREAM_STATIC_LIGHTING_DEBUG
            s_debugState.m_debugEnabled = EditorGUILayout.Foldout(s_debugState.m_debugEnabled, Styles.m_debugLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            if (s_debugState.m_debugEnabled)
            {
                s_debugState.m_indexOffset = EditorGUILayout.IntField(Styles.m_vertOffset, s_debugState.m_indexOffset);
                s_debugState.m_showFace = EditorGUILayout.Toggle(Styles.m_selectedFaceLabel, s_debugState.m_showFace);
                s_debugState.m_showLightBlockerSamples = EditorGUILayout.Toggle(Styles.m_lightBlockerLabel, s_debugState.m_showLightBlockerSamples);
                if (s_debugState.m_showLightBlockerSamples)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20f);
                    s_debugState.m_testLight = EditorGUILayout.ObjectField(Styles.m_sampleLightLabel, s_debugState.m_testLight, typeof(Light), true) as Light;
                    EditorGUILayout.EndHorizontal();
                }
                s_debugState.m_showSamplePatch = EditorGUILayout.Toggle(Styles.m_samplePatchLabel, s_debugState.m_showSamplePatch);
                s_debugState.m_showAdjacencies = EditorGUILayout.Toggle(Styles.m_adjacenciesLabel, s_debugState.m_showAdjacencies);
                s_debugState.m_showAOSamples = EditorGUILayout.Toggle(Styles.m_aoSamplesLabel, s_debugState.m_showAOSamples);
                s_debugState.m_showBentNormalSamples = EditorGUILayout.Toggle(Styles.m_avgNormalsLabel, s_debugState.m_showBentNormalSamples);
                s_debugState.m_showBVH = EditorGUILayout.Toggle(Styles.m_bvhLabel, s_debugState.m_showBVH);
                s_debugState.m_showNormals = EditorGUILayout.Toggle(Styles.m_normalsLabel, s_debugState.m_showNormals);
                s_debugState.m_showTessTriangles = EditorGUILayout.Toggle(Styles.m_tessellationLabel, s_debugState.m_showTessTriangles);
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(s_debugState);
                EditorUtility.SetDirty(source);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            if (s_debugState != null && s_debugState.m_showSamplePatch && GUILayout.Button("Tessellate triangles") && s_bvhHandle.IsValid())
            {
                List<int> subMesh = new List<int>
                {
                    s_debugState.m_vertexSampleIndex
                };

                subMesh.AddRange(s_cacheWrapper.GetAdjacencies(s_debugState.m_vertexSampleIndex));

                // triangles with indices into mesh
                List<int> subMeshFaces = s_cacheWrapper.GetAllFacesForIndex(s_debugState.m_vertexSampleIndex);

                foreach (int face in subMeshFaces)
                {
                    int v0 = source.m_sourceMesh.triangles[face];
                    int v1 = source.m_sourceMesh.triangles[face+1];
                    int v2 = source.m_sourceMesh.triangles[face+2];

                    s_cacheWrapper.GetTrisForEdge(v0, v1);
                    s_cacheWrapper.GetTrisForEdge(v0, v2);
                    s_cacheWrapper.GetTrisForEdge(v1, v2);
                }

                // tessellate the sub mesh represented by the adjacencies list, triangles should
                // list all the faces for the adjacencies in question
                VertexBakerLib.Instance.TessellateTriangles(s_bvhHandle, subMeshFaces);
            }

#endif
        }

#if _DAYDREAM_STATIC_LIGHTING_DEBUG
        public void OnSceneGUI()
        {
            // collecting mouse input to handle custom picking
            int id = GUIUtility.GetControlID(s_controlHash, FocusType.Passive);

            EventType eType = Event.current.GetTypeForControl(id);

            if (eType == EventType.MouseDown)
            {
                if ((Event.current.button == 0 || Event.current.button == 1))
                {
                    if (Event.current.button == 0)
                    {
                        s_position = Event.current.mousePosition;
                        s_inputUpdate = true;
                    }
                }
            }
        }
#endif

#if _DAYDREAM_STATIC_LIGHTING_DEBUG
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawBakerGizmos(DaydreamVertexLighting source, GizmoType gizmoType)
        {

            if (s_bvhHandle != null && !s_bvhHandle.IsValid())
            {
                TryLoadBVH(source.GetComponent<MeshFilter>());
            }

            if (s_bvhWrapper.IsValid() && s_cacheWrapper.IsValid() && s_bvhHandle.IsValid())
            {
                if (s_debugState.m_worldVerPos == null || s_debugState.m_worldVerPos.Length == 0)
                {
                    BuildWorldVertices(source.GetComponent<MeshFilter>());
                }

                // update selection
                if (Event.current != null && s_inputUpdate)
                {
                    s_inputUpdate = false;
                    Ray ray = HandleUtility.GUIPointToWorldRay(s_position);
                    float farPlane = 10000.0f;
                    if (Camera.main != null)
                    {
                        farPlane = Camera.main.farClipPlane;
                    }

                    int index = VertexBakerLib.Instance.RayToIndex(s_bvhHandle, ray.origin, ray.origin + ray.direction * farPlane);
                    int colFace = VertexBakerLib.Instance.RayToTriangle(s_bvhHandle, ray.origin, ray.origin + ray.direction * farPlane);
                    if (index < 0 || colFace < 0)
                    {
                        return;
                    }

                    s_currentFace = colFace;
                    s_currentIdx = index;

                    if(s_debugState.m_showFace) Debug.Log("Selected face " + s_currentFace);
                }

                if (s_currentIdx + s_debugState.m_indexOffset < 0) return;

                int faceIdx = s_currentFace % (s_debugState.m_triangles.Length);
                int idx = (s_currentIdx + s_debugState.m_indexOffset) % s_debugState.m_worldVerPos.Length;

                s_debugState.m_vertexSampleIndex = idx;

                float patchScale = Mathf.Max(source.transform.localToWorldMatrix.m00, source.transform.localToWorldMatrix.m11, source.transform.localToWorldMatrix.m22);
                float radius = s_cacheWrapper.GetPatchRadius(idx);
                float sampleRadius = radius * patchScale;

                if(faceIdx > -1 && s_debugState.m_showFace)
                {
                    Color c = Color.red + Color.green;
                    PushGizmoColor(c);

                    int[] triangles = s_debugState.m_triangles;
                    
                    Vector3 n = s_debugState.m_worldNormals[triangles[faceIdx]] * 0.02f;
                    Vector3 v0 = s_debugState.m_worldVerPos[triangles[faceIdx]];
                    Vector3 v1 = s_debugState.m_worldVerPos[triangles[faceIdx + 1]];
                    Vector3 v2 = s_debugState.m_worldVerPos[triangles[faceIdx + 2]];

                    Gizmos.DrawLine(n + v0, n + v1);
                    Gizmos.DrawLine(n + v0, n + v2);
                    Gizmos.DrawLine(n + v1, n + v2);

                    PopGizmoColor();

                }

                if (s_debugState.m_showBVH)
                {
                    List<Bounds> bounds = s_bvhWrapper.GetBounds();

                    foreach (Bounds b in bounds)
                    {
                        Gizmos.DrawWireCube(b.center, b.size);
                    }
                }

                if (s_debugState.m_showSamplePatch)
                {
                    Color c = Color.green;
                    PushGizmoColor(c);
                    Gizmos.DrawWireSphere(s_debugState.m_worldVerPos[idx], sampleRadius);

                    Vector3[] corners = s_bvhWrapper.GetPatchCorners(idx);

                    PushGizmoColor(Color.blue);
                    Vector3 n = s_debugState.m_worldNormals[idx] * 0.07f;
                    Gizmos.DrawLine(n + corners[0], n + corners[1]); // TL->TR
                    Gizmos.DrawLine(n + corners[1], n + corners[2]); // TR->BR
                    Gizmos.DrawLine(n + corners[2], n + corners[3]); // BR->BL
                    Gizmos.DrawLine(n + corners[3], n + corners[0]); // BL->TL
                    PopGizmoColor();

                    PopGizmoColor();
                }

                if (s_debugState.m_showAOSamples)
                {

                    UpdateAOSample(source, idx);
                    if (s_occluderTestData.m_testPoints.Count > 0)
                    {
                        PushGizmoColor(Color.cyan);
                        Gizmos.DrawWireSphere(s_debugState.m_worldVerPos[idx], s_cacheWrapper.GetPatchRadius(idx) * BakeData.Instance().GetBakeSettings().SelectedBakeSet.m_occlusionRayLength);
                        PushGizmoColor(Color.green * 1.5f);
                        Gizmos.DrawSphere(s_debugState.m_worldVerPos[idx], 0.01f);
                        PopGizmoColor();
                        PopGizmoColor();

                        PushGizmoColor(Color.green + Color.cyan * 0.5f);
                        for (int i = 0; i < s_occluderTestData.m_testPoints.Count; ++i)
                        {
                            Gizmos.DrawSphere(s_occluderTestData.m_testPoints[i], sampleRadius * 0.1F);
                        }
                        PopGizmoColor();
                        PushGizmoColor(Color.red);
                        for (int i = 0; i < s_occluderTestData.m_colPoints.Count; ++i)
                        {
                            Gizmos.DrawSphere(s_occluderTestData.m_colPoints[i], sampleRadius * 0.1F);
                        }
                        PopGizmoColor();

                        PushGizmoColor(new Color(1f, 0f, 1f));
                        Gizmos.DrawLine(s_debugState.m_worldVerPos[idx], s_debugState.m_worldVerPos[idx] + s_occluderTestData.m_accumulatedRayDir);
                        PopGizmoColor();

                        s_debugState.m_accessability = s_occluderTestData.m_accessability;
                    }

                }

                if (s_debugState.m_showAdjacencies)
                {
                    List<int> adj = s_cacheWrapper.GetAdjacencies(idx);
                    PushGizmoColor(Color.cyan);
                    Gizmos.DrawSphere(s_debugState.m_worldVerPos[idx], SphereSize(sampleRadius));
                    PopGizmoColor();

                    PushGizmoColor(Color.red);
                    for (int i = 0; i < adj.Count; ++i)
                    {
                        if (s_debugState.m_worldVerPos.Length > adj[i])
                        {
                            Gizmos.DrawSphere(s_debugState.m_worldVerPos[adj[i]], SphereSize(sampleRadius));
                        }
                    }
                    PopGizmoColor();
                }

                if (s_debugState.m_showLightBlockerSamples)
                {
                    UpdateLightBlockerSamplers(source, idx);
                }

                if (s_debugState.m_showNormals)
                {
                    PushGizmoColor(Color.green);
                    Gizmos.DrawLine(s_debugState.m_worldVerPos[idx], s_debugState.m_worldVerPos[idx] + s_debugState.m_worldNormals[idx]);
                    if (s_debugState.m_showAdjacencies)
                    {
                        List<int> adj = s_cacheWrapper.GetAdjacencies(idx);
                        for (int i = 0; i < adj.Count; ++i)
                        {
                            Gizmos.DrawLine(s_debugState.m_worldVerPos[adj[i]], s_debugState.m_worldVerPos[adj[i]] + s_debugState.m_worldNormals[idx]);
                        }
                    }
                    PopGizmoColor();
                }

                if (s_debugState.m_showBentNormalSamples)
                {
                    List<int> adj = s_cacheWrapper.GetAdjacencies(idx);

                    PushGizmoColor(Color.red);
                    Vector3 pos = s_debugState.m_worldVerPos[idx];
                    Vector3 norm = s_cacheWrapper.GetBentNormal(idx);
                    // normal move to world space
                    norm = source.transform.TransformVector(norm).normalized;

                    Gizmos.DrawLine(pos, pos + norm);

                    for (int i = 0; i < adj.Count; ++i)
                    {
                        pos = s_debugState.m_worldVerPos[adj[i]];
                        norm = s_cacheWrapper.GetBentNormal(adj[i]);
                        // normal move to world space
                        norm = source.transform.TransformVector(norm).normalized;

                        Gizmos.DrawLine(pos, pos + norm);
                    }
                    PopGizmoColor();
                }

                if (s_debugState.m_showTessTriangles && source.VertexLighting != null)
                {
                    if (s_debugState.m_tessFaces == null)
                    {
                        MeshFilter meshFilter = source.GetComponent<MeshFilter>();
                        //MeshRenderer mr = source.GetComponent<MeshRenderer>();
                        BuildTessFaces(s_bvhHandle, meshFilter, source.VertexLighting);
                        //mr.additionalVertexStreams = null;
                        //meshFilter.sharedMesh.SetColors(colors);
                    }

                    Color c = Color.green;
                    PushGizmoColor(c);

                    int[] triangles = s_debugState.m_triangles;

                    for (int f = 0; f< s_debugState.m_tessFaces.Length; ++f)
                    {
                        int face = s_debugState.m_tessFaces[f];
                        Vector3 n = s_debugState.m_worldNormals[triangles[face]] * 0.07f;
                        Vector3 v0 = s_debugState.m_worldVerPos[triangles[face]];
                        Vector3 v1 = s_debugState.m_worldVerPos[triangles[face+1]];
                        Vector3 v2 = s_debugState.m_worldVerPos[triangles[face+2]];

                        Gizmos.DrawLine(n + v0, n + v1);
                        Gizmos.DrawLine(n + v0, n + v2);
                        Gizmos.DrawLine(n + v1, n + v2); 
                    }
                    
                    PopGizmoColor();
                }
                else
                {
                    s_debugState.m_tessFaces = null;
                }

            }

        }
#endif


        public static void UpdateAOSample(DaydreamVertexLighting source, int idx)
        {
            s_debugState.m_lastVertexSampleIndex = s_debugState.m_vertexSampleIndex;

            Vector3 worldPos = s_debugState.m_worldVerPos[idx];
            Vector3 worldNormal = s_debugState.m_worldNormals[idx];
            Vector3 bentNormal = source.transform.TransformVector(s_cacheWrapper.GetBentNormal(idx)).normalized;

            Vector3 bentWorldNormal = Vector3.Lerp(worldNormal, bentNormal, BakeData.Instance().GetBakeSettings().SelectedBakeSet.m_normalBend);

            s_occluderTestData.m_testPoints.Clear();
            s_occluderTestData.m_colPoints.Clear();

            VertexBakerLib.Instance.OccludersAtVertex(s_bvhHandle, idx, worldPos, worldNormal, bentWorldNormal
                , ref s_occluderTestData.m_testPoints, ref s_occluderTestData.m_testPoints
                , ref s_occluderTestData.m_accessability, ref s_occluderTestData.m_accumulatedRayDir);
        }

        public static void UpdateLightBlockerSamplers(DaydreamVertexLighting source, int idx)
        {
            if (idx >= s_debugState.m_worldVerPos.Length) return;

            float patchScale = Mathf.Max(source.transform.localToWorldMatrix.m00, source.transform.localToWorldMatrix.m11, source.transform.localToWorldMatrix.m22);
            float sampleRadius = patchScale * s_cacheWrapper.GetPatchRadius(idx);
            Vector3 worldPos = s_debugState.m_worldVerPos[idx];
            Vector3 worldNormal = s_debugState.m_worldNormals[idx];
            Vector3 bentNormal = source.transform.TransformVector(s_cacheWrapper.GetBentNormal(idx)).normalized;

            Transform tr = source.transform;
            Transform lightTr = s_debugState.m_testLight.transform;

            Vector4 riat = new Vector4(s_debugState.m_testLight.range
                , s_debugState.m_testLight.intensity
                , s_debugState.m_testLight.spotAngle
                , (float)s_debugState.m_testLight.type);

            List<Vector3> startPoints = new List<Vector3>();
            List<Vector3> endPoints = new List<Vector3>();
            List<Vector3> colPoints = new List<Vector3>();
            VertexBakerLib.Instance.LightBlockersAtVertex(s_bvhHandle, idx, tr.position, worldNormal, lightTr.position, lightTr.forward, s_debugState.m_testLight.color.ToVector3(), riat, ref startPoints, ref endPoints, ref colPoints);

            PushGizmoColor(Color.cyan);
            for (int i = 0; i < startPoints.Count; ++i)
            {
                Gizmos.DrawLine(startPoints[i], endPoints[i]);
            }
            PopGizmoColor();

            PushGizmoColor(Color.red);
            for (int i = 0; i < colPoints.Count; ++i)
            {
                Gizmos.DrawLine(endPoints[i], colPoints[i]);
            }

            // estimate color
            Color sampleColor = s_debugState.m_testLight.color;
            sampleColor *= (startPoints.Count - colPoints.Count) / startPoints.Count;
            PushGizmoColor(sampleColor);
            Gizmos.DrawSphere(worldPos, sampleRadius * 0.1F);
            PopGizmoColor();

            PushGizmoColor(new Color(1f, 0f, 1f));
            Gizmos.DrawLine(worldPos, worldPos + bentNormal);
            PopGizmoColor();


            if (s_bvhWrapper != null)
            {
                s_bvhWrapper.GetSamplePatch(idx, ref s_samplePatch);
                PushGizmoColor(new Color(1f, 1f, 0f, 0.25f));
                Vector3 center = s_samplePatch.m_center;

                PopGizmoColor();
                PushGizmoColor(new Color(1f, 0f, 1f, 1f));
                Gizmos.DrawSphere(center, sampleRadius * 0.1F);
                PopGizmoColor();

                PushGizmoColor(Color.red);
                Gizmos.DrawLine(center, center + s_samplePatch.m_basis0);
                PopGizmoColor();

                PushGizmoColor(Color.blue);
                Gizmos.DrawLine(center, center + s_samplePatch.m_basis1);
                PopGizmoColor();
            }

        }

        public static void PushGizmoColor(Color color)
        {
            s_gizmoColors.Push(Gizmos.color);
            Gizmos.color = color;
        }

        public static void PopGizmoColor()
        {
            if (s_gizmoColors.Count > 0)
            {
                Gizmos.color = s_gizmoColors.Pop();
            }
        }

        private static float SphereSize(float sampleRadius)
        {
            return Mathf.Min(0.1f, sampleRadius * 0.25f);
        }

        //private static void BuildWorldVertices(DaydreamVertexLighting source)
        //{
        //    s_debugState.m_worldVerPos = source.m_sourceMesh.vertices;
        //    s_debugState.m_worldNormals = source.m_sourceMesh.normals;
        //    for (int i = 0; i < s_debugState.m_worldNormals.Length; ++i)
        //    {
        //        s_debugState.m_worldVerPos[i] = source.transform.TransformPoint(s_debugState.m_worldVerPos[i]);
        //        s_debugState.m_worldNormals[i] = source.transform.TransformVector(s_debugState.m_worldNormals[i]).normalized;
        //    }
        //}

#region Test Methods
        public void TestBuildBVHNative(DaydreamVertexLighting source)
        {
            DateTime start = DateTime.Now;
            MeshFilter meshFilter = source.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                VertexBakerLib.Instance.BuildBVH(new MeshFilter[] { meshFilter }); ;
            }

            VertexBakerLib.Log("Seconds to complete: " + (DateTime.Now - start).TotalSeconds);
        }

        public void TestLoadBVH(DaydreamVertexLighting source)
        {
            DateTime start = DateTime.Now;
            MeshFilter meshFilter = source.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                try
                {
                    VertexBakerLib.BVHHandle bvhHandle = null;
                    if (VertexBakerLib.Instance.LoadBVH(meshFilter, ref bvhHandle))
                    {
                        VertexBakerLib.Instance.FreeHandle(bvhHandle.Ptr());
                    }

                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message + "\n" + e.StackTrace);
                }
            }

            VertexBakerLib.Log("Seconds to complete: " + (DateTime.Now - start).TotalSeconds);

        }

        public void RefreshVisuals(DaydreamVertexLighting source)
        {
            s_bvhWrapper.UnLoad();
            s_cacheWrapper.UnLoad();

            MeshFilter meshFilter = source.GetComponent<MeshFilter>();
            string filepath = BVH.ConvertMeshIdToBVHPath(meshFilter.GetUniqueId());

            s_bvhWrapper.SetPath(filepath);
            s_bvhWrapper.Validate();

            s_cacheWrapper.SetPath(filepath);
            s_cacheWrapper.Validate();
        }
#endregion

    }
}