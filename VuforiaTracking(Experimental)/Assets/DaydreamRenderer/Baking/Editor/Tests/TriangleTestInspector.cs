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

namespace daydreamrenderer
{
    [CustomEditor(typeof(TriangleTest), true)]
    public class TriangleTestInspector : Editor
    {

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        static void DrawGizmos(TriangleTest source, GizmoType gizmoType)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(source.m_p0.transform.position, source.m_p1.transform.position);
            Gizmos.DrawLine(source.m_p1.transform.position, source.m_p2.transform.position);
            Gizmos.DrawLine(source.m_p2.transform.position, source.m_p0.transform.position);

            float colX = 0f, colY = 0f, colZ = 0f;
            if (VertexBakerLib.Instance.Triangle2LineSegment(
               source.m_p0.transform.position
                , source.m_p2.transform.position
                , source.m_p1.transform.position
                , source.m_s0.transform.position
                , source.m_s1.transform.position
                , true
                , ref colX
                , ref colY
                , ref colZ))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(source.m_s0.transform.position, source.m_s1.transform.position);
            }
            else
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(source.m_s0.transform.position, source.m_s1.transform.position);
            }

            Vector3 colPoint = new Vector3(colX, colY, colZ);
            source.m_colPoint.transform.position = colPoint;

            //if (m_isColliding)
            //{
            //    Gizmos.color = Color.red;
            //    Gizmos.DrawLine(m_s0.transform.position, m_s1.transform.position);
            //}
            //else
            //{
            //    Gizmos.color = Color.cyan;
            //    Gizmos.DrawLine(m_s0.transform.position, m_s1.transform.position);
            //}
        }
    }
}
