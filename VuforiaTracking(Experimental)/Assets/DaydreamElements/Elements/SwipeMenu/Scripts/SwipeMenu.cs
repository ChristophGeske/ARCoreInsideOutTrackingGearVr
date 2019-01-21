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

namespace DaydreamElements.SwipeMenu {

  /// SwipeMenu is optimized for situations where the user has a small number of
  /// options(usually 2-4) that they need to switch between as quickly as possible
  /// and still retain point-and-click functionality.  This is achieved by using
  /// swipe gestures on the touch pad to switch between modes and displaying
  /// feedback on the controller.
  public class SwipeMenu : MonoBehaviour {
    private GameObject[] menuItems;
    private GameObject[] menuSlots;
    private bool firstTouch = true;
    private Vector2 touchOrigin;

    [System.Serializable]
    public class MenuSprites {
      /// Sprite to use for the sliding icons (must match size of slotIcons).
      public Sprite icon;

      /// Sprite to use for the slot icons (must match size of icons).
      public Sprite slot;
    }

    /// Prefab to create menu icons from.
    public GameObject menuIconPrefab;

    /// The radius in meters between the center of the menu and the sliding icons.
    [SerializeField]
    private float iconRadius;

    /// The radius in meters between the center of the menu and the icon slots.
    [SerializeField]
    private float slotRadius;

    /// List of sprites to use for the menu icons.
    [SerializeField]
    private MenuSprites[] menuSprites;

    /// Swipe actions return the index of the selected menu item.
    public delegate void SwipeAction(int selectedIx);
    public event SwipeAction OnSwipeSelect;

    void Start() {
      int numItems = menuSprites.Length;
      float radiansPerItem = 2.0f * Mathf.PI / numItems;
      menuItems = new GameObject[numItems];
      menuSlots = new GameObject[numItems];
      for (int i = 0; i < numItems; ++i) {
        float theta = i * radiansPerItem;
        Vector3 offset = new Vector3(Mathf.Sin(theta), 0.0f, Mathf.Cos(theta));

        menuItems[i] = CreateIcon(menuSprites[i].icon);
        menuItems[i].transform.localPosition = offset * iconRadius;

        menuSlots[i] = CreateIcon(menuSprites[i].slot);
        menuSlots[i].transform.localPosition = offset * slotRadius;
      }
    }

    GameObject CreateIcon(Sprite icon) {
      GameObject item = Instantiate(menuIconPrefab, transform);
      SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
      Debug.Assert(spriteRenderer != null);
      spriteRenderer.sprite = icon;
      return item;
    }

    void Update() {
      // Menu constants.
      int numItems = menuItems.Length;
      float radiansPerItem = 2.0f * Mathf.PI / numItems;
      const float MIN_INNER_RADIUS = 0.02f;
      const float MAX_OUTER_RADIUS = 0.25f;
      const float SELECT_RADIUS = 0.04f;

      // Touchpad constants.
      bool touching = GvrControllerInput.IsTouching;
      Vector2 touchPos = GvrControllerInput.TouchPos;
      touchPos.y = -touchPos.y;

      // Set the origin of the touch.
      if (touching && firstTouch) {
        touchOrigin = touchPos;
        firstTouch = false;
      } else if (!touching) {
        firstTouch = true;
      }

      if (GvrControllerInput.ClickButton) {
        touchOrigin = touchPos;
      }

      // Get the touch delta from the origin.
      Vector2 p = (touchPos - touchOrigin) * MAX_OUTER_RADIUS;
      float pMag = p.magnitude;

      for (int i = 0; i < numItems; ++i) {
        float theta = radiansPerItem * i;
        Vector2 dir2D = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));
        Vector3 dir = new Vector3(Mathf.Sin(theta), 0.0f, Mathf.Cos(theta));
        Vector3 lerpPos = Vector3.Lerp(menuItems[i].transform.localPosition, dir * iconRadius, 0.1f);
        menuItems[i].transform.localPosition = lerpPos;

        if (touching && pMag > MIN_INNER_RADIUS &&
            Mathf.Deg2Rad * Vector2.Angle(dir2D, p) < radiansPerItem * 0.5f) {
          float blendDist = Mathf.Min(1.0f, (pMag - MIN_INNER_RADIUS) / SELECT_RADIUS);
          float dist = slotRadius * blendDist + iconRadius * (1.0f - blendDist);
          menuItems[i].transform.localPosition = dir * dist;
          if (pMag - MIN_INNER_RADIUS >= SELECT_RADIUS) {
            OnSwipeSelect.Invoke(i);
          }
        }
      }
    }

    void OnDestroy() {
      for (int i = 0; i < menuItems.Length; ++i) {
        Destroy(menuItems[i]);
        Destroy(menuSlots[i]);
      }
    }
  }
}
