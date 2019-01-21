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
using System.Collections.Generic;
using DaydreamElements.Common;
using DaydreamElements.Common.IconMenu;

namespace DaydreamElements.ClickMenu {

  /// A circular section of a menu.
  /// Contains a tooltip, a foreground icon, a background icon and
  /// shares an auto-generated mesh background.
  [RequireComponent(typeof(SpriteRenderer))]
  [RequireComponent(typeof(MeshCollider))]
  public class ClickMenuIcon : MonoBehaviour,
                             IPointerEnterHandler,
                             IPointerExitHandler {
    private const int NUM_SIDES_CIRCLE_MESH = 48;

    /// Distance to push the icon when hovered in meters.
    private const float HOVERING_Z_OFFSET = 0.04f;

    /// Distance to push the menu in depth during fades in meters.
    private const float CLOSING_Z_OFFSET = 0.0f;

    /// Distance to push the menu out while it is being hidden in meters.
    private const float HIDING_Z_OFFSET = 0.02f;
    /// Default pop out distance in meters.
    private const float SHOWING_Z_OFFSET = 0.0f;

    /// Distance the background is pushed when idle in meters.
    private const float BACKGROUND_PUSH = 0.01f;

    /// Radius from center to menu item in units of scale.
    private const float ITEM_SPACING = 0.15f;

    /// The spacing for this many items is the minimum spacing.
    private const int MIN_ITEM_SPACE = 5;

    /// Time for the fading animation in seconds.
    private const float CLOSE_ANIMATION_SECONDS = 0.12f;
    private const float HIDE_ANIMATION_SECONDS  = 0.12f;
    private const float HOVER_ANIMATION_SECONDS = 0.06f;
    private const float SHOW_ANIMATION_SECONDS  = 0.24f;

    /// Scaling factor for the tooltip text.
    private const float TOOLTIP_SCALE = 0.1f;

    /// Distance in meters the icon hovers above pie slices to prevent occlusion.
    private const float ICON_Z_OFFSET = 0.1f;

    private Vector3 startPosition;
    private Vector3 startScale;
    private Vector3 menuCenter;
    private Quaternion menuOrientation;
    private float menuScale;
    private ClickMenuIcon parentMenu;
    private List<ClickMenuIcon> childMenus;
    private GameObject background;
    private ClickMenuItem menuItem;
    private AssetTree.Node menuNode;
    /// Manages events for all icons in the menu.
    public ClickMenuRoot menuRoot { private get; set; }
    private GameObject tooltip;
    private Vector3 localOffset;
    private MeshCollider meshCollider;
    private Mesh sharedMesh;
    private MaterialPropertyBlock propertyBlock;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer backgroundSpriteRenderer;
    private MeshRenderer tooltipRenderer;

    private GameObject pieBackground;
    private MeshRenderer pieMeshRenderer;
    private Color pieStartColor;

    private const float INNER_RADIUS = 0.6f;
    private const float OUTER_RADIUS = 1.6f;
    private float startAngle;
    private float endAngle;

    /// The time when the fade started. Used to interpolate fade parameters
    private float timeAtFadeStart;
    private float alphaAtFadeStart;
    private float scaleAtFadeStart;
    private float zOffsetAtFadeStart;
    private float highlightAtFadeStart;

    private bool selected;
    private bool buttonActive;
    private bool isBackButton;

    /// The most recent fade is at the front of the list.
    // Old fades are removed after they finish.
    List<FadeParameters> fades = new List<FadeParameters>(){
      new FadeParameters(IconState.Closed, IconState.Closed)
    };

    /// Describes how a button should behave in a particular state.
    //  Provides functionality to interpolate to the new draw style.
    private class FadeParameters {
      public FadeParameters(IconState nextState, IconState prevState) {
        state = nextState;
        switch(state) {
        case IconState.Shown:
          alpha = 1.0f;
          if (prevState == IconState.Hovering) {
            duration = HOVER_ANIMATION_SECONDS;
          } else {
            duration = SHOW_ANIMATION_SECONDS;
          }
          buttonActive = true;
          highlight = 0.0f;
          scale = 1.0f;
          zOffset = SHOWING_Z_OFFSET;
          break;
        case IconState.Hovering:
          alpha = 1.0f;
          duration = HOVER_ANIMATION_SECONDS;
          buttonActive = true;
          highlight = 1.0f;
          scale = 1.0f;
          zOffset = HOVERING_Z_OFFSET;
          break;
        case IconState.Hidden:
          alpha = 0.0f;
          duration = HIDE_ANIMATION_SECONDS;
          buttonActive = false;
          highlight = 0.0f;
          scale = 1.5f;
          zOffset = HIDING_Z_OFFSET;
          break;
        case IconState.Closed:
          alpha = 0.0f;
          duration = CLOSE_ANIMATION_SECONDS;
          buttonActive = false;
          highlight = 0.0f;
          scale = 0.1f;
          zOffset = CLOSING_Z_OFFSET;
          break;
        }
      }

      public IconState state;
      public float duration;
      public float scale;
      public float alpha;
      public float zOffset;
      public float highlight;
      public bool buttonActive = false;
    };

    public GameObject tooltipPrefab;

    void Awake() {
      sharedMesh = new Mesh();
      sharedMesh.name = "Pie Mesh";
      meshCollider = GetComponent<MeshCollider>();
      propertyBlock = new MaterialPropertyBlock();
      spriteRenderer = GetComponent<SpriteRenderer>();
      childMenus = new List<ClickMenuIcon>();
      buttonActive = false;
      selected = false;
      isBackButton = false;
    }

    /// A "dummy" icon will be invisible and non-interactable
    /// The top icon in the hierarchy will be a dummy icon
    public void SetDummy() {
      buttonActive = false;
    }

    /// Called to make this icon visible and interactable
    public void Initialize(ClickMenuRoot root, ClickMenuIcon _parentMenu, AssetTree.Node node,
                           Vector3 _menuCenter, float scale, Vector3 offset) {
      string name = (node == null ? "Back " : ((ClickMenuItem)node.value).toolTip);
      gameObject.name = name + " Item";
      parentMenu = _parentMenu;
      startPosition = transform.position;
      startScale = transform.localScale;
      menuRoot = root;
      menuNode = node;
      menuCenter = _menuCenter;
      menuOrientation = transform.rotation;
      menuScale = scale;
      localOffset = offset;
      background = null;
      if (node != null) {
        // Set foreground icon
        menuItem = (ClickMenuItem)node.value;
        spriteRenderer.sprite = menuItem.icon;

        // Set background icon
        if (menuItem.background) {
          background = new GameObject(name + " Item Background");
          background.transform.parent = transform.parent;
          background.transform.localPosition = transform.localPosition + transform.forward * BACKGROUND_PUSH;
          background.transform.localRotation = transform.localRotation;
          background.transform.localScale = transform.localScale;
          backgroundSpriteRenderer = background.AddComponent<SpriteRenderer>();
          backgroundSpriteRenderer.sprite = menuItem.background;
        }

        // Set tooltip text
        tooltip = Instantiate(tooltipPrefab);
        tooltip.name = name + " Tooltip";
        tooltip.transform.parent = transform.parent;
        tooltip.transform.localPosition = menuCenter;
        tooltip.transform.localRotation = menuOrientation;
        tooltip.transform.localScale = transform.localScale * TOOLTIP_SCALE;
        tooltip.GetComponent<TextMesh>().text = menuItem.toolTip.Replace('\\','\n');
        tooltipRenderer = tooltip.GetComponent<MeshRenderer>();
        SetTooltipAlpha(0.0f);
      } else {
        // This is a back button
        spriteRenderer.sprite = root.backIcon;
        isBackButton = true;
      }

      pieBackground = null;
      pieMeshRenderer = null;
      if (root.pieMaterial) {
        pieBackground = new GameObject(name + " Pie Background");
        pieBackground.transform.SetParent(transform.parent, false);
        pieBackground.transform.localPosition = transform.localPosition;
        pieBackground.transform.localRotation = transform.localRotation;
        pieBackground.transform.localScale = transform.localScale;
        pieBackground.AddComponent<MeshFilter>().sharedMesh = sharedMesh;
        pieMeshRenderer = pieBackground.AddComponent<MeshRenderer>();
        pieMeshRenderer.sharedMaterial = root.pieMaterial;
        pieStartColor = root.pieMaterial.GetColor("_Color");
      }

      parentMenu.childMenus.Add(this);
      StartFade(IconState.Shown);
      SetButtonTransparency(0.0f);
      SetPieMeshTransparency(0.0f, 0.0f);
    }

    private void MakeMeshColliderCircle() {
      Vector3[] vertices = new Vector3[NUM_SIDES_CIRCLE_MESH*2 + 1];
      int[] triangles = new int[NUM_SIDES_CIRCLE_MESH * 9];

      vertices[0] = new Vector3(0.0f, 0.0f, ICON_Z_OFFSET);

      float pushScaled = GetCurrentZOffset() / transform.localScale[0];

      for (int i = 0; i < NUM_SIDES_CIRCLE_MESH; i++) {
        float angle = i * 2.0f * Mathf.PI / NUM_SIDES_CIRCLE_MESH;
        float x = Mathf.Sin(angle) * INNER_RADIUS;
        float y = Mathf.Cos(angle) * INNER_RADIUS;
        vertices[i + 1] = new Vector3(x, y, ICON_Z_OFFSET);
        vertices[i + NUM_SIDES_CIRCLE_MESH + 1] = new Vector3(x, y, pushScaled + ICON_Z_OFFSET);
        int nextIx = (i == NUM_SIDES_CIRCLE_MESH - 1 ? 1 : i + 2);
        triangles[i * 9 + 0] = i + 1;
        triangles[i * 9 + 1] = nextIx;
        triangles[i * 9 + 2] = 0;
        triangles[i * 9 + 3] = i + 1;
        triangles[i * 9 + 4] = i + NUM_SIDES_CIRCLE_MESH + 1;
        triangles[i * 9 + 5] = nextIx;
        triangles[i * 9 + 6] = nextIx;
        triangles[i * 9 + 7] = i + NUM_SIDES_CIRCLE_MESH + 1;
        triangles[i * 9 + 8] = nextIx + NUM_SIDES_CIRCLE_MESH;
      }

      sharedMesh.vertices = vertices;
      sharedMesh.triangles = triangles;
    }

    private void MakeMeshCollider() {
      if (localOffset.sqrMagnitude <= Mathf.Epsilon) {
        MakeMeshColliderCircle();
        return;
      }

      int numSides = (int)((endAngle - startAngle) * 8.0f) + 1;
      Vector3[] vertices = new Vector3[numSides * 4 + 4];
      int[] triangles = new int[numSides * 18 + 12];

      float outerRadius = localOffset.magnitude + Mathf.Min(localOffset.magnitude - INNER_RADIUS, OUTER_RADIUS - 1.0f);
      float pushScaled = GetCurrentZOffset() / transform.localScale[0];
      float x = Mathf.Sin(startAngle);
      float y = Mathf.Cos(startAngle);
      vertices[0] = new Vector3(x * INNER_RADIUS, y * INNER_RADIUS, pushScaled + ICON_Z_OFFSET) - localOffset;
      vertices[1] = new Vector3(x * outerRadius, y * outerRadius, pushScaled + ICON_Z_OFFSET) - localOffset;
      vertices[2] = new Vector3(x * INNER_RADIUS, y * INNER_RADIUS, ICON_Z_OFFSET) - localOffset;
      vertices[3] = new Vector3(x * outerRadius, y * outerRadius, ICON_Z_OFFSET) - localOffset;

      triangles[0] = 0;
      triangles[1] = 1;
      triangles[2] = 2;
      triangles[3] = 3;
      triangles[4] = 2;
      triangles[5] = 1;

      for (int i = 0; i < numSides; i++) {
        float angle = startAngle + (i + 1) * (endAngle - startAngle) / numSides;
        x = Mathf.Sin(angle);
        y = Mathf.Cos(angle);
        vertices[i*4 + 4] = new Vector3(x * INNER_RADIUS, y * INNER_RADIUS, pushScaled + ICON_Z_OFFSET) - localOffset;
        vertices[i*4 + 5] = new Vector3(x * outerRadius, y * outerRadius, pushScaled + ICON_Z_OFFSET) - localOffset;
        vertices[i*4 + 6] = new Vector3(x * INNER_RADIUS, y * INNER_RADIUS, ICON_Z_OFFSET) - localOffset;
        vertices[i*4 + 7] = new Vector3(x * outerRadius, y * outerRadius, ICON_Z_OFFSET) - localOffset;

        triangles[i*18 + 6]  = i*4 + 0;
        triangles[i*18 + 7]  = i*4 + 2;
        triangles[i*18 + 8]  = i*4 + 4;
        triangles[i*18 + 9]  = i*4 + 6;
        triangles[i*18 + 10] = i*4 + 4;
        triangles[i*18 + 11] = i *4 + 2;

        triangles[i * 18 + 12] = i*4 + 2;
        triangles[i * 18 + 13] = i*4 + 3;
        triangles[i * 18 + 14] = i*4 + 6;
        triangles[i * 18 + 15] = i*4 + 7;
        triangles[i * 18 + 16] = i*4 + 6;
        triangles[i * 18 + 17] = i*4 + 3;

        triangles[i * 18 + 18] = i*4 + 3;
        triangles[i * 18 + 19] = i*4 + 1;
        triangles[i * 18 + 20] = i*4 + 7;
        triangles[i * 18 + 21] = i*4 + 5;
        triangles[i * 18 + 22] = i*4 + 7;
        triangles[i * 18 + 23] = i*4 + 1;
      }

      int lastTriangleIx = numSides * 18 + 6;
      int lastVertIx = numSides * 4;
      triangles[lastTriangleIx + 0] = lastVertIx + 2;
      triangles[lastTriangleIx + 1] = lastVertIx + 1;
      triangles[lastTriangleIx + 2] = lastVertIx + 0;
      triangles[lastTriangleIx + 3] = lastVertIx + 1;
      triangles[lastTriangleIx + 4] = lastVertIx + 2;
      triangles[lastTriangleIx + 5] = lastVertIx + 3;

      sharedMesh.vertices = vertices;
      sharedMesh.triangles = triangles;
    }

    /// Shows a new menu, returns true if a fade needs to occur.
    public static bool ShowMenu(ClickMenuRoot root, AssetTree.Node treeNode, ClickMenuIcon parent,
                            Vector3 center, Quaternion orientation, float scale) {
      // Determine how many children are in the sub-menu
      List<AssetTree.Node> childItems = treeNode.children;

      // If this is the end of a menu, invoke the action and return early
      if (childItems.Count == 0) {
        if (parent.menuItem.closeAfterSelected) {
          root.CloseAll();
        }
        return false;
      }

      // Radius needs to expand when there are more icons
      float radius = ITEM_SPACING * Mathf.Max(childItems.Count, MIN_ITEM_SPACE) / (2.0f * Mathf.PI);

      // Create and arrange the icons in a circle
      float arcAngle = 2.0f * Mathf.PI / childItems.Count;
      for (int i = 0; i < childItems.Count; ++i) {
        float angle = i * arcAngle;
        Vector3 posOffset = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0.0f) * radius;

        ClickMenuIcon childMenu = (ClickMenuIcon)Instantiate(root.menuIconPrefab, root.transform);
        childMenu.transform.position = center + (orientation * posOffset);
        childMenu.transform.rotation = orientation;
        childMenu.transform.localScale = Vector3.one * scale;
        childMenu.startAngle = angle - arcAngle / 2;
        childMenu.endAngle = angle + arcAngle / 2;
        childMenu.Initialize(root, parent, childItems[i], center, scale, posOffset / scale);
      }

      // Also create a back button
      ClickMenuIcon backButton = (ClickMenuIcon)Instantiate(root.menuIconPrefab, root.transform);
      backButton.transform.position = center;
      backButton.transform.rotation = orientation;
      backButton.transform.localScale = Vector3.one * scale;
      backButton.Initialize(root, parent, null, center, scale, Vector3.zero);
      return true;
    }

    /// Returns true if a fade starts.
    private bool ShowChildMenu() {
      return ShowMenu(menuRoot, menuNode, this, menuCenter, menuOrientation, menuScale);
    }

    /// Closes this menu and shows the parent level
    public void ShowParentMenu() {
      if (!parentMenu) {
        menuRoot.CloseAll();
      } else {
        FadeChildren(IconState.Closed);
        parentMenu.FadeChildren(IconState.Shown);
        childMenus.Clear();
      }
    }

    void Update() {
      ContinueFade();

      var highlight = GetCurrentHighlight();
      SetTooltipAlpha(highlight);

      // The back button needs special handling because the tooltips draw over it.
      // Make it fade out unless it is highlighted.
      if (isBackButton) {
        SetButtonTransparency(highlight);
      }

      if (buttonActive) {
        // Make button interactive and process clicks
        MakeMeshCollider();
        if (selected && (GvrControllerInput.ClickButtonDown)) {
          menuRoot.MakeSelection(menuItem);
          if (isBackButton) {
            parentMenu.ShowParentMenu();
          } else {
            if (ShowChildMenu()) {
              parentMenu.FadeChildren(IconState.Hidden);
            }
          }
        }
      }
    }

    /// Shrink / Grow button
    // t is the scale of the button.
    // depth adjusts the buttons z position to move a button toward or away from viewer
    private void SetButtonScale(float t, float depth) {
      float scaleMult = Mathf.Max(t, 0.01f);

      Vector3 delta = (startPosition - menuCenter) * t;
      transform.position = menuCenter + delta - transform.forward * depth;
      transform.localScale = startScale * scaleMult;
      if (background) {
        background.transform.position = menuCenter + transform.forward * BACKGROUND_PUSH + delta;
        background.transform.localScale = startScale * scaleMult;
      }
      if (pieBackground) {
        pieBackground.transform.position = menuCenter + delta  - pieBackground.transform.forward * depth;
        pieBackground.transform.localScale = startScale * scaleMult;
      }
    }

    private void SetButtonTransparency(float alpha) {
      Color alphaColor = new Color(1.0f, 1.0f, 1.0f, alpha);
      if (isBackButton) {
        alphaColor.a = Mathf.Min(zOffsetAtFadeStart / HOVERING_Z_OFFSET, alpha);
      }
      spriteRenderer.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor("_Color", alphaColor);
      spriteRenderer.SetPropertyBlock(propertyBlock);
      if (background) {
        backgroundSpriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", alphaColor);
        backgroundSpriteRenderer.SetPropertyBlock(propertyBlock);
      }
    }

    // Draw the pie background with highlight
    private void SetPieMeshTransparency(float alpha, float highlight) {
      if (pieMeshRenderer) {
        Color pieColor = pieStartColor;
        pieColor.a = pieColor.a * alpha * (1.0f - highlight) + highlight;
        pieMeshRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", pieColor);
        pieMeshRenderer.SetPropertyBlock(propertyBlock);
      }
    }

    private void SetTooltipAlpha(float alpha) {
      if (tooltip) {
        Color alphaColor = new Color(1.0f, 1.0f, 1.0f, alpha);
        tooltipRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", alphaColor);
        tooltipRenderer.SetPropertyBlock(propertyBlock);
      }
    }

    void OnDestroy() {
      Destroy(sharedMesh);
      if (pieBackground) {
        Destroy(pieBackground);
      }
      if (background) {
        Destroy(background);
      }
      if (tooltip) {
        Destroy(tooltip);
      }
      if (parentMenu) {
        parentMenu.childMenus.Remove (this);
      }
    }

    private void FadeChildren(IconState nextState) {
      foreach (ClickMenuIcon child in childMenus) {
        child.StartFade(nextState);
      }
    }

    private void StartFade(IconState nextState) {
      FadeParameters prev = fades[0];
      if (prev.state == nextState) {
        return;
      }

      if (fades.Count > 1) {

        // A fade is currently in progress. Save off any values where they are now.
        float t = GetCurrentProgress();
        alphaAtFadeStart = GetCurrentAlpha();
        scaleAtFadeStart = Mathf.Lerp(scaleAtFadeStart, prev.scale, t);
        zOffsetAtFadeStart = GetCurrentZOffset();
        highlightAtFadeStart = GetCurrentHighlight();

        // Delete any old fades
        fades.RemoveRange(1, fades.Count - 1);
      }

      FadeParameters nextFade = new FadeParameters(nextState, prev.state);
      fades.Insert(0, nextFade);

      timeAtFadeStart = Time.time;
      buttonActive = nextFade.buttonActive;

      if (!buttonActive) {
        // Collisions are only allowed on active buttons.
        meshCollider.sharedMesh = null;
      }
    }

    private void FinishFade() {

      // Save values from the completed fade.
      FadeParameters finishedFade = fades[0];
      alphaAtFadeStart = GetCurrentAlpha();
      scaleAtFadeStart = finishedFade.scale;
      zOffsetAtFadeStart = GetCurrentZOffset();
      highlightAtFadeStart = GetCurrentHighlight();

      // remove any old fades
      if (fades.Count > 1) {
        fades.RemoveRange(1, fades.Count - 1);
      }

      if (finishedFade.buttonActive) {
        MakeMeshCollider();
        meshCollider.sharedMesh = sharedMesh;
        buttonActive = true;
      }
      if (finishedFade.state == IconState.Closed) {
        Destroy(gameObject);
      }
    }

    /// Updates icon with interpolated fade values.
    private void ContinueFade() {
      if (fades.Count < 2) {
        // no fade in progress. Do nothing.
        return;
      }


      float t = GetCurrentProgress();
      float scale = Mathf.Lerp(scaleAtFadeStart, fades[0].scale, t);
      float alpha = GetCurrentAlpha();

      SetButtonScale(scale, GetCurrentZOffset());
      SetButtonTransparency(alpha);
      SetPieMeshTransparency(alpha, GetCurrentHighlight());
      if (fades[0].state == IconState.Closed) {
        selected = false;
      } else if (fades[0].state == IconState.Hidden) {
        MakeMeshCollider();
        selected = false;
      } else {
        // Fade in.
        MakeMeshCollider();
      }

      if (!(GetCurrentProgress() < 1.0f)) {
        FinishFade();
        return;
      }
    }

    /// Starts a fade if the there is a match for fromState
    private void FadeIfNeeded(IconState fromState,
                      IconState toState)
    {
      if (fades[0].state != fromState) {
        return;
      }
      StartFade(toState);
    }

    /// returns 1 if completed, 0 if just started or values in between depending on time
    private float GetCurrentProgress() {
      if (fades.Count < 2) {
        return 1.0f;
      }
      return Mathf.Clamp01((Time.time - timeAtFadeStart) / fades[0].duration);
    }

    /// The width in z of the button mesh we create.
    /// Also used to move button forward in z.
    private float GetCurrentZOffset() {
      float zOffset = zOffsetAtFadeStart;
      if (fades.Count > 1) {
        if (fades[0].state == IconState.Closed &&
           fades[1].state == IconState.Hovering) {
          // Special Behavior: One button is hovering above the others.
          // Force it to close with the same height as other buttons.
          zOffset = SHOWING_Z_OFFSET;
        }
        zOffset = Mathf.Lerp(zOffset, fades[0].zOffset, GetCurrentProgress());
      }
      return zOffset;
    }

    private float GetCurrentHighlight() {
      if (buttonActive==false) {
        //  Special Behavior: disabled buttons can not be highlighted.
        return 0.0f;
      }
      float highlight = highlightAtFadeStart;
      if (fades.Count > 1) {
        highlight = Mathf.Lerp(highlightAtFadeStart, fades[0].highlight, GetCurrentProgress());
      }
      return highlight;
    }

    private float GetCurrentAlpha() {
      float alpha = alphaAtFadeStart;
      if (fades.Count > 1) {
        float t = GetCurrentProgress();
        if(fades[0].alpha < alphaAtFadeStart) {
          // Special Behavior: fade out 50% faster.
          t = Mathf.Clamp01(t * 1.5f);
        }
        alpha = Mathf.Lerp(alphaAtFadeStart, fades[0].alpha, t);
      }
      return alpha;
    }

    /// Recursively closes this menu and its children
    public void CloseAll() {
      foreach (ClickMenuIcon child in childMenus) {
        child.CloseAll();
      }
      if (buttonActive) {
        StartFade(IconState.Closed);
      } else {
        // If the button is already disabled, destroy it instantly
        Destroy(gameObject);
      }
    }

    public ClickMenuIcon DeepestMenu() {
      foreach (ClickMenuIcon child in childMenus) {
        if (child.childMenus.Count > 0) {
          return child.DeepestMenu();
        }
      }
      return this;
    }

    public void OnPointerEnter(PointerEventData eventData) {
      if (!selected) {
        menuRoot.MakeHover(menuItem);
      }
      selected = true;
      FadeIfNeeded(IconState.Shown, IconState.Hovering);
    }

    public void OnPointerExit(PointerEventData eventData) {
      selected = false;
      FadeIfNeeded(IconState.Hovering, IconState.Shown);
    }


  }


}

