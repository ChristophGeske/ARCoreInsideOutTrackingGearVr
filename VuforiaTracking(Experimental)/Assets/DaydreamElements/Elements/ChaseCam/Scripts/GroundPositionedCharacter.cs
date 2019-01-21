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

namespace DaydreamElements.Chase {
  /// Positioned character that stands on the ground with gravity,
  /// and has a simple move and rotate animation using CharacterController.
  [RequireComponent(typeof(CharacterController))]
  public class GroundPositionedCharacter : BasePositionedCharacter {
    /// Speed of forward movement.
    [Tooltip("Forward movement speed")]
    public float movementSpeed = 4.0f;

    /// Speed of rotations.
    [Tooltip("Rotational movement speed")]
    public float rotationSpeed = 3.0f;

    /// Max character height
    [Tooltip("Distance to consider target reached at. This value needs to be at" +
             " least the distance of your characters origin to the ground. If" +
             " your player isn't reaching it's destination, try raising this value.")]
    public float reachedTargetThreshold = .001f;

    /// Force to pull player down with character controller.
    private float gravity = 9.8f;

    /// Character controller.
    protected CharacterController character;

    protected virtual void Start() {
      character = GetComponent<CharacterController>();
      if (character == null) {
        Debug.LogError("Positioned character requires a character controller");
        return;
      }
    }

    /// Override this method to create custom player movement.
    /// This method will be called every frame until the character
    /// reaches it's final destination.
    protected override void MoveCharacter() {
      UpdatePosition();
      UpdateRotation();
    }

    /// Rotate character towards the target position.
    protected virtual void  UpdateRotation() {
      Vector3 targetLocal = character.transform.InverseTransformPoint(TargetPosition);
      float angle = Mathf.Atan2(targetLocal.x, targetLocal.z) * Mathf.Rad2Deg;

      Quaternion targetRotation = character.transform.rotation
        * Quaternion.Euler(0, angle, 0);

      character.transform.rotation = Quaternion.Lerp(
        character.transform.rotation,
        targetRotation,
        Time.deltaTime * rotationSpeed);
    }

    /// Move the character towards the target position.
    protected virtual void UpdatePosition() {
      Vector3 targetFlat = new Vector3(TargetPosition.x, 0, TargetPosition.z);
      Vector3 positionFlat = new Vector3(transform.position.x, 0, transform.position.z);

      float remaningDistance = (targetFlat - positionFlat).magnitude;
      Vector3 moveDirection = (targetFlat - positionFlat).normalized * movementSpeed * Time.deltaTime;

      if (moveDirection.magnitude > remaningDistance) {
        moveDirection = targetPosition - transform.position;
      }

      moveDirection.y -= gravity * Time.deltaTime;

      character.Move(moveDirection);
    }

    /// Override for custom logic on what's considered reaching the target.
    protected override bool DidCharacterReachTargetPosition() {
      Vector3 targetFlat = new Vector3(TargetPosition.x, 0, TargetPosition.z);
      Vector3 positionFlat = new Vector3(transform.position.x, 0, transform.position.z);

      float distance = (targetFlat - positionFlat).magnitude;
      return distance <= reachedTargetThreshold;
    }
  }
}
