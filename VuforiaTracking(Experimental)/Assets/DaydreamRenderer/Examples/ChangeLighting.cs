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
using daydreamrenderer;
using System.Collections.Generic;

public class ChangeLighting : MonoBehaviour {

    public Material[] m_sceneMaterials = new Material[0];

    public string[] m_lightingIds = new string[] { "Lights On", "Lights Off", "Sunset" };
    public float m_lightScale = 1f;
    public float m_setIndex = 0;

    private int m_lastIndex = 0;

    [ContextMenu("Setup Materials")]
    void SetupMaterials()
    {
        List<Material> mats = new List<Material>();
        List<GameObject> roots = daydreamrenderer.Utilities.GetAllRoots();
        foreach(GameObject go in roots)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach(Renderer r in renderers)
            {
                foreach(Material m in r.sharedMaterials)
                {
                    if (m != null && m.shader.name.ToLower().Contains("daydream"))
                    {
                        mats.Add(m);
                    }
                }
            }
        }

        m_sceneMaterials = mats.ToArray();
    }

    void Start()
    {
        DaydreamVertexLighting.UpdateAllVertexLighting(m_lightingIds[(int)m_setIndex]);
    }

    void OnDisable()
    {
        // restore
        foreach (Material m in m_sceneMaterials)
        {
            m.SetFloat("_StaticLightingScale", 1f);
        }
    }

    void Update()
    {
        int setIndex = (int)m_setIndex;
        if (m_lastIndex != setIndex)
        {
            m_lastIndex = setIndex;
            DaydreamVertexLighting.UpdateAllVertexLighting(m_lightingIds[setIndex % m_lightingIds.Length]);
        }

        foreach (Material m in m_sceneMaterials)
        {
            m.SetFloat("_StaticLightingScale", m_lightScale);
        }

    }
}
