//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(UIParticleContainer), true)]
public class UIParticleContainerInspector : UIWidgetInspector
{
    protected override void DrawCustomProperties()
    {
        GUILayout.Space(6f);

        NGUIEditorTools.DrawProperty("Group", serializedObject, "mDepthGroupID");
        NGUIEditorTools.DrawProperty("Particle", serializedObject, "particle_name");
        NGUIEditorTools.DrawProperty("ContinueTime", serializedObject, "ContinueTime");
        NGUIEditorTools.DrawProperty("IsAutoPlay", serializedObject, "IsAutoPlay");
        NGUIEditorTools.DrawProperty("PlayType", serializedObject, "play_type");

        base.DrawCustomProperties();
    }
}
