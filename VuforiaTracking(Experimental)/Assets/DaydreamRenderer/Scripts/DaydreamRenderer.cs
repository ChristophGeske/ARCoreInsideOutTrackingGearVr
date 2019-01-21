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

//#define _USE_INTERNAL_FPS_COUNTER

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace daydreamrenderer
{
[ExecuteInEditMode]
public class DaydreamRenderer : MonoBehaviour
{
    //////////////////////
    // Public Variables
    //////////////////////
    public int   m_shadowWidth      = 128;
    public int   m_shadowHeight     = 128;
    public float m_sharpness        = 0.5f;
    public float m_maxShadowDist    = 100.0f;
    public bool  m_shadowSettings   = true;
    public bool  m_ambientSettings  = true;
    public bool  m_fogSettings      = true;
    public bool  m_showFPS          = true;
    public int   m_maxShadowCasters = 1;
    public bool  m_daydreamLighting = true;
#if UNITY_EDITOR
    public bool m_uiEnabled = false;
#endif

    public Color m_globalAmbientUp = new Color(0.1f, 0.1f, 0.2f);
    public Color m_globalAmbientDn = new Color(0.05f, 0.05f, 0.025f);

    public Vector4 m_fogLinear    = new Vector4(0.0f, 100.0f, 1.0f, 0.1f);
    public Vector4 m_fogHeight    = new Vector4(0.0f, 1.5f, 10.0f, 0.2f);
    public Color   m_fogColorNear = new Color(1.0f, 0.75f, 0.3f, 1.0f);
    public Color   m_fogColorFar  = new Color(0.5f, 0.375f, 0.15f, 1.0f);

    public bool m_fogEnable = false;
    public bool m_heightFogEnable = false;
    public int  m_fogMode = FogMode.Linear;
    public bool m_enableEnlighten = false;
    public bool m_enableManualLightingComponents = false;
    public bool m_enableStaticLightingForScene = false;

    //////////////////////
    // Enumerations/Types
    //////////////////////
    public class FogMode
    {
        static public int Linear = 0;
        static public int Exp    = 1;
        static public int Exp2   = 2;
    };

    private enum ShadowType
    {
        SHDTYPE_PROJECTED = 0,
        SHDTYPE_MASK,
        SHDTYPE_COUNT,
        SHDTYPE_UNKNOWN = SHDTYPE_COUNT
    };
         
    //////////////////////
    // Internal Variables
    //////////////////////
    private Camera          m_camera          = null;
    private RenderTexture[] m_shadowmap       = new RenderTexture[4] { null, null, null, null };
    private RenderTexture   m_workBuffer      = null;
    private RenderTexture   m_shadowComposite = null;
    private Material        m_filterMaterial  = null;
    private Bounds          m_shadowBounds    = new Bounds();
    private Matrix4x4       m_texMatrix;
    private Matrix4x4[]     m_lightMatrix;

    private Matrix4x4       m_viewCamViewMatrix;
    private Matrix4x4       m_viewCamProjMatrix;

    private Vector4         m_horizVec;
    private Vector4         m_vertVec;

    private int[]           m_casterLayers   = null;
    private int[]           m_receiverLayers = null;
    
    private int m_shadowLayer           = -1;
    private int m_shadowReceiverLayer   = -1;
    private int m_filterParamID         = -1;
    private int m_screenProjID          = -1;
    private int m_globalAmbientUpID     = -1;
    private int m_globalAmbientDnID     = -1;
    private int m_globalFogMode         = -1;
    private int m_globalFogLinearID     = -1;
    private int m_globalFogHeightID     = -1;
    private int m_globalFogNearColorID  = -1;
    private int m_globalFogFarColorID   = -1;
    private int m_frameID               =  0;
    private int m_currentWidth          =  0;
    private int m_currentHeight         =  0;
    private ShadowType m_shadowType;
    private int[] m_shadowProjID = new int[4];
    private int[] m_shadowmapID  = new int[4];

    private static Vector4 s_precomputedFogLinear = new Vector4(1, 0, 0, 0);
    private static Vector4 s_precomputedFogHeight = new Vector4(1, 0, 0, 0);

    /////////////////////////
    // Performance Tracking
    /////////////////////////
#if _USE_INTERNAL_FPS_COUNTER
    private float m_aveFPS = 60.0f;
    private float m_aveMS = 1.0f / 60.0f;
    private GUIStyle m_guiStyle;
#endif

    //////////////////////
    // Constants
    //////////////////////
#if UNITY_EDITOR
    private Vector4 c_projectionEditor  = new Vector4(0.5f, -0.5f, 0.5f, 0.5f);
#endif
    private Vector4 c_projectionPlaying = new Vector4(0.5f,  0.5f, 0.5f, 0.5f);
    private Color   c_shadowClearColor  = new Color(1, 0, 0, 0);
        
    //////////////////////
    // Public API
    //////////////////////
    public void Start()
    {
        m_shadowType = ShadowType.SHDTYPE_UNKNOWN;

        //Create the pre-filter material
        if (m_filterMaterial == null)
        {
            Shader filter    = Shader.Find("Unlit/shadowFilter");
            m_filterMaterial = new Material(filter);
        }

        //Convert strings to IDs
        m_shadowLayer = LayerMask.NameToLayer("Shadow");
        if (m_shadowLayer < 0)
        {
            //find the first unused layer...
            for (int i = 31; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)))
                {
                    m_shadowLayer = i;
                    break;
                }
            }
        }

        m_shadowReceiverLayer = LayerMask.NameToLayer("ShadowReceiver");
        if (m_shadowReceiverLayer < 0)
        {
            //find the first unused layer...
            for (int i = 31; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(LayerMask.LayerToName(i)) && i != m_shadowLayer)
                {
                    m_shadowReceiverLayer = i;
                    break;
                }
            }
        }

        m_globalAmbientUpID = Shader.PropertyToID("_globalAmbientUp");
        m_globalAmbientDnID = Shader.PropertyToID("_globalAmbientDn");
        UpdateAmbientParam();

        m_globalFogMode        = Shader.PropertyToID("_fogMode");
        m_globalFogLinearID    = Shader.PropertyToID("_fogLinear");
        m_globalFogHeightID    = Shader.PropertyToID("_fogHeight");
        m_globalFogNearColorID = Shader.PropertyToID("_fogNearColor");
        m_globalFogFarColorID  = Shader.PropertyToID("_fogFarColor");
        UpdateFogParam();
        
        m_filterParamID = Shader.PropertyToID("_uvTransform");
        m_screenProjID  = Shader.PropertyToID("_ScreenProjection");
        for (int i = 0; i < 4; i++)
        {
            m_shadowProjID[i] = Shader.PropertyToID("_shadowProj" + i.ToString());
            m_shadowmapID[i]  = Shader.PropertyToID("_Shadowmap"  + i.ToString());
        }

        //Setup the camera
        m_camera = gameObject.GetComponent<Camera>();
        if (m_camera == null)
        {
            m_camera = gameObject.AddComponent<Camera>();
        }

        EnableEnlighten(m_enableEnlighten);

        m_viewCamViewMatrix = m_camera.worldToCameraMatrix;
        m_viewCamProjMatrix = m_camera.projectionMatrix;
        m_camera.enabled = false;
        m_texMatrix = textureMatrix();
        m_lightMatrix = new Matrix4x4[4];
        buildCompositeBuffer();
    #if _USE_INTERNAL_FPS_COUNTER
        m_guiStyle = new GUIStyle();
        m_guiStyle.fontSize = 48;
        m_guiStyle.normal.textColor = new Color(1.0f, 0.25f, 0.25f);
    #endif
    }

    public void OnEnable()
    {
        m_camera = gameObject.GetComponent<Camera>();
        if (m_camera == null)
        {
            m_camera = gameObject.AddComponent<Camera>();
        }
        Camera.onPreRender += ShadowPreRender;

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.update += Update;
        Undo.undoRedoPerformed += UndoCallback;
    #endif
    }

    void OnDisable()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= Update;
        Undo.undoRedoPerformed -= UndoCallback;
    #endif
        Camera.onPreRender -= ShadowPreRender;
    }

    public void OnDestroy()
    {
        if (m_shadowmap != null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (m_shadowmap[i] != null)
                {
                    m_shadowmap[i].Release();
                    m_shadowmap[i] = null;
                }
            }
        }
        if (m_workBuffer != null)
        {
            m_workBuffer.Release();
            m_workBuffer = null;
        }
        if (m_shadowComposite != null)
        {
            m_shadowComposite.Release();
            m_shadowComposite = null;
        }
    }

#if _USE_INTERNAL_FPS_COUNTER
    void OnGUI()
    {
        if (m_showFPS)
        {
            string fps = string.Format("FPS {0:0.00}, {1:0.00}ms", m_aveFPS, m_aveMS);
            GUI.Label(new Rect(240, 420, 150, 40), fps, m_guiStyle);
        }
    }
#endif

#if UNITY_EDITOR
    public void UndoCallback()
    {
        if (!m_uiEnabled) { return; }

        UpdateFilterParam();
        UpdateAmbientParam();
        UpdateFogParam();
    }
#endif

    // Update is called once per frame
    public void Update()
    {
        if (ShadowCaster.s_clearShadowCasters)
        {
            ClearShadowMaps();
            ShadowCaster.s_clearShadowCasters = false;
        }

        if (m_maxShadowCasters > 0)
        {
            //handle adjustments to the settings/quality
            if (m_currentWidth != m_shadowWidth || m_currentHeight != m_shadowHeight)
            {
                rebuildShadowBuffers();
            }

            //update shadow map(s)
            if (ShadowCaster.s_shadowCastingObjects != null)
            {
                m_frameID++;
                if (m_maxShadowCasters == 1)
                {
                    if ((m_frameID & 1) != 0 && ShadowCaster.s_shadowCastingObjects[0].Count > 0)
                    {
                        float blend = buildShadowCamera(0);
                        if (blend > 0.0f)
                        {
                            updateShadowmap(0, blend);
                        }
                    }
                }
                else //The shadow mask adds additional "fixed" overhead (i.e. overhead from Unity)
                {
                    int index = (m_frameID & 1) * 2;

                    //split the work between frames.
                    if (ShadowCaster.s_shadowCastingObjects[index].Count > 0 && m_maxShadowCasters > index)
                    {
                        float blend = buildShadowCamera(index);
                        if (blend > 0.0f)
                        {
                            updateShadowmap(index, blend);
                        }
                    }
                    if (ShadowCaster.s_shadowCastingObjects[index + 1].Count > 0 && m_maxShadowCasters > index + 1)
                    {
                        float blend = buildShadowCamera(index + 1);
                        if (blend > 0.0f)
                        {
                            updateShadowmap(index + 1, blend);
                        }
                    }

                    //combine all shadows into a screenspace "shadow mask"
                    renderShadowMask();
                }
            }
        }

        //Set the shadow type in the receiver materials if it has changed.
        if (m_maxShadowCasters == 1)
        {
            setShadowType(ShadowType.SHDTYPE_PROJECTED);
        }
        else if (m_maxShadowCasters > 1)
        {
            setShadowType(ShadowType.SHDTYPE_MASK);
        }

#if _USE_INTERNAL_FPS_COUNTER
        if (m_showFPS)
        {
            //determine the average fps (for display later).
            const float weight = 0.01f; //smaller values = smoother/more averaged fps reading, larger values = more accurate to the current frame.
            float dt = Time.deltaTime;
            m_aveFPS = m_aveFPS * (1.0f - weight) + (1.0f / dt) * weight;
            m_aveMS = 1000.0f / m_aveFPS;
        }
#endif
    }

    public void UpdateFilterParam()
    {
        m_horizVec.Set(1.0f / (float)m_currentWidth, 0.0f, 0.0f, m_sharpness);
        m_vertVec.Set(0.0f, 1.0f / (float)m_currentHeight, 0.0f, m_sharpness);
    }

    public void UpdateAmbientParam()
    {
        Shader.SetGlobalColor(m_globalAmbientUpID, m_globalAmbientUp);
        Shader.SetGlobalColor(m_globalAmbientDnID, m_globalAmbientDn);
    }

    public void EnableEnlighten(bool enable)
    {
        m_enableEnlighten = enable;
        if (enable)
        {
            Shader.EnableKeyword("_USE_ENLIGHTEN");
        }
        else
        {
            Shader.DisableKeyword("_USE_ENLIGHTEN");
        }
    }

    public void UpdateFogParam()
    {
        //precompute the scale factor.
        s_precomputedFogLinear.x = 1.0f / (m_fogLinear.y - m_fogLinear.x);
        s_precomputedFogLinear.y =-m_fogLinear.x * s_precomputedFogLinear.x;
        s_precomputedFogLinear.z = m_fogLinear.z;
        s_precomputedFogLinear.w = m_fogLinear.w;

        s_precomputedFogHeight.x = m_fogHeight.x;
        s_precomputedFogHeight.y = m_fogHeight.y - m_fogHeight.x;
        s_precomputedFogHeight.z = m_fogHeight.z;
        s_precomputedFogHeight.w = m_fogHeight.w;

        Shader.SetGlobalVector(m_globalFogLinearID,   s_precomputedFogLinear);
        Shader.SetGlobalVector(m_globalFogHeightID,   s_precomputedFogHeight);
        Shader.SetGlobalColor(m_globalFogNearColorID, m_fogColorNear);
        Shader.SetGlobalColor(m_globalFogFarColorID,  m_fogColorFar);

        if (m_fogEnable)
        {
            if (m_heightFogEnable)
            {
                Shader.DisableKeyword("_DAYDREAM_FOG");
                Shader.EnableKeyword("_FOG_HEIGHT");
            }
            else
            {
                Shader.EnableKeyword("_DAYDREAM_FOG");
                Shader.DisableKeyword("_FOG_HEIGHT");
            }

            Shader.SetGlobalFloat(m_globalFogMode, (float)m_fogMode);
        }
        else
        {
            Shader.DisableKeyword("_DAYDREAM_FOG");
            Shader.DisableKeyword("_FOG_HEIGHT");
        }
    }

    public void ShadowPreRender(Camera cam)
    {
        Camera eyeCamera = Camera.main;
#if UNITY_EDITOR
        if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
        {
            eyeCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
        }
#endif
        if (cam == null || cam != eyeCamera) { return; }

        m_viewCamViewMatrix = cam.worldToCameraMatrix;
        m_viewCamProjMatrix = cam.projectionMatrix;
    }

    public void ClearShadowMaps()
    {
        RenderTexture prev = RenderTexture.active;
        if (m_shadowComposite != null)
        {
            RenderTexture.active = m_shadowComposite;
            GL.Clear(false, true, Color.white);
        }
        for (int i = 0; i < 4; i++)
        {
            RenderTexture.active = m_shadowmap[i];
            GL.Clear(false, true, Color.white);
        }

        RenderTexture.active = prev;
    }

    //////////////////////
    // Internal
    //////////////////////

    private void buildCompositeBuffer()
    {
        int screenWidth   = Camera.main.pixelWidth;
        int screenHeight  = Camera.main.pixelHeight;
        m_shadowComposite = new RenderTexture(screenWidth >> 2, screenHeight >> 1, 16);
        m_shadowComposite.Create();
    }

    private void rebuildShadowBuffers()
    {
        m_currentWidth  = m_shadowWidth;
        m_currentHeight = m_shadowHeight;
        if (m_workBuffer != null)
        {
            m_workBuffer.Release();
        }

        if (m_shadowmap == null)
        {
            m_shadowmap = new RenderTexture[4];
        }

        for (int i = 0; i < 4; i++)
        {
            if (m_shadowmap[i] != null)
            {
                m_shadowmap[i].Release();
            }

            m_shadowmap[i] = new RenderTexture(m_currentWidth, m_currentHeight, 16, RenderTextureFormat.RGHalf);
            m_shadowmap[i].Create();
        }

        m_workBuffer = new RenderTexture(m_currentWidth, m_currentHeight, 0, RenderTextureFormat.RGHalf);
        m_workBuffer.Create();

        UpdateFilterParam();
    }

    private float buildShadowCamera(int index)
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 camDir = Camera.main.transform.forward;

        if (!calcShadowCasterObjectBounds(index, camPos, camDir))
        {
            return 0.0f;
        }

        Vector3 center = m_shadowBounds.center;
        Vector3 closestCaster = Vector3.zero;
        float blend = calcShadowSource(center, ref closestCaster);
        buildTightFittingShadowFrustum(center, closestCaster);

        return blend;
    }

    private void updateShadowmap(int index, float blend)
    {
        m_camera.clearFlags      = CameraClearFlags.SolidColor;
        m_camera.cullingMask     = 1 << m_shadowLayer;
        m_camera.backgroundColor = c_shadowClearColor;
        m_camera.stereoTargetEye = StereoTargetEyeMask.None;
        m_camera.SetTargetBuffers(m_shadowmap[index].colorBuffer, m_shadowmap[index].depthBuffer);

        m_lightMatrix[index] = m_texMatrix * m_camera.projectionMatrix * m_camera.worldToCameraMatrix;
        Shader.SetGlobalMatrix(m_shadowProjID[0], m_lightMatrix[index]);
        Shader.SetGlobalTexture(m_shadowmapID[0], m_shadowmap[index]);

        enableShadowBuild(index, true);
        DaydreamMeshRenderer.s_ignoreLights = true;
        m_camera.Render();
        DaydreamMeshRenderer.s_ignoreLights = false;

        //blend the shadow intensity during the pre-filter to avoid yet another multiply in the final pixel shader.
        m_horizVec.z = blend;
        m_vertVec.z  = blend;

        //pre-filter the shadow map
        prefilterShadowPass(m_horizVec, m_shadowmap[index],  m_workBuffer);
        prefilterShadowPass(m_vertVec,  m_workBuffer, m_shadowmap[index]);

        enableShadowBuild(index, false);
    }

    private void renderShadowMask()
    {
        if (m_camera == null) { return; }

        Camera eyeCamera = Camera.main;
#if UNITY_EDITOR
        if (!Application.isPlaying && UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
        {
            eyeCamera = UnityEditor.SceneView.lastActiveSceneView.camera;
        }
#endif
        //composite shadows into a lower resolution buffer.
        m_camera.CopyFrom(eyeCamera);
        m_camera.stereoTargetEye = StereoTargetEyeMask.None;
        m_camera.worldToCameraMatrix = m_viewCamViewMatrix;
        m_camera.projectionMatrix    = m_viewCamProjMatrix;

        m_camera.clearFlags = CameraClearFlags.SolidColor;
        m_camera.cullingMask = 1 << m_shadowReceiverLayer;
        m_camera.backgroundColor = Color.white;
        m_camera.SetTargetBuffers(m_shadowComposite.colorBuffer, m_shadowComposite.depthBuffer);
        for (int i = 0; i < 4; i++)
        {
            Shader.SetGlobalTexture(m_shadowmapID[i], m_shadowmap[i]);
            Shader.SetGlobalMatrix(m_shadowProjID[i], m_lightMatrix[i]);
        }
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            Shader.SetGlobalVector(m_screenProjID, c_projectionPlaying);
        }
        else
        {
            Shader.SetGlobalVector(m_screenProjID, c_projectionEditor);
        }
#else
        Shader.SetGlobalVector(m_screenProjID, c_projectionPlaying);
#endif

        enableShadowReceive(true);
        m_camera.Render();
        enableShadowReceive(false);

        m_camera.ResetWorldToCameraMatrix();
        Shader.SetGlobalTexture(m_shadowmapID[0], m_shadowComposite);

        Matrix4x4 compositeMtx = m_texMatrix * m_viewCamProjMatrix * m_viewCamViewMatrix;
        Shader.SetGlobalMatrix(m_shadowProjID[0], compositeMtx);
    }

    private bool calcShadowCasterObjectBounds(int index, Vector3 camPos, Vector3 camDir)
    {
        if (ShadowCaster.s_shadowCastingObjects == null || ShadowCaster.s_shadowCastingObjects[index] == null)
        {
            return false;
        }
        List<GameObject> shadowObjects = ShadowCaster.s_shadowCastingObjects[index];

        float maxDist2 = m_maxShadowDist * m_maxShadowDist;
        //first determine the bounds of the shadow casting objects.
        int len = shadowObjects.Count;
        int shadowObjCount = 0;
        for (int i = 0; i < len; i++)
        {
            GameObject obj = shadowObjects[i];
            Renderer rd = obj.GetComponent<Renderer>();

            //if the object is too far away, just ignore.
            //note the actual radius should be | |center-camPos|.extents |, use the camDir as an approximation.
            float radius = vec3DotAbs(camDir, rd.bounds.extents);
            Vector3 offset = (rd.bounds.center - camPos);
            if (Vector3.Dot(offset, offset) - radius * radius > maxDist2)
            {
                continue;
            }

            //otherwise adjust the bounds.
            if (shadowObjCount == 0)
            {
                m_shadowBounds = rd.bounds;
            }
            else
            {
                m_shadowBounds.Encapsulate(rd.bounds);
            }
            shadowObjCount++;
        }

        return (shadowObjCount > 0);
    }

    private float calcShadowSource(Vector3 shadowCenter, ref Vector3 closestCaster)
    {
        if (ShadowSource.s_shadowSources == null)
        {
            return 0.0f;
        }
        List<ShadowSourceObject> sources = ShadowSource.s_shadowSources;

        //find the caster that "best" fits the object set.
        int casterCount = sources.Count;
        if (casterCount < 1) { return 0.0f; }

        Light dirLight = null;
        float minWeight0 = float.MaxValue;
        float minWeight1 = float.MaxValue;
        for (int s = 0; s < casterCount; s++)
        {
            GameObject obj = sources[s].m_obj;
            Light light = obj.GetComponent<Light>();

            Vector3 pos;
            if (light.type == LightType.Directional)
            {
                dirLight = light;
                continue;
            }
            else
            {
                pos = obj.transform.position;
            }
            float weight = ((pos - shadowCenter).sqrMagnitude) / (light.range * light.range);

            if (weight < minWeight0)
            {
                minWeight1 = minWeight0;
                minWeight0 = weight;
                closestCaster = pos;
            }
            else if (weight < minWeight1)
            {
                minWeight1 = weight;
            }
        }

        //build a weight so that the blend is zero before a transition between casters and is zero when outside of
        //the light's range.
        float blend = 0.0f;
        if (casterCount > 1 && minWeight0 < 1.0f)
        {
            float w0 = 1.0f / minWeight0;

            //clamp the effect of the secondary source to the range of its light.
            float w1 = Mathf.Max(1.0f / minWeight1, 1.0f);
            float wScale = 1.0f / (w0 + w1);

            //blend = 0.0 when w0 <= 0.5, 1.0 when w0 = 1.0; transitions happen at weight = 0.5
            w0 *= wScale;   //we only care about weight 0, so weight 1 is ignored for the final computations.
            blend = Mathf.Min(Mathf.Max(w0 - 0.5f, 0.0f) * 2.0f, 1.0f) * Mathf.Clamp01(1.0f - minWeight0);
        }
        else if (casterCount == 1 && minWeight0 < 1.0f)
        {
            blend = Mathf.Clamp01(1.0f - minWeight0);
        }
        else if (dirLight != null)  //blend in a directional light shadow if one is required.
        {
            float minRangeDist = minWeight0 - 1.0f;

            blend = Mathf.Min(minRangeDist*4.0f, 1.0f);
            closestCaster = shadowCenter - dirLight.transform.forward * 16.0f;
        }

        return blend;
    }

    private void buildTightFittingShadowFrustum(Vector3 shadowCenter, Vector3 closestCaster)
    {
        if (m_camera == null)
        {
            return;
        }

        //then build a camera with a tight fitting frustum.
        m_camera.transform.position = closestCaster;
        m_camera.transform.LookAt(shadowCenter);
        m_camera.aspect = 1.0f;

        const float gutter = 0.001f;
        const float zNear  = 0.1f;
        const float zFar   = 128.0f;

        float rAABB = m_shadowBounds.extents.magnitude;
        float d = (shadowCenter - closestCaster).magnitude;

        float sinAngleXZ = Mathf.Min(rAABB / d, 0.9998f);
        float sinAngleYZ = Mathf.Min(rAABB / d, 0.9998f);
        float tanAngleXZ = Mathf.Min(sinAngleXZ / Mathf.Sqrt(1.0f - sinAngleXZ * sinAngleXZ), 57.0f );
        float tanAngleYZ = Mathf.Min(sinAngleYZ / Mathf.Sqrt(1.0f - sinAngleYZ * sinAngleYZ), 57.0f);

        float rXZ = tanAngleXZ + gutter;
        float rYZ = tanAngleYZ + gutter;

        m_camera.projectionMatrix = perspectiveCustom(rXZ, rYZ, zNear, zFar);
        m_camera.transform.hasChanged = true;
    }

    private void enableShadowBuild(int index, bool enable)
    {
        if (ShadowCaster.s_shadowCastingObjects == null) { return; }
        List<GameObject> casters = ShadowCaster.s_shadowCastingObjects[index];

        int count = casters.Count;
        if (count < 1) { return; }

        if (m_casterLayers == null || m_casterLayers.Length < count)
        {
            m_casterLayers = new int[count];
        }

        if (enable)
        {
            for (int i = 0; i < count; i++)
            {
                m_casterLayers[i] = casters[i].layer;
                casters[i].layer  = m_shadowLayer;

                Material[] materials = casters[i].GetComponent<Renderer>().sharedMaterials;
                if(materials != null && materials.Length > 0)
                {
                    for(int m = 0; m < materials.Length; ++m)
                    {
                        materials[m].EnableKeyword("BUILD_SHADOWMAP");
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                casters[i].layer = m_casterLayers[i];

                Material[] materials = casters[i].GetComponent<Renderer>().sharedMaterials;
                if (materials != null && materials.Length > 0)
                {
                    for (int m = 0; m < materials.Length; ++m)
                    {
                        materials[m].DisableKeyword("BUILD_SHADOWMAP");
                    }
                }
            }
        }
    }

    private void setShadowType(ShadowType type)
    {
        if (m_shadowType == type) { return; }

        if (ShadowReceiver.s_shadowReceivingObjects == null) { return; }
        List<GameObject> receivers = ShadowReceiver.s_shadowReceivingObjects;

        int count = receivers.Count;
        if (count < 1) { return; }

        string enable  = (type == ShadowType.SHDTYPE_PROJECTED) ? "SHDTYPE_PROJECTED" : "SHDTYPE_MASK";
        string disable = (type == ShadowType.SHDTYPE_MASK)      ? "SHDTYPE_PROJECTED" : "SHDTYPE_MASK";

        for (int i = 0; i < count; i++)
        {
            if (receivers[i] == null) { continue; }
            Renderer renderer = receivers[i].GetComponent<Renderer>();
            if (renderer == null) { continue; }

            Material[] material = receivers[i].GetComponent<Renderer>().sharedMaterials;
            if (material == null) { continue; }

            int mcount = material.Length;
            for (int m=0; m<mcount; m++)
            {
                material[m].EnableKeyword(enable);
                material[m].DisableKeyword(disable);
            }
        }

        m_shadowType = type;
    }

    private void enableShadowReceive(bool enable)
    {
        if (ShadowReceiver.s_shadowReceivingObjects == null) { return; }
        List<GameObject> receivers = ShadowReceiver.s_shadowReceivingObjects;

        int count = receivers.Count;
        if (count < 1) { return; }

        if (m_receiverLayers == null || m_receiverLayers.Length < count)
        {
            m_receiverLayers = new int[count];
        }
               
        if (enable)
        {
            for (int i = 0; i < count; i++)
            {
                m_receiverLayers[i] = receivers[i].layer;
                receivers[i].layer = m_shadowReceiverLayer;

                Material[] material = receivers[i].GetComponent<Renderer>().sharedMaterials;
                if (material == null) { continue; }

                int mcount = material.Length;
                for (int m=0; m<mcount; m++)
                {
                    material[m].EnableKeyword("REC_SHADOWMAP");
                }
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                receivers[i].layer = m_receiverLayers[i];

                Material[] material = receivers[i].GetComponent<Renderer>().sharedMaterials;
                if (material == null) { continue; }

                int mcount = material.Length;
                for (int m=0; m<mcount; m++)
                {
                    material[m].DisableKeyword("REC_SHADOWMAP");
                }
            }
        }
    }

    private float vec3DotAbs(Vector3 a, Vector3 b)
    {
        return Mathf.Abs(a.x * b.x) + Mathf.Abs(a.y * b.y) + Mathf.Abs(a.z * b.z);
    }

    private void prefilterShadowPass(Vector4 param, RenderTexture src, RenderTexture dst)
    {
        m_filterMaterial.SetVector(m_filterParamID, param);
        dst.DiscardContents();
        Graphics.Blit(src, dst, m_filterMaterial, 0);
    }

    private Matrix4x4 perspectiveCustom(float radiusX, float radiusY, float near, float far)
    {
        float x = 1.0f / radiusX;// 2.0f * near / radiusX;
        float y = 1.0f / radiusY;// 2.0f * near / radiusY;
        float a = 0.0f; //assumed that the matrix is NOT off-center, the offsets would go here (a & b).
        float b = 0.0f;
        float c = -(far + near) / (far - near);
        float d = -(2.0f * far * near) / (far - near);
        float e = -1.0f;

        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = x;
        m.m02 = a;
        m.m11 = y;
        m.m12 = b;
        m.m22 = c;
        m.m23 = d;
        m.m32 = e;
        m.m33 = 0;

        return m;
    }

    private Matrix4x4 textureMatrix()
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = 0.5f;
        m.m11 = 0.5f;
        m.m22 = 0.5f;
        m.m03 = 0.5f;
        m.m13 = 0.5f;
        m.m23 = 0.5f;

        return m;
    }
}
}