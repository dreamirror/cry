using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;

[CustomEditor(typeof(CharacterAnimation), true)]
public class CharacterAnimationInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("CharacterAnimationInspector", new Color(0.7f, 0.7f, 1f));

    class LoopStateList : InspectorList<CharacterAnimation.LoopState>
    {
        CharacterAnimation animation;
        public LoopStateList(CharacterAnimation animation)
            : base(s_Util, "LoopState", false)
        {
            use_clone = false;
            this.animation = animation;
            if (animation.LoopStates == null) animation.LoopStates = new CharacterAnimation.LoopState[0];
        }

        override public CharacterAnimation.LoopState[] Datas { get { return animation.LoopStates; } set { animation.LoopStates = value; } }

        override public string GetDataName(int index) { return Datas[index].name; }
        override public void SetDataName(int index, string name) { }

        override public CharacterAnimation.LoopState CreateNewData() { if (animation.CurrentState == null) return null; CharacterAnimation.LoopState new_data = new CharacterAnimation.LoopState(animation.CurrentState.name, 0, 0, 1); return new_data; }
        override public CharacterAnimation.LoopState CloneData(int index) { return null; }

        protected override void OnInspectorItem(int index, CharacterAnimation.LoopState selected)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUIUtility.labelWidth = 60f;
            EditorGUILayout.LabelField(selected.name);
            selected.range_min = EditorGUILayout.IntField("Min", selected.range_min);
            selected.range_max = EditorGUILayout.IntField("Max", selected.range_max);
            selected.loop_count = EditorGUILayout.IntField("Loop", selected.loop_count);
            EditorGUIUtility.labelWidth = 0f;
            EditorGUILayout.EndHorizontal();
        }
    }
    LoopStateList loop_state_list = null;
    CharacterAnimation m_Prefab = null;
    void OnEnable()
    {
        CharacterAnimation character_animation = (CharacterAnimation)target;
        loop_state_list = new LoopStateList(character_animation);

        CheckAnimations();

        var obj = PrefabUtility.GetPrefabParent(character_animation.gameObject) as GameObject;
        if (obj != null)
            m_Prefab = obj.GetComponent<CharacterAnimation>();
        else
            m_Prefab = null;
    }

    void CheckAnimations()
    {
        //        if (!EditorUtility.IsPersistent(target))
        {
            CharacterAnimation character_animation = (CharacterAnimation)target;
            if (character_animation.Animation == null)
                return;

            List<string> animations = new List<string>();
            animations.Add("none");
            foreach (AnimationState state in character_animation.Animation)
            {
                animations.Add(state.name);
            }
            s_Animations = animations.ToArray();
        }
    }

    static string[] s_Animations;
    Dictionary<string, bool> fold_movestate = new Dictionary<string, bool>();
    Dictionary<string, bool> fold_fxstate = new Dictionary<string, bool>();

    static List<HFX_ParticleSystem> m_Particles = new List<HFX_ParticleSystem>();

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        CharacterAnimation character_animation = (CharacterAnimation)target;

#if SH_ASSETBUNDLE
        if (m_Prefab != null)
        {
            if (m_Prefab.PrefabState == false)
            {
                Debug.LogFormat("[{0}] CreateImport", m_Prefab.name);
                string skin = null;
                if (character_animation.CurrentSkin != null && character_animation.CurrentSkin.Asset != null)
                    skin = character_animation.CurrentSkin.Asset.name;
                character_animation.CreateImport();
                if (string.IsNullOrEmpty(skin) == false)
                    character_animation.SetSkin(skin);
            }
        }
#endif

        EditorGUILayout.BeginHorizontal();  // whole

#if SH_ASSETBUNDLE
        if (!EditorUtility.IsPersistent(character_animation))
        {
            if (GUILayout.Button("Create Import"))
            {
                character_animation.CreateImport();
                CheckAnimations();
            }
        }
#endif

        if (GUILayout.Button(string.Format("Idle Mode({0})", character_animation.GetIdleRotation())))
            character_animation.SetIdleMode();
        if (GUILayout.Button("Battle Mode"))
            character_animation.SetBattleMode();

        EditorGUILayout.EndHorizontal();  // whole


        if (character_animation.Animation == null)
            return;

        if (s_Animations == null || character_animation.Animation.GetClipCount() != s_Animations.Length)
            CheckAnimations();

        EditorGUILayout.BeginVertical();  // whole
        EditorGUILayout.Separator();

        character_animation.CheckDefaultState();

//        if (EditorApplication.isPlaying)
        {
            if (s_Util.SeparatorToolbarFold("Debug", null))
            {
                if (character_animation != null && character_animation.CurrentState != null)
                    EditorGUILayout.LabelField(string.Format("{0} : {1}/{2}", character_animation.CurrentState.name, character_animation.PlaybackTime, character_animation.CurrentState.length));
                else
                    EditorGUILayout.LabelField("none");
                if (character_animation.RootBone != null)
                {
                    if (character_animation.CurrentMoveState != null)
                        EditorGUILayout.LabelField(string.Format("RootBonePos : {0} / {1} ({2:p}), Move : {3}", character_animation.RootBonePos, character_animation.CurrentMoveState.length, character_animation.RootBonePos.x / character_animation.CurrentMoveState.length, character_animation.MoveValue));
                }
                if (character_animation.FxBone != null && character_animation.CurrentFxState != null)
                {
                    EditorGUILayout.LabelField(string.Format("FxBonePos : {0} / {1} ({2:p}), MoveFx : {3}", character_animation.FxBonePos, character_animation.CurrentFxState.fixed_length, character_animation.FxBonePos.x / character_animation.CurrentFxState.length, character_animation.MoveValueFx));
                    EditorGUILayout.LabelField(string.Format("FxBones : {0}", character_animation.FxBone.localPosition));
                }
                EditorGUILayout.Toggle("IsUIMode", character_animation.IsUIMode);
                EditorUtility.SetDirty((MonoBehaviour)character_animation);
            }
        }

        //        if (EditorApplication.isPlaying == false)
        {
            //if (EditorApplication.isPlaying == false)
                OnInspectorSkin(character_animation);
            OnInspectorPlay(character_animation);

            if (character_animation.MoveStates == null)
                character_animation.MoveStates = new CharacterAnimation.MoveState[0];
            if (character_animation.FxStates == null)
                character_animation.FxStates = new CharacterAnimation.FxState[0];
        }

        if (GUILayout.Button("Play Head"))
            character_animation.PlayHead();

        bool fold_move = s_Util.SeparatorToolbarFold("MoveStates", string.Format("Move States ({0})", character_animation.MoveStates.Length), false, false);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            character_animation.RefreshMove(true);
        }
        s_Util.EndToolbar();

        if (fold_move == true)
        {
            foreach (CharacterAnimation.MoveState state in character_animation.MoveStates)
            {
                EditorGUILayout.BeginHorizontal();

                bool fold = false;
                if (fold_movestate.TryGetValue(state.name, out fold) == false)
                    fold_movestate.Add(state.name, fold);

                bool fold_new = GUILayout.Toggle(fold, state.name, "Foldout");
                if (fold_new != fold)
                {
                    fold_movestate.Remove(state.name);
                    fold_movestate.Add(state.name, fold_new);
                }

                bool new_enabled = GUILayout.Toggle(state.enabled, "");
                if (new_enabled != state.enabled)
                {
                    state.enabled = new_enabled;
                    character_animation.SetMoveState(true);
                }
                EditorGUIUtility.labelWidth = 60f;
                EditorGUILayout.LabelField("Length", state.length.ToString());
                state.fixed_length = EditorGUILayout.FloatField("Fixed", state.fixed_length);
                if (GUILayout.Button("Get"))
                {
                    state.fixed_length = state.length;
                }
                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndHorizontal();

                if (fold_new == true)
                {
                    EditorCurveBinding bind = new EditorCurveBinding();
                    bind.path = "Root";
                    bind.type = typeof(Transform);
                    bind.propertyName = "m_LocalPosition.x";

                    AnimationClip clip = character_animation.Animation[state.name].clip;
                    AnimationCurve data = AnimationUtility.GetEditorCurve(clip, bind);
                    foreach (var key in data.keys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.LabelField(string.Format("{0} ({1})", key.time * clip.frameRate, key.time.ToString()), key.value.ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        int enable_fx_count = character_animation.FxStates.Count(s => s.enabled);
        bool fold_fx = s_Util.SeparatorToolbarFold("FxStates", string.Format("Fx States ({0}/{1})", enable_fx_count, character_animation.FxStates.Length), false, false);
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
        {
            character_animation.RefreshFx(true);
        }
        s_Util.EndToolbar();

        if (fold_fx == true)
        {
            foreach (CharacterAnimation.FxState state in character_animation.FxStates)
            {
                EditorGUILayout.BeginHorizontal();

                bool fold = false;
                if (fold_fxstate.TryGetValue(state.name, out fold) == false)
                    fold_fxstate.Add(state.name, fold);

                if (state.SubBoneNames == null)
                    state.SubBoneNames = new string[0];

                state.CheckBone(character_animation.transform);

                bool fold_new = GUILayout.Toggle(fold, string.Format("{0} ({1}/{2})", state.name, state.SubBones.Count(b => b != null), state.SubBones.Count), "Foldout");
                if (fold_new != fold)
                {
                    fold_fxstate.Remove(state.name);
                    fold_fxstate.Add(state.name, fold_new);
                }

                bool new_enabled = GUILayout.Toggle(state.enabled, "");
                if (new_enabled != state.enabled)
                {
                    state.enabled = new_enabled;
                    character_animation.SetFxState(true);
                }
                EditorGUIUtility.labelWidth = 60f;
                EditorGUILayout.LabelField("Length", state.length.ToString());
                state.fixed_length = EditorGUILayout.FloatField("Fixed", state.fixed_length);
                if (GUILayout.Button("Get"))
                {
                    state.fixed_length = state.length;
                }
                EditorGUIUtility.labelWidth = 0f;
                EditorGUILayout.EndHorizontal();

                if (fold_new == true)
                {
                    int remove_index = -1;
                    for (int bone_index = 0; bone_index < state.SubBoneNames.Length; ++bone_index)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("-", GUILayout.Width(22f)))
                        {
                            remove_index = bone_index;
                        }
                        Transform new_bone = EditorGUILayout.ObjectField("SubBone", state.SubBones[bone_index], typeof(Transform), true) as Transform;
                        if (new_bone != state.SubBones[bone_index])
                        {
                            state.SubBones[bone_index] = new_bone;
                            state.SubBoneNames[bone_index] = CoreUtility.GetHierachy(new_bone, character_animation.transform);
                            EditorUtility.SetDirty((MonoBehaviour)character_animation);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    if (remove_index != -1)
                    {
                        List<string> temp_list = new List<string>(state.SubBoneNames);
                        temp_list.RemoveAt(remove_index);
                        state.SubBoneNames = temp_list.ToArray();
                    }

                    EditorGUI.indentLevel = 2;
                    {
                        Transform new_bone = EditorGUILayout.ObjectField("SubBone", null, typeof(Transform), true) as Transform;
                        if (new_bone != null)
                        {
                            Array.Resize<string>(ref state.SubBoneNames, state.SubBoneNames.Length + 1);
                            state.SubBoneNames[state.SubBoneNames.Length - 1] = CoreUtility.GetHierachy(new_bone, character_animation.transform);
                            EditorUtility.SetDirty((MonoBehaviour)character_animation);
                        }
                    }
                    EditorGUI.indentLevel = 0;

                    EditorCurveBinding bind = new EditorCurveBinding();
                    bind.path = "fx_bone";
                    bind.type = typeof(Transform);
                    bind.propertyName = "m_LocalPosition.x";

                    AnimationClip clip = character_animation.Animation[state.name].clip;
                    AnimationCurve data = AnimationUtility.GetEditorCurve(clip, bind);
                    foreach (var key in data.keys)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(20f);
                        EditorGUILayout.LabelField(string.Format("{0} ({1})", key.time * clip.frameRate, key.time.ToString()), key.value.ToString());
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        loop_state_list.OnInspectorGUI();

        EditorGUILayout.EndVertical();    // whole

        if (GUI.changed)
            EditorUtility.SetDirty((MonoBehaviour)character_animation);
    }

    static void RefreshParticles(CharacterAnimation character_animation, bool refresh_force = false)
    {
        List<HFX_ParticleSystem> particle_systems = new List<HFX_ParticleSystem>(character_animation.GetComponentsInChildren<HFX_ParticleSystem>());
        particle_systems.AddRange(GameObject.FindObjectsOfType<HFX_ParticleSystem>());
        if (particle_systems.Count == 0)
        {
            m_Particles.Clear();
            return;
        }

        if (character_animation.CurrentState != null)
        {
            string find_name = character_animation.name + "_" + character_animation.CurrentState.name;
            m_Particles = particle_systems.Where(ps => ps.name.StartsWith(find_name)).ToList();
        }

        if (EditorPrefs.GetBool("CharacterInspectorDefaultEffectMode", false) == true)
        {
            string find_name = character_animation.name + "_" + "default";
            m_Particles.AddRange(particle_systems.Where(ps => ps.name.StartsWith(find_name)));
        }
    }

    static public void OnInspectorPlayback(string name, CharacterAnimation character_animation, bool reset_playback)
    {
        if (!EditorUtility.IsPersistent(character_animation))
        {
            EditorGUILayout.BeginHorizontal();
            float playback_time = EditorGUILayout.FloatField(name + "Time", character_animation.PlaybackTime);
            playback_time = character_animation.PlaybackTime + (playback_time - character_animation.PlaybackTime) * 0.1f;

            bool update_playback_local = playback_time != character_animation.PlaybackTime;
            bool playback_back = playback_time < character_animation.PlaybackTime;
            if (update_playback_local)
            {
                if (character_animation.IsPlaying == false)
                    character_animation.Play(true);
                else
                    character_animation.IsPause = true;

                character_animation.UpdatePlay(playback_time);
            }
            if (EditorApplication.isPlaying == false || character_animation.GetComponent<Character>().enabled == false)
            {
                RefreshParticles(character_animation);
                if (IsApplyEffect == true && m_Particles != null && character_animation.IsPlaying == true)
                {
                    foreach (var particle in m_Particles)
                    {
                        if (particle.IsPause == false)
                            particle.IsPause = true;
                        if (particle.IsPlaying == false || particle.IsPlayingAll() == false && (playback_back == true || reset_playback == true))
                        {
                            particle.Stop();
                            particle.Play(true, particle.Seed);
                        }
                        particle.SetPlaybackTime(playback_time);
                        particle.SetLightingMax(1f);
                    }
                }
            }
            float length = character_animation.CurrentState ? character_animation.CurrentState.length : 0f;
            EditorGUILayout.LabelField(string.Format("/ {0}", length));
            EditorGUILayout.EndHorizontal();

            character_animation.PlaybackSpeed = EditorGUILayout.Slider(name + "Speed", character_animation.PlaybackSpeed, 0f, 2f);
            if (character_animation.PlaybackSpeed < 0f)
                character_animation.PlaybackSpeed = 0f;

            // for move
            EditorGUILayout.BeginHorizontal();
            character_animation.UseMove = EditorGUILayout.Toggle("Move", character_animation.UseMove);
            character_animation.MoveValue = EditorGUILayout.Vector3Field("", character_animation.MoveValue);
            EditorGUILayout.EndHorizontal();
//            EditorGUILayout.Vector3Field("", character_animation.MoveValueFx);
        }

        int cur_index = GetAnimIndex(character_animation.CurrentState);
        int prev_index = GetAnimIndex(character_animation.PrevState);
        int default_index = GetAnimIndex(character_animation.DefaultState);

        int new_index = EditorGUILayout.Popup("Current", cur_index, s_Animations);
        if (new_index != cur_index)
        {
            character_animation.Stop();
            foreach (var particle in m_Particles)
                particle.Stop();

            character_animation.CurrentState = character_animation.Animation[s_Animations[new_index]];
            RefreshParticles(character_animation, true);
        }

        new_index = EditorGUILayout.Popup("Prev", prev_index, s_Animations);
        if (new_index != prev_index) character_animation.PrevState = character_animation.Animation[s_Animations[new_index]];

        new_index = EditorGUILayout.Popup("Default", default_index, s_Animations);
        if (new_index != default_index) character_animation.DefaultState = character_animation.Animation[s_Animations[new_index]];

        bool camera_mode = EditorPrefs.GetBool("CharacterInspectorCameraMode", false);
        bool default_effect_mode = EditorPrefs.GetBool("CharacterInspectorDefaultEffectMode", false);
        EditorGUILayout.BeginHorizontal();
        bool new_camera_mode = EditorGUILayout.Toggle("Camera Mode", camera_mode);
        if (new_camera_mode != camera_mode)
            EditorPrefs.SetBool("CharacterInspectorCameraMode", new_camera_mode);
        EditorGUIUtility.labelWidth = 90f;
        bool new_default_effect_mode = EditorGUILayout.Toggle("Default Effect", default_effect_mode);
        if (new_default_effect_mode != default_effect_mode)
            EditorPrefs.SetBool("CharacterInspectorDefaultEffectMode", new_default_effect_mode);
        EditorGUILayout.EndHorizontal();

        if (new_camera_mode && EditorApplication.isPlaying == false)
        {
            GameObject camera2 = GameObject.Find("2_CameraPivot");
            if (camera2 != null)
            {
                var cam_anim = camera2.GetComponent<Animation>();
                if (cam_anim != null && cam_anim.clip != null)
                {
                    if (AnimationMode.InAnimationMode() == false)
                        cam_anim.clip.SampleAnimation(cam_anim.gameObject, character_animation.PlaybackTime);
                    else
                        AnimationMode.SampleAnimationClip(cam_anim.gameObject, cam_anim.clip, character_animation.PlaybackTime);
                }
            }
        }
    }

    static int GetAnimIndex(AnimationState state)
    {
        if (state == null)
            return 0;

        return Array.IndexOf(s_Animations, state.name);
    }

    public void OnInspectorSkin(CharacterAnimation character_animation)
    {
        if (character_animation.Skins.Length == 0 || EditorUtility.IsPersistent(character_animation.gameObject) == true)
            return;

        int index = 0;
        if (character_animation.CurrentSkin != null)
            index = Array.FindIndex(character_animation.Skins, s => s == character_animation.CurrentSkin.Asset.name);
        int new_index = EditorGUILayout.Popup("Skin", index, character_animation.Skins);
        if (character_animation.Skins.Length > 0 && (index != new_index || character_animation.CurrentSkin == null))
        {
            character_animation.SetSkin(character_animation.Skins[new_index]);
        }
    }

    public bool OnInspectorPlay(CharacterAnimation character_animation)
    {
        GUI.changed = false;

        //        Color backup_color = GUI.color;
        bool reset_playback = false;
        if (!EditorUtility.IsPersistent(character_animation))
        {
            EditorGUILayout.BeginHorizontal();
            if (character_animation.IsPlaying == true)
            {
                if (character_animation.IsPause == false)
                {
                    if (GUILayout.Button("Pause"))
                    {
                        character_animation.IsPause = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Resume"))
                    {
                        character_animation.IsPause = false;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("Play"))
                {
                    character_animation.Play(false);
                }
            }
            if (GUILayout.Button("Reset"))
            {
                reset_playback = true;
                character_animation.Reset();
                foreach (var particle in m_Particles)
                {
                    HFX_ParticleSystemInspector.GetSeed(particle);
                }
            }
            if (GUILayout.Button("Stop"))
            {
                character_animation.Stop();
                foreach (var particle in m_Particles)
                    particle.Stop();
            }
            EditorGUILayout.EndHorizontal();
        }
        OnInspectorPlayback("Playback ", character_animation, reset_playback);

        if (GUI.changed == true)
            EditorUtility.SetDirty((MonoBehaviour)character_animation);

        if (EditorApplication.isPlaying == false && character_animation.IsPlaying)
            EditorUtility.SetDirty((MonoBehaviour)character_animation);

        return true;
    }

    static void OnSceneGUIInternal(CharacterAnimation character_animation)
    {
        if (character_animation.Animation == null || EditorApplication.isPlaying && UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path.StartsWith("Assets/AniTest") == false)
            return;

        int width = 240, height = 210;
        Rect r = new Rect(Screen.width - width, Screen.height - height - 40, width, height);

        Vector2 mouse = Event.current.mousePosition;

        Rect r2 = r;
        r2.yMin -= 30;
        r2.xMin -= 10;
        r2.xMax += 10;
        r2.yMax += 10;

        if (r2.Contains(mouse) && Event.current.type == EventType.Layout)
        {
            int controlID = GUIUtility.GetControlID(1024, FocusType.Passive);
            HandleUtility.AddControl(controlID, 0F);
        }

        Handles.BeginGUI();
        GUILayout.BeginArea(r, character_animation.gameObject.name, "Window");
        bool reset_playback = false;
        EditorGUILayout.BeginHorizontal();
        if (character_animation.IsPlaying == true)
        {
            if (character_animation.IsPause == false)
            {
                if (GUILayout.Button("Pause"))
                {
                    character_animation.IsPause = true;
                }
            }
            else
            {
                if (GUILayout.Button("Resume"))
                {
                    character_animation.IsPause = false;
                }
            }
        }
        else
        {
            if (GUILayout.Button("Play"))
            {
                character_animation.Play(false);
            }
        }
        if (GUILayout.Button("Reset"))
        {
            reset_playback = true;
            character_animation.Reset();
            foreach (var particle in m_Particles)
            {
                HFX_ParticleSystemInspector.GetSeed(particle);
            }
        }
        if (GUILayout.Button("Stop"))
        {
            character_animation.Stop();
            foreach (var particle in m_Particles)
                particle.Stop();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 50f;
        OnInspectorPlayback("", character_animation, reset_playback);

        EditorGUIUtility.labelWidth = 0f;

        EditorGUILayout.BeginHorizontal();
        bool linked = EditorPrefs.GetBool("CharacterInspectorLinkEffect", true);
        int particle_count = character_animation.transform.GetComponentsInChildren<HFX_ParticleSystem>().Sum(p => p.ParticleCount);
        IsApplyEffect = EditorGUILayout.ToggleLeft(string.Format("Effect - {0}", particle_count), IsApplyEffect, GUILayout.Width(100f));
        GUILayout.FlexibleSpace();

        if (linked == true && HFX_ParticleSystemInspector.SceneGUIInspector == null)
        {
            HFX_ParticleSystemInspector.SceneGUIInspector = OnSceneGUIInspector;
            HFX_ParticleSystemInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
            HFX_EmitterInspector.SceneGUIInspector = OnSceneGUIInspector;
            HFX_EmitterInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
        }
        if (HFX_ParticleSystemInspector.SceneGUIInspector != null)
        {
            if (GUILayout.Button("Unlink"))
            {
                EditorPrefs.SetBool("CharacterInspectorLinkEffect", false);
                HFX_ParticleSystemInspector.SceneGUIInspector = null;
                HFX_ParticleSystemInspector.OnPlaybackChanged = null;
                HFX_EmitterInspector.SceneGUIInspector = null;
                HFX_EmitterInspector.OnPlaybackChanged = null;
            }
        }
        else
        {
            if (GUILayout.Button("Link"))
            {
                EditorPrefs.SetBool("CharacterInspectorLinkEffect", true);
                HFX_ParticleSystemInspector.SceneGUIInspector = OnSceneGUIInspector;
                HFX_ParticleSystemInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
                HFX_EmitterInspector.SceneGUIInspector = OnSceneGUIInspector;
                HFX_EmitterInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (EditorApplication.isPlaying == false && character_animation.IsPlaying)
            EditorUtility.SetDirty((MonoBehaviour)character_animation);

        GUILayout.EndArea();

        Handles.EndGUI();
    }

    public void OnSceneGUI()
    {
        //         if (EditorApplication.currentScene.StartsWith("Assets/AniTest") == false)
        //             return;
        // 
        CharacterAnimation character_animation = (CharacterAnimation)target;
        if (character_animation == null)
            return;

        OnSceneGUIInternal(character_animation);
    }

    static public string OnInspectorAnimation(CharacterAnimation character_animation, string name)
    {
        if (character_animation == null || character_animation.Animation == null || character_animation.AnimationStates == null || character_animation.AnimationStates.Length == 0)
            return "";

        if (character_animation.AnimationStates == null)
            return "";

        int selected_animation_index = Array.FindIndex(character_animation.AnimationStates, s => s != null && s.name == name);
        int new_animation_index = EditorGUILayout.Popup("Animation", selected_animation_index, character_animation.AnimationStates.Where(s => s != null).Select(s => s.name).ToArray());
        if (new_animation_index != selected_animation_index)
        {
            if (new_animation_index < 0 || new_animation_index >= character_animation.AnimationStates.Length)
                name = null;
            else
                name = character_animation.AnimationStates[new_animation_index].name;
        }
        return name;
    }

    static bool OnSceneGUIInspector(UnityEngine.Object target)
    {
        Component obj = (Component)target;
        CharacterAnimation character_animation = obj.transform.GetComponentInParent<CharacterAnimation>();
        if (character_animation != null)
        {
            OnSceneGUIInternal(character_animation);
            return true;
        }
        return false;
    }

    static void OnEffectPlaybackChanged()
    {
        //IsApplyEffect = false;
    }

    static bool IsApplyEffect = true;
    //     void Awake()
    //     {
    //         HFX_ParticleSystemInspector.SceneGUIInspector = OnSceneGUIInspector;
    //         HFX_ParticleSystemInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
    //         HFX_EmitterInspector.SceneGUIInspector = OnSceneGUIInspector;
    //         HFX_EmitterInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
    //     }
}
