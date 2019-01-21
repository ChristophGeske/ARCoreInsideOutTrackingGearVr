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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DaydreamElements.Common;

namespace DaydreamElements.ArmModels {

  /// Selects an specified Arm Model mode when clicked on.
  public class ModeSelector : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
    [Tooltip("The arm model mode to select.")]
    public ModeController.Mode mode;

    [Tooltip("The mode controller to set the mode on.")]
    public ModeController controller;

    [Tooltip("Scaled up when the selector is highlighted, down when it is not.")]
    public Transform highlightGlow;

    [Tooltip("Scale of the highlight when the selector isn't being pointed at.")]
    public float highlightDisabledScale = 1.0f;

    [Tooltip("Scale of the highlight when the selector is being pointed at.")]
    public float highlightEnabledScale = 5.0f;

    [Tooltip("Speed that the highlight lerps to it's target scale.")]
    public float highlightScaleSpeed = 12.0f;

    [Tooltip("Object that spins when the mode is selected with the pointer.")]
    public Transform spinningObject;

    [Tooltip("Rate at which the spin object spins about it's y-axis in degrees.")]
    public float spinDegreesPerSecond = 45.0f;

    [Tooltip("Rate at which the spin object lerps back to the original position.")]
    public float endSpinSpeed = 8.0f;

    private float targetHighlightScale;
    private bool shouldSpin;
    private GvrAudioSource sound;
    private float initialVolume;
    private AudioFader audioFader;

    public void OnPointerClick(PointerEventData eventData) {
      if (controller == null) {
        return;
      }

      controller.CurrentMode = mode;
    }

    public void OnPointerEnter(PointerEventData eventData) {
      SetHighlightEnabled(true);
      shouldSpin = true;

      if (sound != null && !sound.isPlaying) {
        sound.Play();
      }

      if (audioFader != null) {
        audioFader.targetVolume = initialVolume;
      }
    }

    public void OnPointerExit(PointerEventData eventData) {
      SetHighlightEnabled(false);
      shouldSpin = false;
      if (audioFader != null) {
        audioFader.targetVolume = 0.0f;
      }
    }

    void OnEnable() {
      SetHighlightEnabledImmediate(false);
      shouldSpin = false;
      if (spinningObject != null) {
        spinningObject.localRotation = Quaternion.identity;
      }
    }

    void Awake() {
      sound = GetComponent<GvrAudioSource>();
      if (sound != null) {
        initialVolume = sound.volume;
      }

      audioFader = GetComponent<AudioFader>();
    }

    void Update() {
      if (highlightGlow == null) {
        return;
      }

      Vector3 currentScale = highlightGlow.localScale;
      currentScale.y = Mathf.Lerp(currentScale.y, targetHighlightScale, highlightScaleSpeed * Time.deltaTime);
      highlightGlow.localScale = currentScale;

      UpdateSpin();

      if (sound != null && sound.volume == 0.0f
        && audioFader != null && audioFader.targetVolume == 0.0f) {
        sound.Stop();
      }
    }

    private void UpdateSpin() {
      if (spinningObject == null) {
        return;
      }

      if (shouldSpin) {
        Vector3 euler = spinningObject.eulerAngles;
        euler.y += spinDegreesPerSecond * Time.deltaTime;
        spinningObject.eulerAngles = euler;
      } else {
        spinningObject.rotation =
          Quaternion.Lerp(spinningObject.localRotation, Quaternion.identity, endSpinSpeed * Time.deltaTime);
      }
    }

    private void SetHighlightEnabled(bool active) {
      if (active) {
        targetHighlightScale = highlightEnabledScale;
      } else {
        targetHighlightScale = highlightDisabledScale;
      }
    }

    private void SetHighlightEnabledImmediate(bool active) {
      SetHighlightEnabled(active);

      if (highlightGlow != null) {
        Vector3 currentScale = highlightGlow.localScale;
        currentScale.y = targetHighlightScale;
        highlightGlow.localScale = currentScale;
      }
    }
  }
}