using UnityEngine;
using System.Collections;
using HeroFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class SkillCamera : MonoBehaviour {
    public float frame = 0f;

    Animation m_Animation;
    Camera m_Camera;

    void Awake()
    {
        m_Animation = gameObject.GetComponent<Animation>();
        m_Camera = gameObject.GetComponentInChildren<Camera>();
    }

    void OnEnable()
    {
        SetDefault();
    }

    public void SetDefault()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        m_Camera.orthographicSize = 18 * CameraAspect.Ratio;
        m_Camera.backgroundColor = Color.black;
        m_Camera.transform.localPosition = new Vector3(0f, 17.5f, -100f);
        m_Camera.transform.localRotation = Quaternion.identity;
        m_Camera.transform.localScale = Vector3.one;
    }

    public void ApplyAnimation(AnimationClip clip, float playback_time, bool is_reverse)
    {
        clip.SampleAnimation(gameObject, playback_time);
        m_Camera.orthographicSize *= CameraAspect.Ratio;

        if (is_reverse)
        {
            Vector3 pos = transform.localPosition;
            pos.x *= -1f;
            transform.localPosition = pos;

            pos = m_Camera.transform.localPosition;
            pos.x *= -1f;
            m_Camera.transform.localPosition = pos;

            Vector3 rot = transform.localRotation.eulerAngles;
            rot.y *= -1f;
            rot.z *= -1f;
            transform.localRotation = Quaternion.Euler(rot);

            rot = m_Camera.transform.localRotation.eulerAngles;
            rot.y *= -1f;
            rot.z *= -1f;
            m_Camera.transform.localRotation = Quaternion.Euler(rot);
        }
    }

#if UNITY_EDITOR
    public bool CheckFrame(AnimationClip clip)
    {
        EditorCurveBinding bind = new EditorCurveBinding();
        bind.path = "";
        bind.type = typeof(SkillCamera);
        bind.propertyName = "frame";

        AnimationCurve data = AnimationUtility.GetEditorCurve(clip, bind);
        if (data == null)
        {
            Debug.LogErrorFormat("[Error] SkillCamera.Frame not exists");
            return false;
        }

        if (data.keys.Length != 2)
        {
            Debug.LogErrorFormat("[Error] SkillCamera.Frame Count : {0}", data.keys.Length);
            return false;
        }

        bool re = true;
        float frameRate = clip.frameRate;
        foreach (var key in data.keys)
        {
            float playback_time = key.value / frameRate;

            if (playback_time != key.time)
            {
                Debug.LogErrorFormat("[Error:{0}] SkillCamera.Frame Number", key.time);
                re = false;
            }
        }
        return re;
    }

    // Update is called once per frame
    void Update ()
    {
        if (EditorApplication.isPlaying == true || Selection.activeGameObject == null || m_Animation.clip == null)
            return;

        if (CoreUtility.GetParentComponent<SkillCamera>(Selection.activeGameObject.transform) == null)
            return;

        if (AnimationMode.InAnimationMode() == false)
        {
            return;
        }

        if (CheckFrame(m_Animation.clip) == false)
            return;

        float frameRate = m_Animation.clip.frameRate;
        float playback_time = frame / frameRate;

        GameObject obj = GameObject.Find("Characters/Mine/Center");
        if (obj != null)
        {
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                Transform child = obj.transform.GetChild(i);
                if (child.gameObject.activeInHierarchy == false)
                    continue;

                var character_animation = child.GetComponent<CharacterAnimation>();
                if (character_animation == null)
                    continue;

                if (playback_time != character_animation.PlaybackTime)
                {
                    bool playback_back = playback_time < character_animation.PlaybackTime;

                    if (character_animation.IsPlaying == false || character_animation.CurrentState != null || character_animation.CurrentState.name != "skill_leader")
                        character_animation.Play(true, "skill_leader");
                    else
                        character_animation.IsPause = true;

                    character_animation.UpdatePlay(playback_time);

                    HFX_ParticleSystem[] particle_systems = character_animation.GetComponentsInChildren<HFX_ParticleSystem>();
                    if (particle_systems == null || particle_systems.Length == 0)
                        continue;

                    foreach (var particle in particle_systems)
                    {
                        if (particle.IsPause == false)
                            particle.IsPause = true;
                        if (particle.IsPlaying == false || particle.IsPlayingAll() == false && playback_back == true)
                        {
                            particle.Stop();
                            particle.Play(true, particle.Seed);
                        }
                        particle.SetPlaybackTime(playback_time);
                        particle.SetLightingMax(1f);
                    }

                    return;
                }
            }
        }
	}
#endif
}
