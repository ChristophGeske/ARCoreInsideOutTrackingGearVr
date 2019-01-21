// Copyright 2016 Google Inc. All rights reserved.
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
using UnityEditor;
using System.Collections;

#pragma warning disable 0618 // Ignore GvrAudio* deprecation

/// A custom editor for properties on the GvrAudioSource script. This appears in the Inspector
/// window of a GvrAudioSource object.
[CustomEditor(typeof(GvrAudioSource))]
[CanEditMultipleObjects]
public class GvrAudioSourceEditor : Editor {
  private SerializedProperty clip = null;
  private SerializedProperty loop = null;
  private SerializedProperty mute = null;
  private SerializedProperty pitch = null;
  private SerializedProperty playOnAwake = null;
  private SerializedProperty priority = null;
  private SerializedProperty spatialBlend = null;
  private SerializedProperty volume = null;
  private SerializedProperty dopplerLevel = null;
  private SerializedProperty spread = null;
  private SerializedProperty rolloffMode = null;
  private SerializedProperty maxDistance = null;
  private SerializedProperty minDistance = null;
  private SerializedProperty bypassRoomEffects = null;
  private SerializedProperty directivityAlpha = null;
  private SerializedProperty directivitySharpness = null;
  private SerializedProperty listenerDirectivityAlpha = null;
  private SerializedProperty listenerDirectivitySharpness = null;
  private Texture2D directivityTexture = null;
  private SerializedProperty gainDb = null;
  private SerializedProperty hrtfEnabled = null;
  private SerializedProperty occlusionEnabled = null;

  private GUIContent clipLabel = new GUIContent("AudioClip",
      "The AudioClip asset played by the GvrAudioSource.");
  private GUIContent loopLabel = new GUIContent("Loop",
      "Sets the source to loop.");
  private GUIContent muteLabel = new GUIContent("Mute",
      "Mutes the sound.");
  private GUIContent pitchLabel = new GUIContent("Pitch",
      "Sets the frequency of the sound. Use this to slow down or speed up the sound.");
  private GUIContent priorityLabel = new GUIContent("Priority",
      "Sets the priority of the source. Note that a sound with a larger priority value will more " +
      "likely be stolen by sounds with smaller priority values.");
  private GUIContent spatialBlendLabel = new GUIContent("Spatial Blend",
      "Sets how much this source is treated as a 3D source. Setting this value to 0 will ignore " +
      "distance attenuation and doppler effects. However, it does not affect panning the sound " +
      "around the listener.");
  private GUIContent volumeLabel = new GUIContent("Volume",
      "Sets the overall volume of the sound.");
  private GUIContent dopplerLevelLabel = new GUIContent("Doppler Level",
      "Specifies how much the pitch is changed based on the relative velocity between the source " +
      "and the listener.");
  private GUIContent spreadLabel = new GUIContent("Spread",
      "Source spread in degrees.");
  private GUIContent rolloffModeLabel = new GUIContent("Volume Rolloff",
      "Which type of rolloff curve to use.");
  private GUIContent maxDistanceLabel = new GUIContent("Max Distance",
      "Max distance is the distance a sound stops attenuating at.");
  private GUIContent minDistanceLabel = new GUIContent("Min Distance",
      "Within the min distance, the volume will stay at the loudest possible. " +
      "Outside this min distance it will begin to attenuate.");
  private GUIContent playOnAwakeLabel = new GUIContent("Play On Awake",
      "Play the sound when the scene loads.");
  private GUIContent bypassRoomEffectsLabel = new GUIContent("Bypass Room Effects",
      "Sets whether the room effects for the source should be bypassed.");
  private GUIContent directivityLabel = new GUIContent("Directivity",
      "Controls the pattern of sound emission of the source. This can change the perceived " +
      "loudness of the source depending on which way it is facing relative to the listener. " +
      "Patterns are aligned to the 'forward' direction of the parent object.");
  private GUIContent directivityAlphaLabel = new GUIContent("Alpha",
      "Controls the balance between dipole pattern and omnidirectional pattern for source " +
      "emission. By varying this value, differing directivity patterns can be formed.");
  private GUIContent directivitySharpnessLabel = new GUIContent("Sharpness",
      "Sets the sharpness of the directivity pattern. Higher values will result in increased " +
      "directivity.");
  private GUIContent listenerDirectivityLabel = new GUIContent("Listener Directivity",
      "Controls the pattern of sound sensitivity of the listener for the source. This can " +
      "change the perceived loudness of the source depending on which way the listener is facing " +
      "relative to the source. Patterns are aligned to the 'forward' direction of the listener.");
  private GUIContent listenerDirectivityAlphaLabel = new GUIContent("Alpha",
      "Controls the balance between dipole pattern and omnidirectional pattern for listener " +
      "sensitivity. By varying this value, differing directivity patterns can be formed.");
  private GUIContent listenerDirectivitySharpnessLabel = new GUIContent("Sharpness",
      "Sets the sharpness of the listener directivity pattern. Higher values will result in " +
      "increased directivity.");
  private GUIContent gainLabel = new GUIContent("Gain (dB)",
      "Applies a gain to the source for adjustment of relative loudness.");
  private GUIContent hrtfEnabledLabel = new GUIContent("Enable HRTF",
      "Sets HRTF binaural rendering for the source. Note that this setting has no effect when " +
      "stereo quality mode is selected globally.");
  private GUIContent occlusionLabel = new GUIContent("Enable Occlusion",
      "Sets whether the sound of the source should be occluded when there are other objects " +
      "between the source and the listener.");

  void OnEnable () {
    clip = serializedObject.FindProperty("sourceClip");
    loop = serializedObject.FindProperty("sourceLoop");
    mute = serializedObject.FindProperty("sourceMute");
    pitch = serializedObject.FindProperty("sourcePitch");
    playOnAwake = serializedObject.FindProperty("playOnAwake");
    priority = serializedObject.FindProperty("sourcePriority");
    spatialBlend = serializedObject.FindProperty("sourceSpatialBlend");
    volume = serializedObject.FindProperty("sourceVolume");
    dopplerLevel = serializedObject.FindProperty("sourceDopplerLevel");
    spread = serializedObject.FindProperty("sourceSpread");
    rolloffMode = serializedObject.FindProperty("sourceRolloffMode");
    maxDistance = serializedObject.FindProperty("sourceMaxDistance");
    minDistance = serializedObject.FindProperty("sourceMinDistance");
    bypassRoomEffects = serializedObject.FindProperty("bypassRoomEffects");
    directivityAlpha = serializedObject.FindProperty("directivityAlpha");
    directivitySharpness = serializedObject.FindProperty("directivitySharpness");
    listenerDirectivityAlpha = serializedObject.FindProperty("listenerDirectivityAlpha");
    listenerDirectivitySharpness = serializedObject.FindProperty("listenerDirectivitySharpness");
    directivityTexture = Texture2D.blackTexture;
    gainDb = serializedObject.FindProperty("gainDb");
    hrtfEnabled = serializedObject.FindProperty("hrtfEnabled");
    occlusionEnabled = serializedObject.FindProperty("occlusionEnabled");
  }

  /// @cond
  public override void OnInspectorGUI () {
    serializedObject.Update();

    // Add clickable script field, as would have been provided by DrawDefaultInspector()
    MonoScript script = MonoScript.FromMonoBehaviour (target as MonoBehaviour);
    EditorGUI.BeginDisabledGroup (true);
    EditorGUILayout.ObjectField ("Script", script, typeof(MonoScript), false);
    EditorGUI.EndDisabledGroup ();

    EditorGUILayout.PropertyField(clip, clipLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(mute, muteLabel);
    EditorGUILayout.PropertyField(bypassRoomEffects, bypassRoomEffectsLabel);
    EditorGUILayout.PropertyField(playOnAwake, playOnAwakeLabel);
    EditorGUILayout.PropertyField(loop, loopLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(priority, priorityLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(volume, volumeLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(pitch, pitchLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(spatialBlend, spatialBlendLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.Slider(gainDb, GvrAudio.minGainDb, GvrAudio.maxGainDb, gainLabel);

    EditorGUILayout.Separator();

    EditorGUILayout.PropertyField(dopplerLevel, dopplerLevelLabel);
    EditorGUILayout.PropertyField(spread, spreadLabel);
    EditorGUILayout.PropertyField(rolloffMode, rolloffModeLabel);
    ++EditorGUI.indentLevel;
    EditorGUILayout.PropertyField(minDistance, minDistanceLabel);
    EditorGUILayout.PropertyField(maxDistance, maxDistanceLabel);
    --EditorGUI.indentLevel;
    if (rolloffMode.enumValueIndex == (int)AudioRolloffMode.Custom) {
      EditorGUILayout.HelpBox("Custom rolloff mode is not supported, no distance attenuation " +
                              "will be applied.", MessageType.Warning);
    }

    EditorGUILayout.Separator();

    // Draw the listener directivity properties.
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();
    GUILayout.Label(listenerDirectivityLabel);
    ++EditorGUI.indentLevel;
    EditorGUILayout.Slider(listenerDirectivityAlpha, 0.0f, 1.0f, listenerDirectivityAlphaLabel);
    EditorGUILayout.Slider(listenerDirectivitySharpness, 1.0f, 10.0f,
                           listenerDirectivitySharpnessLabel);
    --EditorGUI.indentLevel;
    EditorGUILayout.EndVertical();
    DrawDirectivityPattern(listenerDirectivityAlpha.floatValue,
                           listenerDirectivitySharpness.floatValue,
                           GvrAudio.listenerDirectivityColor,
                           (int)(3.0f * EditorGUIUtility.singleLineHeight));
    EditorGUILayout.EndHorizontal();

    EditorGUILayout.Separator();

    // Draw the source directivity properties.
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();
    GUILayout.Label(directivityLabel);
    ++EditorGUI.indentLevel;
    EditorGUILayout.Slider(directivityAlpha, 0.0f, 1.0f, directivityAlphaLabel);
    EditorGUILayout.Slider(directivitySharpness, 1.0f, 10.0f, directivitySharpnessLabel);
    --EditorGUI.indentLevel;
    EditorGUILayout.EndVertical();
    DrawDirectivityPattern(directivityAlpha.floatValue, directivitySharpness.floatValue,
                           GvrAudio.sourceDirectivityColor,
                           (int)(3.0f * EditorGUIUtility.singleLineHeight));
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.PropertyField(occlusionEnabled, occlusionLabel);

    EditorGUILayout.Separator();

    // HRTF toggle can only be modified through the Inspector in Edit mode.
    EditorGUI.BeginDisabledGroup (EditorApplication.isPlaying);
    EditorGUILayout.PropertyField(hrtfEnabled, hrtfEnabledLabel);
    EditorGUI.EndDisabledGroup ();

    serializedObject.ApplyModifiedProperties();
  }
  /// @endcond

  private void DrawDirectivityPattern (float alpha, float sharpness, Color color, int size) {
    directivityTexture.Resize(size, size);
    // Draw the axes.
    Color axisColor = color.a * Color.black;
    for (int i = 0; i < size; ++i) {
      directivityTexture.SetPixel(i, size / 2, axisColor);
      directivityTexture.SetPixel(size / 2, i, axisColor);
    }
    // Draw the 2D polar directivity pattern.
    float offset = 0.5f * size;
    float cardioidSize = 0.45f * size;
    Vector2[] vertices = GvrAudio.Generate2dPolarPattern(alpha, sharpness, 180);
    for (int i = 0; i < vertices.Length; ++i) {
      directivityTexture.SetPixel((int)(offset + cardioidSize * vertices[i].x),
                                  (int)(offset + cardioidSize * vertices[i].y), color);
    }
    directivityTexture.Apply();
    // Show the texture.
    GUILayout.Box(directivityTexture);
  }
}

#pragma warning restore 0618 // Restore warnings
