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
using UnityEngine.UI;

namespace DaydreamElements.ArmModels {

  public class ScalarSliderUI : MonoBehaviour {
    public Text scalarLabel;
    public Slider scalarSlider;

    public event Action OnChanged;
    public event Action OnSliderReleased;
    public event Action OnSliderEntered;
    public event Action OnSliderExited;

    [Range(0, 10)]
    public int decimalPlaces = 3;

    public float Scalar {
      get {
        return RoundToDecimals(scalarSlider.value);
      }
      set {
        scalarSlider.value = value;
      }
    }

    void OnEnable() {
      scalarSlider.onValueChanged.AddListener(OnValueChanged);
      AddHandlers(scalarSlider);
    }

    void OnDisable() {
      scalarSlider.onValueChanged.RemoveListener(OnValueChanged);
      RemoveHandlers(scalarSlider);
    }

    private void AddHandlers(Slider slider) {
      SliderCallbacks callback = slider.GetComponent<SliderCallbacks>();
      if (callback == null) {
        callback = slider.gameObject.AddComponent<SliderCallbacks>();
      }

      callback.OnEndDragCallback += OnEndDrag;
      callback.OnEnterCallback += OnEnter;
      callback.OnExitCallback += OnExit;
    }

    private void RemoveHandlers(Slider slider) {
      SliderCallbacks callback = slider.GetComponent<SliderCallbacks>();
      if (callback != null) {
        callback.OnEndDragCallback -= OnEndDrag;
        callback.OnEnterCallback -= OnEnter;
        callback.OnExitCallback -= OnExit;
      }
    }

    private void OnValueChanged(float value) {
      scalarLabel.text = "=" + RoundToDecimals(value);
      if (OnChanged != null) {
        OnChanged();
      }
    }

    private void OnEndDrag() {
      if (OnSliderReleased != null) {
        OnSliderReleased();
      }
    }

    private void OnEnter() {
      if (OnSliderEntered != null) {
        OnSliderEntered();
      }
    }

    private void OnExit() {
      if (OnSliderExited != null) {
        OnSliderExited();
      }
    }

    private float RoundToDecimals(float toRound) {
      if (decimalPlaces < 1) {
        return Mathf.Round(toRound);
      }

      float factor = Mathf.Pow(10.0f, decimalPlaces);
      return Mathf.Round(toRound * factor) / factor;
    }
  }
}
