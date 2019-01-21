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
using DaydreamElements.Tunneling;

namespace DaydreamElements.Chase {
  /// The chase cam trails behind a chase target object.
  public class ChaseCam : MonoBehaviour  {
    [Tooltip("Character that chase cam will follow")]
    public BasePositionedCharacter chaseCharacter;

    [Tooltip("Trigger to activate left rotation")]
    public BaseActionTrigger rotateLeftTrigger;

    [Tooltip("Trigger to activate right rotation")]
    public BaseActionTrigger rotateRightTrigger;

    /// Offset from the chase target.
    [Tooltip("Offset from the chased target")]
    public Vector3 trailingOffset = new Vector3(0, 2, 3);

    /// Bounding radius around chase target where no movement will occur.
    [Tooltip("Radius around chase target where no movement will occur")]
    [Range(0,20)]
    public float boundingRadius = 2.0f;

    /// Speed that we follow the target at.
    [Tooltip("Speed multiplier for camera movement")]
    public float movementSpeed = .06f;

    /// Speed that we rotate at.
    [Tooltip("Speed multiplier applied to rotations")]
    public float rotationSpeed = 45.0f;

    /// Gizmos drawn in the scene view for debugging issues.
    [Tooltip("Gizmos drawn in the scene view for debugging issues")]
    public bool showDebugGizmos;

    /// Flag to enable or disable horizontal tunnelling.
    [Tooltip("Flag to enable or disable horizontal tunneling")]
    public bool allowHorizontalTunneling = true;

    /// Precent of horizontal movement to enable tunneling.
    [Tooltip("Precent of horizontal movement to enable tunneling")]
    [Range(0, 1)]
    public float horizontalTunnelingThreshold = .4f;

    [Tooltip("The vignette that is controlled by this locomotion.")]
    [SerializeField]
    private BaseVignetteController vignetteController;

    /// Helper for getting the transform of the character.
    private Transform ChaseTargetTransform {
      get {
        return registeredCharacter != null
          ? registeredCharacter.transform : null;
      }
    }

    /// True if we're currently rotating.
    private bool isRotating;

    /// Direction to rotate -1=left, 1=right
    private int rotationDirection = 1;

    /// Position of chase cam in the last frame.
    private Vector3 cameraPreviousPosition = Vector3.zero;

    private Vector3 characterDestination;

    /// Private reference so we can unregister if character changes.
    private BasePositionedCharacter registeredCharacter;

    // Distance we'll consider the camera in position at.
    private float destinationThreshold = .01f;

    private bool moveToDestination;

    void Start() {
      cameraPreviousPosition = transform.position;

      // Start camera at the correct offset from chase target.
      if (chaseCharacter == null) {
        Debug.LogError("Chase camera needs target character to follow");
        return;
      }

      HandleCharacterRegistration();

      transform.position = CameraOffsetPosition();
      characterDestination = ChaseTargetTransform.position;
    }

    void OnDisable() {
      if (vignetteController) {
        vignetteController.HideVignette();
      }
    }

    void Update() {
      if (ChaseTargetTransform == null) {
        return;
      }

      HandleCharacterRegistration();
      HandlePositionChange();

      bool isDiagonal = HandleDiagnonalTracking();
      bool isRotating = HandleRotationChange();
      bool shouldTunnel = isDiagonal || isRotating;

      if (shouldTunnel && vignetteController != null) {
        vignetteController.ShowVignette();
      } else if (vignetteController != null) {
        vignetteController.HideVignette();
      }
    }

    private void OnDrawGizmos() {
      if (showDebugGizmos == false) {
        return;
      }

      // Draw the bounding circle character move in without camera movement.
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(BoundsCenterInFrontOfCamera(), boundingRadius);

      // Draw the target position of the chase camera.
      Gizmos.color = Color.green;
      Gizmos.DrawSphere(CameraOffsetPosition(), .25f);

      // Show the destination the character is moving towards.
      if (moveToDestination) {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(characterDestination, .1f);
      }
    }

    /// Return true if we're moving diagonal.
    private bool HandleDiagnonalTracking() {
      // Get a normalized vector from last frame to current frame to check x delta.
      Vector3 moveDelta = Vector3.zero - transform.InverseTransformPoint(cameraPreviousPosition);
      cameraPreviousPosition = transform.position;
      moveDelta.Normalize();
      return Mathf.Abs(moveDelta.x) >= horizontalTunnelingThreshold;
    }

    private void HandleCharacterRegistration() {
      // Check if character has changed, and register for callbacks.
      if (chaseCharacter != registeredCharacter) {
        if (registeredCharacter) {
          registeredCharacter.positionDelegate -= CharacterDestinationChanged;
        }

        if (chaseCharacter != null) {
          registeredCharacter = chaseCharacter;
          registeredCharacter.positionDelegate += CharacterDestinationChanged;
        }
      }
    }

    // True if both target and chase cam are in final positions.
    private bool IsAtFinalDestination() {
        return IsTargetAtDestination() && IsChaseCameraAtDistination();
    }

    // Handle camera position changes based on chase target.
    private void HandlePositionChange() {
      // Check if character moved outside the view bounds.
      if (moveToDestination == false &&
          IsPositionOutsideBounds(ChaseTargetTransform.position)) {
          moveToDestination = true;
      }

      // Don't move camera.
      if (moveToDestination == false) {
        return;
      }

      // Animate camera tracking the chase target.
      MoveCameraIntoPosition();

      // Stop tracking, and enable bounds once destination is reached.
      if (IsAtFinalDestination()) {
        moveToDestination = false;
      }
    }

    public bool IsTargetAtDestination() {
      return !chaseCharacter.IsMoving;
    }

    public bool IsChaseCameraAtDistination() {
      return IsNearby(CameraOffsetPosition(),
        transform.position, destinationThreshold);
    }

    public bool IsNearby(Vector3 positionA, Vector3 positionB, float threshold) {
      float distance = (positionB - positionA).magnitude;
      return  distance <= threshold;
    }

    /// Returns true if we're rotating, false otherwise.
    private bool HandleRotationChange() {
      CheckForPressingRotate();

      if (isRotating == false) {
        return false;
      }

      transform.RotateAround(
        ChaseTargetTransform.position,
        Vector3.up,
        rotationSpeed * rotationDirection * Time.deltaTime);

      return true;
    }

    private void CheckForPressingRotate() {
      if (rotateLeftTrigger == null || rotateRightTrigger == null) {
        Debug.LogError("Can't check for rotation without triggers");
        return;
      }

      bool leftRotate = rotateLeftTrigger.TriggerActive();
      bool rightRotate = rotateRightTrigger.TriggerActive();

      isRotating = leftRotate || rightRotate;

      if (!isRotating) {
        return;
      }

      rotationDirection = leftRotate ? -1 : 1;
    }

    private void MoveCameraIntoPosition() {
      Vector3 dst = CameraOffsetPosition();

      transform.position = Vector3.Lerp(
        transform.position,
        dst,
        movementSpeed * Time.deltaTime);
    }

    /// Returns the ideal camera position based on the chase target location.
    private Vector3 CameraOffsetPosition() {
      Vector3 targetLocal = transform.InverseTransformPoint(ChaseTargetTransform.position);

      Vector3 dstLocalPosition = new Vector3(
        targetLocal.x,
        trailingOffset.y + targetLocal.y,
        targetLocal.z - trailingOffset.z);

      return transform.TransformPoint(dstLocalPosition);
    }

    /// Origin point of our bounding circle.
    private Vector3 TargetBoundsOrigin() {
      return characterDestination;
    }

    private Vector3 BoundsCenterInFrontOfCamera() {
      Vector3 flatPlayerDirection = transform.forward;
      flatPlayerDirection.y = 0;
      flatPlayerDirection.Normalize();

      Vector3 boxPosition = transform.position - new Vector3(0, trailingOffset.y, 0)
        + (flatPlayerDirection * trailingOffset.z);

      return boxPosition;
    }

    /// True if target has moved outside the bounding radius.
    private bool IsPositionOutsideBounds(Vector3 aPosition) {
      Vector3 diff = aPosition - BoundsCenterInFrontOfCamera();
      return diff.magnitude > boundingRadius;
    }

    // Character delegate callback.
    private void CharacterDestinationChanged(
        BasePositionedCharacter character, Vector3 destination) {
      if (character != registeredCharacter) {
        Debug.Log("Received new destination from unknown delegate");
        return;
      }

      characterDestination = destination;

      if (moveToDestination == false) {
        moveToDestination = IsPositionOutsideBounds(characterDestination);
      }
    }
  }
}
