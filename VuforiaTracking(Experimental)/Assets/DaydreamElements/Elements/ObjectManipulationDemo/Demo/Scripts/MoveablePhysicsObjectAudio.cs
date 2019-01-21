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

namespace DaydreamElements.ObjectManipulation {

  using System;
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  /// Plays sounds when a moveable physics object is interacted with
  /// or collides with surfaces.
  public class MoveablePhysicsObjectAudio : MonoBehaviour {
    public enum ObjectState{ None, Selected, Released }

    [Tooltip("The MoveablePhysicsObject used by this script.")]
    public MoveablePhysicsObject moveablePhysicsObject;

    [Tooltip("The Rigidbody used by this script.")]
    public Rigidbody rigidbodyCmp;

    [Tooltip("Sound played when the object is picked up.")]
    public GvrAudioSource selectSound;

    [Tooltip("A random sound from this list is played on collision.")]
    public GvrAudioSource[] bounceSounds;

    private bool isSelected;

    private float lastBounceTime;

    private BaseInteractiveObject.ObjectState state;
    private BaseInteractiveObject.ObjectState stateLastFrame;

    private const float BOUNCE_SOUND_DELAY = 0.5f;
    private const float MIN_BOUNCE_VELOCITY = 0.5f;

    void OnValidate() {
      if (!moveablePhysicsObject) {
        moveablePhysicsObject = GetComponent<MoveablePhysicsObject>();
      }
      if (!rigidbodyCmp) {
        rigidbodyCmp = moveablePhysicsObject.GetComponent<Rigidbody>();
      }
    }

    void Awake() {
      isSelected = false;
    }

    void Update() {
      state = moveablePhysicsObject.State;

      if (state == BaseInteractiveObject.ObjectState.Selected) {
        isSelected = true;
      } else {
        isSelected = false;
      }

      if (isSelected && state != stateLastFrame) {
        if (selectSound != null) {
          selectSound.Play();
        }
      }

      stateLastFrame = moveablePhysicsObject.State;
    }

    void OnCollisionEnter() {
      if (rigidbodyCmp.isKinematic == false && bounceSounds.Length != 0) {
        if (Time.time > lastBounceTime + BOUNCE_SOUND_DELAY
          && rigidbodyCmp.velocity.magnitude > MIN_BOUNCE_VELOCITY) {
          int soundIndex = UnityEngine.Random.Range(0, bounceSounds.Length);
          bounceSounds[soundIndex].Play();
          lastBounceTime = Time.time;
        }
      }
    }
  }
}