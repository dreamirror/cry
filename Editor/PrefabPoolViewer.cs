using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class PrefabPoolViewer : EditorWindow
{
    static public PrefabPoolViewer instance;

    [MenuItem("Utility/PrefabPool Status", false, 9)]
    static public void OpenTool()
    {
        EditorWindow.GetWindow<PrefabPoolViewer>(false, "PrefabPool Status", true).Show();
    }

    Vector2 mScroll = Vector2.zero;

    void OnEnable() { instance = this; PrefabPoolManager.OnChanged = OnSelectionChange; }
    void OnDisable() { instance = null; PrefabPoolManager.OnChanged = null; }
    void OnSelectionChange() { Repaint(); }

    /// <summary>
    /// Draw the custom wizard.
    /// </summary>
    /// 
    static readonly Color active_color = Color.white;
    static readonly Color deactive_color = new Color(1f, 0.6f, 0.6f);


    void OnGUI()
    {
        if (EditorApplication.isPlaying == false)
            return;

        GUILayout.Space(6f);

        mScroll = GUILayout.BeginScrollView(mScroll);

        if (NGUIEditorTools.DrawHeader(string.Format("Pool ({0})", PrefabPoolManager.Instance.Count)))
        {
            if (PrefabPoolManager.Instance.Count > 0)
            {
                NGUIEditorTools.BeginContents();
                foreach (var container in PrefabPoolManager.Instance.Pool.OrderBy(v => v.Name))
                {
                    GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(20f));

                    if (container.Count > 0)
                        GUI.contentColor = active_color;
                    else
                        GUI.contentColor = deactive_color;

                    EditorGUILayout.LabelField(container.Name, GUILayout.Width(200f));
                    EditorGUILayout.LabelField(string.Format("Using : {0}", container.Count), GUILayout.Width(100f));
                    EditorGUILayout.LabelField(string.Format("Free : {0}", container.FreeCount), GUILayout.Width(100f));
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
                NGUIEditorTools.EndContents();
            }
        }

        GUILayout.EndScrollView();
    }
}