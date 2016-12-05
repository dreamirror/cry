using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(AtlasSprite), true)]
public class AtlasSpriteInspector : Editor
{
    void OnSelectAtlas(Object obj)
    {
        AtlasSprite sprite = serializedObject.targetObject as AtlasSprite;
        sprite.atlas = obj as AtlasSheet;

        NGUITools.SetDirty(serializedObject.targetObject);
        NGUISettings.atlas = obj as UIAtlas;
    }

    /// <summary>
    /// Sprite selection callback function.
    /// </summary>

    void SelectSprite(string spriteName)
    {
        AtlasSprite sprite = serializedObject.targetObject as AtlasSprite;
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

        AtlasSprite sprite = (AtlasSprite)target;
        serializedObject.ApplyModifiedProperties();

        if (sprite == null || !sprite.isValid) return;
    }

    protected bool ShouldDrawProperties()
    {
        GUILayout.BeginHorizontal();
        if (NGUIEditorTools.DrawPrefixButton("Atlas"))
            ComponentSelector.Show<AtlasSheet>(OnSelectAtlas);
        SerializedProperty atlas = NGUIEditorTools.DrawProperty("", serializedObject, "mAtlas", GUILayout.MinWidth(20f));

        if (GUILayout.Button("Rand", GUILayout.Width(40f)))
        {
            if (atlas != null && Selection.objects.Length > 0)
            {
                AtlasSheet atl = atlas.objectReferenceValue as AtlasSheet;

                foreach (var sprite_obj in Selection.gameObjects)
                {
                    AtlasSprite sprite = sprite_obj.GetComponent<AtlasSprite>();
                    sprite.spriteName = atl.Atlas.spriteList[UnityEngine.Random.Range(0, atl.Atlas.spriteList.Count - 1)].name;
                }
            }
        }

        if (GUILayout.Button("Edit", GUILayout.Width(40f)))
        {
            if (atlas != null)
            {
                AtlasSheet atl = atlas.objectReferenceValue as AtlasSheet;
                NGUISettings.atlas = atl.Atlas;
                NGUIEditorTools.Select(atl.gameObject);
            }
        }
        GUILayout.EndHorizontal();

        SerializedProperty sp = serializedObject.FindProperty("mSpriteName");

        NGUIEditorTools.DrawAdvancedSpriteField((atlas.objectReferenceValue as AtlasSheet).Atlas, sp.stringValue, SelectSprite, false);

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
        AtlasSprite asp = target as AtlasSprite;
        if (asp == null || !asp.isValid)
            return;

        Sprite s = asp.Renderer.sprite;

        NGUIEditorTools.DrawSprite(s.texture, rect, asp.Renderer.color, s.textureRect, s.border);
    }
}
