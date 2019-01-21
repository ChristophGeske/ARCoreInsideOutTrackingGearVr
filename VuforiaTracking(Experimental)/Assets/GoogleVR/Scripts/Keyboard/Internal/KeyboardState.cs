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

public class KeyboardState {

  internal string editorText = string.Empty;
  internal GvrKeyboardInputMode mode = GvrKeyboardInputMode.DEFAULT;
  internal bool isValid = false;
  internal bool isReady = false;
  internal Matrix4x4 worldMatrix;

  public void CopyFrom(KeyboardState other) {
    editorText = other.editorText;
    mode = other.mode;
    isValid = other.isValid;
    isReady = other.isReady;
    worldMatrix = other.worldMatrix;
  }
}
