using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class AssetManagerViewer : EditorWindow
{
    static public AssetManagerViewer instance;

    [MenuItem("Utility/AssetManager Status", false, 9)]
    static public void OpenTool()
    {
        EditorWindow.GetWindow<AssetManagerViewer>(false, "AssetManager Status", true).Show();
    }

    Vector2 mScroll = Vector2.zero;

    void OnEnable() { instance = this; AssetManager.OnChanged = OnSelectionChange; }
    void OnDisable() { instance = null; AssetManager.OnChanged = null; }
    void OnSelectionChange() { Repaint(); }

    /// <summary>
    /// Draw the custom wizard.
    /// </summary>

    static readonly Color active_color = Color.white;
    static readonly Color deactive_color = new Color(1f, 0.6f, 0.6f);

    void OnGUI()
    {
        if (EditorApplication.isPlaying == false)
            return;

        GUILayout.Space(6f);

        mScroll = GUILayout.BeginScrollView(mScroll);

        if (NGUIEditorTools.DrawHeader(string.Format("Character ({0})", AssetManager.Characters.Count)))
        {
            NGUIEditorTools.BeginContents();
            if (AssetManager.Characters != null)
            {
                foreach (AssetData container in AssetManager.Characters.Values.OrderBy(v => v.Prefab.name))
                {
                    GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));

                    if (container.UsingList.Count > 0)
                        GUI.contentColor = active_color;
                    else
                        GUI.contentColor = deactive_color;

                    EditorGUILayout.LabelField(container.Prefab.name, GUILayout.Width(200f));
                    EditorGUILayout.LabelField(string.Format("Using : {0}", container.UsingList.Count), GUILayout.Width(100f));
                    EditorGUILayout.LabelField(string.Format("Free : {0}", container.FreeList.Count), GUILayout.Width(100f));
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
            NGUIEditorTools.EndContents();
        }

        if (NGUIEditorTools.DrawHeader(string.Format("CharacterSkin ({0})", AssetManager.CharacterSkins.Count)))
        {
            NGUIEditorTools.BeginContents();
            if (AssetManager.CharacterSkins != null)
            {
                foreach (AssetData container in AssetManager.CharacterSkins.Values.OrderBy(v => v.Prefab.name))
                {
                    GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));

                    if (container.UsingList.Count > 0)
                        GUI.contentColor = active_color;
                    else
                        GUI.contentColor = deactive_color;

                    EditorGUILayout.LabelField(container.Prefab.name, GUILayout.Width(200f));
                    EditorGUILayout.LabelField(string.Format("Using : {0}", container.UsingList.Count), GUILayout.Width(100f));
                    EditorGUILayout.LabelField(string.Format("Free : {0}", container.FreeList.Count), GUILayout.Width(100f));
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
            NGUIEditorTools.EndContents();
        }

        if (NGUIEditorTools.DrawHeader(string.Format("Character CutScene({0})", AssetManager.CharacterCutScenes.Count)))
        {
            NGUIEditorTools.BeginContents();
            if (AssetManager.CharacterCutScenes != null)
            {
                foreach (AssetData container in AssetManager.CharacterCutScenes.Values.OrderBy(v => v.Prefab.name))
                {
                    GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));

                    if (container.UsingList.Count > 0)
                        GUI.contentColor = active_color;
                    else
                        GUI.contentColor = deactive_color;

                    EditorGUILayout.LabelField(container.Prefab.name, GUILayout.Width(200f));
                    EditorGUILayout.LabelField(string.Format("Using : {0}", container.UsingList.Count), GUILayout.Width(100f));
                    EditorGUILayout.LabelField(string.Format("Free : {0}", container.FreeList.Count), GUILayout.Width(100f));
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
            NGUIEditorTools.EndContents();
        }
        GUILayout.EndScrollView();
    }
}