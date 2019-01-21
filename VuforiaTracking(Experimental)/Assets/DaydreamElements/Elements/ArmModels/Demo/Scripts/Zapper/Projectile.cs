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

namespace DaydreamElements.ArmModels {

  /// Projectile fired by the Zapper.
  public class Projectile : MonoBehaviour {
    [Tooltip("Speed of the projectile in meters per second.")]
    public float speed = 10.0f;

    [Tooltip("Time that can elapse before the projectile is automatically destroyed.")]
    public float lifeTime = 5.0f;

    [Tooltip("Time that can elapse before the projectile is automatically destroyed.")]
    public float destroyDelay = 0.5f;

    [Tooltip("Prefab to instantiate when the proejctile hits a collider.")]
    public GameObject splatPrefab;

    [Tooltip("LayerMask to determine what objects hits will be detected with.")]
    public LayerMask layerMask;

    [Tooltip("Radius of sphereCast used to detect if the projectile hit something.")]
    public float hitRadius = 0.01f;

    [Tooltip("Amount of force applied to the hit rigidbody when a hit is detected.")]
    public float hitForce = 100.0f;

    private float elapsedLife;
    private bool destroying;
    private bool frameDelay;

    void Update() {
      if (destroying) {
        return;
      }

      if (frameDelay == false) {
        frameDelay = true;
        return;
      }

      float moveDistance = speed * Time.deltaTime;
      Vector3 oldPosition = transform.position;
      Vector3 direction = transform.forward;
      Vector3 newPosition = oldPosition + (direction * moveDistance);
      transform.position = newPosition;

      DetectHit(oldPosition, newPosition);

      elapsedLife += Time.deltaTime;
      if (elapsedLife >= lifeTime && !destroying) {
        Destroy(gameObject);
      }
    }

    private void DetectHit(Vector3 oldPosition, Vector3 newPosition) {
      if (destroying) {
        return;
      }

      Vector3 diff = newPosition - oldPosition;
      Vector3 direction = diff.normalized;
      float maxDistance = diff.magnitude;
      Ray ray = new Ray(oldPosition, direction);
      RaycastHit hitInfo;
      if (Physics.SphereCast(ray, hitRadius, out hitInfo, maxDistance, layerMask.value)) {
        // Set the position to the hit position so the proejctile doesn't go through the thing that
        // was hit.
        Vector3 hitCenter = ray.GetPoint(hitInfo.distance);
        transform.position = hitCenter;

        // If a rigidbody was hit, apply a force to it.
        Rigidbody hitRigidbody = hitInfo.collider.GetComponent<Rigidbody>();
        if (hitRigidbody != null) {
          hitRigidbody.AddForceAtPosition(-hitInfo.normal * hitForce, hitInfo.point, ForceMode.Force);
        }

        if (splatPrefab != null) {
          GameObject splat = GameObject.Instantiate(splatPrefab);
          splat.transform.position = transform.position;
        }

        StartCoroutine(DestroyAferDelay());
      }
    }

    private IEnumerator DestroyAferDelay() {
      destroying = true;
      yield return new WaitForSeconds(destroyDelay);
      Destroy(gameObject);
    }
  }
}
