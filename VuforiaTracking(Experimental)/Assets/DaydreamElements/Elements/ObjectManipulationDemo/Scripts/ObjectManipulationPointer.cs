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

// The controller is not available for versions of Unity without the
// GVR native integration.

using UnityEngine;
using UnityEngine.EventSystems;

namespace DaydreamElements.ObjectManipulation {

  /// A simple pointer class to handle object interaction.
  public class ObjectManipulationPointer : GvrBasePointer {

    /// Distance from the pointer that raycast hits will be detected.
    [Tooltip("Distance from the pointer that raycast hits will be detected.")]
    public float maxPointerDistance = 20.0f;

    /// Distance from the pointer that the reticle will be drawn at when hitting nothing.
    [Tooltip("Distance from the pointer that the reticle will be drawn at when hitting nothing.")]
    public float defaultReticleDistance = 20.0f;

    /// Distance from the pointer that the line will be drawn at when hitting nothing.
    [Tooltip("Distance from the pointer that the line will be drawn at when hitting nothing.")]
    public float defaultLineDistance = 0.3f;

    /// The laser visual used by the pointer.
    [Tooltip("The laser visual used by the pointer.")]
    public FlexLaserVisual flexLaserVisual;

    /// The reticle used by the pointer.
    [Tooltip("The reticle used by the pointer.")]
    public FlexReticle reticle;

    private bool isHittingTarget;

    private static Transform selectedTransform;
    private static Vector3 selectedPoint;

    public static void SetSelected(Transform t, Vector3 localSpaceHitPosition) {
      selectedTransform = t;
      selectedPoint = localSpaceHitPosition;
    }

    public static void ReleaseSelected(Transform t) {
      if (selectedTransform == t) {
        selectedTransform = null;
      }
    }

    public static bool IsObjectSelected() {
      return selectedTransform != null;
    }

    public override float MaxPointerDistance {
      get {
        return maxPointerDistance;
      }
    }

    public override void GetPointerRadius(out float enterRadius, out float exitRadius) {
      enterRadius = 0.1f;
      exitRadius = 0.1f;
    }

    public override void OnPointerEnter(RaycastResult raycastResult, bool isInteractive) {
      OnPointerHover(raycastResult, isInteractive);
    }

    public override void OnPointerHover(RaycastResult raycastResult, bool isInteractive) {
      if (!IsObjectSelected()) {
        reticle.SetTargetPosition(raycastResult.worldPosition, PointerTransform.position);
        Vector3 ray = raycastResult.worldPosition - PointerTransform.position;
        if (flexLaserVisual != null) {
          if (ray.magnitude < defaultLineDistance) {
            flexLaserVisual.SetReticlePoint(raycastResult.worldPosition);
          } else {
            SetLaserToDefaultDistance();
          }
        }
        isHittingTarget = true;
      }
    }

    public override void OnPointerExit(GameObject previousObject) {
      isHittingTarget = false;
    }

    public override void OnPointerClickDown() {
    }

    public override void OnPointerClickUp() {
    }

    protected override void Start() {
      base.Start();
    }

    void Update() {
      if (!isHittingTarget) {
        reticle.SetTargetPosition(GetPointAlongPointer(defaultReticleDistance), PointerTransform.position);
        SetLaserToDefaultDistance();
      }
      if (IsObjectSelected()) {
        reticle.Hide();
        float dist = (selectedTransform.position - PointerTransform.position).magnitude;
        flexLaserVisual.SetReticlePoint(GetPointAlongPointer(dist));
      } else {
        reticle.Show();
      }

      flexLaserVisual.UpdateVisual(selectedTransform, selectedPoint);
    }

    private void SetLaserToDefaultDistance() {
      Vector3 direction = (reticle.TargetPosition - PointerTransform.position).normalized;
      Vector3 laserPoint = transform.position + (direction * defaultLineDistance);

      flexLaserVisual.SetReticlePoint(laserPoint);
    }
  }
}
