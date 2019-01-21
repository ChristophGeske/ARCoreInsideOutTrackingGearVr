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

/// Base class for creating a custom controller triggers.
/// Classes that are creating generic components can use
/// triggers to deouple their logic from specific buttons
/// on the controller. For example TeleportController uses
/// triggers so that you can customize which button active
/// teleporting and rotating logic.
public abstract class BaseActionTrigger : MonoBehaviour {
  /// Return true if trigger is active.
  abstract public bool TriggerActive();
}
