using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;

[CustomEditor(typeof(CharacterCutScene), true)]
public class CharacterCutSceneInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("CharacterCutSceneInspector", new Color(0.7f, 0.7f, 1f));

    void OnEnable()
    {
        CheckAnimations();
    }

    void CheckAnimations()
    {
        //        if (!EditorUtility.IsPersistent(target))
        {
            CharacterCutScene character_animation = (CharacterCutScene)target;
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

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        CharacterCutScene character_animation = (CharacterCutScene)target;

        EditorGUILayout.BeginHorizontal();  // whole

        if (!EditorUtility.IsPersistent(character_animation))
        {
            if (GUILayout.Button("Create Import"))
            {
                character_animation.CreateImport();
                CheckAnimations();
            }
        }

        EditorGUILayout.EndHorizontal();  // whole


        if (character_animation.Animation == null)
            return;

        if (s_Animations == null || character_animation.Animation.GetClipCount() != s_Animations.Length)
            CheckAnimations();

        EditorGUILayout.BeginVertical();  // whole
        EditorGUILayout.Separator();

        //        if (EditorApplication.isPlaying)
        {
            if (s_Util.SeparatorToolbarFold("Debug", null))
            {
                if (character_animation != null && character_animation.CurrentState != null)
                    EditorGUILayout.LabelField(string.Format("{0} : {1}/{2}", character_animation.CurrentState.name, character_animation.PlaybackTime, character_animation.CurrentState.length));
                else
                    EditorGUILayout.LabelField("none");
                EditorUtility.SetDirty((MonoBehaviour)character_animation);
            }
        }

        //        if (EditorApplication.isPlaying == false)
        {
            OnInspectorPlay(character_animation);
        }

        EditorGUILayout.EndVertical();    // whole

        if (GUI.changed)
            EditorUtility.SetDirty((MonoBehaviour)character_animation);
    }

    static public void OnInspectorPlayback(string name, CharacterCutScene character_animation, bool reset_playback)
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
            float length = character_animation.CurrentState ? character_animation.CurrentState.length : 0f;
            EditorGUILayout.LabelField(string.Format("/ {0}", length));
            EditorGUILayout.EndHorizontal();

            character_animation.PlaybackSpeed = EditorGUILayout.Slider(name + "Speed", character_animation.PlaybackSpeed, 0f, 2f);
            if (character_animation.PlaybackSpeed < 0f)
                character_animation.PlaybackSpeed = 0f;
        }

        int cur_index = GetAnimIndex(character_animation.CurrentState);

        int new_index = EditorGUILayout.Popup("Current", cur_index, s_Animations);
        if (new_index != cur_index)
        {
            character_animation.Stop();
            character_animation.CurrentState = character_animation.Animation[s_Animations[new_index]];
        }
        else if (cur_index == 0)
        {
            character_animation.CurrentState = character_animation.Animation["default"];
        }
        if (character_animation.IsPlaying == false)
            character_animation.Play(false);
    }

    static int GetAnimIndex(AnimationState state)
    {
        if (state == null)
            return 0;

        return Array.IndexOf(s_Animations, state.name);
    }

    public bool OnInspectorPlay(CharacterCutScene character_animation)
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
            }
            if (GUILayout.Button("Stop"))
            {
                character_animation.Stop();
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

    static void OnSceneGUIInternal(CharacterCutScene character_animation)
    {
        if (character_animation.Animation == null || EditorApplication.isPlaying && UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path.StartsWith("Assets/AniTest") == false)
            return;

        int width = 240, height = 20+20*4;
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
        }
        if (GUILayout.Button("Stop"))
        {
            character_animation.Stop();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUIUtility.labelWidth = 50f;
        OnInspectorPlayback("", character_animation, reset_playback);

        EditorGUIUtility.labelWidth = 0f;

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
        CharacterCutScene character_animation = (CharacterCutScene)target;
        if (character_animation == null)
            return;

        OnSceneGUIInternal(character_animation);
    }

    static public string OnInspectorAnimation(CharacterCutScene character_animation, string name)
    {
        if (character_animation == null || character_animation.Animation == null || character_animation.AnimationStates == null || character_animation.AnimationStates.Length == 0)
            return "";

        int selected_animation_index = Array.FindIndex(character_animation.AnimationStates, s => s != null && s.name == name);
        int new_animation_index = EditorGUILayout.Popup("Animation", selected_animation_index, character_animation.AnimationStates.Select(s => s.name).ToArray());
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
        CharacterCutScene character_animation = obj.transform.GetComponentInParent<CharacterCutScene>();
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

    //     void Awake()
    //     {
    //         HFX_ParticleSystemInspector.SceneGUIInspector = OnSceneGUIInspector;
    //         HFX_ParticleSystemInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
    //         HFX_EmitterInspector.SceneGUIInspector = OnSceneGUIInspector;
    //         HFX_EmitterInspector.OnPlaybackChanged = OnEffectPlaybackChanged;
    //     }
}
