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
#if UNITY_EDITOR

namespace daydreamrenderer
{
    [ExecuteInEditMode]
    public class TriangleTest : MonoBehaviour
    {
        void Start()
        {
            if (m_p0 == null)
            {
                m_p0 = GameObject.Find("colTestSphere0");
                if (m_p0 == null)
                {
                    m_p0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_p0.name = "colTestSphere0";
                    m_p0.transform.parent = gameObject.transform;
                }
            }

            if (m_p1 == null)
            {
                m_p1 = GameObject.Find("colTestSphere1");
                if (m_p1 == null)
                {
                    m_p1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_p1.name = "colTestSphere1";
                    m_p1.transform.parent = gameObject.transform;
                }
            }

            if (m_p2 == null)
            {
                m_p2 = GameObject.Find("colTestSphere2");
                if (m_p2 == null)
                {
                    m_p2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_p2.name = "colTestSphere2";
                    m_p2.transform.parent = gameObject.transform;
                }
            }

            if (m_s0 == null)
            {
                m_s0 = GameObject.Find("colSegStart");
                if (m_s0 == null)
                {
                    m_s0 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_s0.name = "colSegStart";
                    m_s0.transform.parent = gameObject.transform;
                }
            }

            if (m_s1 == null)
            {
                m_s1 = GameObject.Find("colSegEnd");
                if (m_s1 == null)
                {
                    m_s1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_s1.name = "colSegEnd";
                    m_s1.transform.parent = gameObject.transform;
                }
            }

            if (m_colPoint == null)
            {
                m_colPoint = GameObject.Find("colPoint");
                if (m_colPoint == null)
                {
                    m_colPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    m_colPoint.name = "colPoint";
                    m_colPoint.transform.parent = null;
                }
            }
        }

        public bool m_isColliding = false;
        public GameObject m_colPoint;

        public GameObject m_s0;
        public GameObject m_s1;

        public GameObject m_p0;
        public GameObject m_p1;
        public GameObject m_p2;
    }
}
#endif