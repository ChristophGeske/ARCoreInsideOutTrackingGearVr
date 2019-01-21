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

  /// Returns a ball to the spawner when it collides with this object.
  public class BallReturner : MonoBehaviour {
    [Tooltip("The spawner to return the ball to.")]
    public BallSpawner ballSpawner;

    void OnTriggerEnter(Collider other) {
      if (ballSpawner == null) {
        return;
      }

      Ball ball = other.GetComponent<Ball>();

      if (ball == null) {
        return;
      }

      ballSpawner.ReturnBall(ball);
    }
  }
}
