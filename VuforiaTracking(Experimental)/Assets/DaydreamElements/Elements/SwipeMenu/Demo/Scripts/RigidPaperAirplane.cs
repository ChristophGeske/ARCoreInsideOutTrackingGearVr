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

  [RequireComponent(typeof(Rigidbody))]
  [RequireComponent(typeof(GvrAudioSource))]
  public class RigidPaperAirplane : MonoBehaviour {
    private const float killHeight = -10.0f;

    private Rigidbody rigidBody;
    private GvrAudioSource audioSource;

    public bool isSpinning = false;
    public ColorUtil.Type type;

    void Start() {
      audioSource = GetComponent<GvrAudioSource>();
      rigidBody = GetComponent<Rigidbody>();
      rigidBody.maxAngularVelocity = 0.0f;
      rigidBody.freezeRotation = true;
      ColorUtil.Colorize(type, gameObject);
    }

    void Update () {
      // Remove game object when it falls under the floor.
      if (transform.position.y < killHeight) {
        Destroy(gameObject);
      }

      // Always point in the direction of motion.
      if (!isSpinning) {
        Vector3 forward = transform.rotation * Vector3.forward;
        Quaternion rotation = Quaternion.FromToRotation(forward, rigidBody.velocity);
        transform.rotation = rotation * transform.rotation;
      }
    }

    public void Spin() {
      if (!isSpinning) {
        audioSource.Play();
        rigidBody.freezeRotation = false;
        rigidBody.maxAngularVelocity = 8.0f;
        rigidBody.angularVelocity = Random.onUnitSphere * 8.0f;
        isSpinning = true;
      }
    }
  }
}
