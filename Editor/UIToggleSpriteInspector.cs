//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2015 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Inspector class used to edit UISprites.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UIToggleSprite), true)]
public class UIToggleSpriteInspector : UIBasicSpriteEditor
{
    /// <summary>
    /// Atlas selection callback.
    /// </summary>

    void OnSelectAtlas(Object obj)
    {
        serializedObject.Update();
        SerializedProperty sp = serializedObject.FindProperty("mAtlas");
        sp.objectReferenceValue = obj;
        serializedObject.ApplyModifiedProperties();
        NGUITools.SetDirty(serializedObject.targetObject);
        NGUISettings.atlas = obj as UIAtlas;
    }


    /// <summary>
    /// Sprite selection callback function.
    /// </summary>

    void SelectSprite(string spriteName)
    {
        serializedObject.Update();
        SerializedProperty sp = serializedObject.FindProperty("mSpriteName");
        sp.stringValue = spriteName;

        SerializedProperty sp2 = serializedObject.FindProperty("m_NormalName");
        sp2.stringValue = spriteName;

        serializedObject.ApplyModifiedProperties();
        NGUITools.SetDirty(serializedObject.targetObject);
        NGUISettings.selectedSprite = spriteName;
    }

    void SelectSprite2(string spriteName)
    {
        serializedObject.Update();
        SerializedProperty sp = serializedObject.FindProperty("m_ActiveName");
        sp.stringValue = spriteName;
        serializedObject.ApplyModifiedProperties();
        NGUITools.SetDirty(serializedObject.targetObject);
        //NGUISettings.selectedSprite = spriteName;
    }
    /// <summary>
    /// Draw the atlas and sprite selection fields.
    /// </summary>

    protected override bool ShouldDrawProperties()
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
        SerializedProperty sp2 = serializedObject.FindProperty("m_ActiveName");
        NGUIEditorTools.DrawAdvancedSpriteField(atlas.objectReferenceValue as UIAtlas, sp2.stringValue, SelectSprite2, false);
        return true;
    }

    /// <summary>
    /// All widgets have a preview.
    /// </summary>

    public override bool HasPreviewGUI()
    {
        return (Selection.activeGameObject == null || Selection.gameObjects.Length == 1);
    }

    /// <summary>
    /// Draw the sprite preview.
    /// </summary>

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        UISprite sprite = target as UISprite;
        if (sprite == null || !sprite.isValid) return;

        Texture2D tex = sprite.mainTexture as Texture2D;
        if (tex == null) return;

        UISpriteData sd = sprite.atlas.GetSprite(sprite.spriteName);
        NGUIEditorTools.DrawSprite(tex, rect, sd, sprite.color);
    }
}
