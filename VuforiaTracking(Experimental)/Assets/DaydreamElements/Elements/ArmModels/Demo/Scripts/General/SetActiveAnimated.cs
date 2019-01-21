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

namespace DaydreamElements.ArmModels {

  /// Used to animate the scale of a set of objects when a GameObject is enabled/disabled.
  public class SetActiveAnimated : MonoBehaviour {
    [Serializable]
    private class Entry {
      public Transform target = null;
      public float delaySeconds = 0.2f;
      public float deactivateDelaySeconds = 0.0f;
      public float animationDuration = 0.2f;
      public Vector3 disabledScale = Vector3.zero;
      public float disabledAlpha = 1.0f;

      [NonSerialized]
      public Vector3 initialScale;

      [NonSerialized]
      public MeshRenderer[] renderers;

      [NonSerialized]
      public CanvasGroup[] canvasGroups;
    }

    [SerializeField]
    private Entry[] entries;

    private bool currentlyActive;
    private int runningAnimations;
    private MaterialPropertyBlock materialPropertyBlock;
    private int alphaId;
    private int colorId;

    private delegate float EaseFunction(float ratio);

    public void SetActive(bool active) {
      if (currentlyActive == active) {
        return;
      }

      if (active) {
        if (gameObject.activeSelf) {
          StartActivateAnimations();
        } else {
          gameObject.SetActive(true);
        }
      } else {
        StartDeactivateAnimations();
      }

      currentlyActive = active;
    }

    void Awake() {
      materialPropertyBlock = new MaterialPropertyBlock();
      alphaId = Shader.PropertyToID("_Alpha");
      colorId = Shader.PropertyToID("_Color");

      for (int i = 0; i < entries.Length; i++) {
        Entry entry = entries[i];
        if (entry.target != null) {
          entry.initialScale = entry.target.localScale;
          entry.renderers = entry.target.GetComponentsInChildren<MeshRenderer>();
          entry.canvasGroups = entry.target.GetComponentsInChildren<CanvasGroup>();
        }
      }
    }

    void OnEnable() {
      StartActivateAnimations();
    }

    void Update() {
      if (!currentlyActive && runningAnimations <= 0) {
        gameObject.SetActive(false);
      }
    }

    private void StartActivateAnimations() {
      StopAllCoroutines();
      runningAnimations = 0;

      for (int i = 0; i < entries.Length; i++) {
        StartCoroutine(ActivateAnimation(entries[i], EasingFunctions.RatioEaseOutCubic, true));
      }
    }

    private void StartDeactivateAnimations() {
      StopAllCoroutines();
      runningAnimations = 0;

      for (int i = 0; i < entries.Length; i++) {
        StartCoroutine(ActivateAnimation(entries[i], EasingFunctions.RatioEaseOutCubic, false));
      }
    }

    private IEnumerator ActivateAnimation(Entry entry, EaseFunction easeFunction, bool activate) {
      runningAnimations++;
      Vector3 finalDisabledScale = Vector3.Scale(entry.initialScale, entry.disabledScale);
      Vector3 targetScale = activate ? entry.initialScale : finalDisabledScale;
      Vector3 startScale = activate ? finalDisabledScale : entry.initialScale;
      float targetAlpha = activate ? 1.0f : entry.disabledAlpha;
      float startAlpha = activate ? entry.disabledAlpha : 1.0f;

      if (entry.target != null) {
        entry.target.localScale = startScale;
        SetAlphaForEntry(entry, startAlpha);
      }

      if (activate) {
        yield return new WaitForSeconds(entry.delaySeconds);
      } else {
        yield return new WaitForSeconds(entry.deactivateDelaySeconds);
      }

      float elapsedTime = 0.0f;
      Vector3 scaleDiff = entry.initialScale - finalDisabledScale;
      while (elapsedTime < entry.animationDuration) {
        elapsedTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(elapsedTime / entry.animationDuration);
        if (!activate) {
          ratio = 1.0f - ratio;
        }

        if (entry.target != null) {
          float easedRatio = easeFunction(ratio);
          entry.target.localScale = finalDisabledScale + (scaleDiff * easedRatio);

          float range = 1.0f - entry.disabledAlpha;
          float alpha = entry.disabledAlpha + (range * easedRatio);
          SetAlphaForEntry(entry, alpha);
        }
        yield return null;
      }

      if (entry.target != null) {
        entry.target.localScale = targetScale;
        SetAlphaForEntry(entry, targetAlpha);
      }

      runningAnimations--;
    }

    private void SetAlphaForEntry(Entry entry, float alpha) {
      if (entry.renderers == null) {
        return;
      }

      for (int i = 0; i < entry.renderers.Length; i++) {
        MeshRenderer meshRenderer = entry.renderers[i];
        meshRenderer.GetPropertyBlock(materialPropertyBlock);

        meshRenderer.enabled = alpha != 0.0f;

        if (meshRenderer.sharedMaterial.HasProperty(alphaId)) {
          materialPropertyBlock.SetFloat(alphaId, alpha);
        }

        if (meshRenderer.sharedMaterial.HasProperty(colorId)) {
          Color color = meshRenderer.sharedMaterial.color;
          color.a = color.a * alpha;
          materialPropertyBlock.SetColor(colorId, color);
        }

        meshRenderer.SetPropertyBlock(materialPropertyBlock);
      }

      for (int i = 0; i < entry.canvasGroups.Length; i++) {
        CanvasGroup canvasGroup = entry.canvasGroups[i];
        canvasGroup.alpha = alpha;
      }
    }
  }
}