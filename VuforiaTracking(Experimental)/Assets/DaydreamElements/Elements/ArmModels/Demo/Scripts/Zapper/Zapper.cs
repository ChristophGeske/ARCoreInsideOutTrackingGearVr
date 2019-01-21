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
using UnityEngine.EventSystems;

namespace DaydreamElements.ArmModels {

  /// Custom pointer used to spawn a projectile for a zapper.
  public class Zapper : GvrBasePointer {
    public GameObject projectilePrefab;
    public Transform pointer;
    public Transform barrel;
    public Transform reticle;
    public Transform gunVisual;
    public Transform controllerVisual;
    public LayerMask reticleLayerMask;
    public float defaultReticleDistance = 2.5f;
    public float fireCooldownSeconds = 0.3f;

    private bool isHittingTarget;
    private Vector3 hitPos;
    private float fireCooldownTimerSeconds;

    public override float MaxPointerDistance {
      get {
        return float.MaxValue;
      }
    }

    public override float CameraRayIntersectionDistance {
      get {
        return defaultReticleDistance;
      }
    }

    public override Transform PointerTransform {
      get {
        return pointer;
      }
    }

    public override void OnPointerEnter(RaycastResult raycastResult, bool isInteractive) {
      hitPos = raycastResult.worldPosition;
      isHittingTarget = true;
    }

    public override void OnPointerHover(RaycastResult raycastResult, bool isInteractive) {
      hitPos = raycastResult.worldPosition;
      isHittingTarget = true;
    }

    public override void OnPointerExit(GameObject previousObject) {
      isHittingTarget = false;
    }

    public override void OnPointerClickDown() {
    }

    public override void OnPointerClickUp() {
    }

    public override void GetPointerRadius(out float enterRadius, out float exitRadius) {
      enterRadius = 0.0f;
      exitRadius = 0.0f;
    }

    void LateUpdate() {
      fireCooldownTimerSeconds -= Time.deltaTime;
      fireCooldownTimerSeconds = Mathf.Clamp(fireCooldownTimerSeconds, 0.0f, float.MaxValue);

      if (GvrControllerInput.ClickButtonDown && fireCooldownTimerSeconds == 0.0f) {
        SpawnProjectile();
        fireCooldownTimerSeconds = fireCooldownSeconds;
      }

      UpdateReticle();
    }

    private void SpawnProjectile() {
      if (projectilePrefab == null) {
        return;
      }

      if (barrel == null) {
        return;
      }

      GameObject projectile = GameObject.Instantiate(projectilePrefab);
      projectile.transform.position = barrel.position;
      projectile.transform.LookAt(reticle);
    }

    private void UpdateReticle() {
      if (reticle == null) {
        return;
      }

      if (pointer == null) {
        return;
      }

      Vector3 localPos = pointer.InverseTransformPoint(hitPos);
      Quaternion rot = Quaternion.identity;
      if (isHittingTarget) {
        rot = Quaternion.FromToRotation(Vector3.forward, localPos);
      }

      controllerVisual.localRotation = rot;
      gunVisual.localRotation = rot;


      Vector3 reticlePos;
      if (isHittingTarget) {
        reticlePos = hitPos;
      } else {
        Ray ray = new Ray(pointer.position, pointer.forward);
        reticlePos = ray.GetPoint(defaultReticleDistance);
      }

      Camera cam = Camera.main;

      reticle.transform.position = reticlePos;
      reticle.LookAt(cam.transform);
      reticle.forward = -reticle.forward;

      float reticleDistanceFromCamera = (reticle.position - cam.transform.position).magnitude;
      float scale = 0.1f * reticleDistanceFromCamera;
      reticle.localScale = new Vector3(scale, scale, scale);
    }
  }
}