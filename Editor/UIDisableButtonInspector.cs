using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
#if UNITY_3_5
[CustomEditor(typeof(UIDisableButton))]
#else
[CustomEditor(typeof(UIDisableButton), true)]
#endif
public class UIDisableButtonInspector : UIButtonEditor {

    protected override void DrawProperties()
    {
        base.DrawProperties();
        GUILayout.BeginHorizontal();
        NGUIEditorTools.DrawProperty("DisableDuration", serializedObject, "disableDuration", GUILayout.Width(140f));
        GUILayout.Label("seconds");
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        NGUIEditorTools.DrawProperty("ManualReset", serializedObject, "manual_reset", GUILayout.Width(140f));
        NGUIEditorTools.DrawProperty("id", serializedObject, "manual_reset_id", GUILayout.Width(200f));
        GUILayout.EndHorizontal();
        GUILayout.Space(3f);
    }
}

