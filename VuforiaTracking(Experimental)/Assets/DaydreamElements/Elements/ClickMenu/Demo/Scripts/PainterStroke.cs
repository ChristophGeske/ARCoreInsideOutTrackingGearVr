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
using UnityEngine.EventSystems;

namespace DaydreamElements.ClickMenu {

  /// An individual stroke of paint.
  [RequireComponent(typeof(MeshFilter))]
  [RequireComponent(typeof(MeshRenderer))]
  [RequireComponent(typeof(MeshCollider))]
  public class PainterStroke : MonoBehaviour,
                               IPointerEnterHandler,
                               IPointerExitHandler {
    private MeshFilter meshFilter;

    public bool isHovered;
    public bool isClicking;
    public Painter painter;
    public Painter.Stroke stroke;

    public void Init(Painter _painter, Painter.Stroke _stroke) {
      painter = _painter;
      stroke = _stroke;
    }

    void Awake() {
      isHovered = false;
      meshFilter = GetComponent<MeshFilter>();
      meshFilter.sharedMesh = new Mesh();
    }

    void Update() {
      if (painter.IsEraser && GvrControllerInput.ClickButtonDown &&
          painter.menuRoot.IsMenuOpen()==false) {
        isClicking = true;
      } else if (GvrControllerInput.ClickButtonUp) {
        isClicking = false;
      }
      if (painter.IsEraser && isClicking) {
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        if (isHovered) {
          painter.RemoveStroke(stroke);
        }
      }
    }

    void OnDestroy() {
      Destroy(meshFilter.sharedMesh);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      isHovered = false;
    }
  }
}
