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

namespace DaydreamElements.Chase {
  /// Animates a fox character around the scene towards a target position.
  [RequireComponent(typeof(Animator))]
  public class FoxPositionedCharacter : GroundPositionedCharacter {
    /// Animation controller for the fox.
    private Animator foxAnimator;

    public bool isRunning = false;

    protected override void Start() {
      base.Start();
      foxAnimator = GetComponent<Animator>();
    }

    private bool IsReady() {
      if (this.character == null) {
        Debug.LogError("Missing fox character controller");
        return false;
      }

      if (foxAnimator == null) {
        Debug.Log("Missing fox animator");
        return false;
      }

      return true;
    }

    /// Start our run animation when we receive a new destination.
    public override void SetTargetPosition(Vector3 worldPosition) {
      // Always invoke the base implentation.
      base.SetTargetPosition(worldPosition);
      ShowRunAnimation(true);
    }

    /// Stop our run animation once we reach destination.
    protected override void Update() {
      base.Update();

      // Stop our run animation once our destination os reached.
      if (IsMoving == false) {
        ShowRunAnimation(false);
      }
    }

    /// Start or stop the run animation.
    private void ShowRunAnimation(bool shouldRun) {
      foxAnimator.SetBool("walking", shouldRun);
      isRunning = shouldRun;
    }

    void OnTriggerEnter(Collider c) {
      if (c.GetComponentInChildren<CollectibleCoin>() == null) {
        return;
      }

      // Let's make sure the fox is facing this coin before jumping.
      const float maximumFacingAngle = 45.0f;
      float angleToCoin = Vector3.Angle(
        transform.forward, c.transform.position - transform.position);

      if (Mathf.Abs(angleToCoin) > maximumFacingAngle) {
        return;
      }

      Debug.Log("Jumping fox... trigger entered: " + c.gameObject.name);
      foxAnimator.SetTrigger("pickup");
    }
  }
}
