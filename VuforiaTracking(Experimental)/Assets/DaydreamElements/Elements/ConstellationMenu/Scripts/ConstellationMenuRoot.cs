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
using DaydreamElements.Common.IconMenu;
using UnityEngine.Events;

namespace DaydreamElements.ConstellationMenu {
  /// This script is attached to the game object responsible for spawning
  /// the Constellation menu.
  public class ConstellationMenuRoot : MonoBehaviour,
                             IPointerEnterHandler,
                             IPointerExitHandler {

    ///  Controls size of Menu
    private const float MENU_SCALE = 0.1f;
    /// The position and orientation of the menu.
    private Vector3 menuCenter;
    private Quaternion menuOrientation = Quaternion.identity;
    /// Reticle distance to return to after closing the menu.
    private float reticleDistance;
    private ConstellationMenuIcon dummyParent;
    private bool selected;
    private bool graphDirty;

    [Tooltip("The tree of menu items")]
    [SerializeField]
    private ConstellationMenuTree menuTree;

    public GvrLaserPointer laserPointer;
    public UnityEvent OnMenuOpened;
    public UnityEvent OnMenuClosed;
    public ConstellationMenuItemEvent OnItemSelected;
    public ConstellationMenuItemEvent OnItemHovered;

    [System.Serializable]
    public class ConstellationMenuItemEvent : UnityEvent<ConstellationMenuItem>
    {
    }

    public enum GvrMenuActivationButton {
      ClickButtonDown,
      ClickButtonUp,
      AppButtonDown,
      AppButtonUp
    }

    [Tooltip("Input event to trigger the menu")]
    public GvrMenuActivationButton menuActivationButton = GvrMenuActivationButton.ClickButtonDown;

    public enum GvrMenuActivationMode {
      Clutch,
      Click,
      WaitClutch,
      WaitClick
    }

    [Tooltip("Input event to trigger the menu")]
    public GvrMenuActivationMode menuActivationMode = GvrMenuActivationMode.Clutch;

    [Tooltip("Prefab used for each item in the menu")]
    public ConstellationMenuIcon menuIconPrefab;

    [Tooltip("Controls the length of the line between icons. Increase if using larger icons.")]
    [Range(0.1f, 50.0f)]
    public float lineShorteningDistance = .1f;

    [Tooltip("Maximum number of meters the reticle can move per second.")]
    [Range(0.1f, 50.0f)]
    public float reticleDelta = 10.0f;

    [Tooltip("Distance away from the controller of the menu in meters.")]
    [Range(0.6f, 5.0f)]
    public float menuDistance = 0.75f;

    [Tooltip("Angle away from the menu center to cause a closure.")]
    [Range(5.0f, 30.0f)]
    public float closeAngle = 15.0f;

    [Tooltip("Angle of gaze vs pointer needed to open a menu.")]
    [Range(30.0f, 50.0f)]
    public float openFovAngle = 35.0f;

    [Tooltip("Defines the decoration label to use for icons which are expandable")]
    public Sprite expandableIconDecoration;

    [Tooltip("Defines the background to use for icons which are not expandable")]
    public Sprite iconBackground;

    [Tooltip("Seconds the laser has to point at an icon make it hover. Only applies to icons not" +
             " in the highlighted path")]
    [Range(0.0f, 1.0f)]
    public float hoverDelay = 0.1f;

    /// The icon most recently in the hover state.
    /// If another icon starts to hover this one will stop
    /// The hover icon is also used to estimate menu depth
    private ConstellationMenuIcon hoverIcon;

    /// A cached search result for the icon at the deepest level.
    /// It may be deleted at any moment, or may be closed and not visible.
    private ConstellationMenuIcon deepestIcon;

    /// Scale factor to apply to the menuDistance used to
    /// determine the max distance of the pointer. Without this scale factor,
    /// the max distance will fall short of the menu by an increasing amount as the
    /// pointer moves away from the center of the menu.
    private const float POINTER_DISTANCE_SCALE = 1.15f;

    void Awake() {
      selected = false;
    }

    public bool IsMenuOpen() {
      return (dummyParent != null) && (dummyParent.State != IconState.Closed);
    }

    /// Determine if the activation button is held down.
    private bool IsButtonClicked() {
      switch (menuActivationButton) {
        case GvrMenuActivationButton.ClickButtonDown:
          return GvrControllerInput.ClickButtonDown;
        case GvrMenuActivationButton.ClickButtonUp:
          return GvrControllerInput.ClickButtonUp;
        case GvrMenuActivationButton.AppButtonDown:
          return GvrControllerInput.AppButtonDown;
        case GvrMenuActivationButton.AppButtonUp:
          return GvrControllerInput.AppButtonUp;
        default:
          return false;
      }
    }

    private void SetMenuLocation() {
      Vector3 laserEndPt = laserPointer.GetPointAlongPointer(menuDistance);

     /* // Get the position and orientation from the arm model.
      Vector3 pointerPosition = laserPointer.transform.position;
      Vector3 ray = laserPointer.transform.rotation * Vector3.forward;

      // Calculate the intersection of the point with the head sphere.
      laserEndPt = pointerPosition + ray * menuDistance;*/

      // Center and orient the menu
      menuCenter = laserEndPt;

      Vector3 headRight = Camera.main.transform.right;
      headRight.y = 0.0f;
      headRight.Normalize();
      Vector3 cameraCenter = Camera.main.transform.position;
      Vector3 rayFromCamera = (laserEndPt - cameraCenter).normalized;
      Vector3 up = Vector3.Cross(rayFromCamera, headRight);
      menuOrientation = Quaternion.LookRotation(rayFromCamera, up);
    }

    private bool IsMenuInFOV() {
      Vector3 cameraCenter = Camera.main.transform.position;
      Vector3 menuCenterRelativeToCamera = menuCenter - cameraCenter;
      Vector3 gazeDirection = Camera.main.transform.rotation * Vector3.forward;
      return Vector3.Angle(menuCenterRelativeToCamera.normalized, gazeDirection.normalized) < openFovAngle;
    }

    private bool IsPointingAway() {
      if (!laserPointer.IsAvailable) {
        return true;
      }

      // Get the position and orientation form the arm model
      Vector3 laserEnd = laserPointer.MaxPointerEndPoint;
      Vector3 cameraCenter = Camera.main.transform.position;
      Vector3 menuCenterRelativeToCamera = menuCenter - cameraCenter;
      Vector3 laserEndRelativeToCamera = laserEnd - cameraCenter;


      float angleToMenuCenter = Vector3.Angle(laserEndRelativeToCamera.normalized,
                                              menuCenterRelativeToCamera.normalized);
      if (angleToMenuCenter < closeAngle) {
        return false;
      }
      return IsPointingAwayFromIcon(cameraCenter, laserEndRelativeToCamera.normalized, dummyParent);
    }

    private bool IsPointingAwayFromIcon(Vector3 cameraCenter, Vector3 laserEndToCameraNormalized,
                                        ConstellationMenuIcon icon) {
      if (icon == null) {
        return true;
      }
      return icon.GetClosestAngle(cameraCenter, laserEndToCameraNormalized) > closeAngle;
    }

    public void CloseAll() {
      selected = false;
      if (IsMenuOpen()) {
        dummyParent.CloseRecursive();
        laserPointer.defaultReticleDistance = reticleDistance;
        if (OnMenuClosed != null) {
          OnMenuClosed.Invoke();
        }
      }
    }

    void Update() {
      // Update the menu state if it needs to suddenly open or close
      if (!dummyParent && IsButtonClicked()) {
        SetMenuLocation();
        if (IsMenuInFOV()) {
          reticleDistance = laserPointer.defaultReticleDistance;
          laserPointer.defaultReticleDistance = menuDistance * POINTER_DISTANCE_SCALE;
          dummyParent = ShowRootIcon();
        }
      } else if ((GvrControllerInput.ClickButtonDown && !selected) ||
                 IsPointingAway()) {
        CloseAll();
      } else if (dummyParent && GvrControllerInput.AppButtonUp) {
        CloseAll();
      }
    }

    /// Helper function to create a top level icon.
    /// The root icon does not have any interaction or icon
    /// ShowRootIcon() initializes the root icon and opens the next level of icons.
    private ConstellationMenuIcon ShowRootIcon() {
      ConstellationMenuIcon rootIcon = (ConstellationMenuIcon)Instantiate(menuIconPrefab, transform);
      rootIcon.transform.position = menuCenter;
      rootIcon.transform.rotation = menuOrientation;
      rootIcon.Initialize(this, menuTree.tree.Root, MENU_SCALE);
      ConstellationMenuIcon.ShowMenu(this, menuTree.tree.Root, rootIcon,  menuOrientation,
                           MENU_SCALE);
      rootIcon.SetDummy();
      if (OnMenuOpened != null) {
        OnMenuOpened.Invoke();
      }
      return rootIcon;
    }

    public void OnPointerEnter(PointerEventData eventData) {
      selected = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
      selected = false;
    }


    public void MakeSelection(ConstellationMenuItem item) {
      if (OnItemSelected != null) {
        OnItemSelected.Invoke(item);
      }
    }

    public void MakeHover(ConstellationMenuItem item) {
      if (OnItemHovered != null) {
        OnItemHovered.Invoke(item);
      }
    }

    /// Force cache clean after nodes are added/removed
    public void MarkGraphDirty() {
      graphDirty = true;
    }

    public float MenuDepth() {
      if (dummyParent == null) {
        return 0;
      }
      float hoverDepth = 0;
      if (hoverIcon != null) {
        // hovering on leaf node moves the menu deeper as if it had children.
        // This is so hovering has the same results across siblings.
        hoverDepth = (hoverIcon.MenuLayer + 1);
      }
      if (graphDirty) {
        deepestIcon = dummyParent.GetDeepestIcon();
        graphDirty = false;
      }
      return Mathf.Max(hoverDepth, deepestIcon.MenuLayer);
    }

    public ConstellationMenuIcon GetHoverIcon() {
      return hoverIcon;
    }

    public void StartFadeOnIcon(ConstellationMenuIcon icon,
                                IconState newState) {

      bool hoverChanged = false;
      if (newState == IconState.Hovering) {
        if (hoverIcon != null) {
          hoverIcon.FadeIfNeeded(IconState.Hovering,
                                 IconState.Shown);
        }
        hoverIcon = icon;
        hoverChanged = true;
      }

      if (icon == hoverIcon &&
          newState == IconState.Closed) {
        hoverIcon = null;
        hoverChanged = true;
      }

      if (hoverChanged) {
        dummyParent.OnHoverChange(GetHoverIcon());
      }
    }
  }
}
