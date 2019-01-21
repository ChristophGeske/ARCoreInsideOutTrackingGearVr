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

namespace DaydreamElements.ConstellationMenu {

  /// A network of icons laid out in concentric circles.
  //
  // Initialize() must be called on the top icon
  // ShowMenu() creates each deeper layer of icons.
  //
  // Icons can be in the following states:
  // * Shown - Fully shown
  // * Hidden - The icon is still displayed, but is more transparent.  The collision size is reduced
  //   from the Shown state.
  // * Hovering - The hovering icon changes the behavior, appearance, and position of other icons.
  // * Closed - In the process of being removed.
  //
  // Icon appearance will depend on relationship with the hover icon.  Relationships may be:
  // * AncestorOfRhs - Has a background a tooltip and a line to its parent,
  // * DescendantOfRhs - Same but the background is faded out a little bit
  // * UnkownRelationship - No background or tooltip and is harder to select.
  [RequireComponent(typeof(LineRenderer))]
  [RequireComponent(typeof(SpriteRenderer))]
  [RequireComponent(typeof(BoxCollider))]
  public class ConstellationMenuIcon : MonoBehaviour,
                             IPointerEnterHandler,
                             IPointerExitHandler {
    /// Distance between the background and foreground icons in meters.
    private const float BACKGROUND_ICON_ZOFFSET = -0.005f;
    private const float CHILD_LINE_ZOFFSET = -0.01f;
    private const float PARENT_LINE_ZOFFSET = -0.04f;

    /// Radius from center to menu item in units of scale.
    private const float ITEM_SPACING = 0.15f;

    /// Size of collision box used to detect selection
    private const float COLLIDER_SIZE = 1.0f;
    /// Size of collision box when the icon is "hidden"
    private const float HIDDEN_COLLIDER_SIZE = 0.7f;
    /// Moves items closer together when there are not many of them in the menu layer
    private const float MAX_DISTANCE_BETWEEN_ITEMS = 0.1f;
    /// The spacing for this many items is the minimum spacing.
    private const int MIN_ITEM_SPACE = 5;

    /// Time for the fading animations in seconds.
    private const float ANIMATION_DURATION_SECONDS = 0.24f;

    /// Scaling factor for the tooltip text.
    private const float TOOLTIP_TEXT_SCALE = 0.1f;

    /// Distance between center of icon and the tooltip
    private float TOOLTIP_TEXT_OFFSET = -0.7f;

    /// Scaling factor for tooltip sprites.
    private float TOOLTIP_SPRITE_SCALE = 1.0f;

    /// Distance between center of icon and the tooltip sprite
    private float TOOLTIP_SPRITE_OFFSET = -0.5f;

    /// Shifts each layer of icons away from selection layer progressively.
    private const float ICON_SELECTION_OFFSET_FACTOR = -0.10f;
    /// Shifts the entire menu further away as the visible graph gets deeper.
    private const float MENU_SELECTION_OFFSET_FACTOR = -0.025f;

    /// Sets both size and position of icon based on state
    private const float DEFAULT_ICON_SCALE = 0.0f;
    private const float SHOWN_ICON_SCALE = 1.0f;

    /// Sets transparency of icon based on state
    private const float DEFAULT_ICON_ALPHA = 0.0f;
    private const float HIDDEN_ICON_ALPHA = 0.6f;
    private const float SHOWN_ICON_ALPHA = 1.0f;

    /// Set icon's background alpha depending on state and relationship to the hover icon.
    private const float DEFAULT_ICON_BG_ALPHA = 0.0f;
    private const float CLOSED_ICON_BG_ALPHA = 0.0f;
    private const float ANCESTOR_ICON_BG_ALPHA = 0.9f;
    private const float DESCENDANT_ICON_BG_ALPHA = 0.6f;

    // Sets icon decoration label alpha based on state, children, and relationship to hover icon.
    private const float DESCENDANT_DECORATION_LABEL_ALPHA = .6f;
    private const float DEFAULT_DECORATION_LABEL_ALPHA = 0.0f;

    private const float DEFAULT_TOOLTIP_ALPHA = 0.0f;
    private const float SELECTED_PATH_TOOLTIP_ALPHA = 1.0f;
    private static Color CLEAR_WHITE = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    /// Cached storage for line points.
    private Vector3[] lineToParentPositions = new Vector3[2];
    private LineRenderer lineToParent;
    private Vector3 startPosition;
    private Vector3 startScale;
    private Vector3 menuCenter;

    private Quaternion menuOrientation;
    private float menuScale;

    private ConstellationMenuIcon parentMenu;
    private List<ConstellationMenuIcon> childMenus;
    private GameObject background;
    private ConstellationMenuItem menuItem;
    private AssetTree.Node menuNode;

    /// Manages events for all icons in the menu.
    public ConstellationMenuRoot menuRoot { private get; set; }

    private GameObject tooltip;
    private BoxCollider boxCollider;
    private MaterialPropertyBlock propertyBlock;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer backgroundSpriteRenderer;

    private Renderer tooltipRenderer;

    // Each icon is shifted in Z based on what other icons are showing.
    private IconValue selectionOffset;
    // The background is shown or hidden based on which icon is hovering.
    private IconValue iconBgAlpha = new IconValue(DEFAULT_ICON_BG_ALPHA);
    // The icon fades in/out when Shown/Closed
    private IconValue iconAlpha = new IconValue(DEFAULT_ICON_ALPHA);
    private IconValue iconScale = new IconValue(DEFAULT_ICON_SCALE);
    private IconValue tooltipAlpha = new IconValue(DEFAULT_TOOLTIP_ALPHA);

    /// Each icon's orientation is based on the camera transform when the first icon was created.
    private Vector3 savedCameraPosition;
    private Quaternion savedCameraRotation;

    // set when this icon starts a hover.
    private float hoverStartTime;
    // current sensitivity to the laser pointer.
    private float hoverDelaySeconds;
    private float angleToRoot;
    public int MenuLayer {
      get {
        int depth = 0;
        var next = this;
        while (next.parentMenu != null) {
          ++depth;
          next = next.parentMenu;
        }
        return depth;
      }
    }

    /// returns true if behavior or visual parameters are changing.
    private bool IsFadeInProgress {
      get {
        return (parentMenu != null) && fadeInProgress;
      }
    }

    /// True while the laser is pointing at this icon
    private bool selected;
    /// Set when the laser starts pointing at this icon
    private float selectionStartTime;
    /// True if the button can be "selected"
    private bool buttonActive;
    ///  True after state change while visual parameters are changing.
    private bool fadeInProgress;
    /// State determines visual parameters and logical behavior of the icon
    private IconState state = IconState.Closed;

    /// Visual parameters to use for the icon's next state.
    private FadeParameters fadeParameters;

    [SerializeField]
    private GameObject tooltipPrefab;

    /// If the icon can be expanded, this prefab will be instantiated as a decoration label.
    [SerializeField]
    private GameObject decorationLabelPrefab = null;
    private SpriteRenderer decorationLabelRenderer;
    private GameObject decorationLabel;
    private IconValue decorationLabelAlpha = new IconValue(DEFAULT_DECORATION_LABEL_ALPHA);

    /// Describes how a button should behave in a particular state.
    public class FadeParameters {

      public FadeParameters(IconState nextState) {
        state = nextState;
        switch (state) {
        case IconState.Shown:
          alpha = SHOWN_ICON_ALPHA;
          buttonActive = true;
          colliderSize = COLLIDER_SIZE;
          break;

        case IconState.Hovering:
          alpha = SHOWN_ICON_ALPHA;
          buttonActive = true;
          colliderSize = COLLIDER_SIZE;
          break;

        case IconState.Hidden:
          alpha = HIDDEN_ICON_ALPHA;
          buttonActive = true;
          colliderSize = HIDDEN_COLLIDER_SIZE;
          break;

        case IconState.Closed:
          alpha = DEFAULT_ICON_ALPHA;
          buttonActive = false;
          colliderSize = 0.0f;
          break;
        }
      }

      public IconState state;
      public float colliderSize;
      public float duration;
      public float alpha;
      public bool buttonActive = false;
    };

    /// Get visual parameters and logical behavior of the icon.
    public IconState State {
      get { return state; }
    }

    void Awake() {
      boxCollider = GetComponent<BoxCollider>();
      propertyBlock = new MaterialPropertyBlock();
      spriteRenderer = GetComponent<SpriteRenderer>();
      childMenus = new List<ConstellationMenuIcon>();
      buttonActive = false;
      selected = false;
    }

    /// A "dummy" icon will be invisible and non-interactable
    /// The top icon in the hierarchy will be a dummy icon
    public void SetDummy() {
      buttonActive = false;
    }

    /// The minimal preparation for use. If these are not set ShowChildLayer() can't work.
    public void Initialize(ConstellationMenuRoot root, AssetTree.Node asset, float scale) {
      menuRoot = root;
      menuRoot.MarkGraphDirty();
      menuNode = asset;
      menuScale = scale;
      menuOrientation = transform.rotation;
      menuCenter = transform.position;
      startPosition = transform.position;
      savedCameraPosition = Camera.main.transform.position;
      savedCameraRotation = Camera.main.transform.rotation;
      state = IconState.Shown;
    }

    /// Called to make this icon visible and interactable
    public void Initialize(ConstellationMenuRoot root, ConstellationMenuIcon _parentMenu, AssetTree.Node node,
                           Vector3 _menuCenter, float scale, IconState initialState) {
      parentMenu = _parentMenu;
      startPosition = transform.position;
      startScale = transform.localScale;
      menuRoot = root;
      menuNode = node;
      menuCenter = _menuCenter;
      savedCameraPosition = _parentMenu.savedCameraPosition;
      savedCameraRotation = _parentMenu.savedCameraRotation;
      menuOrientation = transform.rotation;
      // Icons are oriented up in world space.
      Vector3 iconToCamera = savedCameraPosition - transform.position;
      Quaternion rotationToCamera = Quaternion.FromToRotation(-transform.forward, iconToCamera.normalized);
      transform.localRotation = rotationToCamera * transform.localRotation;
      menuScale = scale;
      background = null;
      state = IconState.Closed;
      if (node != null) {
        // Set foreground icon
        menuItem = (ConstellationMenuItem)node.value;
        spriteRenderer.sprite = menuItem.icon;

        gameObject.name = menuItem.name;
        // Set background icon
        background = new GameObject(name + " Item Background");
        background.transform.parent = transform.parent;
        background.transform.localPosition = transform.localPosition + transform.forward * BACKGROUND_ICON_ZOFFSET;
        background.transform.localRotation = transform.localRotation;
        background.transform.localScale = transform.localScale;
        backgroundSpriteRenderer = background.AddComponent<SpriteRenderer>();

        backgroundSpriteRenderer.sprite = menuRoot.iconBackground;
        if (menuNode.children.Count != 0 && 
           decorationLabelPrefab != null && 
           menuRoot.expandableIconDecoration) {
          decorationLabel = Instantiate(decorationLabelPrefab, background.transform);
          decorationLabelRenderer = decorationLabel.GetComponent<SpriteRenderer>();
          decorationLabelRenderer.sprite = menuRoot.expandableIconDecoration;
          decorationLabel.SetActive(false);
        }
        menuRoot.MarkGraphDirty();

        float tooltipOffset;
        if (menuItem.tooltipSprite) {
          tooltip = new GameObject(name + " Tooltip Sprite");
          SpriteRenderer tooltipSpriteRenderer = tooltip.AddComponent<SpriteRenderer>();
          tooltipSpriteRenderer.sprite = menuItem.tooltipSprite;
          tooltipRenderer = tooltipSpriteRenderer;
          tooltip.transform.localScale *= TOOLTIP_SPRITE_SCALE;
          tooltipOffset = TOOLTIP_SPRITE_OFFSET;
        } else {
          tooltip = Instantiate(tooltipPrefab);
          tooltip.name = name + " Tooltip Text";
          tooltip.GetComponent<TextMesh>().text = menuItem.toolTip.Replace('\\', '\n');
          tooltipRenderer = tooltip.GetComponent<MeshRenderer>();
          tooltip.transform.localScale = Vector3.one * TOOLTIP_TEXT_SCALE;
          tooltipOffset = TOOLTIP_TEXT_OFFSET;
        }
        tooltip.transform.SetParent(transform, false);
        tooltip.transform.localPosition = Vector3.up * tooltipOffset;

        tooltip.transform.rotation = transform.rotation;
        SetRendererAlpha(tooltipRenderer, DEFAULT_TOOLTIP_ALPHA);
      }

      parentMenu.childMenus.Add(this);
      lineToParent = GetComponent<LineRenderer>();
      lineToParent.startColor = Color.clear;
      lineToParent.endColor = Color.clear;


      UpdateLines();
      SetDecorationLabelAlpha(decorationLabelAlpha.ValueAtTime(Time.time));
      SetBackgroundTransparency(iconBgAlpha.ValueAtTime(Time.time));
      SetRendererAlpha(spriteRenderer, iconAlpha.ValueAtTime(Time.time));

      StartFade(initialState);
      SetColliderSize(0.0f);
    }

    private void UpdateSelectionOffset() {
      if (state == IconState.Closed) {
        // Closing icons changes the menu depth.
        // Prevent Closing icons from moving as they fade out.
        return;
      }
      float deepestMenuLayer = menuRoot.MenuDepth();
      // Calculates distance from camera based on the deepestIcon
      float offset = Mathf.Max(0.0f, (deepestMenuLayer - MenuLayer)) * ICON_SELECTION_OFFSET_FACTOR
        + deepestMenuLayer * MENU_SELECTION_OFFSET_FACTOR;
      selectionOffset.FadeTo(offset, ANIMATION_DURATION_SECONDS, Time.time);
    }

    private void UpdateLines() {
      if (!parentMenu) {
        return;
      }
      Vector3 parentEndpoint = parentMenu.transform.position - transform.forward * PARENT_LINE_ZOFFSET;
      Vector3 childEndpoint = transform.position - transform.forward * CHILD_LINE_ZOFFSET;
      Vector3 delta = childEndpoint - parentEndpoint;
      float shortenedLength = delta.magnitude - menuRoot.lineShorteningDistance;
      if (shortenedLength < 0) {
        lineToParent.enabled = false;
        return;
      }
      lineToParent.enabled = true;
      delta.Normalize();
      Vector3 halfLine = delta.normalized * shortenedLength * 0.5f;
      Vector3 midpoint = (childEndpoint + parentEndpoint) * 0.5f;
      lineToParentPositions[0] = midpoint - halfLine;
      lineToParentPositions[1] = midpoint + halfLine;
      lineToParent.SetPositions(lineToParentPositions);
    }

    /// Creates and shows a layer of icons below this one.
    /// Returns true if a fade needs to occur.
    public static bool ShowMenu(ConstellationMenuRoot root, AssetTree.Node treeNode, ConstellationMenuIcon parent,
                                Quaternion orientation, float scale,
                                IconState initialState = IconState.Shown) {
      // Determine how many children are in the sub-menu
      List<AssetTree.Node> childItems = treeNode.children;

      // If this is the end of a menu, invoke the action and return early
      if (childItems.Count == 0) {
        root.MakeSelection(parent.menuItem);
        root.CloseAll();
        return false;
      }
      if (parent.childMenus.Count == childItems.Count) {
        // children already exist. just fade them in.
        bool stateChanged = false;
        foreach (ConstellationMenuIcon child in parent.childMenus) {
          if (child.StartFade(initialState)) {
            stateChanged = true;
          }
          // Close any children.
          child.FadeChildren(IconState.Closed);
        }
        return stateChanged;
      }

      List<Vector3> childPositions;

      float radius;
      if (parent.parentMenu != null) {
        radius = (parent.menuCenter - parent.startPosition).magnitude + ITEM_SPACING;
        childPositions = ArrangeInCircle(childItems.Count, radius,
                                         parent.angleToRoot - Mathf.PI * .5f,
                                         parent.angleToRoot + Mathf.PI * .5f,
                                         ITEM_SPACING);
      } else {
        // Radius needs to expand when there are more icons
        radius = ITEM_SPACING * Mathf.Max(childItems.Count, MIN_ITEM_SPACE) / (2.0f * Mathf.PI);
        //  add one unused position to item positions
        childPositions = ArrangeInCircle(childItems.Count + 1, radius);
      }
      for (int i = 0; i < childItems.Count; ++i) {
        Vector3 posOffset = childPositions[i];
        ConstellationMenuIcon childMenu = (ConstellationMenuIcon)Instantiate(root.menuIconPrefab, root.transform);
        childMenu.transform.position = parent.menuCenter + (orientation * posOffset);
        childMenu.transform.rotation = orientation;
        childMenu.transform.localScale = Vector3.one * scale;
        childMenu.Initialize(root, parent, childItems[i], parent.menuCenter, scale, initialState);
        childMenu.angleToRoot = Mathf.Atan2(posOffset[1], posOffset[0]);
      }

      return true;
    }

    /// Create and arrange the icons in a circle
    private static List<Vector3> ArrangeInCircle(int numItems,
                                                 float radius,
                                                 float startingAngle = 0,
                                                 float stoppingAngle = 2.0f * Mathf.PI,
                                                 float maximumDistanceBetweenIcons = -1) {

      var results = new List<Vector3>();
      if (numItems == 1) {
        float angle = startingAngle + (stoppingAngle - startingAngle) / 2;
        results.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f) * radius);
      } else {
        float maxAngleBetweenIcons = 2.0f * Mathf.Asin(maximumDistanceBetweenIcons / (2.0f * radius));
        float angleBetweenIcons = (stoppingAngle - startingAngle) / (numItems - 1);
        if (maximumDistanceBetweenIcons > 0 && angleBetweenIcons > maxAngleBetweenIcons) {
          // Shrink the range between start_angle and end_angle so that items are closer together.
          angleBetweenIcons = maxAngleBetweenIcons;
          startingAngle = startingAngle + (stoppingAngle - startingAngle
          - maxAngleBetweenIcons * (numItems - 1)) / 2;
          stoppingAngle = startingAngle + maxAngleBetweenIcons * (numItems - 1);
        }

        for (int i = 0; i < numItems; ++i) {
          float angle = startingAngle + (i) * angleBetweenIcons;
          results.Add(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0.0f) * radius);
        }
      }
      return results;
    }

    /// Creates and shows a layer of icons below this one.
    /// Returns true if a fade starts.
    private bool ShowChildLayer(IconState state = IconState.Shown) {
      return ShowMenu(menuRoot, menuNode, this, menuOrientation, menuScale, state);
    }

    /// Closes layers deeper than this. Shows sibling icons in the same layer.
    public void ShowThisLayer(IconState initialState) {
      if (parentMenu == null) {
        // The root icon has no siblings. Close everything.
        menuRoot.CloseAll();
      } else {
        FadeChildren(IconState.Closed);
        parentMenu.ShowChildLayer(initialState);
      }
    }

    /// Fades out and closes all ConstellationMenuIcon's icons except those between the root and
    /// this node.  Ignores children nodes.
    public void HideAllExceptParentsAndChildren() {
      ConstellationMenuIcon menu = this;
      ConstellationMenuIcon parent = menu.parentMenu;
      while (parent != null) {
        foreach (var sibling in parent.childMenus) {
          if (sibling != menu) {
            sibling.FadeChildren(IconState.Closed);
            sibling.StartFade(IconState.Hidden);
          }
        }
        menu = parent;
        parent = parent.parentMenu;
      }
    }

    void Update() {
      if (state == IconState.Closed) {
        // If we are closing make sure children close first.
        FadeChildren(IconState.Closed);
        if (!IsFadeInProgress && childMenus.Count == 0) {
          // After fade finishes and children close, destroy this object
          Destroy(gameObject);
        }
      }

      // The top menu can not be selected and has no visuals attached.
      if (parentMenu == null) {
        return;
      }
      HoverIfNeeded();
      ContinueFade();
      SetDecorationLabelAlpha(decorationLabelAlpha.ValueAtTime(Time.time));
      SetBackgroundTransparency(iconBgAlpha.ValueAtTime(Time.time));
      SetRendererAlpha(tooltipRenderer, tooltipAlpha.ValueAtTime(Time.time));

      if (buttonActive) {
        if (selected && (GvrControllerInput.ClickButtonDown)) {
          menuRoot.MakeSelection(menuItem);
          if (childMenus.Count == 0) {
            menuRoot.CloseAll();
          }
        }
      }

    }

    void LateUpdate() {
      if (!parentMenu) {
        return;
      }
      // selectionOffset depends on the state of other nodes, so we change it in LateUpdate
      UpdateSelectionOffset();
      SetScaleAndPosition(iconScale.ValueAtTime(Time.time), selectionOffset.ValueAtTime(Time.time));
    }

    /// Shrink / Grow button
    // scaleAndOffset is the scale of the button and the offset from the parent
    // depth adjusts the buttons z position to move a button toward or away from viewer
    private void SetScaleAndPosition(float scaleAndOffset, float depth) {
      float scaleMult = Mathf.Max(scaleAndOffset, 0.01f);
      Vector3 scaledPosition = parentMenu.transform.position * (1.0f - scaleMult) + startPosition *
        scaleMult;
      transform.position =  scaledPosition - transform.forward * (depth);
      transform.localScale = startScale * scaleMult;
      UpdateLines();
      if (background) {
        background.transform.position = scaledPosition - transform.forward *
        (depth + BACKGROUND_ICON_ZOFFSET);
        background.transform.localScale = startScale * scaleMult;
        background.transform.rotation = transform.rotation;
      }
    }

    void SetRendererAlpha(Renderer target, float alpha) {
      if (target == null) {
        return;
      }
      var alphaColor = new Color(1.0f, 1.0f, 1.0f, alpha);
      target.GetPropertyBlock(propertyBlock);
      propertyBlock.SetColor("_Color", alphaColor);
      target.SetPropertyBlock(propertyBlock);
    }

    void SetDecorationLabelAlpha(float alpha) {
      if (decorationLabel == null) {
        return;
      }
      if (Mathf.Approximately(0.0f, alpha) ){
        if (decorationLabel.activeSelf) {
          decorationLabel.SetActive(false);
        }
        return;
      }
      if (!decorationLabel.activeSelf) {
        decorationLabel.SetActive(true);
      }
      SetRendererAlpha(decorationLabelRenderer, alpha);

    }

    void SetBackgroundTransparency(float backgroundAlpha) {
      if (!background) {
        return;
      }
      SetRendererAlpha(backgroundSpriteRenderer, backgroundAlpha);

      if (lineToParent != null && MenuLayer > 1) {
        lineToParent.startColor = CLEAR_WHITE;
        lineToParent.endColor = new Color(1.0f, 1.0f, 1.0f, backgroundAlpha);;
      }
    }

    private void SetColliderSize(float size) {
      boxCollider.enabled = !Mathf.Approximately(0.0f, size);
      boxCollider.size = new Vector3(size, size, Mathf.Min(size,BACKGROUND_ICON_ZOFFSET));
    }

    void OnDestroy() {
      if (background) {
        Destroy(background);
      }
      if (tooltip) {
        Destroy(tooltip);
      }
      if (parentMenu) {
        parentMenu.childMenus.Remove(this);
      }
    }

    private bool FadeChildren(IconState nextState) {
      bool stateChanged = false;
      foreach (ConstellationMenuIcon child in childMenus) {
        if (child.StartFade(nextState)) {
          stateChanged = true;
        }
      }
      return stateChanged;
    }

    private bool StartFade(IconState nextState) {
      if(state == nextState) {
        return false;
      }

      if (nextState == IconState.Closed && childMenus.Count > 0) {
        FadeChildren(IconState.Closed);
      }
      state = nextState;
      fadeInProgress = true;

      if (nextState == IconState.Closed) {
        menuRoot.MarkGraphDirty();
      }

      fadeParameters = new FadeParameters(nextState);

      buttonActive = fadeParameters.buttonActive;
      SetColliderSize(Mathf.Min(boxCollider.size.x, fadeParameters.colliderSize));

      menuRoot.StartFadeOnIcon(this, nextState);
      ConstellationMenuIcon hoverIcon = (menuRoot == null) ? null : menuRoot.GetHoverIcon();
      // Change the background based on the relationship between this icon and the hoverIcon
      var relationship = GetRelationship(hoverIcon);
      tooltipAlpha.FadeTo(GetTooltipAlpha(relationship),
                          ANIMATION_DURATION_SECONDS, Time.time);
      iconBgAlpha.FadeTo(GetBackgroundAlpha(relationship),
                         ANIMATION_DURATION_SECONDS,
                         Time.time);
      decorationLabelAlpha.FadeTo(GetDecorationLabelAlpha(relationship),
                                ANIMATION_DURATION_SECONDS, Time.time);
      iconScale.FadeTo(SHOWN_ICON_SCALE,
                       ANIMATION_DURATION_SECONDS,
                       Time.time);
      iconAlpha.FadeTo(fadeParameters.alpha,
                       ANIMATION_DURATION_SECONDS,
                       Time.time);
      return true;
    }

    /// It is easy or difficult to hover based on the relationship to the most recent hover icon
    float GetHoverDelay(IconRelationship relationship) {
      switch (relationship) {
      case IconRelationship.DescendantOfRhs:
      case IconRelationship.AncestorOfRhs:
        return 0.0f;
      default:
        return menuRoot.hoverDelay;
      }
    }

    /// The decoration label is only shown if the icon has children and is a descendant of the hover
    /// icon.  DescendantOfRhs is also used if hover icon is null.
    float GetDecorationLabelAlpha(IconRelationship relationship) {
      if (state == IconState.Hovering ||
          state == IconState.Closed ||
          relationship != IconRelationship.DescendantOfRhs ||
          menuNode.children.Count == 0) {
        return DEFAULT_DECORATION_LABEL_ALPHA;
      }

      return DESCENDANT_DECORATION_LABEL_ALPHA;
    }

    /// Background is determined by the relationship to the most recent hover Icon
    private float GetBackgroundAlpha(IconRelationship relationship) {
      if (state == IconState.Closed) {
        return CLOSED_ICON_BG_ALPHA;
      }
      float newAlpha;
      switch (relationship) {
      case IconRelationship.DescendantOfRhs:
        newAlpha = DESCENDANT_ICON_BG_ALPHA;
        break;
      case IconRelationship.AncestorOfRhs:
        newAlpha = ANCESTOR_ICON_BG_ALPHA;
        break;
      default:
        newAlpha = DEFAULT_ICON_BG_ALPHA;
        break;
      }
      return newAlpha;
    }

    private float GetTooltipAlpha(IconRelationship relationship) {
      if (state == IconState.Closed) {
        return DEFAULT_TOOLTIP_ALPHA;
      }
      float newAlpha;
      switch (relationship) {
      case IconRelationship.DescendantOfRhs:
      case IconRelationship.AncestorOfRhs:
        newAlpha = SELECTED_PATH_TOOLTIP_ALPHA;
        break;
      default:
        newAlpha = DEFAULT_TOOLTIP_ALPHA;
        break;
      }
      return newAlpha;
    }


    /// Start hovering if the icon has been selected long enough
    void HoverIfNeeded() {
      if (!selected) {
        return;
      }
      if (!(hoverStartTime < selectionStartTime)) {
        return;
      }
      if ((Time.time - selectionStartTime) < hoverDelaySeconds) {
        return;
      }

      hoverStartTime = Time.time;
      menuRoot.MakeHover(menuItem);
      if (state == IconState.Hidden) {
        parentMenu.ShowChildLayer(IconState.Hidden);
        StartFade(IconState.Hovering);
      } else {
        FadeIfNeeded(IconState.Shown, IconState.Hovering);
      }
      if (menuNode.children.Count > 0) {
        ShowChildLayer(IconState.Hidden);
      }

      // If we just displayed children
      // then hide everything that isn't needed.
      // always do hideall on hover.
      HideAllExceptParentsAndChildren();
    }

    /// Called recursively when any icon starts to hover.
    public void OnHoverChange(ConstellationMenuIcon hoverIcon) {
      foreach (var child in childMenus) {
        child.OnHoverChange(hoverIcon);
      }

      var relationship = GetRelationship(hoverIcon);
      decorationLabelAlpha.FadeTo(GetDecorationLabelAlpha(relationship),
                                  ANIMATION_DURATION_SECONDS, Time.time);
      // Change the background based on the relationship between this icon and the hoverIcon
      iconBgAlpha.FadeTo(GetBackgroundAlpha(relationship),
                         ANIMATION_DURATION_SECONDS, Time.time);

      // Change the tooltip based on the relationship between this icon and the hoverIcon
      tooltipAlpha.FadeTo(GetTooltipAlpha(relationship),
                         ANIMATION_DURATION_SECONDS, Time.time);

      // Set how easy/difficult it is to hover based on this relationship
      hoverDelaySeconds = GetHoverDelay(relationship);
    }

    /// Returns IconRelationship.
    /// AncestorOfRhs if this class is a descendant of relative (or they are the same)
    /// DescendantOfRhs if the opposite is true (or rhsIcon is null)
    /// or UnkownRelationship if neither is true.
    public IconRelationship GetRelationship(ConstellationMenuIcon rhsIcon) {
      if (rhsIcon == null || IsDescendentOf(rhsIcon)) {
        return IconRelationship.DescendantOfRhs;
      } else if (rhsIcon.IsDescendentOf(this)) {
        return IconRelationship.AncestorOfRhs;
      } else {
        return IconRelationship.UnkownRelationship;
      }
    }

    /// Recursive call to find the child at the deepest MenuLayer
    public ConstellationMenuIcon GetDeepestIcon() {
      ConstellationMenuIcon returnValue = this;
      foreach (var child in childMenus) {
        if (child.state == IconState.Closed) {
          continue;
        }
        ConstellationMenuIcon otherIcon = child.GetDeepestIcon();
        if (otherIcon.MenuLayer > returnValue.MenuLayer) {
          returnValue = otherIcon;
        }
      }
      return returnValue;
    }

    public bool IsDescendentOf(ConstellationMenuIcon parent) {
      ConstellationMenuIcon child = this;
      while (child != null) {
        if (child == parent) {
          return true;
        }
        child = child.parentMenu;
      }
      return false;
    }

    // Apply any logic attached to the end of changing the icon's alpha.
    private void FinishFade() {
      fadeInProgress = false;
      SetColliderSize(fadeParameters.colliderSize);
    }

    /// Updates icon with interpolated fade values.
    private void ContinueFade() {
      if (!IsFadeInProgress) {
        // no fade in progress. Do nothing.
        return;
      }

      SetRendererAlpha(spriteRenderer, iconAlpha.ValueAtTime(Time.time));

      if (state == IconState.Closed) {
        selected = false;
      } else if (state == IconState.Hidden) {
        selected = false;
      }

      if (!(iconAlpha.GetProgress(Time.time) < 1.0f) &&
          !(iconBgAlpha.GetProgress(Time.time) < 1.0f) &&
          !(decorationLabelAlpha.GetProgress(Time.time) < 1.0f)) {
        FinishFade();
        return;
      }
    }

    /// Starts a fade if the there is a match for fromState
    public void FadeIfNeeded(IconState fromState,
                             IconState toState) {
      if (state != fromState) {
        return;
      }
      StartFade(toState);
    }

    /// Recursively closes this menu and its children
    public void CloseRecursive() {
      foreach (ConstellationMenuIcon child in childMenus) {
        child.CloseRecursive();
      }
      StartFade(IconState.Closed);
    }

    public void OnPointerEnter(PointerEventData eventData) {
      if (parentMenu == null) {
        return;
      }
      if (!buttonActive) {
        return;
      }
      if (!selected) {
        selected = true;
        selectionStartTime = Time.time;
      }
    }


    public void OnPointerExit(PointerEventData eventData) {
      if (parentMenu == null) {
        return;
      }
      selected = false;
    }

    /// Recursively calculates the angle between a ray and the icons.
    public float GetClosestAngle(Vector3 originPosition,
                                 Vector3 normalizedRayFromOrigin) {
      Vector3 iconToOrigin = transform.position - originPosition;
      float bestAngle = Vector3.Angle(normalizedRayFromOrigin, iconToOrigin.normalized);
      foreach (var child in childMenus) {
        float childAngle = child.GetClosestAngle(originPosition, normalizedRayFromOrigin);
        bestAngle = Mathf.Min(childAngle, bestAngle);
      }
      return bestAngle;
    }
  }





}
