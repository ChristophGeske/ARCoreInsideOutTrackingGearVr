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

namespace DaydreamElements.Main {

  /// Script used for controlling the background of a Breadcrumb Trail.
  [RequireComponent(typeof(RectTransform))]
  public class BreadcrumbBackground : MonoBehaviour {
    private RectTransform rectTransform;
  
    [SerializeField]
    private RectTransform breadcrumbsTransform;

    void Awake() {
      rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate() {
      // Match the width of the breadcrumbs
      Vector2 breadcrumbsSize = breadcrumbsTransform.sizeDelta;
      Vector2 backgroundSize = rectTransform.sizeDelta;
      if (backgroundSize.x != breadcrumbsSize.x) {
        backgroundSize.x = breadcrumbsSize.x;
        rectTransform.sizeDelta = backgroundSize;
      }
  
      // Match the height of the breadcrumbs by scaling instead of
      // changing the width of the background.
      // This is because the background is a sliced sprite and
      // we want the height of the background to perfectly match the
      // height of the source image so that the background is maximally rounded.
      float scale = breadcrumbsSize.y / backgroundSize.y;
      Vector3 backgroundScale = rectTransform.localScale;
      if (backgroundScale.y != scale) {
        backgroundScale.y = scale;
        rectTransform.localScale = backgroundScale;
      }
    }
  }
}
