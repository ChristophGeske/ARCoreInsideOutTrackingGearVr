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

namespace DaydreamElements.ArmModels {

  using System;
  using System.Collections;
  using UnityEngine;

  /// This script is used to throw an object by applying a force to it.
  /// Until it is thrown, the object is kinematic.
  [RequireComponent(typeof(Rigidbody))]
  public class Throwable : MonoBehaviour {

    public event Action OnThrown;

    protected Rigidbody rigidBody;

    public virtual void Throw(Vector3 force) {
      rigidBody.isKinematic = false;
      rigidBody.AddForce(force, ForceMode.Impulse);

      if (OnThrown != null) {
        OnThrown();
      }
    }

    public virtual void Hold() {
      rigidBody.isKinematic = true;
      rigidBody.velocity = Vector3.zero;
      rigidBody.angularVelocity = Vector3.zero;
    }

    protected virtual void Awake() {
      rigidBody = GetComponent<Rigidbody>();
      Hold();
    }
  }
}