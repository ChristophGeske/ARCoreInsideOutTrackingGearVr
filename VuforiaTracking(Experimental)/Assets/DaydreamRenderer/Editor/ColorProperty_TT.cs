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

public class ColorProperty_TT : MaterialPropertyDrawer
{
    string m_tooltip;
    GUIContent m_guiLabel = null;

    public ColorProperty_TT(string tooltip)
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

        ColorPropertyInternal(position, prop, m_guiLabel);
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return base.GetPropertyHeight(prop, label, editor);
    }

    // internal color property
    private Color ColorPropertyInternal(Rect position, MaterialProperty prop, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = prop.hasMixedValue;
        bool hdr = (prop.flags & MaterialProperty.PropFlags.HDR) != MaterialProperty.PropFlags.None;
        bool showAlpha = true;
        Color colorValue = EditorGUI.ColorField(position, label, prop.colorValue, true, showAlpha, hdr, null);
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
        {
            prop.colorValue = colorValue;
        }
        return prop.colorValue;
    }
}
