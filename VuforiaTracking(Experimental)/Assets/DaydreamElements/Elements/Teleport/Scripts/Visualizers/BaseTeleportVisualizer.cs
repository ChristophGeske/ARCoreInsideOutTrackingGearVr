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
using System.Collections;

namespace DaydreamElements.Teleport {
  /// Base class for a teleport visualization of current selection.
  public abstract class BaseTeleportVisualizer : MonoBehaviour {
    // Behavior during teleport.
    public abstract void OnTeleport();

    // Start teleport selection.
    public abstract void StartSelection(Transform controller);

    /// End teleport selection.
    public abstract void EndSelection();

    /// Update the current visualized selection.
    public abstract void UpdateSelection(
      Transform controllerTransform, BaseTeleportDetector.Result selectionResult);
  }
}
