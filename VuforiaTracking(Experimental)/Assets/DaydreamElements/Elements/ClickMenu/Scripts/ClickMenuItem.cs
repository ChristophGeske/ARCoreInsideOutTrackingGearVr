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

namespace DaydreamElements.ClickMenu {

  /// Data structure to hold information about each menu item.
  [CreateAssetMenu(
    fileName = "ClickMenuItem",
    menuName = "DaydreamElements/ClickMenu/Item",
    order = 1000)]
  /// Serializes a section of the menu.
  public class ClickMenuItem : ScriptableObject {
    [Tooltip("Assign a unique id for this item.")]
    public int id;
    [Tooltip("Foreground sprite")]
    public Sprite icon;
    [Tooltip("Optional background sprite")]
    public Sprite background;
    [Tooltip("Text shown on hover")]
    public string toolTip;
    public bool closeAfterSelected = true;
  }
}
