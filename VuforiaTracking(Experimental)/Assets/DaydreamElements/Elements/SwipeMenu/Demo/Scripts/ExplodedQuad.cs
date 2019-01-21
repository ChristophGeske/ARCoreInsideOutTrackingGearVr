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

namespace DaydreamElements.SwipeMenu {

  /// A piece of the exploded balloon.
  [RequireComponent(typeof(Rigidbody))]
  public class ExplodedQuad : MonoBehaviour {
    private const float LIFE_TIME = 2.0f;
    private const float GRAVITY_FORCE = 1.0f;

    private Rigidbody rigidBody;
    private float startTime;

    void Awake() {
      rigidBody = GetComponent<Rigidbody>();
      startTime = Time.realtimeSinceStartup;
    }

    void Update() {
      rigidBody.AddForce(Vector3.down * GRAVITY_FORCE);
      float t = (Time.realtimeSinceStartup - startTime) / LIFE_TIME;
      transform.localScale *= 0.95f;
      if (t >= 1.0f) {
        Destroy(gameObject);
      }
    }
  }
}
