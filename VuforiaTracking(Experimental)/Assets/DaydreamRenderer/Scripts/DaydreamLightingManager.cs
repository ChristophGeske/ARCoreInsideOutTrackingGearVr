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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace daydreamrenderer
{

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class DaydreamLightingManager
    {
        const int kMinObjectCount = 512;
        const int kMinCameraCount = 8;
        const int kMinLightCount = 16;
        public static List<DaydreamMeshRenderer> s_objectList = new List<DaydreamMeshRenderer>(kMinObjectCount);
        public static bool m_frameStarted = true;

        private int m_lightCount = 0;
        private int m_startupCount = 0;
        private bool m_callbacksEnabled = false;

#if UNITY_EDITOR
        public static List<DaydreamLight> s_inEditorUpdateList = new List<DaydreamLight>(kMinLightCount);
        static DaydreamLightingManager _instance = new DaydreamLightingManager();
#else
        static DaydreamLightingManager _instance = null;
#endif

        public static void Init()
        {
            if(_instance == null)
            {
                _instance = new DaydreamLightingManager();
            }

            if(!_instance.m_callbacksEnabled)
            {
                _instance.SetupCallbacks();
            }
        }

        static DaydreamLightingManager()
        {
            Init();
            DaydreamMeshRenderer.s_startup = true;
        }

        public void SetupCallbacks()
        {
            Camera.onPreRender -= ProcessLighting;
            Camera.onPreCull -= OnPreCull;
            Camera.onPreRender += ProcessLighting;
            Camera.onPreCull += OnPreCull;
            Camera.onPostRender += OnPostRender;

            m_callbacksEnabled = true;
        }

        public void RemoveCallbacks()
        {
            Camera.onPreRender -= ProcessLighting;
            Camera.onPreCull -= OnPreCull;
            Camera.onPostRender -= OnPostRender;

            m_callbacksEnabled = false;
        }

        public void OnPreCull(Camera camera)
        {
            DaydreamLightingManager.s_objectList.Clear();
            DaydreamMeshRenderer.Clear();
        }

        public void OnPostRender(Camera camera)
        {
            if(m_startupCount++ >= Mathf.Max(3, Camera.allCamerasCount))
            {
                m_startupCount = 0;
                DaydreamMeshRenderer.s_startup = false;
                Camera.onPostRender -= OnPostRender;
            }
        }

        public void ProcessLighting(Camera camera)
        {

            if (DaydreamLight.s_resortLights)
            {
                DaydreamLight.s_resortLights = false;
                DaydreamLight.ResortLights();
            }

            #if UNITY_EDITOR
            for(int i = 0, k = s_inEditorUpdateList.Count; i < k; ++i){
                if (i >= s_inEditorUpdateList.Count) break;
                s_inEditorUpdateList[i].InEditorUpdate();
            }
            #endif

            bool changed = true;
            if (DaydreamLight.s_masterLightArray != null && DaydreamLight.s_masterLightArray.Length > 0)
            {
                // update lights
                DaydreamLight.StartFrame(camera);

                DaydreamLight lightData = null;

                for (int i = 0, k = DaydreamLight.s_masterLightArray.Length; i < k; ++i)
                {
                    if (i >= DaydreamLight.s_masterLightArray.Length) break;

                    lightData = DaydreamLight.s_masterLightArray[i];

                    // these should update once per frame regardless of camera count
                    if(m_frameStarted)
                    {
                        #if UNITY_EDITOR
                        lightData.InEditorUpdate();
                        #endif
                        lightData.CheckForChange();
                        lightData.UpdateFrame();
                    }

                    // this updates each frame for each camera
                    lightData.UpdateViewSpace();
                }

                // first time through this should get set back to false so other cameras
                // don't update light values that only need to be udpate once per frame
                m_frameStarted = false;

                changed = DaydreamLight.AnyLightChanged() || m_lightCount != DaydreamLight.GetLightCount();
                m_lightCount = DaydreamLight.GetLightCount();
            }

            if(s_objectList != null)
            {
                DaydreamMeshRenderer.StartFrame();

                // process objects
                for (int i = 0, k = s_objectList.Count; i < k; ++i)
                {
                    DaydreamMeshRenderer dmr = s_objectList[i];

                    if (dmr != null && !dmr.m_didInit)
                    {
                        dmr.DMRInit();
                    }
#if UNITY_EDITOR
                    if (dmr != null&& !dmr.InEditorUpdate())
                    {
                        continue;
                    }
#endif
                    if(dmr != null)
                    {
						dmr.UpdateStaticState();
						dmr.ApplyLighting(changed);  
                    }

                }
            }

        }

    }
}
