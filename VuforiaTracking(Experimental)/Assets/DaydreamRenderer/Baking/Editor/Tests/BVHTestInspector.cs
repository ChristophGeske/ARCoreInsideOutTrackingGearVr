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
using System;
using System.Collections.Generic;

namespace daydreamrenderer
{
    [CustomEditor(typeof(BVHTest), true)]
    public class BVHTestInspector : Editor
    {
        static VertexBakerLib.Handle m_bvhSceneHandle = null;

        public override void OnInspectorGUI()
        {
            BVHTest source = target as BVHTest;

            if (GUILayout.Button("Build Scene BVH"))
            {
                if (m_bvhSceneHandle != null)
                {
                    m_bvhSceneHandle.FreeHandle();
                }

                MeshFilter[] meshes = source.GetComponentsInChildren<MeshFilter>();
                VertexBakerLib.Handle handle = null;
                VertexBakerLib.Instance.LoadBVHScene(new List<MeshFilter>(meshes), out handle);

                m_bvhSceneHandle = handle;
            }
            base.OnInspectorGUI();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmos(BVHTest source, GizmoType gizmoType)
        {
            Debug.DrawLine(source.m_s0.transform.position, source.m_s1.transform.position, Color.cyan);

            // if not loaded yet or invalid load the bvh scene
            //if (m_bvhSceneHandle == null || !m_bvhSceneHandle.IsValid())
            //{
            //    MeshFilter[] meshes = GetComponentsInChildren<MeshFilter>();
            //    m_bvhSceneHandle = null;
            //    VertexBakerLib.Instance.LoadBVHScene(new List<MeshFilter>(meshes), out m_bvhSceneHandle);
            //}

            if (m_bvhSceneHandle != null && m_bvhSceneHandle.IsValid())
            {
                source.m_centers.Clear();
                source.m_sizes.Clear();
                VertexBakerLib.Instance.BVHToLineSegment(m_bvhSceneHandle, source.m_s0.transform.position, source.m_s1.transform.position, source.m_centers, source.m_sizes);
            }

            if (source.m_centers.Count > 0 && source.m_centers.Count == source.m_sizes.Count)
            {

                Color save = Gizmos.color;
                Gizmos.color = Color.red;
                for (int i = 0, k = source.m_centers.Count; i < k; ++i)
                {
                    Gizmos.DrawWireCube(source.m_centers[i], source.m_sizes[i]);
                }
                Gizmos.color = save;
            }
        }

    }
}