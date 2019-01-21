﻿// Copyright 2017 Google Inc. All rights reserved.
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
  /// Base class for teleport transitions.
  public abstract class BaseTeleportTransition : MonoBehaviour {
    /// True if transition is in progress.
    public abstract bool IsTransitioning { get; }

    /// Start a transition.
    public abstract void StartTransition(
      Transform player, Transform controller, Vector3 target);

    /// Cancel a transition in progress.
    public abstract void CancelTransition();
  }
}
