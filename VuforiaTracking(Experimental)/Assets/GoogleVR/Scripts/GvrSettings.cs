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


/// <summary>
/// Accesses and configures Daydream settings.
/// </summary>

using UnityEngine;
using System;
using System.Runtime.InteropServices;

#if UNITY_2017_2_OR_NEWER
using UnityEngine.XR;
#else
using XRDevice = UnityEngine.VR.VRDevice;
using XRSettings = UnityEngine.VR.VRSettings;
#endif  // UNITY_2017_2_OR_NEWER

#if UNITY_EDITOR
using UnityEditor;
#endif  // UNITY_EDITOR

public static class GvrSettings {
  /// Name of 'None' VR SDK, as returned by `VRSettings.loadedDeviceName`.
  public const string VR_SDK_NONE = "None";

  /// Name of Daydream GVR SDK, as returned by `VRSettings.loadedDeviceName`.
  public const string VR_SDK_DAYDREAM = "daydream";

  /// Name of Cardboard GVR SDK, as returned by `VRSettings.loadedDeviceName` and supportedDevices.
  public const string VR_SDK_CARDBOARD = "cardboard";

  private const string METHOD_GET_WINDOW = "getWindow";
  private const string METHOD_RUN_ON_UI_THREAD = "runOnUiThread";
  private const string METHOD_SET_SUSTAINED_PERFORMANCE_MODE = "setSustainedPerformanceMode";

  // Viewer type.
  public enum ViewerPlatformType {
    Error = -1,  // Plugin-only value; does not exist in the NDK.
    Cardboard,
    Daydream
  }
  public static ViewerPlatformType ViewerPlatform {
    // Expose a setter only for the editor emulator, for development testing purposes.
#if UNITY_EDITOR
    get {
      return editorEmulatorOnlyViewerPlatformType;
    }
    set {
      editorEmulatorOnlyViewerPlatformType = value;
    }
#elif !UNITY_ANDROID
    // Running in non-Android player.
    get {
      return ViewerPlatformType.Error;
    }
#else
    // Running on Android.
    get {
      IntPtr gvrContextPtr = GetValidGvrNativePtrOrLogError();
      if (gvrContextPtr == IntPtr.Zero) {
        return ViewerPlatformType.Error;
      }
      return (ViewerPlatformType) gvr_get_viewer_type(gvrContextPtr);
    }
#endif  // UNITY_EDITOR
  }
#if UNITY_EDITOR
  private static ViewerPlatformType editorEmulatorOnlyViewerPlatformType =
    ViewerPlatformType.Daydream;
#endif  // UNITY_EDITOR

  // The developer is expected to remember whether sustained performance mode is set
  // at runtime, via the checkbox in Player Settings.
  // This state may be recorded here in a future release.
  public static bool SustainedPerformanceMode {
    set {
      SetSustainedPerformanceMode(value);
    }
  }

  // Handedness preference.
  public enum UserPrefsHandedness {
    Error = -1,  // Plugin-only value, does not exist in the NDK.
    Right,
    Left
  }

  public static UserPrefsHandedness Handedness {
#if UNITY_EDITOR
    // Expose a setter only for the editor emulator, for development testing purposes.
    get {
      return (UserPrefsHandedness)EditorPrefs.GetInt(EMULATOR_HANDEDNESS_PREF_NAME, (int)UserPrefsHandedness.Right);
    }
    set {
      EditorPrefs.SetInt(EMULATOR_HANDEDNESS_PREF_NAME, (int)value);
    }
#elif !UNITY_ANDROID
    // Running in non-Android player.
    get {
      return UserPrefsHandedness.Error;
    }
#else
    // Running on Android.
    get {
      IntPtr gvrContextPtr = GetValidGvrNativePtrOrLogError();
      if (gvrContextPtr == IntPtr.Zero) {
        Debug.LogError("Unable to determine GVR user prefs' handedness");
        return UserPrefsHandedness.Error;
      }

      IntPtr gvrUserPrefsPtr = gvr_get_user_prefs(gvrContextPtr);
      if (gvrUserPrefsPtr == IntPtr.Zero) {
        Debug.Log("Zero GVR user prefs pointer, unable to determine GVR user prefs' handedness");
        return UserPrefsHandedness.Error;
      }

      return (UserPrefsHandedness) gvr_user_prefs_get_controller_handedness(gvrUserPrefsPtr);
    }
#endif  // UNITY_EDITOR
  }
#if UNITY_EDITOR
  // This allows developers to test handedness in the editor emulator.
  private const string EMULATOR_HANDEDNESS_PREF_NAME = "GoogleVREditorEmulatorHandedness";
#endif  // UNITY_EDITOR

  private static void SetSustainedPerformanceMode(bool enabled) {
#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject androidActivity = null;
    try {
      androidActivity = GvrActivityHelper.GetActivity();
    } catch (AndroidJavaException e) {
      Debug.LogError("Exception while connecting to the Activity: " + e);
      return;
    }

    AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>(METHOD_GET_WINDOW);
    if (androidWindow == null) {
      Debug.LogError("No window found on the current android activity");
      return;
    }

    // The sim thread in Unity is single-threaded, so we don't need to lock when accessing
    // or assigning androidWindow.
    androidActivity.Call(METHOD_RUN_ON_UI_THREAD, new AndroidJavaRunnable(() => {
          androidWindow.Call(METHOD_SET_SUSTAINED_PERFORMANCE_MODE, enabled);
          Debug.Log("Set sustained performance mode: " + (enabled ? "ON" : "OFF"));
      })
    );
#endif  // UNITY_ANDROID && !UNITY_EDITOR
  }

  /// Wraps call to `VRDevice.GetNativePtr()` and logs error if a supported GVR SDK is not active or
  /// if the returned native pointer is `IntPtr.Zero`.
  public static IntPtr GetValidGvrNativePtrOrLogError() {
    if (!XRSettings.enabled) {
      Debug.LogError("VR is disabled");
      return IntPtr.Zero;
    }
    if (XRSettings.loadedDeviceName != VR_SDK_DAYDREAM
        && XRSettings.loadedDeviceName != VR_SDK_CARDBOARD) {
      Debug.LogErrorFormat("Loaded VR SDK '{0}' must be '{1}' or '{2}'",
          XRSettings.loadedDeviceName, VR_SDK_DAYDREAM, VR_SDK_CARDBOARD);
      return IntPtr.Zero;
    }
    IntPtr gvrContextPtr = XRDevice.GetNativePtr();
    if (gvrContextPtr == IntPtr.Zero) {
      Debug.LogError("Unexpected zero GVR native context pointer");
      return gvrContextPtr;
    }
    return gvrContextPtr;
  }

#if UNITY_ANDROID && !UNITY_EDITOR
  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern IntPtr gvr_get_user_prefs(IntPtr gvrContextPtr);

  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern int gvr_get_viewer_type(IntPtr gvrContextPtr);

  [DllImport(GvrActivityHelper.GVR_DLL_NAME)]
  private static extern int gvr_user_prefs_get_controller_handedness(IntPtr gvrUserPrefsPtr);
#endif  // UNITY_ANDROID && !UNITY_EDITOR
}
