using UnityEngine;
using System.Collections;
using HeroFX;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(CharacterActionBuffComponent), true)]
public class CharacterActionBuffComponentInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("CharacterActionBuffComponentInspector", new Color(0.9f, 0.6f, 0.6f));

    class ActionEffectLoopList : InspectorList<CharacterAction_Effect>
    {
        public CharacterActionBuffData data { get; set; }
        public ActionEffectLoopList(CharacterActionBuffData data)
            : base(s_Util, "Loop", false)
        {
            this.data = data;
        }

        override public CharacterAction_Effect[] Datas { get { if (data.loop == null) data.loop = new CharacterAction_Effect[0]; return data.loop; } set { data.loop = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_Effect CreateNewData() { CharacterAction_Effect new_data = new CharacterAction_Effect(); return new_data; }
        override public CharacterAction_Effect CloneData(int index) { CharacterAction_Effect clone_data = Datas[index].Clone(null, null); return clone_data; }

        override protected void OnInspectorItem(int index, CharacterAction_Effect selected)
        {
            selected.OnInspectorItem(index, selected);

            EditorGUIUtility.labelWidth = 40f;
            eAttachParticle new_attach_type = (eAttachParticle)EditorGUILayout.EnumPopup("Attach", selected.AttachType, GUILayout.Width(140f));
            switch (new_attach_type)
            {
                case eAttachParticle.Target:
                case eAttachParticle.TargetScale:
                case eAttachParticle.World:
                case eAttachParticle.WorldScale:
                    selected.AttachType = new_attach_type;
                    break;
            }
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    class ActionEffectHitList : InspectorList<CharacterAction_Effect>
    {
        public CharacterActionBuffData data { get; set; }
        public ActionEffectHitList(CharacterActionBuffData data)
            : base(s_Util, "Hit", false)
        {
            this.data = data;
        }

        override public CharacterAction_Effect[] Datas { get { if (data.hit == null) data.hit = new CharacterAction_Effect[0]; return data.hit; } set { data.hit = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_Effect CreateNewData() { CharacterAction_Effect new_data = new CharacterAction_Effect(); return new_data; }
        override public CharacterAction_Effect CloneData(int index) { CharacterAction_Effect clone_data = Datas[index].Clone(null, null); return clone_data; }

        override protected void OnInspectorItem(int index, CharacterAction_Effect selected)
        {
            selected.OnInspectorItem(index, selected);
        }
    }

    ActionEffectLoopList loop_list = new ActionEffectLoopList(null);
    ActionEffectHitList hit_list = new ActionEffectHitList(null);

    string[] Names;
    ColorContainer color_container;
    void OnEnable()
    {
        color_container = AssetDatabase.LoadAssetAtPath<ColorContainer>("Assets/Character/000_tween.prefab");
        RefreshNames();
    }

    void RefreshNames()
    {
        var names = color_container.Colors.Select(c => c.name).ToList();
        names.Add("New");
        Names = names.ToArray();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CharacterActionBuffComponent component = ((CharacterActionBuffComponent)target);
        CharacterActionBuffData data = component.data;
        loop_list.data = data;
        hit_list.data = data;

        data.AnimationName = EditorGUILayout.TextField("Animation", data.AnimationName);
        data.Freeze = EditorGUILayout.Toggle("Freeze", data.Freeze);
        EditorGUILayout.BeginHorizontal();
        int select_index = EditorGUILayout.Popup("State Color", color_container.GetSelectedIndex(data.StateColorName), Names);

        ColorData new_color;
        if (select_index == Names.Length-1)
        {
            var color_list = new List<ColorData>(color_container.Colors);
            new_color = new ColorData();
            new_color.name = "New";
            color_list.Add(new_color);
            color_container.Colors = color_list.ToArray();

            RefreshNames();
        }
        new_color = color_container.Colors[select_index];
        data.StateColorName = new_color.name;
        Color32 change_color = EditorGUILayout.ColorField(new_color.color);
        if (change_color.Equals(new_color.color) == false)
        {
            new_color.color = change_color;
            EditorUtility.SetDirty(color_container);
        }
        EditorGUILayout.EndHorizontal();

        data.TweenName = HFX_TweenSystemInspector.OnInspectorTween(component.GetComponent<HFX_TweenSystem>(), data.TweenName);

        if (component.sub_components == null)
        {
            component.sub_components = new CharacterActionBuffComponent[0];
            EditorUtility.SetDirty(target);
        }
        SerializedProperty sp = serializedObject.FindProperty("sub_components");
        EditorGUILayout.PropertyField(sp, new GUIContent(string.Format("Sub Actions ({0})", component.sub_components.Length)), true);

        loop_list.OnInspectorGUI();
        hit_list.OnInspectorGUI();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
        serializedObject.ApplyModifiedProperties();
    }
}