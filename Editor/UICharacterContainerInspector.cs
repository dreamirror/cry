//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(UICharacterContainer), true)]
public class UICharacterContainerInspector : UIWidgetInspector
{
    protected override void DrawCustomProperties()
    {
        GUILayout.Space(6f);

        NGUIEditorTools.DrawProperty("Group", serializedObject, "mDepthGroupID");
        NGUIEditorTools.DrawProperty("Info", serializedObject, "Info");

        base.DrawCustomProperties();
    }
}
