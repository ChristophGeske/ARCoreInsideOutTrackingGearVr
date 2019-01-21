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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderCallbacks : MonoBehaviour, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler {
  public event Action OnEndDragCallback;
  public event Action OnEnterCallback;
  public event Action OnExitCallback;

  private bool isDragging;
  private bool isHovering;

  void OnEnable() {
    isDragging = false;
    isHovering = false;
  }

  public void OnBeginDrag(PointerEventData eventData) {
    isDragging = true;
  }

  public void OnEndDrag(PointerEventData eventData) {
    if (OnEndDragCallback != null) {
      OnEndDragCallback();
    }

    isDragging = false;

    if (!isHovering && OnExitCallback != null) {
      OnExitCallback();
    }
  }

  public void OnPointerEnter(PointerEventData eventData) {
    if (OnEnterCallback != null) {
      OnEnterCallback();
    }

    isHovering = true;
  }

  public void OnPointerExit(PointerEventData eventData) {
    if (!isDragging && OnExitCallback != null) {
      OnExitCallback();
    }

    isHovering = false;
  }
}
