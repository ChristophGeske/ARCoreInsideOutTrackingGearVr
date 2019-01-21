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

[ExecuteInEditMode]
public class ShadowCaster : MonoBehaviour
{
    public int  m_shadowID = 0;

    private const int c_maximumShadowCastingSets = 4;

    private bool m_isShadowCaster  = false;
#if UNITY_EDITOR
    private int  m_onStartShadowID = 0;
#endif

    public static List<GameObject>[] s_shadowCastingObjects = null;
    public static List<int>[] s_shadowCastingObjLayers = null;
    public static bool s_clearShadowCasters = false;

    // Use this for initialization
    public void Start()
    {
        enableShadowCasting();
    }

    public void OnDestroy()
    {
        if (m_isShadowCaster)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                int count = renderers.Length;
                for (int i=0; i<count; i++)
                {
                    removeFromShadowList(renderers[i].gameObject, m_shadowID);
                }
            }
            m_isShadowCaster = false;
        }
    }

    public void enableShadowCasting()
    {
        if (s_shadowCastingObjects == null)
        {
            s_shadowCastingObjects   = new List<GameObject>[c_maximumShadowCastingSets];
            s_shadowCastingObjLayers = new List<int>[c_maximumShadowCastingSets];
            for (int i = 0; i < c_maximumShadowCastingSets; i++)
            {
                s_shadowCastingObjects[i]   = new List<GameObject>();
                s_shadowCastingObjLayers[i] = new List<int>();
            }
        }

        bool prevCaster = m_isShadowCaster;
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            m_isShadowCaster = (renderers[0].shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
        }
        else
        {
            m_isShadowCaster = false;
        }

        if (m_isShadowCaster && m_isShadowCaster != prevCaster)
        {
            int count = renderers.Length;
            for (int i = 0; i < count; i++)
            {
                addToShadowList(renderers[i].gameObject, m_shadowID);
            }
        }

#if UNITY_EDITOR
        //handle the case where the id is changed in the editor.
        m_onStartShadowID = m_shadowID;
#endif
    }

    public void disableShadowCasting()
    {
        if (m_isShadowCaster)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            int count = renderers.Length;
            for (int i = 0; i < count; i++)
            {
                removeFromShadowList(renderers[i].gameObject, m_shadowID);
            }
            s_clearShadowCasters = true;
        }
        m_isShadowCaster = false;
    }

    public void OnEnable()
    {
        enableShadowCasting();
    }

    public void OnDisable()
    {
        disableShadowCasting();
    }

    // Update is called once per frame
    public void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers != null && renderers.Length > 0)
            {
                int count = renderers.Length;
                //handle the case when the shadow ID is changed in the UI.
                if (m_shadowID != m_onStartShadowID)
                {
                    for (int i = 0; i < count; i++)
                    {
                        removeFromShadowList(renderers[i].gameObject, m_onStartShadowID);
                    }
                    m_isShadowCaster = false;
                }

                //handle the case where the shadow casting type is changed in the UI.
                bool castShadows = (renderers[0].shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.On);
                for (int i = 0; i < count; i++)
                {
                    if (castShadows != m_isShadowCaster && castShadows)
                    {
                        addToShadowList(renderers[i].gameObject, m_shadowID);
                    }
                    else if (castShadows != m_isShadowCaster && !castShadows)
                    {
                        removeFromShadowList(renderers[i].gameObject, m_shadowID);
                        s_clearShadowCasters = true;
                    }
                }
                m_isShadowCaster = castShadows;
            }
        }
#endif
    }

    private static void addToShadowList(GameObject obj, int shadowID)
    {
        if (shadowID < 0 || shadowID >= c_maximumShadowCastingSets) { shadowID = c_maximumShadowCastingSets-1; }
        //for now assume that the object is not added multiple times...
        s_shadowCastingObjects[shadowID].Add(obj);
        s_shadowCastingObjLayers[shadowID].Add(obj.layer);
    }

    static GameObject s_findObject;

    static private bool isObject(GameObject obj)
    {
        return (obj == s_findObject);
    }

    private static void removeFromShadowList(GameObject obj, int shadowID)
    {
        if (s_shadowCastingObjects == null || s_shadowCastingObjects[shadowID] == null)
        {
            return;
        }

        if (shadowID < 0 || shadowID >= c_maximumShadowCastingSets) { shadowID = c_maximumShadowCastingSets - 1; }

        //remove the first occurance of the object (assumed to be entered only once).
        s_findObject = obj;
        int index = s_shadowCastingObjects[shadowID].FindIndex(isObject);
        if (index > -1 && index < s_shadowCastingObjects[shadowID].Count)
        {
            obj.layer = s_shadowCastingObjLayers[shadowID][index];

            s_shadowCastingObjects[shadowID].RemoveAt(index);
            s_shadowCastingObjLayers[shadowID].RemoveAt(index);
        }
    }
}
