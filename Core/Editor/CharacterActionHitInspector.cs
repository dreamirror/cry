using UnityEngine;
using System.Collections;
using HeroFX;
using UnityEditor;

[CustomEditor(typeof(CharacterActionHitComponent), true)]
public class CharacterActionHitComponentInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("CharacterActionHitComponentInspector", new Color(0.9f, 0.6f, 0.6f));

    class ActionEffectList : InspectorList<CharacterAction_Effect>
    {
        public CharacterActionHitData data { get; set; }
        public ActionEffectList(CharacterActionHitData data)
            : base(s_Util, "Effect", false)
        {
            this.data = data;
        }

        override public CharacterAction_Effect[] Datas { get { return data.effects; } set { data.effects = value; } }

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
    ActionEffectList list = new ActionEffectList(null);

    public override void OnInspectorGUI()
    {
        CharacterActionHitComponent component = ((CharacterActionHitComponent)target);
        CharacterActionHitData data = component.data;
        list.data = data;

        data.AnimationName = EditorGUILayout.TextField("Animation", data.AnimationName);
        data.TweenTarget = (eCharacterTweenTarget)EditorGUILayout.EnumPopup("Tween Target", data.TweenTarget);
        data.TweenName = HFX_TweenSystemInspector.OnInspectorTween(component.GetComponent<HFX_TweenSystem>(), data.TweenName);
        list.OnInspectorGUI();

        if (GUI.changed)
            EditorUtility.SetDirty(target);
    }
}