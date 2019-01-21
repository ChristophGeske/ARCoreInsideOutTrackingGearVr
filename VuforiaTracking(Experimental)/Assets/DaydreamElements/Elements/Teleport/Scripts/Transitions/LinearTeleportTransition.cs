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
using System.Collections;

namespace DaydreamElements.Teleport {
  /// Animate player teleport transition with a linear animation.
  public class LinearTeleportTransition : BaseTeleportTransition {
    /// Speed of Lerp transition.
    [Tooltip("Speed of transition")]
    public float transitionSpeed = 10.0f;

    /// True if transition is in progress.
    public override bool IsTransitioning { get { return isTransitioning; } }

    private bool isTransitioning;
    private Vector3 targetPosition;
    private Transform player;

    void Update () {
      if (isTransitioning == false) {
        return;
      }

      // Animate player to position with linear steps
      player.position = Vector3.MoveTowards(
        player.position,
        targetPosition,
        transitionSpeed * Time.deltaTime);

      // Check if transition is finished.
      if (player.transform.position.Equals(targetPosition)) {
        isTransitioning = false;
      }
    }

    public override void StartTransition(
        Transform playerTransform, Transform controller, Vector3 target) {
      player = playerTransform;
      targetPosition = target;
      isTransitioning = true;
    }

    public override void CancelTransition() {
      isTransitioning = false;
    }
  }
}

