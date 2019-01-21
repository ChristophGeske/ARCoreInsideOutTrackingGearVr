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

namespace DaydreamElements.Common {

  using UnityEngine;
  using System.Collections;

  public class EonSpinner : MonoBehaviour {
    
    public float acceleration = 10f;
    public float targetSpeed = 40f;
    private float initTargetSpeed;
    private float currentSpeed = 0f;
    private float rotZ = 0f;
    private Vector3 lastPos;
    private Vector3 posDelta;

    void Start() {
      initTargetSpeed = targetSpeed;
      lastPos = transform.position;
    }

    void Update() {
      float dt = Time.deltaTime;

      posDelta = transform.position - lastPos;
      targetSpeed += posDelta.magnitude * acceleration;

      currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, dt * acceleration);
      rotZ += dt * currentSpeed;
      rotZ = Mathf.Repeat(rotZ,360f);
      transform.localRotation = Quaternion.AngleAxis(rotZ, Vector3.forward);

      targetSpeed = Mathf.Lerp(targetSpeed, initTargetSpeed, dt);
      lastPos = transform.position;
    }
  }
}
