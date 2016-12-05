using UnityEngine;
using System.Collections;
using UnityEditor;
using HeroFX;

[CustomEditor(typeof(TextManager), true)]
public class TextManagerInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("TextManagerInspector", new Color(0.6f, 0.9f, 0.6f));

    public override void OnInspectorGUI()
    {
        TextManager manager = (TextManager)target;

        if (EditorApplication.isPlaying)
        {
            s_Util.SeparatorToolbar("Debug", null);
            OnInspectorDebug("DamagePhysic", manager.DamagePhysic);
            OnInspectorDebug("DamageMagic", manager.DamageMagic);
            OnInspectorDebug("Heal", manager.Heal);
            OnInspectorDebug("Mana", manager.Mana);
            OnInspectorDebug("Message", manager.Message);
        }

        s_Util.SeparatorToolbar("System", null);
        OnInspectorPrefab("DamagePhysic", manager.DamagePhysic);
        OnInspectorPrefab("DamageMagic", manager.DamageMagic);
        OnInspectorPrefab("Heal", manager.Heal);
        OnInspectorPrefab("Mana", manager.Mana);
        OnInspectorPrefab("Message", manager.Message);

        if (GUI.changed || EditorApplication.isPlaying)
            EditorUtility.SetDirty(target);
    }

    void OnInspectorDebug(string name, TextManager.TextPrefab prefab)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} : {1}/{2}", name, prefab.PlayCount, prefab.PlayCount + prefab.FreeCount));
        EditorGUILayout.EndHorizontal();
    }

    void OnInspectorPrefab(string name, TextManager.TextPrefab prefab)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUIUtility.labelWidth = 60f;
        prefab.prefab = EditorGUILayout.ObjectField(name, prefab.prefab, typeof(TextAnimation), false) as TextAnimation;
        prefab.prewarm = EditorGUILayout.IntField("Prewarm", prefab.prewarm, GUILayout.Width(100f));
        EditorGUILayout.EndHorizontal();
        EditorGUIUtility.labelWidth = 0f;
    }
}