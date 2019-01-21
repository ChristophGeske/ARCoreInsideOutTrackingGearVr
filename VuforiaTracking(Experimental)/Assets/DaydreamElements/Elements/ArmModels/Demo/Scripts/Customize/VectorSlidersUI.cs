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

  public class VectorSlidersUI : MonoBehaviour {
    public Text xLabel;
    public Slider xSlider;
    public Text yLabel;
    public Slider ySlider;
    public Text zLabel;
    public Slider zSlider;

    public event Action OnChanged;
    public event Action OnSlidersReleased;
    public event Action OnSliderEntered;
    public event Action OnSliderExited;

    [Range(0, 10)]
    public int decimalPlaces = 3;

    public Vector3 Vector {
      get {
        return new Vector3(RoundToDecimals(xSlider.value), RoundToDecimals(ySlider.value),
          RoundToDecimals(zSlider.value));
      }
      set {
        xSlider.value = value.x;
        ySlider.value = value.y;
        zSlider.value = value.z;
      }
    }

    void OnEnable() {
      xSlider.onValueChanged.AddListener(OnXValueChanged);
      ySlider.onValueChanged.AddListener(OnYValueChanged);
      zSlider.onValueChanged.AddListener(OnZValueChanged);

      AddHandlers(xSlider);
      AddHandlers(ySlider);
      AddHandlers(zSlider);

      OnXValueChanged(xSlider.value);
      OnYValueChanged(ySlider.value);
      OnZValueChanged(zSlider.value);
    }

    void OnDisable() {
      xSlider.onValueChanged.RemoveListener(OnXValueChanged);
      ySlider.onValueChanged.RemoveListener(OnYValueChanged);
      zSlider.onValueChanged.RemoveListener(OnZValueChanged);

      RemoveHandlers(xSlider);
      RemoveHandlers(ySlider);
      RemoveHandlers(zSlider);
    }

    private void OnXValueChanged(float value) {
      OnValueChanged(xLabel, value, "X=");
    }

    private void OnYValueChanged(float value) {
      OnValueChanged(yLabel, value, "Y=");
    }

    private void OnZValueChanged(float value) {
      OnValueChanged(zLabel, value, "Z=");
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

    private void OnValueChanged(Text label, float value, string prefix) {
      label.text = prefix + RoundToDecimals(value);
      if (OnChanged != null) {
        OnChanged();
      }
    }

    private void OnEndDrag() {
      if (OnSlidersReleased != null) {
        OnSlidersReleased();
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
