// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace DaydreamElements.Common {

  using UnityEngine;
  using System.Collections;

  public class GlobalDynamicWindColorSimple : MonoBehaviour {
    /// Simple global effect states

    // Global system on/off
    public bool GlobalEffectActive = true;

    // System animates vertex colors
    public bool AffectColor = true;
    // System animates vertex positions
    public bool AffectMotion = true;

    // Global effect masking from radial influences on/off
    public bool RadialEffectorInfluence = true;
    // Per-object effect activation on/off
    public bool PerObjectActivation = true;

    // Per-object activation decays over time
    public bool ActivationDecay = false;

    /// Global effect properties

    // Global effect overrides
    float globalEffectScale = 0f;
    float globalEffectTargetScale = 1f;

    float globalColorEffectScale = 0f;
    float globalWindEffectScale = 0f;

    float radialEffectorScale = 0f;
    float radialEffectorTargetScale = 1f;

    // Floor for color activation
    public float saturationMin = 0.01f;

    // Wind effect params
    public float windSpeed = 2.0f;
    public float windMagnitude = 1.0f;
    public float windTurbulence = 2.0f;

    // Wind XZ direction
    public float windDirectionX = 0f;
    public float windDirectionZ = 0f;
    public float gustDensity = 0.5f;
    public float gustColorScale = 0.05f;

    // Effect responders
    public DynamicWindColorResponder[] Responders;
    float responderCount;

    public DynamicWindColorEffector effectorA;

    // Global shader property IDs
    int globalEffectScaleID;
    int globalColorEffectScaleID;
    int globalWindEffectScaleID;
    int radialEffectorInfluenceID;

    int saturationMinID;

    int windSpeedID;
    int windMagnitudeID;
    int windTurbulenceID;

    int windDirectionXID;
    int windDirectionZID;
    int gustDensityID;
    int gustColorScaleID;

    int effectorAID;

    void OnDrawGizmos() {
      effectorA.DrawGizmos();
    }

    void OnDrawGizmosSelected() {
      if (Responders != null && Responders.Length == 0) {
        Responders = FindObjectsOfType( typeof(DynamicWindColorResponder)) as DynamicWindColorResponder[];
      }
    }

    void Awake() {
      globalEffectScaleID = Shader.PropertyToID("_GlobalEffectScale");
      globalColorEffectScaleID = Shader.PropertyToID("_GlobalColorEffectScale");
      globalWindEffectScaleID = Shader.PropertyToID("_GlobalWindEffectScale");
      radialEffectorInfluenceID = Shader.PropertyToID("_RadialEffectorInfluence");

      saturationMinID = Shader.PropertyToID("_SaturationMin");

      windSpeedID = Shader.PropertyToID("_WindSpeed");
      windMagnitudeID = Shader.PropertyToID("_WindMagnitude");
      windTurbulenceID = Shader.PropertyToID("_WindTurbulence");

      windDirectionXID = Shader.PropertyToID("_WindDirectionX");
      windDirectionZID = Shader.PropertyToID("_WindDirectionZ");
      gustDensityID = Shader.PropertyToID("_GustDensity");
      gustColorScaleID = Shader.PropertyToID("_GustColorScale");

      effectorAID = Shader.PropertyToID("_EffectorA");
    }

    void Start() {
      Initialize();

      responderCount = Responders.Length;
    }

    void Update() {
      float dt = Time.deltaTime;

      if (GlobalEffectActive == true) { globalEffectTargetScale = 1f; }
      else { globalEffectTargetScale = 0f; }

      if (GlobalEffectActive == true && RadialEffectorInfluence == true) { radialEffectorTargetScale = 1f; }
      else { radialEffectorTargetScale = 0f; }

      if (AffectColor == true) { globalColorEffectScale = 1f; }
      else { globalColorEffectScale = 0f; }

      if (AffectMotion == true) { globalWindEffectScale = 1f; }
      else { globalWindEffectScale = 0f; }

      globalEffectScale = Mathf.Lerp(globalEffectScale, globalEffectTargetScale, 10*dt);
      radialEffectorScale = Mathf.Lerp(radialEffectorScale, radialEffectorTargetScale, 10*dt);

      effectorA.UpdateEffector();

      SetGlobalShaderParams();
      UpdateResponders();
    }

    void Initialize() {
      globalEffectScale = 0f;
      globalEffectTargetScale = 1f;

      globalColorEffectScale = 0f;
      globalWindEffectScale = 0f;

      radialEffectorScale = 0f;
      radialEffectorTargetScale = 1f;

      effectorA.Initialize();
    }

    void SetGlobalShaderParams() {
      Shader.SetGlobalFloat(globalEffectScaleID, globalEffectScale);

      Shader.SetGlobalFloat(globalColorEffectScaleID, globalColorEffectScale);
      Shader.SetGlobalFloat(globalWindEffectScaleID, globalWindEffectScale);

      Shader.SetGlobalFloat(radialEffectorInfluenceID, radialEffectorScale);

      Shader.SetGlobalFloat(saturationMinID, saturationMin);

      Shader.SetGlobalFloat(windSpeedID, windSpeed);
      Shader.SetGlobalFloat(windMagnitudeID, windMagnitude);
      Shader.SetGlobalFloat(windTurbulenceID, windTurbulence);

      Shader.SetGlobalFloat(windDirectionXID, windDirectionX);
      Shader.SetGlobalFloat(windDirectionZID, windDirectionZ);
      Shader.SetGlobalFloat(gustDensityID, gustDensity);
      Shader.SetGlobalFloat(gustColorScaleID, gustColorScale);

      Shader.SetGlobalVector(effectorAID, effectorA.effectorProperties);
    }

    void UpdateResponders() {
      for (int i=0; i<responderCount; i++) {
        Responders[i].SetProperties(PerObjectActivation, ActivationDecay);
        Responders[i].ActivateFromEffector(effectorA.effectorProperties);
      }
    }
  }
}