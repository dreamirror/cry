using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(UGUISprite), true)]
public class UGUISpriteInspector : Editor
{
    void OnSelectAtlas(Object obj)
    {
        UGUISprite sprite = serializedObject.targetObject as UGUISprite;
        sprite.atlas = obj as UIAtlas;

        NGUITools.SetDirty(serializedObject.targetObject);
        NGUISettings.atlas = obj as UIAtlas;
    }

    /// <summary>
    /// Sprite selection callback function.
    /// </summary>

    void SelectSprite(string spriteName)
    {
        UGUISprite sprite = serializedObject.targetObject as UGUISprite;
        sprite.spriteName = spriteName;
        if (sprite.isValid)
        {
            NGUITools.SetDirty(serializedObject.targetObject);
            NGUISettings.selectedSprite = spriteName;
        }
    }

    /// <summary>
    /// Draw the inspector properties.
    /// </summary>

    public override void OnInspectorGUI()
    {
        NGUIEditorTools.SetLabelWidth(80f);
        EditorGUILayout.Space();

        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(!ShouldDrawProperties());
        EditorGUI.EndDisabledGroup();

        EditorGUIUtility.labelWidth = 0;

        EditorGUILayout.Space();

        UGUISprite sprite = (UGUISprite)target;
        sprite.UseNativeSize = EditorGUILayout.Toggle("Use Native Size", sprite.UseNativeSize);
        sprite.UsePivot = EditorGUILayout.Toggle("Use Pivot", sprite.UsePivot);
        EditorGUILayout.BeginHorizontal();
        sprite.m_Pivot = EditorGUILayout.Vector2Field("Pivot", sprite.m_Pivot);
        if (GUILayout.Button("Set Pivot"))
        {
            if (sprite != null)
                sprite.SetPivot();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sprite.m_AnimationType = (UGUISprite.AnimationType)EditorGUILayout.EnumPopup("Animation", sprite.m_AnimationType);
        sprite.IsPlayAnimationOnStart = EditorGUILayout.Toggle("PlayAnimationOnStart", sprite.IsPlayAnimationOnStart);
        sprite.IsPlayAnimationOnEnable = EditorGUILayout.Toggle("PlayAnimationOnEnable", sprite.IsPlayAnimationOnEnable);

        var sp = serializedObject.FindProperty("Animations");
        EditorGUILayout.PropertyField(sp, new GUIContent("Animations"), true);

        sprite.AnimationLength = EditorGUILayout.FloatField("Animation Length", sprite.AnimationLength);
        sprite.AnimationDelay = EditorGUILayout.FloatField("Animation Delay", sprite.AnimationDelay);

        serializedObject.ApplyModifiedProperties();

//        UGUISprite sprite = target as UGUISprite;
        if (sprite == null || !sprite.isValid) return;
    }

    protected bool ShouldDrawProperties()
    {
        GUILayout.BeginHorizontal();
        if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            ComponentSelector.Show<UIAtlas>(OnSelectAtlas);
        SerializedProperty atlas = NGUIEditorTools.DrawProperty("", serializedObject, "mAtlas", GUILayout.MinWidth(20f));

        if (GUILayout.Button("Edit", GUILayout.Width(40f)))
        {
            if (atlas != null)
            {
                UIAtlas atl = atlas.objectReferenceValue as UIAtlas;
                NGUISettings.atlas = atl;
                NGUIEditorTools.Select(atl.gameObject);
            }
        }
        GUILayout.EndHorizontal();

        SerializedProperty sp = serializedObject.FindProperty("mSpriteName");
        NGUIEditorTools.DrawAdvancedSpriteField(atlas.objectReferenceValue as UIAtlas, sp.stringValue, SelectSprite, false);

        return true;
    }
}
