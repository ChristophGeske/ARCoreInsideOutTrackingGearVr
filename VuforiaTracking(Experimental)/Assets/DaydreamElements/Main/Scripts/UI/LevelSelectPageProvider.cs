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
using UnityEngine.Assertions;
using System.Collections;
using DaydreamElements.Common;

namespace DaydreamElements.Main {

  /// Custom page provider used by PagedScrollRect to build the pages of
  /// the level select menu.
  public class LevelSelectPageProvider : MonoBehaviour, IPageProvider {
    /// The spacing between pages in local coordinates.
    [Tooltip("The spacing between pages.")]
    public float spacing = 2000.0f;

    [SerializeField]
    private GameObject pagePrefab;

    [SerializeField]
    private LevelSelectHierarchyData menuHierarchy;

    [SerializeField]
    private BreadcrumbTrail breadcrumbTrail;

    private AssetTree.Node currentNode;
    private int pageCount;

    private const string HOME_NAME = "Elements";

    public float GetSpacing() {
      return spacing;
    }

    public int GetNumberOfPages() {
      return pageCount;
    }

    public int MaxChildrenPerPage {
      get {
        if (pagePrefab == null) {
          Debug.LogError("pagePrefab is not set.");
          return 0;
        }

        LevelSelectPage page = pagePrefab.GetComponent<LevelSelectPage>();
        if (page == null) {
          Debug.LogError("pagePrefab does not contain FileSelectorPage component.");
          return 0;
        }

        return page.MaxButtonsPerPage;
      }
    }

    public AssetTree.Node CurrentNode {
      get {
        return currentNode;
      }
      set {
        currentNode = value;

        pageCount = Mathf.CeilToInt((float)currentNode.children.Count / (float)MaxChildrenPerPage);
        pageCount = Mathf.Max(pageCount, 1);

        // If the PagedScrollRect already has active pages we need to Reset it
        // So that it will pick up the change to the working directories.
        PagedScrollRect scrollRect = GetComponent<PagedScrollRect>();
        if (scrollRect != null && scrollRect.ActivePage != null) {
          scrollRect.Reset();
        }
      }
    }

    public bool IsOnRootPage {
      get {
        return CurrentNode == menuHierarchy.ButtonTree.Root;
      }
    }

    public RectTransform ProvidePage(int index) {
      GameObject pageTransform = GameObject.Instantiate(pagePrefab);
      RectTransform page = pageTransform.GetComponent<RectTransform>();

      Vector2 middleAnchor = new Vector2(0.5f, 0.5f);
      page.anchorMax = middleAnchor;
      page.anchorMin = middleAnchor;

      LevelSelectPage levelSelectPage = page.GetComponent<LevelSelectPage>();
      Assert.IsNotNull(levelSelectPage);

      int maxChildrenPerPage = MaxChildrenPerPage;
      int startingChildIndex = index * maxChildrenPerPage;
      int childIndex = startingChildIndex;

      int childCount = CurrentNode.children.Count;

      while (childIndex < startingChildIndex + maxChildrenPerPage && childIndex < childCount) {
        LevelSelectButton tile;

        tile = levelSelectPage.AddButton(CurrentNode.children[childIndex].value as LevelSelectButtonData);
        tile.OnButtonClicked += OnButtonClicked;

        childIndex++;
      }

      return page;
    }

    public void RemovePage(int index, RectTransform page) {
      GameObject.Destroy(page.gameObject);
    }

    void Awake() {
      CurrentNode = menuHierarchy.ButtonTree.Root;
      breadcrumbTrail.OnBreadcrumbChosen += OnBreadcrumbChosen;
    }

    void Start() {
      BreadcrumbData breadcrumbData = new BreadcrumbData();
      breadcrumbData.displayName = HOME_NAME;
      breadcrumbData.name = (CurrentNode.value as LevelSelectButtonData).DisplayName;
      breadcrumbTrail.AddBreadcrumb(breadcrumbData);
    }

    void OnDestroy() {
      if (breadcrumbTrail != null) {
        breadcrumbTrail.OnBreadcrumbChosen -= OnBreadcrumbChosen;
      }
    }

    private void OnBreadcrumbChosen(BreadcrumbData data) {
      AssetTree.Node node = CurrentNode.parent;
      while (node != null) {
        LevelSelectButtonData buttonData = node.value as LevelSelectButtonData;
        if (buttonData.DisplayName == data.name) {
          CurrentNode = node;
          break;
        }

        node = node.parent;
      }
    }

    private void OnButtonClicked(LevelSelectButtonData data) {
      if (!string.IsNullOrEmpty(data.SceneName)) {
        return;
      }

      BreadcrumbData breadcrumbData = new BreadcrumbData();
      breadcrumbData.displayName = data.DisplayName;
      breadcrumbData.name = data.DisplayName;
      breadcrumbTrail.AddBreadcrumb(breadcrumbData);

      foreach (AssetTree.Node node in CurrentNode.children) {
        LevelSelectButtonData nodeData = node.value as LevelSelectButtonData;
        if (nodeData == data) {
          CurrentNode = node;
          break;
        }
      }
    }
  }
}
