﻿// Copyright 2017 Google Inc. All rights reserved.
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

#if UNITY_HAS_GOOGLEVR && UNITY_EDITOR
using System.Runtime.InteropServices;
using UnityEngine;

namespace Gvr.Internal {
  class InstantPreviewControllerProvider {
    /// <summary>
    /// This is a mirror of Gvr.Internal.ControllerState, but a struct instead.
    /// </summary>
    private struct NativeControllerState {
      public GvrConnectionState connectionState;
      public Quaternion orientation;
      public Vector3 gyro;
      public Vector3 accel;
      public Vector2 touchPos;
      [MarshalAs(UnmanagedType.U1)]
      public bool isTouching;
      [MarshalAs(UnmanagedType.U1)]
      public bool appButtonState;
      [MarshalAs(UnmanagedType.U1)]
      public bool clickButtonState;
      public int batteryLevel;
      [MarshalAs(UnmanagedType.U1)]
      public bool isCharging;
      [MarshalAs(UnmanagedType.U1)]
      public bool isRecentered;
      [MarshalAs(UnmanagedType.U1)]
      public bool homeButtonState;
    }

    private bool prevTouchState;
    private bool prevClickButtonState;
    private bool prevAppButtonState;
    private bool prevHomeButtonState;

    [DllImport(InstantPreview.dllName)]
    private static extern void ReadControllerState(out NativeControllerState nativeControllerState);

    public void ReadState(ControllerState outState) {
      var nativeControllerState = new NativeControllerState();
      ReadControllerState(out nativeControllerState);

      outState.connectionState = nativeControllerState.connectionState;
      outState.orientation = new Quaternion(
        -nativeControllerState.orientation.y,
        -nativeControllerState.orientation.z,
        nativeControllerState.orientation.w,
        nativeControllerState.orientation.x);

      outState.gyro = new Vector3(-nativeControllerState.gyro.x, -nativeControllerState.gyro.y, nativeControllerState.gyro.z);
      outState.accel = new Vector3(nativeControllerState.accel.x, nativeControllerState.accel.y, -nativeControllerState.accel.z);
      outState.touchPos = nativeControllerState.touchPos;
      outState.isTouching = nativeControllerState.isTouching;
      outState.touchDown = nativeControllerState.isTouching && !prevTouchState;
      outState.touchUp = !nativeControllerState.isTouching && prevTouchState;
      outState.appButtonState = nativeControllerState.appButtonState;
      outState.appButtonDown = nativeControllerState.appButtonState && !prevAppButtonState;
      outState.appButtonUp = !nativeControllerState.appButtonState && prevAppButtonState;
      outState.clickButtonState = nativeControllerState.clickButtonState;
      outState.clickButtonDown = nativeControllerState.clickButtonState && !prevClickButtonState;
      outState.clickButtonUp = !nativeControllerState.clickButtonState && prevClickButtonState;
      outState.batteryLevel = (GvrControllerBatteryLevel)nativeControllerState.batteryLevel;
      outState.isCharging = nativeControllerState.isCharging;
      outState.recentered = nativeControllerState.isRecentered;
      outState.homeButtonState = nativeControllerState.homeButtonState;
      outState.homeButtonDown = nativeControllerState.homeButtonState && !prevHomeButtonState;

      prevTouchState = nativeControllerState.isTouching;
      prevAppButtonState = nativeControllerState.appButtonState;
      prevClickButtonState = nativeControllerState.clickButtonState;
      prevHomeButtonState = nativeControllerState.homeButtonState;
    }
  }
}
#endif
