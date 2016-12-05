using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;

public enum eInspectorMapCreatureType
{
    Normal,
    Elite,
    Boss,
    RaidBoss,
}

[CustomEditor(typeof(Character), true)]
public class CharacterInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("CharacterInspector", new Color(0.9f, 0.6f, 0.6f));

    class ActionList : InspectorSelectList<CharacterActionData>
    {
        Character character;
        public ActionList(Character character)
            : base(s_Util, "Character", "Action", false)
        {
            this.character = character;
            if (character.Actions == null) character.Actions = new CharacterActionData[0];
        }

        override public CharacterActionData[] Datas { get { return character.Actions; } set { character.Actions = value; } }
        override public string[] Names { get { return Datas.Select(d => d.AnimationName).ToArray(); } }

        override public string GetDataName(int index) { return Datas[index].AnimationName; }
        override public void SetDataName(int index, string name) { }

        override public CharacterActionData CreateNewData() { CharacterActionData new_data = new CharacterActionData(); return new_data; }
        override public CharacterActionData CloneData(int index) { CharacterActionData clone_data = Datas[index].Clone(false, null, null, 0f); return clone_data; }
    }

    class ActionEffectCastingList : InspectorList<CharacterAction_EffectCasting>
    {
        public Character character { get; set; }
        public CharacterAction_EffectContainer container { get; set; }
        public ActionEffectCastingList(CharacterAction_EffectContainer container)
            : base(s_Util, "Casting", false)
        {
            this.container = container;
        }

        override public CharacterAction_EffectCasting[] Datas { get { if (container.Casting == null) container.Casting = new CharacterAction_EffectCasting[0]; return container.Casting; } set { container.Casting = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_EffectCasting CreateNewData() { CharacterAction_EffectCasting new_data = new CharacterAction_EffectCasting(); return new_data; }
        override public CharacterAction_EffectCasting CloneData(int index) { CharacterAction_EffectCasting clone_data = Datas[index].Clone(null, null); return clone_data; }

        protected override void OnToolbarInspector()
        {
            if (GUILayout.Button("Add to scene", EditorStyles.toolbarButton))
            {
                foreach (var effect in container.Casting)
                {
                    CharacterInspector.AddParticleSystemToScene(character, effect.particle_system_prefab, effect.time);
                }
            }
        }

        override protected void OnInspectorItem(int index, CharacterAction_EffectCasting selected)
        {
            selected.OnInspectorItem(index, selected);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(40f);
            EditorGUIUtility.labelWidth = 40f;
            eAttachParticle new_attach_type = (eAttachParticle)EditorGUILayout.EnumPopup("Attach", selected.AttachType, GUILayout.Width(100f));
            switch(new_attach_type)
            {
                case eAttachParticle.SelfCenter:
                case eAttachParticle.TargetCenter:
                    break;

                default:
                    selected.AttachType = new_attach_type;
                    break;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    public class ActionEffectTargetList : InspectorList<CharacterAction_EffectTarget>
    {
        public Character character { get; set; }
        public CharacterAction_EffectContainer container { get; set; }
        public ActionEffectTargetList(CharacterAction_EffectContainer container)
            : base(s_Util, "Target", false)
        {
            this.container = container;
        }

        override public CharacterAction_EffectTarget[] Datas { get { if (container.Target == null) container.Target = new CharacterAction_EffectTarget[0]; return container.Target; } set { container.Target = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_EffectTarget CreateNewData() { CharacterAction_EffectTarget new_data = new CharacterAction_EffectTarget(); return new_data; }
        override public CharacterAction_EffectTarget CloneData(int index) { CharacterAction_EffectTarget clone_data = Datas[index].Clone(null, null, null); return clone_data; }

        protected override void OnToolbarInspector()
        {
            if (GUILayout.Button("Add to scene", EditorStyles.toolbarButton))
            {
                foreach (var effect in container.Target)
                {
                    CharacterInspector.AddParticleSystemToScene(character, effect.particle_system_prefab, effect.time);
                }
            }
        }

        override protected void OnInspectorItem(int index, CharacterAction_EffectTarget selected)
        {
            selected.OnInspectorItem(index, selected);

            EditorGUILayout.BeginHorizontal();
//            GUILayout.Space(40f);
            EditorGUIUtility.labelWidth = 40f;
            selected.time_tick = EditorGUILayout.FloatField("Tick", selected.time_tick, GUILayout.Width(80f));
            selected.count = EditorGUILayout.IntField("Count", selected.count, GUILayout.Width(80f));
            selected.AttachParticle = (eAttachParticle)EditorGUILayout.EnumPopup("Attach", selected.AttachParticle, GUILayout.Width(100f));
            selected.TweenName = HFX_TweenSystemInspector.OnInspectorTween(character.GetComponent<HFX_TweenSystem>(), selected.TweenName);
            selected.FinishParticleAfterTween = (CharacterAction_EffectTarget.eFinishParticle)EditorGUILayout.EnumPopup("Finish", selected.FinishParticleAfterTween, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    public class ActionEffectHitList : InspectorList<CharacterAction_EffectHit>
    {
        public Character character { get; set; }
        public CharacterAction_EffectContainer container { get; set; }
        public ActionEffectHitList(CharacterAction_EffectContainer container)
            : base(s_Util, "Hit", false)
        {
            this.container = container;
        }

        override public CharacterAction_EffectHit[] Datas { get { if (container.Hit == null) container.Hit = new CharacterAction_EffectHit[0]; return container.Hit; } set { container.Hit = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_EffectHit CreateNewData() { CharacterAction_EffectHit new_data = new CharacterAction_EffectHit(); return new_data; }
        override public CharacterAction_EffectHit CloneData(int index) { CharacterAction_EffectHit clone_data = Datas[index].Clone(null, null, null, null); return clone_data; }

        override protected void OnInspectorItem(int index, CharacterAction_EffectHit selected)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 40f;
            GUILayout.Space(20f);
            selected.IsEnable = EditorGUILayout.Toggle(selected.IsEnable, GUILayout.Width(20f));
            selected.time = EditorGUILayout.FloatField("Time", selected.time, GUILayout.Width(100f));
            EditorGUIUtility.labelWidth = 50f;
            selected.chance = EditorGUILayout.IntField("Chance", selected.chance, GUILayout.Width(100f));
            selected.action_component_prefab = EditorGUILayout.ObjectField("Action", selected.action_component_prefab, typeof(CharacterActionHitComponent), false) as CharacterActionHitComponent;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            selected.target_linked = EditorGUILayout.ToggleLeft("Linked", selected.target_linked, GUILayout.Width(60f));
            if (selected.target_linked == false)
            {
                selected.time_tick = EditorGUILayout.FloatField("Tick", selected.time_tick, GUILayout.Width(80f));
                selected.count = EditorGUILayout.IntField("Count", selected.count, GUILayout.Width(80f));
                EditorGUIUtility.labelWidth = 70f;
                selected.time_gap = EditorGUILayout.FloatField("TimeGap", selected.time_gap, GUILayout.Width(120f));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    public class ActionEffectBuffList : InspectorList<CharacterAction_EffectBuff>
    {
        public Character character { get; set; }
        public CharacterAction_EffectContainer container { get; set; }
        public ActionEffectBuffList(CharacterAction_EffectContainer container)
            : base(s_Util, "Buff", false)
        {
            this.container = container;
        }

        override public CharacterAction_EffectBuff[] Datas { get { if (container.Buff == null) container.Buff = new CharacterAction_EffectBuff[0]; return container.Buff; } set { container.Buff = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_EffectBuff CreateNewData() { CharacterAction_EffectBuff new_data = new CharacterAction_EffectBuff(); return new_data; }
        override public CharacterAction_EffectBuff CloneData(int index) { CharacterAction_EffectBuff clone_data = Datas[index].Clone(null, null, null, null); return clone_data; }

        override protected void OnInspectorItem(int index, CharacterAction_EffectBuff selected)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 40f;
            GUILayout.Space(20f);
            selected.IsEnable = EditorGUILayout.Toggle(selected.IsEnable, GUILayout.Width(20f));
            selected.time = EditorGUILayout.FloatField("Time", selected.time, GUILayout.Width(100f));
            EditorGUIUtility.labelWidth = 50f;
            selected.target_linked = EditorGUILayout.ToggleLeft("Linked", selected.target_linked, GUILayout.Width(60f));
            selected.action_component_prefab = EditorGUILayout.ObjectField("Action", selected.action_component_prefab, typeof(CharacterActionBuffComponent), false) as CharacterActionBuffComponent;
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    public class ActionEffectCameraList : InspectorList<CharacterAction_EffectCamera>
    {
        public Character character { get; set; }
        public CharacterAction_EffectContainer container { get; set; }
        public ActionEffectCameraList(CharacterAction_EffectContainer container)
            : base(s_Util, "Camera", false)
        {
            this.container = container;
        }

        override public CharacterAction_EffectCamera[] Datas { get { if (container.Camera == null) container.Camera = new CharacterAction_EffectCamera[0]; return container.Camera; } set { container.Camera = value; } }

        override public string GetDataName(int index) { return string.Format("{0}", index); }
        override public void SetDataName(int index, string name) { }

        override public CharacterAction_EffectCamera CreateNewData() { CharacterAction_EffectCamera new_data = new CharacterAction_EffectCamera(); return new_data; }
        override public CharacterAction_EffectCamera CloneData(int index) { CharacterAction_EffectCamera clone_data = Datas[index].Clone(null, null, null, null); return clone_data; }

        override protected void OnInspectorItem(int index, CharacterAction_EffectCamera selected)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 40f;
            GUILayout.Space(20f);
            selected.IsEnable = EditorGUILayout.Toggle(selected.IsEnable, GUILayout.Width(20f));
            selected.time = EditorGUILayout.FloatField("Time", selected.time, GUILayout.Width(100f));
            EditorGUIUtility.labelWidth = 50f;
            selected.target_linked = EditorGUILayout.ToggleLeft("Linked", selected.target_linked, GUILayout.Width(60f));
            selected.power = EditorGUILayout.FloatField("Power", selected.power);
            selected.duration = EditorGUILayout.Slider("Duration", selected.duration, 0.01f, 1f);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }

    ActionList action_list = null;
    ActionEffectCastingList action_effect_casting_list = new ActionEffectCastingList(null);
    ActionEffectTargetList action_effect_target_list = new ActionEffectTargetList(null);
    ActionEffectHitList action_effect_hit_list = new ActionEffectHitList(null);
    ActionEffectBuffList action_effect_buff_list = new ActionEffectBuffList(null);
    ActionEffectCameraList action_effect_camera_list = new ActionEffectCameraList(null);
    List<CharacterContainer> containers = null;

    void OnEnable()
    {
        action_list = new ActionList((Character)target);
        containers = new List<CharacterContainer>(GameObject.FindObjectsOfType<CharacterContainer>().Where(c => c.transform.parent != null && c.name != "Center").OrderBy(c => c.transform.parent.name + "/" + c.name));
    }

    void OnDisable()
    {
        action_list = null;
    }

    enum RecordState
    {
        None,
        Waiting,
        WaitingWithCamera,
        Recording,
        Finishing,
    }
    RecordState m_Recording = RecordState.None;
#if SH_ASSETBUNDLE
    float m_RecordFinishTime = 0f;
#endif
    float time_skip = 0f;

    public override void OnInspectorGUI()
    {
//         base.OnInspectorGUI();

        Character character = (Character)target;
        CharacterAnimation character_animation = character.GetComponent<CharacterAnimation>();

        EditorGUILayout.BeginVertical();
        // debug
        if (Application.isPlaying)
        {
            s_Util.SeparatorToolbar("Debug", null);

            int selected_container = EditorPrefs.GetInt("Character_ActionTarget", 1);
            int range = EditorPrefs.GetInt("Character_ActionRange", 1);

            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 70f;
            Time.timeScale = EditorGUILayout.FloatField("TimeScale", Time.timeScale, GUILayout.Width(110f));
            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                Time.timeScale = 1f;
            }
            if (GUILayout.Button("Slow", GUILayout.Width(50f)))
            {
                Time.timeScale = 0.1f;
            }
            if (GUILayout.Button("Pause", GUILayout.Width(50f)))
            {
                Time.timeScale = 0f;
            }
            float new_time_skip = EditorGUILayout.FloatField("TimeSkip", time_skip, GUILayout.Width(110f));
            if (GUILayout.Button("Reset", GUILayout.Width(50f)))
            {
                new_time_skip = 0f;
            }
            if (GUILayout.Button("0.5", GUILayout.Width(50f)))
            {
                new_time_skip = 0.5f;
            }
            if (GUILayout.Button("1", GUILayout.Width(50f)))
            {
                new_time_skip = 1f;
            }
            if (time_skip != new_time_skip)
            {
                time_skip = new_time_skip;
            }
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            GameObject countdown_object = null;
#if SH_ASSETBUNDLE
            if (m_Recording != RecordState.None)
                GUI.backgroundColor = Color.green;

            if ((GUILayout.Button("Record") && m_Recording == RecordState.None) && BattleBase.Instance.IsPause == ePauseType.None)
            {
                m_Recording = RecordState.Waiting;
//                m_Recording = RecordState.WaitingWithCamera;

                if (m_Recording == RecordState.WaitingWithCamera)
                    BattleTest.Instance._capture._forceFilename = string.Format("{0}_{1}_scaled.avi", character.name, action_list.Selected.AnimationName);
                else
                    BattleTest.Instance._capture._forceFilename = string.Format("{0}_{1}.avi", character.name, action_list.Selected.AnimationName);
                BattleTest.Instance._capture.SelectCodec(false);
                BattleTest.Instance._capture.SelectAudioDevice(false);
                // We have to queue the start capture otherwise Screen.width and height aren't correct
                BattleTest.Instance._capture.QueueStartCapture();

                BattleTest.Instance.m_Countdown.gameObject.SetActive(true);
                BattleTest.Instance.m_RecordTime.gameObject.SetActive(true);
                Debug.Log("Record Start");
            }
            GUI.backgroundColor = Color.white;
            if (m_Recording == RecordState.Waiting || m_Recording == RecordState.WaitingWithCamera)
                countdown_object = BattleTest.Instance.m_Countdown.gameObject;
#endif
            if ((GUILayout.Button("Play") || m_Recording == RecordState.Waiting && countdown_object.activeInHierarchy == false) && BattleBase.Instance.IsPause == ePauseType.None)
            {
                if (m_Recording == RecordState.Waiting)
                    m_Recording = RecordState.Recording;

                CharacterContainer action_target = containers[selected_container];

                List<CharacterContainer> target_containers = containers.Where(c => c.transform.parent.name == action_target.transform.parent.name).ToList();
                SkillTargetContainer skill_target = new SkillTargetContainer();
                skill_target.targets = new List<ISkillTarget>();

                skill_target.targets.Add(new SkillTargetDummy(character, action_target.Character));
                int target_index = target_containers.FindIndex(c => c == action_target);
                for (int i = 1; i < range; ++i)
                {
                    if (target_index - i >= 0)
                        skill_target.targets.Add(new SkillTargetDummy(character, target_containers[target_index - i].Character));
                    else
                        skill_target.targets.Add(null);
                    if (target_index + i < target_containers.Count)
                        skill_target.targets.Add(new SkillTargetDummy(character, target_containers[target_index + i].Character));
                    else
                        skill_target.targets.Add(null);
                }
                
                if (skill_target.targets != null)
                {
                    skill_target.main_target = skill_target.targets[0].Character.transform;
                    character.DoActionEditor(action_list.SelectedIndex, skill_target, false, character.Creature.Scale-1f, SkillTargetDummy.BuffDuration);
                    if (new_time_skip > 0f)
                        character.Creature.PlaybackTime += new_time_skip;
                }
            }
            if ((GUILayout.Button("Play With Camera") || m_Recording == RecordState.WaitingWithCamera && countdown_object.activeInHierarchy == false) && BattleBase.Instance.IsPause == ePauseType.None)
            {
                if (m_Recording == RecordState.WaitingWithCamera)
                    m_Recording = RecordState.Recording;

                CharacterContainer action_target = containers[selected_container];

                List<CharacterContainer> target_containers = containers.Where(c => c.transform.parent.name == action_target.transform.parent.name).ToList();
                SkillTargetContainer skill_target = new SkillTargetContainer();
                skill_target.targets = new List<ISkillTarget>();

                skill_target.targets.Add(new SkillTargetDummy(character, action_target.Character));
                int target_index = target_containers.FindIndex(c => c == action_target);
                for (int i = 1; i < range; ++i)
                {
                    if (target_index - i >= 0)
                        skill_target.targets.Add(new SkillTargetDummy(character, target_containers[target_index - i].Character));
                    else
                        skill_target.targets.Add(null);
                    if (target_index + i < target_containers.Count)
                        skill_target.targets.Add(new SkillTargetDummy(character, target_containers[target_index + i].Character));
                    else
                        skill_target.targets.Add(null);
                }

                if (skill_target.targets != null)
                {
                    skill_target.main_target = skill_target.targets[0].Character.transform;
                    character.DoActionEditor(action_list.SelectedIndex, skill_target, true, BattleBase.LightingScaleValue, SkillTargetDummy.BuffDuration);
                    BattleBase.Instance.AddLighting(character.Creature, character.MainAction.FirstActionTime, character.MainAction.Data.Effect.ScaleTime, character.MainAction.Data.Effect.JumpScale);
                    BattleBase.Instance.AddLightingTargets(character.Creature.IsTeam, skill_target.targets, character.Creature);
                    if (new_time_skip > 0f)
                        character.Creature.PlaybackTime += new_time_skip;
                }
            }
            if (GUILayout.Button("Cancel"))
            {
                character.CancelAction(false);
            }
            EditorGUILayout.EndHorizontal();

            if (BattleBase.Instance != null)
            {
                EditorGUILayout.BeginHorizontal();
                BattleBase.Instance.tween_system.DefaultBundleIndex = EditorGUILayout.Popup("tween", BattleBase.Instance.tween_system.DefaultBundleIndex, BattleBase.Instance.tween_system.bundles.Select(b => b.Name).ToArray());
                if (GUILayout.Button("Play"))
                {
                    BattleBase.Instance.tween_system.Play(BattleBase.Instance.tween_system.DefaultBundleIndex, null, character.GetComponent<HFX_TweenSystem>(), character.transform.GetChild(0));
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (character.Creature != null)
            {
                eInspectorMapCreatureType scale_type = eInspectorMapCreatureType.Normal;
                if (character.Creature.Scale == 1f) scale_type = eInspectorMapCreatureType.Normal;
                else if (character.Creature.Scale == 1.2f) scale_type = eInspectorMapCreatureType.Elite;
                else if (character.Creature.Scale == 1.4f) scale_type = eInspectorMapCreatureType.Boss;

                eInspectorMapCreatureType new_scale_type = (eInspectorMapCreatureType)EditorGUILayout.EnumPopup("Scale", scale_type);
                if (scale_type != new_scale_type)
                {
                    float new_scale = 1f;
                    if (new_scale_type == eInspectorMapCreatureType.Normal) new_scale = 1f;
                    else if (new_scale_type == eInspectorMapCreatureType.Elite) new_scale = 1.2f;
                    else if (new_scale_type == eInspectorMapCreatureType.Boss) new_scale = 1.4f;

                    character.Creature.Scale = new_scale;
                    character.transform.localScale = Vector3.one * new_scale;
                }
                character.Creature.IsDead = EditorGUILayout.Toggle("Dead", character.Creature.IsDead);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            List<string> container_names = new List<string>();
            container_names.AddRange(containers.Select(c => c.transform.parent.name + "/" + c.name));
            container_names.Add("Self");
            container_names.Add("Enemy1");
            container_names.Add("Enemy5");
            container_names.Add("Mine1");
            container_names.Add("Mine5");

            selected_container = EditorGUILayout.Popup(selected_container, container_names.ToArray());
            if (selected_container >= 0 && selected_container < container_names.Count)
            {
                switch (container_names[selected_container])
                {
                    case "Self":
                        selected_container = container_names.IndexOf(character.transform.parent.parent.name + "/" + character.transform.parent.name);
                        break;

                    case "Mine1":
                        selected_container = container_names.IndexOf("Mine/Character1");
                        break;

                    case "Mine5":
                        selected_container = container_names.IndexOf("Mine/Character5");
                        break;

                    case "Enemy1":
                        selected_container = container_names.IndexOf("Enemy/Character1");
                        break;

                    case "Enemy5":
                        selected_container = container_names.IndexOf("Enemy/Character5");
                        break;
                }
            }
            EditorPrefs.SetInt("Character_ActionTarget", selected_container);

            range = EditorGUILayout.IntSlider(range, 1, 5);
            EditorPrefs.SetInt("Character_ActionRange", range);
            EditorGUILayout.EndHorizontal();

            if (character.PlayingActions.Count > 0)
            {
                foreach (var action in character.PlayingActions)
                {
                    EditorGUILayout.LabelField(string.Format("{0} - {1}", action.Name, action.Length));
                }
                EditorUtility.SetDirty((MonoBehaviour)character);
            }
#if SH_ASSETBUNDLE
            else if (m_Recording == RecordState.Recording)
            {
                m_Recording = RecordState.Finishing;
                m_RecordFinishTime = Time.time + 3f;
                BattleTest.Instance.m_RecordTime.SetFinish();
            }

            if (m_Recording == RecordState.Finishing && Time.time > m_RecordFinishTime)
            {
                m_Recording = RecordState.None;
                BattleTest.Instance._capture.StopCapture();
                BattleTest.Instance.m_RecordTime.gameObject.SetActive(false);
                Debug.Log("Record Finished");
            }
#endif
        }

        s_Util.SeparatorToolbar("System", null);
        if (GUILayout.Button("Set Sound"))
        {
            foreach (var action in character.Actions)
            {
                string audio_path = string.Format("Assets/Sounds/Character/{0}_{1}.wav", character.name, action.AnimationName);
                AudioClip audio = AssetDatabase.LoadAssetAtPath(audio_path, typeof(AudioClip)) as AudioClip;
                if (audio != null)
                {
                    if (action.Effect.Casting.Length == 0 || action.Effect.Casting[0].time != 0)
                    {
                        var temp_action_list = action.Effect.Casting.ToList();
                        temp_action_list.Add(new CharacterAction_EffectCasting());
                        action.Effect.Casting = temp_action_list.ToArray();
                    }
                    if (action.Effect.Casting[0].sound_list[0].sound == null)
                    {
                        action.Effect.Casting[0].sound_list[0].sound = audio;
                        Debug.LogFormat("Set Sound : {0}", audio.name);
                    }
                }
            }
        }
        GUILayout.BeginHorizontal();
        character.m_DefaultEffect = EditorGUILayout.ObjectField("Default Effect", character.m_DefaultEffect, typeof(HFX_ParticleSystem), false) as HFX_ParticleSystem;
        if (GUILayout.Button("Add to scene", GUILayout.Width(150f)))
        {
            AddParticleSystemToScene(character, character.m_DefaultEffect, 0f);
        }
        GUILayout.EndHorizontal();

        CharacterActionData selected_action = action_list.OnInspectorGUI();

        if (selected_action != null)
        {
            selected_action.AnimationName = CharacterAnimationInspector.OnInspectorAnimation(character_animation, selected_action.AnimationName);
            selected_action.CameraAnimation = EditorGUILayout.ObjectField("Camera Animation", selected_action.CameraAnimation, typeof(AnimationClip), false) as AnimationClip;
            if (selected_action.CameraAnimation != null)
            {
                selected_action.ActionMode = (BattleBase.eActionMode)EditorGUILayout.EnumPopup("ActionMode", selected_action.ActionMode);
                selected_action.DelayedApply = EditorGUILayout.Toggle("Delayed Apply", selected_action.DelayedApply);
            }
            selected_action.Effect.ScaleTime = EditorGUILayout.FloatField("Scale Time", selected_action.Effect.ScaleTime);
            selected_action.Effect.MoveScale = EditorGUILayout.FloatField("Move Scale", selected_action.Effect.MoveScale);
            selected_action.Effect.JumpScale = EditorGUILayout.FloatField("Jump Scale", selected_action.Effect.JumpScale);
            selected_action.Effect.FxHeight = EditorGUILayout.FloatField("Fx Height", selected_action.Effect.FxHeight);
            selected_action.Effect.UseSingTarget = EditorGUILayout.Toggle("Use Single Target", selected_action.Effect.UseSingTarget);
            selected_action.Effect.TargetTimeGap = EditorGUILayout.FloatField("Time Gap", selected_action.Effect.TargetTimeGap);
            selected_action.Effect.TargetTimeGroup = EditorGUILayout.IntField("Time Group", selected_action.Effect.TargetTimeGroup);

            action_effect_casting_list.character = character;
            action_effect_casting_list.container = selected_action.Effect;
            action_effect_casting_list.OnInspectorGUI();

            action_effect_target_list.character = character;
            action_effect_target_list.container = selected_action.Effect;
            action_effect_target_list.OnInspectorGUI();

            action_effect_hit_list.character = character;
            action_effect_hit_list.container = selected_action.Effect;
            action_effect_hit_list.OnInspectorGUI();

            action_effect_buff_list.character = character;
            action_effect_buff_list.container = selected_action.Effect;
            action_effect_buff_list.OnInspectorGUI();

            action_effect_camera_list.character = character;
            action_effect_camera_list.container = selected_action.Effect;
            action_effect_camera_list.OnInspectorGUI();
        }

        EditorGUILayout.EndVertical();

        EditorGUIUtility.labelWidth = 0f;
        EditorUtility.SetDirty((MonoBehaviour)character);
    }

    static public void AddParticleSystemToScene(Character character, HFX_ParticleSystem particle_system, float time)
    {
        if (character == null || particle_system == null)
            return;

        var new_particle_system = (PrefabUtility.InstantiatePrefab(particle_system.gameObject) as GameObject).GetComponent<HFX_ParticleSystem>();
        new_particle_system.transform.SetParent(character.transform, false);
        new_particle_system.SetPlaybackTime(0f);
    }
}

