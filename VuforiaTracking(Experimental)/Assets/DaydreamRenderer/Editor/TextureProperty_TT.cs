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

public class TextureProperty_TT : MaterialPropertyDrawer
{
    string m_tooltip;

    public TextureProperty_TT(string tooltip)
    {
        m_tooltip = tooltip;
    }

    // Draw the property inside the given rect
    public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
    {
        editor.TextureProperty(position, prop, label, m_tooltip, false);
    }

    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        // This value seems to be hardcoded even in the Unity source so... just leave it be for now.
        return base.GetPropertyHeight(prop, label, editor) + 54;
    }
}
