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

using UnityEngine;
using UnityEngine.EventSystems;

namespace DaydreamElements.Chase {

  /// Class for an object that creates a visual point of origin for
  /// the pointer laser.
  public class LaserOriginVisual : MonoBehaviour, IGvrArmModelReceiver {
    /// A game object that is placed at the origin of the laser arc.
    [Tooltip("A game object that is placed at the origin of the laser arc.")]
    public GameObject lineOriginPrefab;

    private MeshRenderer lineOriginRenderer;
    private MaterialPropertyBlock propertyBlock;
    private int alphaID;
    private GameObject lineOrigin;

    public GvrBaseArmModel ArmModel { get; set; }

    void Start () {
      alphaID = Shader.PropertyToID("_Alpha");
      propertyBlock = new MaterialPropertyBlock();
    }

    void Update () {
      if (lineOriginPrefab == null) {
        return;
      }

      if (lineOrigin == null) {
        lineOrigin = Instantiate(lineOriginPrefab) as GameObject;
      }

      if (lineOriginRenderer == null) {
        lineOriginRenderer = lineOrigin.GetComponent<MeshRenderer>();
      }

      // Drive transparency with the preferred alpha from the arm model.
      float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;
      propertyBlock.SetFloat(alphaID, alpha);

      if (lineOriginRenderer != null) {
        lineOriginRenderer.SetPropertyBlock(propertyBlock);
      }
    }

    void LateUpdate() {
      // Move the line origin prefab to the start of the line.
      lineOrigin.transform.position = GvrPointerInputModule.Pointer.PointerTransform.position;
    }
  }
}