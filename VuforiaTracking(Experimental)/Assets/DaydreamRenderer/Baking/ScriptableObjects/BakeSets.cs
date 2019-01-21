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
using System.Collections.Generic;
namespace daydreamrenderer
{
    // contains baked results
    public class BakeSets : ScriptableObject
    {
        public List<MeshContainer> m_containers = new List<MeshContainer>();
        public string m_curBakeSetId;

        public void SetActiveBakeSet(string bakeSetId)
        {
            m_curBakeSetId = bakeSetId;
        }

        public MeshContainer GetActiveContainer()
        {
            return m_containers.Find(delegate (MeshContainer mc)
            {
                return mc != null && mc.m_bakeSetId == m_curBakeSetId;
            });
        }

    }
}