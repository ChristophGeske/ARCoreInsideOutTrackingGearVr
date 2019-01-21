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
using UnityEngine.VR;
using System.Collections;

namespace DaydreamElements.ArmModels {

  /// Arm Model implementation for approximating a swinging motion.
  public class SwingArmModel : GvrArmModel {
    [Tooltip("Portion of controller rotation applied to the shoulder joint.")]
    [Range(0.0f, 1.0f)]
    public float shoulderRotationRatio = 0.5f;

    [Tooltip("Portion of controller rotation applied to the elbow joint.")]
    [Range(0.0f, 1.0f)]
    public float elbowRotationRatio = 0.3f;

    [Tooltip("Portion of controller rotation applied to the wrist joint.")]
    [Range(0.0f, 1.0f)]
    public float wristRotationRatio = 0.2f;

    [Tooltip("Min angle of the controller before starting to lerp towards the shifted joint ratios.")]
    [Range(0.0f, 180.0f)]
    public float minJointShiftAngle = 160.0f;

    [Tooltip("Max angle of the controller before the lerp towards the shifted joint ratios ends.")]
    [Range(0.0f, 180.0f)]
    public float maxJointShiftAngle = 180.0f;

    [Tooltip("Exponent applied to the joint shift ratio to control the curve of the shift.")]
    [Range(1.0f, 20.0f)]
    public float jointShiftExponent = 6.0f;

    [Tooltip("Portion of controller rotation applied to the shoulder joint when the controller is backwards.")]
    [Range(0.0f, 1.0f)]
    public float shiftedShoulderRotationRatio = 0.1f;

    [Tooltip("Portion of controller rotation applied to the elbow joint when the controller is backwards.")]
    [Range(0.0f, 1.0f)]
    public float shiftedElbowRotationRatio = 0.4f;

    [Tooltip("Portion of controller rotation applied to the wrist joint when the controller is backwards.")]
    [Range(0.0f, 1.0f)]
    public float shiftedWristRotationRatio = 0.5f;

    protected override void CalculateFinalJointRotations(Quaternion controllerOrientation, Quaternion xyRotation, Quaternion lerpRotation) {
      // As the controller angle increases the ratio of the rotation applied to each joint shifts.
      float totalAngle = Quaternion.Angle(xyRotation, Quaternion.identity);
      float joingShiftAngleRange = maxJointShiftAngle - minJointShiftAngle;
      float angleRatio = Mathf.Clamp01((totalAngle - minJointShiftAngle) / joingShiftAngleRange);
      float jointShiftRatio = Mathf.Pow(angleRatio, jointShiftExponent);

      // Calculate what portion of the rotation is applied to each joint.
      float finalShoulderRatio = Mathf.Lerp(shoulderRotationRatio, shiftedShoulderRotationRatio, jointShiftRatio);
      float finalElbowRatio = Mathf.Lerp(elbowRotationRatio, shiftedElbowRotationRatio, jointShiftRatio);
      float finalWristRatio = Mathf.Lerp(wristRotationRatio, shiftedWristRotationRatio, jointShiftRatio);

      // Calculate relative rotations for each joint.
      Quaternion swingShoulderRot = Quaternion.Lerp(Quaternion.identity, xyRotation, finalShoulderRatio);
      Quaternion swingElbowRot = Quaternion.Lerp(Quaternion.identity, xyRotation, finalElbowRatio);
      Quaternion swingWristRot = Quaternion.Lerp(Quaternion.identity, xyRotation, finalWristRatio);

      // Calculate final rotations.
      Quaternion shoulderRotation = torsoRotation * swingShoulderRot;
      elbowRotation = shoulderRotation * swingElbowRot;
      wristRotation = elbowRotation * swingWristRot;
      controllerRotation = torsoRotation * controllerOrientation;
      torsoRotation = shoulderRotation;
    }
  }
}