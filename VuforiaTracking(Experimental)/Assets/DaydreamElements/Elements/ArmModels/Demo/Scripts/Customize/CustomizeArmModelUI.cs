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

  /// UI for modifying the properties of _GvrArmModel.cs_ in real-time.
  public class CustomizeArmModelUI : MonoBehaviour {

    [Header("Arm")]
    public GvrArmModel defaultArmModel;
    public GvrArmModel dummyArmModel;
    public GameObject[] dummyObjects;
    public VisualizeArmModel dummyArmVisualizer;
    public Transform defaultLaser;
    public Transform dummyLaser;

    [Header("UI")]
    public VectorSlidersUI elbowRestSliders;
    public VectorSlidersUI wristRestSliders;
    public VectorSlidersUI controllerRestSliders;
    public VectorSlidersUI armExtensionSliders;
    public ScalarSliderUI elbowBendRatioSlider;
    public ScalarSliderUI pointerTiltAngleSlider;
    public Button startOverButton;

    private bool highlightsLocked = false;

    void OnEnable() {
      elbowRestSliders.OnChanged += OnUIChanged;
      elbowRestSliders.OnSlidersReleased += OnSlidersReleased;

      wristRestSliders.OnChanged += OnUIChanged;
      wristRestSliders.OnSlidersReleased += OnSlidersReleased;

      controllerRestSliders.OnChanged += OnUIChanged;
      controllerRestSliders.OnSlidersReleased += OnSlidersReleased;

      armExtensionSliders.OnChanged += OnUIChanged;
      armExtensionSliders.OnSlidersReleased += OnSlidersReleased;

      elbowBendRatioSlider.OnChanged += OnUIChanged;
      elbowBendRatioSlider.OnSliderReleased += OnSlidersReleased;

      pointerTiltAngleSlider.OnChanged += OnUIChanged;
      pointerTiltAngleSlider.OnSliderReleased += OnSlidersReleased;

      SetHighlightCallbacksEnabled(true);

      Reset();
    }

    void OnDisable() {
      elbowRestSliders.OnChanged -= OnUIChanged;
      elbowRestSliders.OnSlidersReleased -= OnSlidersReleased;

      wristRestSliders.OnChanged -= OnUIChanged;
      wristRestSliders.OnSlidersReleased -= OnSlidersReleased;

      controllerRestSliders.OnChanged -= OnUIChanged;
      controllerRestSliders.OnSlidersReleased -= OnSlidersReleased;

      armExtensionSliders.OnChanged -= OnUIChanged;
      armExtensionSliders.OnSlidersReleased -= OnSlidersReleased;

      elbowBendRatioSlider.OnChanged -= OnUIChanged;
      elbowBendRatioSlider.OnSliderReleased -= OnSlidersReleased;

      pointerTiltAngleSlider.OnChanged -= OnUIChanged;
      pointerTiltAngleSlider.OnSliderReleased -= OnSlidersReleased;

      SetHighlightCallbacksEnabled(false);
    }

    public void Reset() {
      SetFromArmModel(defaultArmModel, defaultLaser);
      startOverButton.interactable = false;
      dummyArmVisualizer.SetAllOutlinesEnabled(false);
      highlightsLocked = false;
    }

    public void SetArmModelFromUI(GvrArmModel arm, Transform laser) {
      arm.elbowRestPosition = elbowRestSliders.Vector;
      arm.wristRestPosition = wristRestSliders.Vector;
      arm.controllerRestPosition = controllerRestSliders.Vector;
      arm.armExtensionOffset = armExtensionSliders.Vector;
      arm.elbowBendRatio = elbowBendRatioSlider.Scalar;

      Vector3 euler = laser.localEulerAngles;
      euler.x = pointerTiltAngleSlider.Scalar;
      laser.localEulerAngles = euler;
    }

    private void SetFromArmModel(GvrArmModel arm, Transform laser) {
      elbowRestSliders.Vector = arm.elbowRestPosition;
      wristRestSliders.Vector = arm.wristRestPosition;
      controllerRestSliders.Vector = arm.controllerRestPosition;
      armExtensionSliders.Vector = arm.armExtensionOffset;
      elbowBendRatioSlider.Scalar = arm.elbowBendRatio;
      pointerTiltAngleSlider.Scalar = laser.localEulerAngles.x;
    }

    private void SetHighlightCallbacksEnabled(bool enabled) {
      if (enabled) {
        elbowRestSliders.OnSliderEntered += HighlightElbow;
        elbowRestSliders.OnSliderExited += UnhighlightElbow;
        wristRestSliders.OnSliderEntered += HighlightWrist;
        wristRestSliders.OnSliderExited += UnhighlightWrist;
        armExtensionSliders.OnSliderEntered += HighlightExtension;
        armExtensionSliders.OnSliderExited += UnhighlightExtension;
        elbowBendRatioSlider.OnSliderEntered += HighlightElbowBend;
        elbowBendRatioSlider.OnSliderExited += UnhighlightElbowBend;
        controllerRestSliders.OnSliderEntered += HighlightController;
        controllerRestSliders.OnSliderExited += UnhighlightController;
        pointerTiltAngleSlider.OnSliderEntered += HighlightLaser;
        pointerTiltAngleSlider.OnSliderExited += UnhighlightLaser;
      } else {
        elbowRestSliders.OnSliderEntered -= HighlightElbow;
        elbowRestSliders.OnSliderExited -= UnhighlightElbow;
        wristRestSliders.OnSliderEntered -= HighlightWrist;
        wristRestSliders.OnSliderExited -= UnhighlightWrist;
        armExtensionSliders.OnSliderEntered -= HighlightExtension;
        armExtensionSliders.OnSliderExited -= UnhighlightExtension;
        elbowBendRatioSlider.OnSliderEntered -= HighlightElbowBend;
        elbowBendRatioSlider.OnSliderExited -= UnhighlightElbowBend;
        controllerRestSliders.OnSliderEntered -= HighlightController;
        controllerRestSliders.OnSliderExited -= UnhighlightController;
        pointerTiltAngleSlider.OnSliderEntered -= HighlightLaser;
        pointerTiltAngleSlider.OnSliderExited -= UnhighlightLaser;
      }
    }

    private void OnUIChanged() {
      SetArmModelFromUI(dummyArmModel, dummyLaser);
      startOverButton.interactable = true;
      highlightsLocked = true;
    }

    private void OnSlidersReleased() {
      highlightsLocked = false;
    }

    private void HighlightExtension() {
      SetJointHighlighted(VisualizeArmModel.Joint.BICEP, true);
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, true);
    }

    private void UnhighlightExtension() {
      SetJointHighlighted(VisualizeArmModel.Joint.BICEP, false);
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, false);
    }

    private void HighlightElbow() {
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, true);
    }

    private void UnhighlightElbow() {
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, false);
    }

    private void HighlightElbowBend() {
      SetJointHighlighted(VisualizeArmModel.Joint.FOREARM, true);
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, true);
    }

    private void UnhighlightElbowBend() {
      SetJointHighlighted(VisualizeArmModel.Joint.FOREARM, false);
      SetJointHighlighted(VisualizeArmModel.Joint.ELBOW, false);
    }

    private void HighlightWrist() {
      SetJointHighlighted(VisualizeArmModel.Joint.WRIST, true);
    }

    private void UnhighlightWrist() {
      SetJointHighlighted(VisualizeArmModel.Joint.WRIST, false);
    }

    private void HighlightController() {
      SetJointHighlighted(VisualizeArmModel.Joint.CONTROLLER, true);
    }

    private void UnhighlightController() {
      SetJointHighlighted(VisualizeArmModel.Joint.CONTROLLER, false);
    }

    private void HighlightLaser() {
      SetJointHighlighted(VisualizeArmModel.Joint.LASER, true);
    }

    private void UnhighlightLaser() {
      SetJointHighlighted(VisualizeArmModel.Joint.LASER, false);
    }

    private void SetJointHighlighted(VisualizeArmModel.Joint joint, bool highlighted) {
      if (highlightsLocked) {
        return;
      }

      dummyArmVisualizer.SetOutlineEnabled(joint, highlighted);
    }

    private void SetDummyObjectsVisible(bool visible) {
      foreach (GameObject dummyObject in dummyObjects) {
        dummyObject.SetActive(visible);
      }
    }
  }
}
