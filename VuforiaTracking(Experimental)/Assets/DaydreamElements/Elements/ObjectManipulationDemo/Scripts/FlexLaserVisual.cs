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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DaydreamElements.ObjectManipulation {

  /// A flexible laser visual implementation that bends in a vertex shader.
  [RequireComponent(typeof(MeshRenderer))]
  public class FlexLaserVisual : MonoBehaviour, IGvrArmModelReceiver {

    public GvrBaseArmModel ArmModel { get; set; }

    private MeshRenderer meshRenderer;

    // Transform of the object being selected (overridden reticle position).
    private Transform selectedObject;

    private MaterialPropertyBlock flexMaterialProperties;

    private bool hadSelection = false;
    private float timeSinceLastSelectionChange = 0;

    private int alphaID;
    private int lineJointID;
    private int lineNormalAxisID;

    // Transform of the Gvr reticle.
    private Vector3 controlEndPoint;

    private Vector3 controllerToInfluence;
    private Vector3 selectionToInfluence;
    private Vector3 targetPosition;
    private Vector4 shaderNormal;
    private Vector4[] shaderWorldPositions;

    private const int INFLUENCE_COUNT = 101;
    private const float SELECTION_LERP_SPEED = 2f;

    void Start() {
      shaderWorldPositions = new Vector4[INFLUENCE_COUNT];
      flexMaterialProperties = new MaterialPropertyBlock();
      alphaID = Shader.PropertyToID("_Alpha");
      lineJointID = Shader.PropertyToID("_LineJoint");
      lineNormalAxisID = Shader.PropertyToID("_LineNormalAxis");
      meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetReticlePoint(Vector3 worldPosition) {
      controlEndPoint = worldPosition;
    }

    public void ClearSelectedAndDisable() {
      selectedObject = null;
      gameObject.SetActive(false);
    }

    public void UpdateVisual(Transform t, Vector3 localOffset) {
      selectedObject = t;

      float dt = Time.deltaTime;

      Vector3 currentPosition = transform.position;
      Vector3 endPosition = controlEndPoint;

      timeSinceLastSelectionChange += Time.deltaTime;

      // If there is a selection, set the target position for the end of the
      // laser line to the pivot of the selected object.
      if (selectedObject != null) {
        // Update state if changed.
        if (!hadSelection) {
          timeSinceLastSelectionChange = 0;
          hadSelection = true;
        }
        targetPosition = Vector3.Lerp(targetPosition,
                                      selectedObject.position + selectedObject.rotation * localOffset,
                                      SELECTION_LERP_SPEED * timeSinceLastSelectionChange);
      // If there is no selection, otherwise, lerp the target end point position
      // back to its default value.
      } else {
        // Update state if changed.
        if (hadSelection) {
          timeSinceLastSelectionChange = 0;
          hadSelection = false;
        }
        targetPosition = Vector3.Lerp(targetPosition,
                                      endPosition,
                                      SELECTION_LERP_SPEED * timeSinceLastSelectionChange);
      }

      // Update the position of each influence along the laser curve.
      for (int i = 0; i < INFLUENCE_COUNT; i++) {
        // Get the normalized position of the influence along the laser.
        float influencePos = (float)i / INFLUENCE_COUNT;

        // Get the vector from the controller transform to the control end point (reticle).
        controllerToInfluence = Vector3.Lerp(currentPosition, endPosition, influencePos);
        // Get the vector from the selection transform to this influence.
        selectionToInfluence = Vector3.Lerp(currentPosition, targetPosition, influencePos);

        shaderWorldPositions[i] = Vector3.Lerp(controllerToInfluence, selectionToInfluence, influencePos);
      }

      Vector3 a = (Vector3)shaderWorldPositions[INFLUENCE_COUNT / 2] - currentPosition;
      Vector3 b = targetPosition - (Vector3)shaderWorldPositions[INFLUENCE_COUNT / 2];
      Vector3 normal = Vector3.Cross(a, b);

      if (normal.sqrMagnitude > 0.01f) {
        normal.Normalize();
      } else {
        normal = transform.up;
      }

      shaderNormal = normal;

      // Drive transparency with the preferred alpha from the arm model.
      float alpha = ArmModel != null ? ArmModel.PreferredAlpha : 1.0f;

      // Update laser alpha from arm model preferred alpha.
      flexMaterialProperties.SetFloat(alphaID, alpha);
      // Pass per-influence properties to the flex laser shader.
      flexMaterialProperties.SetVectorArray(lineJointID, shaderWorldPositions);
      flexMaterialProperties.SetVector(lineNormalAxisID, shaderNormal);

      meshRenderer.SetPropertyBlock(flexMaterialProperties);
    }
  }
}
