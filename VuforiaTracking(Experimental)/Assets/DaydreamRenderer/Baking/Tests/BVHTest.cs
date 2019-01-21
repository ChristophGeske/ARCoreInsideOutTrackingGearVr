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
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR

namespace daydreamrenderer
{
    [ExecuteInEditMode]
    public class BVHTest : MonoBehaviour
    {
        public MeshFilter mesh;
        public GameObject m_s0;
        public GameObject m_s1;

        public List<Vector3> m_centers = new List<Vector3>();
        public List<Vector3> m_sizes = new List<Vector3>();


        [ContextMenu("Start")]
        public void Start()
        {
            if (m_s0 == null)
            {
                m_s0 = GameObject.Find("colAABBSegStart");
                if (m_s0 == null)
                {
                    m_s0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_s0.name = "colAABBSegStart";
                    m_s0.transform.parent = gameObject.transform;
                    BakeFilter bf = m_s0.AddComponent<BakeFilter>();
                    bf.m_bakeFilter = BakeFilter.Filter.ExcludeFromBake;
                }
            }

            if (m_s1 == null)
            {
                m_s1 = GameObject.Find("colAABBSegEnd");
                if (m_s1 == null)
                {
                    m_s1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_s1.name = "colAABBSegEnd";
                    m_s1.transform.parent = gameObject.transform;
                    BakeFilter bf = m_s1.AddComponent<BakeFilter>();
                    bf.m_bakeFilter = BakeFilter.Filter.ExcludeFromBake;
                }
            }

            //BuildBVH();
        }

        public void OnDestroy()
        {
            if (m_s0 != null)
            {
                DestroyImmediate(m_s0);
            }
            if (m_s1 != null)
            {
                DestroyImmediate(m_s1);
            }
        }
    }
}
#endif