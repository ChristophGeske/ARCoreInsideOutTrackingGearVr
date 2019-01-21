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

namespace daydreamrenderer
{
    [CustomEditor(typeof(DaydreamMeshRenderer), true)]
    [CanEditMultipleObjects]
    public class DaydreamMeshRendererInspector : Editor
    {
        static bool m_foldout = false;
        public override void OnInspectorGUI()
        {
            if (target == null) return;

            m_foldout = EditorGUILayout.Foldout(m_foldout, "Daydream Lighting Info");

            if (m_foldout)
            {
                EditorGUILayout.HelpBox("Daydream Renderer utilizes a custom lighting system that requires this components " +
                                "in order to provide lighting data to shaders. This component is added automatically. This behavior can be disabled under " +
                                "Window->Daydream Renderer->Import Wizard by unchecking the 'Auto add daydream lighting system components' toggle", MessageType.Info);
            }

            base.OnInspectorGUI();
        }
    }
}
