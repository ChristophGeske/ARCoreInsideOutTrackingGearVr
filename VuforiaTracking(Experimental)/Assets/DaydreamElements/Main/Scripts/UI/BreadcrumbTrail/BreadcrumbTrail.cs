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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

namespace DaydreamElements.Main {

  /// This script is for tracking and visualizing navigation through
  /// hierarchical content. When going a layer deeper into the hierarchy,
  /// call AddBreadcrumb.
  public class BreadcrumbTrail : MonoBehaviour {
    public event Action<BreadcrumbData> OnBreadcrumbChosen;

    [SerializeField]
    private GameObject breadcrumbPrefab;
  
    [SerializeField]
    private GameObject separatorPrefab;
  
    [SerializeField]
    private GameObject ellipsisPrefab;
  
    [SerializeField]
    private RectTransform breadcrumbContainter;
  
    [SerializeField]
    [Range(1, 10)]
    private int maxBreadcrumbs = 5;
  
    private List<Breadcrumb> breadcrumbs = new List<Breadcrumb>();
    private List<GameObject> separators = new List<GameObject>();
    private GameObject ellipsis;

    public void AddBreadcrumb(BreadcrumbData breadcrumbData) {
      // If this isn't the first breadcrumb, we need to add a separator.
      if (breadcrumbs.Count > 0) {
        AddSeparator();
      }
  
      GameObject breadcrumbObj = GameObject.Instantiate(breadcrumbPrefab);
      Breadcrumb breadcrumb = breadcrumbObj.GetComponent<Breadcrumb>();
      Assert.IsNotNull(breadcrumb);
      breadcrumb.OnBreadcrumbClicked += OnBreadcrumbClicked;
      breadcrumb.transform.SetParent(breadcrumbContainter, false);
      breadcrumb.Setup(breadcrumbData);
      breadcrumbs.Add(breadcrumb);
  
      if (breadcrumbs.Count > 1) {
        breadcrumb.AnimateIn();
      }
  
      RefreshBreadcrumbs();
    }

    public void ClearBreadcrumbs() {
      foreach (Breadcrumb breadcrumb in breadcrumbs) {
        Destroy(breadcrumb.gameObject);
      }
      breadcrumbs.Clear();
  
      foreach (GameObject separator in separators) {
        Destroy(separator);
      }
      separators.Clear();
  
      RefreshBreadcrumbs();
  
      breadcrumbContainter.DetachChildren();
    }

    public void SetBreadcrumbs(List<BreadcrumbData> breadcrumbData) {
      ClearBreadcrumbs();
      foreach (BreadcrumbData data in breadcrumbData) {
        AddBreadcrumb(data);
      }
    }

    void Awake() {
      Breadcrumb breadcrumb = breadcrumbPrefab.GetComponent<Breadcrumb>();
      Assert.IsNotNull(breadcrumb);
    }

    void LateUpdate() {
      if (GvrControllerInput.AppButtonDown) {
        if (breadcrumbs.Count > 1) {
          OnBreadcrumbClicked(breadcrumbs[breadcrumbs.Count - 2].Data);
        }
      }
    }

    private void RefreshBreadcrumbs() {
      if (breadcrumbs.Count > maxBreadcrumbs) {
        if (ellipsis == null) {
          ellipsis = GameObject.Instantiate(ellipsisPrefab);
          ellipsis.transform.SetParent(breadcrumbContainter, false);
        }
  
        // Ellipsis is always the third element.
        // The first element is the first breadcrumb.
        // The second element is the first separator.
        ellipsis.transform.SetSiblingIndex(2);
  
        for (int i = 1; i < breadcrumbs.Count; i++) {
          bool breadcrumbActive = i > breadcrumbs.Count - maxBreadcrumbs + 1;
          breadcrumbs[i].gameObject.SetActive(breadcrumbActive);
        }
      } else {
        if (ellipsis != null) {
          Destroy(ellipsis);
          ellipsis = null;
        }
  
        for (int i = 0; i < breadcrumbs.Count; i++) {
          breadcrumbs[i].gameObject.SetActive(true);
        }
      }
  
      for (int i = 0; i < breadcrumbs.Count; i++) {
        Breadcrumb crumb = breadcrumbs[i];
        crumb.Refresh(i, breadcrumbs.Count);
      }
    }

    private void OnBreadcrumbClicked(BreadcrumbData data) {
      for (int i = breadcrumbs.Count - 1; i >= 0; i--) {
        Breadcrumb breadcrumb = breadcrumbs[i];
        if (breadcrumb.Data == data) {
          break;
        }
  
        breadcrumbs.RemoveAt(i);
        breadcrumb.AnimateOutAndDestroy();
  
        if (separators.Count > 0) {
          int separatorIndex = separators.Count - 1;
          GameObject separator = separators[separatorIndex];
          separators.RemoveAt(separatorIndex);
          GameObject.Destroy(separator);
        }
      }
  
      for (int i = 0; i < breadcrumbs.Count; i++) {
        Breadcrumb crumb = breadcrumbs[i];
        crumb.Refresh(i, breadcrumbs.Count);
      }
  
      if (OnBreadcrumbChosen != null) {
        OnBreadcrumbChosen(data);
      }
    }

    private void AddSeparator() {
      GameObject separator = GameObject.Instantiate(separatorPrefab);
      separator.transform.SetParent(breadcrumbContainter, false);
      separators.Add(separator);
    }
  }
}
