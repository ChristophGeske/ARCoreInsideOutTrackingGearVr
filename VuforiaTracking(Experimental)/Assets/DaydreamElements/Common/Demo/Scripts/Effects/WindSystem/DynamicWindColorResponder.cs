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

  public class DynamicWindColorResponder : MonoBehaviour {
    public MeshRenderer meshRenderer;
    MaterialPropertyBlock materialPropertyBlock;

    //Set by GlobalDynamicWindColor
    bool PerObjectActivation = true;
    bool ActivationDecay = true;

    float objectEffectScale = 0f;
    float objectEffectTargetScale = 0f;

    public float activationRate = 1f;
    public float activationDecayRate = 0.01f;
    
    float worldPosX;
    float worldPosZ;
    float perObjectRandom;

    //Shader property IDs
    int objectEffectScaleID;
    int perObjectRandomID;

    void OnDrawGizmosSelected() {
      if (meshRenderer==null) {
        meshRenderer = gameObject.GetComponent( typeof(MeshRenderer) ) as MeshRenderer;
      }
    }

    void Awake() {
      if (meshRenderer==null) {
        meshRenderer = gameObject.GetComponent( typeof(MeshRenderer) ) as MeshRenderer;
      }
    }

    void Start() {
      Initialize();

      materialPropertyBlock = new MaterialPropertyBlock();

      objectEffectScaleID = Shader.PropertyToID("_PerObjectEffectScale");
      perObjectRandomID = Shader.PropertyToID("_PerObjectRandom");

      perObjectRandom = 20f * (transform.position.x + transform.position.z);
    }

    void Update() {
      float dt = Time.deltaTime;
      worldPosX = transform.position.x;
      worldPosZ = transform.position.z;
      
      if (ActivationDecay == true) {
        objectEffectTargetScale -= activationDecayRate;
      }

      objectEffectTargetScale = Mathf.Clamp01(objectEffectTargetScale);
      objectEffectScale = Mathf.Lerp(objectEffectScale, objectEffectTargetScale, dt);

      materialPropertyBlock.SetFloat(objectEffectScaleID, objectEffectScale);
      materialPropertyBlock.SetFloat(perObjectRandomID, perObjectRandom);
      meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    public void Initialize() {
      objectEffectScale = 0f;
      objectEffectTargetScale = 0f;
    }

    public void ActivateFromEffector(Vector4 effector) {
      Vector2 dstToEffector;
      dstToEffector = new Vector2(worldPosX - effector.x, worldPosZ - effector.z);

      float effectorMask = 1.0f - dstToEffector.magnitude / effector.w;
      effectorMask = Mathf.Clamp01(effectorMask);

      if (PerObjectActivation == true) {
        objectEffectTargetScale += effectorMask * activationRate;
      }
    }

    public void SetProperties(bool NewPerObjectActivation, bool NewActivationDecay) {
      PerObjectActivation = NewPerObjectActivation;
      ActivationDecay = NewActivationDecay;
    }
  }
}
