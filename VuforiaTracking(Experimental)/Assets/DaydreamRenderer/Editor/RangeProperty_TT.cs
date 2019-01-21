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
using UnityEditor;
using System;

public class RangeProperty_TT : MaterialPropertyDrawer
{
    string m_tooltip;
    GUIContent m_guiLabel = null;

    public RangeProperty_TT(string tooltip)
    {
        m_tooltip = tooltip;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        //allocate the GUIContent label only once, the first time we find out all of the required information.
        if (m_guiLabel == null)
        {
            m_guiLabel = new GUIContent(label, m_tooltip);
        }

        RangePropertyInternal(position, prop, m_guiLabel);
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return base.GetPropertyHeight(prop, label, editor);
    }

    private float RangePropertyInternal(Rect position, MaterialProperty prop, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;
        float labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 0f;
        float floatValue = EditorGUI.Slider(position, label, prop.floatValue, prop.rangeLimits.x, prop.rangeLimits.y);
        EditorGUI.showMixedValue = false;
        EditorGUIUtility.labelWidth = labelWidth;
        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = floatValue;
        }
        return prop.floatValue;
    }
}
