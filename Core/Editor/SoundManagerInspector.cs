using UnityEngine;
using System.Collections;
using UnityEditor;
using HeroFX;

[CustomEditor(typeof(SoundManager), true)]
public class SoundManagerInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("SoundManagerInspector", new Color(0.6f, 0.9f, 0.6f));

    public override void OnInspectorGUI()
    {
        SoundManager manager = (SoundManager)target;

        if (EditorApplication.isPlaying)
        {
            s_Util.SeparatorToolbar("Debug", null);
            EditorGUILayout.LabelField(string.Format("Playing : {0} / {1}", manager.PlayingList.Count, manager.PlayingList.Count+manager.FreeList.Count));

            foreach (var source in manager.PlayingList)
            {
                EditorGUILayout.LabelField(string.Format("{0} : {1}/{2}", source.clip.name, source.time, source.clip.length));
            }
        }

        s_Util.SeparatorToolbar("System", null);
        manager.Prewarm = EditorGUILayout.IntField("Prewarm", manager.Prewarm);
        manager.volume = EditorGUILayout.Slider("Volume", manager.volume, 0f, 1f);

        if (GUI.changed || EditorApplication.isPlaying)
            EditorUtility.SetDirty(target);
    }
}