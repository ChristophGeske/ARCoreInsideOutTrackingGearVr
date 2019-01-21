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
using System;
using System.IO;
using daydreamrenderer;
using FlatBuffers;
using System.Collections.Generic;

namespace daydreamrenderer
{
    public static class BVH
    {
        public static String ConvertMeshIdToBVHPath(int meshId)
        {
            return FBSConstants.BasePath + "/BVHCache/" + meshId + ".bvh";
        }
    }

    public struct SamplePatch
    {
        public enum Version {
            kSamplePatchV0 = 0,
            kSamplePatchV1, // added faces to sample patch
            kSamplePatchVCurrent = kSamplePatchV1
        };
        
        public Vector3 m_center;
        public Vector3 m_size;
        public Vector3 m_basis0;
        public Vector3 m_basis1;
        public List<int> m_faces;
    }

    [System.Serializable]
    public class BVHNode_FBWrapper : FBSWrapper<fbs_BVH>
    {
        public BVHNode_FBWrapper()
        {
        }

        public BVHNode_FBWrapper(int instanceId)
        {
            string filepath = BVH.ConvertMeshIdToBVHPath(instanceId);
            SetPath(filepath);
        }

        public List<Bounds> GetBounds()
        {
            Validate();
            return m_boundsCache;
        }

        public void GetSamplePatch(int vertIndex, ref SamplePatch sp)
        {
            fbs_SamplePatch patch = m_fbsObj.GetSamplePatch(vertIndex);

            AssignVector3(patch.Center, ref sp.m_center);
            AssignVector3(patch.Size, ref sp.m_size);
            AssignVector3(patch.Basis0, ref sp.m_basis0);
            AssignVector3(patch.Basis1, ref sp.m_basis1);

            if(patch.Version >= (int)SamplePatch.Version.kSamplePatchV1)
            {
                sp.m_faces = new List<int>(patch.FacesLength);
                for (int i = 0; i < patch.FacesLength; ++i)
                {
                    sp.m_faces.Add(System.Convert.ToInt32(patch.GetFaces(i)));
                }
            }else
            {
                sp.m_faces = new List<int>();
            }
        }

        public Vector3[] GetPatchCorners(int vertIndex)
        {
            Validate();
            if (m_fbsObj.SamplePatchLength > vertIndex)
            {
                fbs_SamplePatch patch = m_fbsObj.GetSamplePatch(vertIndex);

                AssignVector3(patch.Center, ref m_center);
                AssignVector3(patch.Size, ref m_size);
                AssignVector3(patch.Basis0, ref m_basis0);
                AssignVector3(patch.Basis1, ref m_basis1);

                float xSize = Mathf.Abs(Vector3.Dot(m_basis0, m_size)) * 0.5f;
                float ySize = Mathf.Abs(Vector3.Dot(m_basis1, m_size)) * 0.5f;

                return new Vector3[] {
                  (m_center - m_basis0*xSize + m_basis1*ySize), // TL
                  (m_center + m_basis0*xSize + m_basis1*ySize), // TR
                  
                  (m_center + m_basis0*xSize - m_basis1*ySize), // BR
                  (m_center - m_basis0*xSize - m_basis1*ySize), // BL
            };

            }

            return null;
        }

        private void ClearCache()
        {
            if (m_boundsCache != null)
            {
                m_boundsCache.Clear();
            }
        }

        protected override bool OnValidate()
        {
            if (m_boundsCache != null && m_fbsObj != null && m_fbsObj.ByteBuffer.Length > 0 && m_fbsObj.MasterListLength == m_boundsCache.Count)
            {
                return true;
            }

            return false;
        }

        protected override void OnUnload()
        {
            ClearCache();
        }

        protected override void OnRebuildData()
        {
            fbs_Bounds fbsBounds = new fbs_Bounds();
            for (int i = 0, k = m_fbsObj.MasterListLength; i < k; ++i)
            {
                fbs_BVHNode node = m_fbsObj.GetMasterList(i);

                node.GetBounds(fbsBounds);
                Bounds bounds = new Bounds();

                fbs_vec3 center = fbsBounds.Center;
                fbs_vec3 size = fbsBounds.Size;
                bounds.center = new Vector3(center.X, center.Y, center.Z);
                bounds.size = new Vector3(size.X, size.Y, size.Z);
                m_boundsCache.Add(bounds);
            }
        }

        protected override fbs_BVH CreateObject(ByteBuffer bb)
        {
            return fbs_BVH.GetRootAsfbs_BVH(bb);
        }

        private void AssignVector3(fbs_vec3 v, ref Vector3 outVec)
        {
            outVec.x = v.X;
            outVec.y = v.Y;
            outVec.z = v.Z;
        }

        private List<Bounds> m_boundsCache = new List<Bounds>();
        // cache patch data
        Vector3 m_center = Vector3.zero;
        Vector3 m_size = Vector3.zero;
        Vector3 m_basis0 = Vector3.zero;
        Vector3 m_basis1 = Vector3.zero;

    }
}