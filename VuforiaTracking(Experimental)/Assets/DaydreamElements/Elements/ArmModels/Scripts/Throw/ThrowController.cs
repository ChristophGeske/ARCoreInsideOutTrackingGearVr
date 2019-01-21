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

  /// Used to throw a held object.
  public class ThrowController : MonoBehaviour {
    [Tooltip("Factor for how much force to throw the ball with.")]
    public float forceMultiplier = 4.0f;

    [Range(0.0f, 1.0f)]
    [Tooltip("How much the throw direction should be weighted towards the forward direction.")]
    public float forwardWeight = 0.4f;

    [Range(0.0f, 1.0f)]
    [Tooltip("How much the throw direction should be weighted toward the user's gaze.")]
    public float gazeWeight = 0.3f;

    [Tooltip("Arm Model used for smoothly switching between different arm models.")]
    public TransitionArmModel transitionArmModel;

    [Tooltip("Arm Model Transitioned to when holding a throwable.")]
    public GvrArmModel throwArmModel;

    [Tooltip("The object to throw.")]
    [SerializeField]
    private Throwable heldThrowable;

    private GvrArmModel initialArmModel;
    private Vector3 throwableVelocity;
    private Vector3 lastThrowablePosition;
    private bool hasBegunThrowGesture;

    private const float VELOCITY_SMOOTHING_FACTOR = 0.2f;

    public Throwable HeldThrowable {
      get {
        return heldThrowable;
      }
      set {
        if (heldThrowable == value) {
          return;
        }

        // Currently held throwable hasn't been thrown yet, so it is left behind.
        if (heldThrowable) {
          heldThrowable.transform.SetParent(null, true);
          heldThrowable = null;
        }

        heldThrowable = value;
        SetupForHeldThrowable();
      }
    }

    void Awake() {
      if (transitionArmModel != null) {
        initialArmModel = transitionArmModel.CurrentArmModel;
      }

      SetupForHeldThrowable();
    }

    void Update() {
      if (HeldThrowable == null) {
        return;
      }

      UpdateBallVelocity();
      TryBeginThrowGesture();

      if (HasCompletedThrowGesture()) {
        Throw();
      }
    }

    private void SetupForHeldThrowable() {
      if (HeldThrowable != null) {
        // Make the new throwable a child of the ThrowController.
        HeldThrowable.transform.SetParent(transform, false);
        HeldThrowable.Hold();
        HeldThrowable.transform.localPosition = Vector3.zero;
        HeldThrowable.transform.localRotation = Quaternion.identity;

        // Reset velocity related variables.
        throwableVelocity = Vector3.zero;
        lastThrowablePosition = HeldThrowable.transform.position;
        hasBegunThrowGesture = false;
        TryBeginThrowGesture();
      } else {
        throwableVelocity = Vector3.zero;
        lastThrowablePosition = Vector3.zero;
        EndThrowGesture();
      }
    }

    private void TryBeginThrowGesture() {
      bool canBeginThrowGesture = !hasBegunThrowGesture
        && GvrControllerInput.ClickButton
        && HeldThrowable != null;

      if (canBeginThrowGesture) {
        hasBegunThrowGesture = true;

        // Transition to the throw arm model, as the throw gesture is now starting.
        if (transitionArmModel != null && throwArmModel != null) {
          transitionArmModel.TransitionToArmModel(throwArmModel);
        }
      }
    }

    private bool HasCompletedThrowGesture() {
      return !GvrControllerInput.ClickButton && hasBegunThrowGesture;
    }

    private void EndThrowGesture() {
      if (!hasBegunThrowGesture) {
        return;
      }

      // The throwable has been released, so transition back to the default arm model.
      if (transitionArmModel != null && initialArmModel != null) {
        transitionArmModel.TransitionToArmModel(initialArmModel);
      }
    }

    private void UpdateBallVelocity() {
      // Calculate the velocity over the previous frame.
      Vector3 throwablePosition = HeldThrowable.transform.position;
      Vector3 diff = throwablePosition - lastThrowablePosition;
      Vector3 frameVelocity = diff / Time.deltaTime;

      // Apply exponential moving average to the velocity.
      throwableVelocity = (frameVelocity * VELOCITY_SMOOTHING_FACTOR) +
      (throwableVelocity * (1.0f - VELOCITY_SMOOTHING_FACTOR));

      // Record the new throwable position.
      lastThrowablePosition = throwablePosition;
    }

    private void Throw() {
      if (HeldThrowable == null) {
        return;
      }

      // Determine the force strength and force direction based on the force multiplier and the velocity.
      float forceStrength = throwableVelocity.magnitude * forceMultiplier;
      Vector3 forceDirection = throwableVelocity.normalized;

      // Weight the forceDirection towards the forward direction of the throw controller.
      float angle = Vector3.Angle(forceDirection, transform.forward);
      float radiansChanged = forwardWeight * angle * Mathf.Deg2Rad;
      forceDirection = Vector3.RotateTowards(forceDirection, transform.forward, radiansChanged, float.MaxValue);

      // Weight the forceDirection towards the forward direction of the camera.
      if (Camera.main != null) {
        Vector3 camForward = Camera.main.transform.forward;
        angle = Vector3.Angle(forceDirection, camForward);
        radiansChanged = gazeWeight * angle * Mathf.Deg2Rad;
        forceDirection = Vector3.RotateTowards(forceDirection, camForward, radiansChanged, float.MaxValue);
      }

      // Throw!
      HeldThrowable.Throw(forceDirection * forceStrength);
      HeldThrowable = null;
    }

#if UNITY_EDITOR
    void OnValidate() {
      if (Application.isPlaying && isActiveAndEnabled) {
        SetupForHeldThrowable();
      }
    }
#endif
  }
}