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
  /// Abstract base class for an character that receives position change requests.
  /// The CharacterPositionPointer is typically used in conjunction with this
  /// class to show a laser pointer that can be used to move a character
  /// around the scene. You'll need to subclass BasePositionedCharacter
  /// and implement the abstract methods for moving the character and
  /// determining when the character has reached its final destination.
  [RequireComponent(typeof(CharacterController))]
  public abstract class BasePositionedCharacter : MonoBehaviour {
    // Prototype for delegate callback.
    public delegate void TargetPositionDelegate(
      BasePositionedCharacter character,
      Vector3 destination);

    /// Delegate for receiving callbacks when target position changes.
    public event TargetPositionDelegate positionDelegate;

    /// Target position that character should move towards.
    protected Vector3 targetPosition;
    public Vector3 TargetPosition {
      get {
        return targetPosition;
      }
    }

    // Flag if player is moving towards target desintation.
    protected bool isMoving;
    public bool IsMoving {
      get {
        return isMoving;
      }
    }

    /// Override this method to move your character to
    /// the target destination. This method will be called
    /// every frame until DidCharacterReachTargetPosition()
    /// returns true.
    protected abstract void MoveCharacter();

    /// Override for custom logic on what's considered reaching the target.
    protected abstract bool DidCharacterReachTargetPosition();

    /// Stop the player from moving. If you override this method, you MUST
    /// call the base class implementation.
    public virtual void StopMoving() {
      targetPosition = transform.position;
      isMoving = false;
    }

    /// Ask the character to move to a new world position. If you override
    /// this method, you MUST call the base class implementation.
    public virtual void SetTargetPosition(Vector3 worldPosition) {
      targetPosition = worldPosition;
      isMoving = true;

      NotifyDelegatePositionChanged();
    }

    /// Notify the chase cam that we're moving to a new target position.
    protected virtual void NotifyDelegatePositionChanged() {
      if (positionDelegate == null) {
        return;
      }

      positionDelegate(this, targetPosition);
    }

    /// Update moves the character into position if we're not at the
    /// target destination. If you override this method, you must call
    /// the base class implementation.
    protected virtual void Update() {
      if (isMoving == false) {
        return;
      }

      MoveCharacter();

      if (DidCharacterReachTargetPosition()) {
        isMoving = false;
        return;
      }
    }
  }
}

