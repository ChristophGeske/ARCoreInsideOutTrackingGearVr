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
namespace daydreamrenderer
{
    public class VertexDebugState : ScriptableObject
    {
        // state for debug output
        public bool m_debugEnabled = false;
        public bool m_showLightBlockerSamples = false;
        public Light m_testLight;
        public bool m_showBentNormalSamples = false;
        public bool m_showAdjacencies = false;
        public int m_vertexSampleIndex = 0;
        public bool m_showAOSamples = false;
        public float m_accessability = 1f;
        public bool m_showSamplePatch = false;
        public bool m_showNormals = false;
        public bool m_showBVH = false;
        public bool m_showTessTriangles = false;
        public int m_indexOffset = 0;
        public bool m_showFace = false;
        [System.NonSerialized]
        public int m_lastVertexSampleIndex = -1;
        [System.NonSerialized]
        public int[] m_triangles;
        [System.NonSerialized]
        public Vector3[] m_worldVerPos;
        [System.NonSerialized]
        public Vector3[] m_worldNormals;
        [System.NonSerialized]
        public int[] m_tessFaces;
    }
}
