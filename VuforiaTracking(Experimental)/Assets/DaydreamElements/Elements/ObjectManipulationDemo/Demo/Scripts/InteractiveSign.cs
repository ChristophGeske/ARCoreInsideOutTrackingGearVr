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

  using UnityEngine;
  using System.Collections;

  // A script that teleports an interactive object to
  // its initial position and rotation on release, and
  // allows the object to maintain facing to another game object.
  public class InteractiveSign : MonoBehaviour {

    [Tooltip("The interactive object component on this game object.")]
    public BaseInteractiveObject interactiveObject;

    [Tooltip("The transform for the player or main camera.")]
    public Transform player;

    [Tooltip("The rigidbody attached to this object.")]
    public Rigidbody rigidbodyCmp;

    [Tooltip("Sound played when the object is selected.")]
    public GvrAudioSource selectSound;

    [Tooltip("A random sound from this list is played on collision.")]
    public GvrAudioSource[] bounceSounds;

    private Vector3 initPosition;
    private Quaternion initRotation;
    private Quaternion targetRotation;

    private bool isSelected;

    private BaseInteractiveObject.ObjectState state;
    private BaseInteractiveObject.ObjectState stateLastFrame;

    private float lastBounceTime;

    private const float BOUNCE_SOUND_DELAY = 0.5f;
    private const float MIN_BOUNCE_VELOCITY = 0.5f;

    void OnValidate() {
      if (interactiveObject == null) {
        interactiveObject = gameObject.GetComponent<BaseInteractiveObject>();
      }
      if (rigidbodyCmp == null) {
        rigidbodyCmp = gameObject.GetComponent<Rigidbody>();
      }
    }

    void Awake() {
      isSelected = false;
    }

    void Start() {
      initPosition = rigidbodyCmp.position;
      initRotation = rigidbodyCmp.rotation;
    }

    void Update() {
      state = interactiveObject.State;

      if (state == BaseInteractiveObject.ObjectState.Selected) {
        isSelected = true;
      } else {
        rigidbodyCmp.position = initPosition;
        rigidbodyCmp.rotation = initRotation;
        isSelected = false;
      }

      if (isSelected && state != stateLastFrame) {
        if (selectSound != null) {
          selectSound.Play();
        }
      }

      stateLastFrame = interactiveObject.State;
    }

    void FixedUpdate() {
      if (state == BaseInteractiveObject.ObjectState.Selected) {
        Vector3 relativePos = player.position - rigidbodyCmp.position;
        Quaternion playerFacing = Quaternion.LookRotation(relativePos);
        targetRotation = rigidbodyCmp.rotation;
        targetRotation = Quaternion.Slerp(targetRotation, playerFacing, Time.deltaTime);
        rigidbodyCmp.MoveRotation(targetRotation);
      }
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
