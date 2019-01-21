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
using UnityEditor;
using System.Collections;

/// A custom editor window used to set editor preferences for GoogleVR.
/// Editor preferences are editor specific options that help build and test
/// applications from within the Unity Editor.
class GvrEditorSettings : EditorWindow {
  void OnGUI () {
    // Label for Controller Emulator settings
    EditorGUILayout.LabelField("Controller Emulator", EditorStyles.boldLabel);

    // Option to control Handedness
    GvrSettings.UserPrefsHandedness oldHandedness = GvrSettings.Handedness;
    GvrSettings.Handedness = (GvrSettings.UserPrefsHandedness) EditorGUILayout.EnumPopup("Handedness", oldHandedness);
    if (oldHandedness != GvrSettings.Handedness) {
      UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
  }
}