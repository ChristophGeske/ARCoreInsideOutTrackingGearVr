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

public enum ShadowType
{
    SHD_TYPE_PERSPECTIVE = 0,
    SHD_TYPE_DIRECTIONAL,
    SHD_TYPE_COUNT
};

public class ShadowSourceObject
{
    public GameObject m_obj;
    public ShadowType m_type;
    public float      m_sharpness;
};

[ExecuteInEditMode]
public class ShadowSource : MonoBehaviour
{
    public float m_sharpness = 0.3f;
    public static List<ShadowSourceObject> s_shadowSources;

    private bool m_isShadowSource = false;
    private ShadowType m_type;
#if UNITY_EDITOR
    private float m_startSharpness;
#endif
        
    // Use this for initialization
    public void Start()
    {
        if (s_shadowSources == null)
        {
            s_shadowSources = new List<ShadowSourceObject>();
        }

        Light light = gameObject.GetComponent<Light>();
        if (light != null)
        {
            m_isShadowSource = (light.shadows != LightShadows.None);
            if (light.type == LightType.Point || light.type == LightType.Area)
            {
                m_type = ShadowType.SHD_TYPE_PERSPECTIVE;
            }
            else
            {
                m_type = ShadowType.SHD_TYPE_DIRECTIONAL;
            }
        }
        else
        {
            m_type = ShadowType.SHD_TYPE_PERSPECTIVE;
        }

        if (m_isShadowSource)
        {
            addToShadowSourceList(gameObject, m_type, m_sharpness);
        }

#if UNITY_EDITOR
        m_startSharpness = m_sharpness;
#endif
    }

    public void OnDestroy()
    {
        if (m_isShadowSource)
        {
            removeFromSourceList(gameObject);
            m_isShadowSource = false;
        }
    }

    // Update is called once per frame
    public void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Light light = gameObject.GetComponent<Light>();
            if (light != null)
            {
                //handle the case where the shadow casting type is changed in the UI.
                bool shadowSource = (light.shadows != LightShadows.None);
                if (shadowSource != m_isShadowSource && shadowSource)
                {
                    addToShadowSourceList(gameObject, m_type, m_sharpness);
                }
                else if (shadowSource != m_isShadowSource && !shadowSource)
                {
                    removeFromSourceList(gameObject);
                }
                else
                {
                    ShadowType type;
                    if (light.type == LightType.Point || light.type == LightType.Area)
                    {
                        type = ShadowType.SHD_TYPE_PERSPECTIVE;
                    }
                    else
                    {
                        type = ShadowType.SHD_TYPE_DIRECTIONAL;
                    }

                    if (type != m_type || m_startSharpness != m_sharpness)
                    {
                        changeSourceSettings(gameObject, type, m_sharpness);
                    }
                }
                m_isShadowSource = shadowSource;
            }
        }
#endif
    }

    private static int findShadowSource(GameObject obj)
    {
        if (s_shadowSources == null)
        {
            return -1;
        }

        int index = 0;
        foreach (ShadowSourceObject shd in s_shadowSources)
        {
            if (shd.m_obj == obj)
            {
                return index;
            }
            index++;
        }

        return -1;
    }

    private static void addToShadowSourceList(GameObject obj, ShadowType type, float sharpness)
    {
        ShadowSourceObject shadowSrcObj = new ShadowSourceObject();
        shadowSrcObj.m_obj       = obj;
        shadowSrcObj.m_type      = type;
        shadowSrcObj.m_sharpness = sharpness;

        //for now assume that the object is not added multiple times...
        s_shadowSources.Add(shadowSrcObj);
    }

    private static void changeSourceSettings(GameObject obj, ShadowType type, float sharpness)
    {
        int index = findShadowSource(obj);
        if (index >= 0)
        {
            s_shadowSources[index].m_type      = type;
            s_shadowSources[index].m_sharpness = sharpness;
        }
    }

    private static void removeFromSourceList(GameObject obj)
    {
        if (s_shadowSources == null) { return; }

        int index = findShadowSource(obj);
        if (index >= 0)
        {
            s_shadowSources.RemoveAt(index);
        }
    }
}
