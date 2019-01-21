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

/// Trigger that fires if user is touching the side of the controller.
public class TouchpadSideTouchDownTrigger : BaseActionTrigger {

  /// Side of the controller.
  public enum Side {
    Left,
    Right
  }

  /// Side of the controller to trigger on.
  [Tooltip("Side of the controller to trigger on")]
  public Side side;

  private float sideWidth = .2f;

  public override bool TriggerActive() {
    if (GvrControllerInput.IsTouching == false) {
      return false;
    }

    float xPos = GvrControllerInput.TouchPos.x;

    // Check for left side active.
    if (xPos <= sideWidth) {
      return side == Side.Left;
    }

    // Check for right side active.
    if (xPos >= (1 - sideWidth)) {
      return side == Side.Right;
    }

    return false;
  }
}
