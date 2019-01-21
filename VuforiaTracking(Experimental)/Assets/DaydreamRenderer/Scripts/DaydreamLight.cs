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
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace daydreamrenderer
{
    using LightModes = TypeExtensions.LightModes;

    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class DaydreamLight : MonoBehaviour
    {
        // sort the direction lights to the top of the list
        public class LightSort : IComparer<DaydreamLight>
        {
            public int Compare(DaydreamLight x, DaydreamLight y)
            {
                if (x.m_light.type == LightType.Directional && y.m_light.type != LightType.Directional)
                {
                    return -1;
                }
                else if (x.m_light.type != LightType.Directional && y.m_light.type == LightType.Directional)
                {
                    return 1;
                }

                return 0;
            }
        }

        public static bool s_resortLights = false;

        // Daydream light manages a list lights in the scene
        public static DaydreamLight[] s_masterLightArray = new DaydreamLight[0];

        [HideInInspector]
        // this field serializes the last light mode
        public int m_lightMode = LightModes.REALTIME;

        // dist from light to object
        [System.NonSerialized]
        public float m_dist = 0f;
        // light data wraps a light
        [System.NonSerialized]
        public Light m_light;
        // cache the world pos for fast access
        [System.NonSerialized]
        public Vector4 m_worlPos;
        // cache the world dir for fast access
        [System.NonSerialized]
        public Vector4 m_worldDir;
        // flagged if any tracked property change (eg range, angle, intensity)
        [System.NonSerialized]
        public bool m_propertiesChanged = true;
        // flagged if light transform has change (ie the light moved)
        [System.NonSerialized]
        public bool m_transformChanged = true;
        // cache the type
        [System.NonSerialized]
        public LightType m_type;
        // track frame updates
        [System.NonSerialized]
        public static int s_viewSpaceId;
        // cache the view matrix
        [System.NonSerialized]
        static Matrix4x4 s_toViewSpace;
        // light mode (real time, mixed, etc)
        [System.NonSerialized]
        public int m_curMode = LightModes.REALTIME;
        // cache the light culling layer
        [System.NonSerialized]
        public int m_cullingMask = 0;
        // track the last range
        [System.NonSerialized]
        public float m_lastRange = 0f;
        // track last intensity
        [System.NonSerialized]
        public float m_lastIntensity = 0f;

        // internal list of lights to accomodate sorting
        static List<DaydreamLight> s_masterLightList = new List<DaydreamLight>();

        // always update on init
        bool m_didInit = false;

        // calculated values that are passed to a shader
        float m_attenW;
        float m_attenX;
        float m_attenY;
        float m_attenZ;
        Vector4 m_viewSpacePos;
        Vector4 m_viewSpaceDir;
        Vector4 m_color;

        // these fields track changes to minimize recalculations as much as possible
        float m_lastAngle = float.MaxValue;
        Color m_lastColor = Color.clear;

        // ensure properties are updated
        bool m_invalidatedProps = true;

        public static void StartFrame(Camera camera)
        {
            s_toViewSpace = camera.worldToCameraMatrix;
            // update frame
            s_viewSpaceId = ++s_viewSpaceId % int.MaxValue;
        }
        
        // Called when property has changed
        public void UpdateFrame()
        {
            float spotAngle = m_light.spotAngle;
            float range = m_light.range;
            if (m_invalidatedProps || m_lastAngle != spotAngle)
            {
                m_attenX = Mathf.Cos(spotAngle * Mathf.Deg2Rad * 0.5f);
                m_attenY = 1f / Mathf.Cos(spotAngle * Mathf.Deg2Rad * 0.25f);
            }

            if (m_invalidatedProps || m_lastRange != range)
            {
                m_attenZ = ComputeLightAttenuation(m_light);
                m_attenW = range * range;
            }

            m_cullingMask = m_light.cullingMask;

            // updates to track changes
            m_type = m_light.type;
            Color color = m_color = m_light.color;
            float intensity = m_light.intensity;
            if (m_type == LightType.Spot)
            {
                m_color = color * intensity * intensity;
            }
            else
            {
                m_color = color * intensity;
            }
            m_lastColor = color;
            m_lastRange = range;
            m_lastAngle = spotAngle;
            m_lastIntensity = intensity;

            m_invalidatedProps = false;
        }

        // called when light transform has changed
        public void UpdateViewSpace()
        {
            Transform t = m_light.transform;
            m_worlPos = t.position;
            m_worlPos.w = 1f;
            m_worldDir = -t.forward;
            m_worldDir.w = 0f;

            // create view space position
            m_viewSpacePos = s_toViewSpace*m_worlPos;
            m_viewSpacePos.w = 1f;

            if(m_type != LightType.Point)
            {
                m_viewSpaceDir = s_toViewSpace*m_worldDir;
                m_viewSpaceDir.w = 0f;
            }
        }
            
        public float GetAttenX()
        {
            return m_attenX;
        }

        public float GetAttenY()
        {
            return m_attenY;
        }

        public float GetAttenZ()
        {
            return m_attenZ;
        }

        public float GetAttenW()
        {
            return m_attenW;
        }

        public void GetViewSpacePos(ref Vector4 viewSpacePos)
        {
            viewSpacePos = m_viewSpacePos;
        }

        public void GetViewSpaceDir(ref Vector4 viewSpaceDir)
        {
            viewSpaceDir = m_viewSpaceDir;
        }

        public void GetColor(ref Vector4 color)
        {
            color = m_color;
        }

        // test for changes and set flags
        public bool CheckForChange()
        {
            Color color = m_light.color;
            if ( !m_didInit 
                || m_lastAngle != m_light.spotAngle
                || color.r != m_lastColor.r
                || color.g != m_lastColor.g
                || color.b != m_lastColor.b
                || color.a != m_lastColor.a
                || m_lastIntensity != m_light.intensity
                || m_lastRange != m_light.range)
            {
                m_didInit = true;
                m_propertiesChanged = true;
            }
            else
            {
                m_propertiesChanged = false;
            }

            if (m_light.type != m_type)
            {
                m_propertiesChanged = true;

                // make sure calculation that depend on properties get updated
                m_invalidatedProps = true;

                // sort may be different now
                DaydreamLight.s_resortLights = true;
            }

            // check transform for changes
            if (m_light.transform.hasChanged)
            {
                m_light.transform.hasChanged = false;
                m_transformChanged = true;
            }
            else
            {
                m_transformChanged = false;
            }

            // return true if anything changed
            return m_transformChanged || m_propertiesChanged;
        }

        public void InEditorUpdate()
        {
#if UNITY_EDITOR
            // Monitor for editor property change 'Light Mode'
            if (!Application.isPlaying)
            {
                if (Selection.activeGameObject == gameObject)
                {
                    Light l = gameObject.GetComponent<Light>();
                    int newMode = l.LightMode();
                    if (newMode != m_lightMode)
                    {
                        // update the light mode

                        if (m_lightMode == LightModes.BAKED)
                        {
                            // if it was baked and is no longer add it to master list
                            RemoveFromEditorUpdateList();
                            AddToMasterList();
                        }
                        else if (newMode == LightModes.BAKED)
                        {
                            // if it is now a baked a light remove it from the master list
                            RemoveFromMasterList();
                            AddToEditorUpdateList();
                        }

                        m_curMode = newMode;
                        m_lightMode = newMode;
                    }
                }
            }
#endif
        }

        public static int GetLightCount()
        {
            return s_masterLightArray.Length;
        }
        
        public static void ResortLights()
        {
            s_masterLightList.Sort(new LightSort());
            s_masterLightArray = s_masterLightList.ToArray();
        }

        public static void ClearList(ref int[] list)
        {
            for(int i = 0, k = list.Length; i < k; ++i)
            {
                list[i] = -1;
            }
        }

        public static bool AnyLightChanged()
        {
            for (int i = 0, k = s_masterLightArray.Length; i < k; ++i)
            {
                if (s_masterLightArray[i].m_propertiesChanged || s_masterLightList[i].m_transformChanged || !s_masterLightArray[i].m_didInit)
                {
                    return true;
                }
            }

            return false;
        }

        public static void GetSortedLights(int updateKey, int layer, bool isStatic, Vector3 objPosition, Bounds bounds, ref int[] outLightList, ref int usedSlots)
        {

            ClearList(ref outLightList);

            float dist2 = 0f;
            DaydreamLight dl = null;
            usedSlots = 0;

            // number of directional lights
            int dirCount = 0;

            // insert sort lights - Note: directional lights are always sorted to the top of the master list
            // this allows for some assumptions
            for (int i = 0, k = s_masterLightArray.Length; i < k; ++i)
            {
                // i'th light data
                dl = s_masterLightArray[i];

                int ithLight = i;

                if((dl.m_cullingMask & (1 << layer)) == 0 || (isStatic && dl.m_curMode != LightModes.REALTIME))
                {
                    continue;
                }

                if (dl.m_type != LightType.Directional)
                {
                    // from cam to light
                    objPosition = bounds.center;

                    // only update if needed
                    float range2 = dl.m_lastRange * dl.m_lastRange;
                    float x = (dl.m_worlPos.x - objPosition.x)*(dl.m_worlPos.x - objPosition.x);
                    float y = (dl.m_worlPos.y - objPosition.y)*(dl.m_worlPos.y - objPosition.y);
                    float z = (dl.m_worlPos.z - objPosition.z)*(dl.m_worlPos.z - objPosition.z);
                    dl.m_dist = (x + y + z) - range2;

                    dist2 = dl.m_dist;

                    for (int j = dirCount; j < outLightList.Length; ++j)
                    {
                        if(outLightList[j] == -1)
                        {
                            // insert the i'th light
                            outLightList[j] = ithLight;
                            usedSlots++;
                            break;
                        }
                        // if the i'th light is closer than the j'th light
                        else if (dist2 < s_masterLightArray[outLightList[j]].m_dist)
                        {
                            // butterfly swap
                            ithLight ^= outLightList[j];
                            outLightList[j] ^= ithLight;
                            ithLight ^= outLightList[j];

                            // update distance to the i'th light
                            dist2= s_masterLightArray[ithLight].m_dist;
                        }
                    }
                }
                else
                {
                    dirCount = i;
                    outLightList[i] = ithLight;
                    usedSlots++;
                }
            }
        }

        void OnEnable()
        {
            DaydreamLightingManager.Init();
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Light l = gameObject.GetComponent<Light>();
                m_lightMode = l.LightMode();
            }
#endif
            // add lights to the master list
            if(m_lightMode != LightModes.BAKED)
            {
                AddToMasterList();
            }
            #if UNITY_EDITOR
            else
            {
                AddToEditorUpdateList();
            }
            #endif
        }

        void OnDisable()
        {
            // remove light from the master list
            #if UNITY_EDITOR
            if(m_lightMode == LightModes.BAKED)
            {
                RemoveFromEditorUpdateList();
            }
            else
            #endif
            {
                RemoveFromMasterList();
            }
        }

        void AddToMasterList()
        {
            m_light = gameObject.GetComponent<Light>();
            m_curMode = m_lightMode;
            m_didInit = false;
            s_masterLightList.Add(this);
            s_masterLightList.Sort(new LightSort());
            s_masterLightArray = s_masterLightList.ToArray();
        }

        void RemoveFromMasterList()
        {
            s_masterLightList.Remove(this);
            s_masterLightArray = s_masterLightList.ToArray();
        }

        #if UNITY_EDITOR
        void AddToEditorUpdateList()
        {
            m_light = gameObject.GetComponent<Light>();
            m_curMode = m_lightMode;
            m_didInit = false;
            DaydreamLightingManager.s_inEditorUpdateList.Add(this);
        }

        void RemoveFromEditorUpdateList()
        {
            DaydreamLightingManager.s_inEditorUpdateList.Remove(this);
        }
        #endif

        static float ComputeLightAttenuation(Light light)
        {

            float lightRange = light.range;
            LightType lightType = light.type;

            // point light attenuation
            float atten = 0f;

            float lightRange2 = lightRange*lightRange;
            const float kl = 1.0f;
            const float kq = 25.0f;

            if (lightType == LightType.Point || lightType == LightType.Area)
            {

                if (lightType == LightType.Point)
                {
                    atten = kq / lightRange2;
                }
                else if (lightType == LightType.Area)
                {
                    atten = kl / lightRange2;
                }

            }
            else if (lightType == LightType.Directional)
            {
                atten = 0.0f;
            }
            else if (lightType == LightType.Spot)
            {
                atten = kq / lightRange2;
            }

            return atten;
        }

    }

}
