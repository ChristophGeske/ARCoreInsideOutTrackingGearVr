///////////////////////////////////////////////////////////////////////////////
//Copyright 2017 Google Inc.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace daydreamrenderer
{
    using DVLEditor = DaydreamVertexLightingEditor;
    using BakeSettings = DDRSettings.BakeSettings;

    [CustomEditor(typeof(VertexLightingOverride), true)]
    public class OverrideInspector : Editor
    {

        static class Content
        {
            public static readonly GUIContent m_ovrdShadowsLabel = new GUIContent("Enable Shadows");
            public static readonly GUIContent m_ovrdAOsLabel = new GUIContent("Enable Ambient Occlusion");
            public static readonly GUIContent m_ovrdBlockerSamples = new GUIContent("Enable Ambient Occlusion");
        }

        void OnEnable()
        {
        }

        public override void OnInspectorGUI()
        {
            VertexLightingOverride source = target as VertexLightingOverride;

            BakeSettings settings = source.m_bakeSettingsOverride;

            DVLEditor.DrawShadowAndAOSettings(settings, source, true);

            if (GUILayout.Button("Copy Global Settings"))
            {
                source.m_bakeSettingsOverride.CopySettings(BakeData.Instance().GetBakeSettings().SelectedBakeSet);
                EditorUtility.SetDirty(source);
            }
        }
    }
}
