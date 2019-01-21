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
public class ShadowReceiver : MonoBehaviour
{
    private bool m_isShadowReceiver = false;
    public static List<GameObject> s_shadowReceivingObjects = null;

    // Use this for initialization
    public void Start()
    {
        if (s_shadowReceivingObjects == null)
        {
            s_shadowReceivingObjects = new List<GameObject>();
        }

        MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
        if (meshRenderers != null && meshRenderers.Length > 0)
        {
            m_isShadowReceiver = meshRenderers[0].receiveShadows;
        }

        if (m_isShadowReceiver)
        {
            int count = meshRenderers.Length;
            for (int i = 0; i < count; i++)
            {
                addToShadowRecieverList(meshRenderers[i].gameObject);
            }
        }
    }

    public void OnDestroy()
    {
        if (m_isShadowReceiver)
        {
            removeFromShadowRecieverList(gameObject);
            m_isShadowReceiver = false;
        }
    }

    // Update is called once per frame
    public void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            MeshRenderer[] meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                int count = meshRenderers.Length;

                //handle the case where the shadow casting type is changed in the UI.
                bool receiveShadows = (meshRenderers[0].receiveShadows);
                if (receiveShadows != m_isShadowReceiver && receiveShadows)
                {
                    for (int i = 0; i < count; i++)
                    {
                        addToShadowRecieverList(meshRenderers[i].gameObject);
                    }
                }
                else if (receiveShadows != m_isShadowReceiver && !receiveShadows)
                {
                    for (int i = 0; i < count; i++)
                    {
                        removeFromShadowRecieverList(meshRenderers[i].gameObject);
                    }
                }
                m_isShadowReceiver = receiveShadows;
            }
        }
#endif
    }

    private static void addToShadowRecieverList(GameObject obj)
    {
        //for now assume that the object is not added multiple times...
        s_shadowReceivingObjects.Add(obj);
    }

    private static void removeFromShadowRecieverList(GameObject obj)
    {
        if (s_shadowReceivingObjects == null) { return; }

        //remove the first occurance of the object (assumed to be entered only once).
        s_shadowReceivingObjects.Remove(obj);
    }
}
