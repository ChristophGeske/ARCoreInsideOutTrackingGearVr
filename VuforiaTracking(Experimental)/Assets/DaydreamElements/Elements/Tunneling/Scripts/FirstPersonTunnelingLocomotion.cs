// Copyright 2016 Google Inc. All rights reserved.
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
using UnityEngine.Assertions;
using System.Collections;

namespace DaydreamElements.Tunneling {

  /// Provides first person camera controls for use with a TunnelingVignette.
  /// Use this to reduce motion sickness.
  ///
  /// If there is a CharacterController this object, the CharacterController will be used
  /// for locomotion. Otherwise, the transform will be modified directly.
  public class FirstPersonTunnelingLocomotion : MonoBehaviour {
    [Tooltip("The max speed to translate the camera (meters per second).")]
    public float maxSpeed = 7.0f;

    [Tooltip("The max angular velocity to rotate the camera (degrees per second).")]
    [Range(0.0f, 180.0f)]
    public float maxAngularSpeed = 30.0f;

    [Tooltip("Smoothing Factor applied to the touch input")]
    public float smoothingFactor = 5.0f;

    [Tooltip("Controls how far the user must touch on the touchpad to be moving at min speed.")]
    [Range(0.0f, 1.0f)]
    public float minInputThreshold = 0.3f;

    [Tooltip("Controls how far the user must touch on the touchpad to be moving at max speed.")]
    [Range(0.25f, 1.0f)]
    public float maxInputThreshold = 0.65f;

    /// This is useful to prevent accidental movement when the user was intending to
    /// click the touchpad instead of move. Also makes it possible for the user to rest their thumb
    /// on the touchpad without moving.
    [Tooltip("Determines if the transform will only be moved after the user has swiped.")]
    public bool onlyMoveAfterSwiping = false;

    [Tooltip("The vignette that is controlled by this locomotion.")]
    [SerializeField]
    private BaseVignetteController vignetteController;

    private bool isMoving = false;
    private CharacterController characterController;
    private Vector2 smoothTouch = Vector2.zero;
    private Vector2? initTouch = null;

    private const float SLOP_VERTICAL = 0.165f;
    private const float SLOP_HORIZONTAL = 0.15f;

    void Awake() {
      // Used for movement if it exists. Otherwise, this script modifies the transform directly.
      characterController = GetComponent<CharacterController>();
      Assert.IsTrue(minInputThreshold < maxInputThreshold);
    }

    void OnDisable() {
      StopMoving();
    }

    protected virtual void Update() {
      if (GvrControllerInput.TouchDown) {
        initTouch = GvrControllerInput.TouchPos;
      } else if (CanStartMoving()) {
        isMoving = true;
        smoothTouch = Vector2.zero;
        vignetteController.ShowVignette();
      } else if (GvrControllerInput.TouchUp) {
        StopMoving();
      }

      if (isMoving) {
        Move();
      }
    }

    protected virtual void Move() {
      Vector2 touchPos = GvrControllerInput.TouchPosCentered;

      bool isTouchTranslating = IsTouchTranslating(touchPos);
      bool isTouchRotating = IsTouchRotating(touchPos);
      UpdateVignetteFOV(isTouchTranslating, isTouchRotating);

      if (vignetteController.IsVignetteReady) {
        float dt = Time.smoothDeltaTime;

        // Threshold input.
        touchPos.x = ApplyInputThreshold(touchPos.x);
        touchPos.y = ApplyInputThreshold(touchPos.y);

        // Square the input, and make sure not to lose the sign. This applies a nice falloff.
        touchPos.x = SquareValue(touchPos.x);
        touchPos.y = SquareValue(touchPos.y);

        // Exponential smoothing on the touch.
        smoothTouch.x = ApplyExponentialSmoothing(smoothTouch.x, touchPos.x, dt);
        smoothTouch.y = ApplyExponentialSmoothing(smoothTouch.y, touchPos.y, dt);

        ApplyRotation(dt);
        ApplyTranslation(dt);
      }
    }

    private void StopMoving() {
      isMoving = false;
      initTouch = null;
      vignetteController.HideVignette();
    }

    private void ApplyRotation(float dt) {
      float angularSpeed = maxAngularSpeed * smoothTouch.x;
      Vector3 angularVelocity = new Vector3(0.0f, angularSpeed, 0.0f);

      Quaternion rotation = transform.rotation * Quaternion.Euler(angularVelocity * dt);
      transform.rotation = rotation;
    }

    private void ApplyTranslation(float dt) {
      float forwardSpeed =  maxSpeed * smoothTouch.y;
      Vector3 velocity = new Vector3(0.0f, 0.0f, forwardSpeed);

      Quaternion cameraRotation = Camera.main.transform.rotation;
      cameraRotation = Quaternion.Euler(new Vector3(0.0f, cameraRotation.eulerAngles.y, 0.0f));
      Vector3 rotatedVelocity = cameraRotation * velocity;

      if (characterController != null) {
        characterController.SimpleMove(rotatedVelocity);
      } else {
        Vector3 position = transform.position;
        position += rotatedVelocity * dt;
        transform.position = position;
      }
    }

    private void UpdateVignetteFOV(bool isTranslating, bool isRotating) {
      if (isRotating) {
        vignetteController.SetFieldOfViewForRotation();
      } else if (isTranslating) {
        vignetteController.SetFieldOfViewForTranslation();
      }
    }

    private bool IsTouchOutsideSlop(Vector2 touch) {
      bool isOutsideSlop = (Mathf.Abs(touch.x - initTouch.Value.x) >= SLOP_HORIZONTAL)
                         || (Mathf.Abs(touch.y - initTouch.Value.y) >= SLOP_VERTICAL);

      return isOutsideSlop;
    }

    private float ApplyInputThreshold(float val) {
      bool isNegative = val < 0.0f;
      val = Mathf.Abs(val);
      val = Mathf.Clamp(val, minInputThreshold, maxInputThreshold);

      if (val != 0.0f) {
        float range = maxInputThreshold - minInputThreshold;
        val -= minInputThreshold;
        val /= range;
      }

      if (isNegative) {
        val = -val;
      }

      return val;
    }

    private float SquareValue(float val) {
      return Mathf.Sign(val) * val * val;
    }

    private float ApplyExponentialSmoothing(float smoothedVal, float val, float dt) {
      float smooth = Mathf.Clamp01(smoothingFactor * dt);
      return (val * smooth) + (smoothedVal * (1.0f - smooth));
    }

    private bool CanStartMoving() {
      if (!GvrControllerInput.IsTouching) {
        return false;
      }

      if (initTouch == null) {
        return false;
      }

      if (isMoving) {
        return false;
      }

      bool isOutsideSlop = IsTouchOutsideSlop(GvrControllerInput.TouchPos);
      bool needsSwipe = onlyMoveAfterSwiping && !isOutsideSlop;


      Vector2 touchPos = GvrControllerInput.TouchPosCentered;
      bool isTouchTranslating = IsTouchTranslating(touchPos);
      bool isTouchRotating = IsTouchRotating(touchPos);
      bool isTouchMoving = isTouchTranslating || isTouchRotating;

      return isTouchMoving && !needsSwipe;
    }

    private bool IsTouchTranslating(Vector2 touchPos) {
      return Mathf.Abs(touchPos.y) > minInputThreshold;
    }

    private bool IsTouchRotating(Vector2 touchPos) {
      return Mathf.Abs(touchPos.x) > minInputThreshold;
    }
  }
}