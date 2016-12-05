using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class SoundManagerAutoInitialize
{
    [RuntimeInitializeOnLoadMethod]
    public static void OnLoad()
    {
        if (SoundManager.Instance == null)
        {
            new GameObject("SoundManager", typeof(SoundManager));
        }
    }
}

public class SoundPlay
{
    int InstanceIndex;

    internal SoundPlay()
    {
        InstanceIndex = SoundManager.InstanceIndex;
    }

    public List<AudioSource> sources = new List<AudioSource>();
    public void Finish()
    {
        if (SoundManager.InstanceIndex != InstanceIndex)
            return;

        foreach (var source in sources.Where(s => s.loop == true))
        {
            source.Stop();
        }
    }
}

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance = null;

    public static int InstanceIndex = 0;

    public float volume = 1f;

    public int Prewarm = 50;

    public bool mute
    {
        set
        {
            UIPlaySound.ForcedStop = value;
            if (value == false)
                StopSound();
        }
    }

    public Stack<AudioSource> FreeList { get; private set; }
    public List<AudioSource> PlayingList { get; private set; }

    public AudioSource m_BGM;

    void Awake()
    {
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);

        m_BGM = GetComponent<AudioSource>();
        m_BGM.ignoreListenerPause = true;
        m_BGM.loop = true;

        FreeList = new Stack<AudioSource>();
        PlayingList = new List<AudioSource>();
        for (int i = 0; i < Prewarm; ++i)
        {
            FreeList.Push(NewSource());
        }

        AudioListener.pause = false;
    }

    void OnDestroy()
    {
        ++InstanceIndex;
    }

    static public SoundPlay PlaySound(AudioClip clip, float time = 0f)
    {
        return Instance.PlaySoundInternal(clip, time);
    }

    static public SoundPlay PlaySound(SoundInfo[] sounds, float time, float tick, int count, bool loop)
    {
        return Instance.PlaySoundInternal(sounds, time, tick, count, loop);
    }

    public void StopSound()
    {
        foreach (var source in PlayingList)
        {
            source.name = "_pool";
            source.Stop();
            source.time = 0f;
            source.enabled = false;
            source.loop = false;
            source.clip = null;
            source.gameObject.SetActive(false);
            FreeList.Push(source);
        }
        PlayingList.Clear();
        ++InstanceIndex;
    }

    void Update()
    {
        var finish_list = PlayingList.Where(a => a.isPlaying == false);
        if (finish_list.Count() > 0)
        {
            foreach (var source in finish_list)
            {
                source.Stop();
                source.time = 0f;
                source.enabled = false;
                source.loop = false;
                source.clip = null;
                source.name = "_pool";
                source.gameObject.SetActive(false);
                FreeList.Push(source);
            }
            PlayingList.RemoveAll(s => s.isPlaying == false && s.enabled == false);
            m_NeedToNormalize = true;
        }
        if (m_NeedToNormalize)
        {
            Normalize();
            m_NeedToNormalize = false;
        }
    }

    void Normalize()
    {
        foreach (var group in PlayingList.GroupBy(s => s.name))
        {
            foreach (var group2 in group.GroupBy(g => g.time))
            {
                int group_count = group2.Count();
                if (group_count == 0)
                    continue;

                float mix_volume = volume / group_count;
                foreach (AudioSource source in group2)
                {
                    source.volume = mix_volume;
                }
            }
        }
    }

    SoundPlay PlaySoundInternal(AudioClip clip, float time)
    {
        if (clip == null
#if !SH_ASSETBUNDLE
            || ConfigData.Instance.UseSound == false || ConfigData.Instance.IsMute == true
#endif
            )
            return null;

        AudioSource source = GetFreeSource(clip);
        if (time > 0f)
        {
            source.PlayDelayed(time);
        }
        else
        {
            source.Play();
            if (time < 0f)
                source.time = -time;
        }

        SoundPlay play = new SoundPlay();
        play.sources.Add(source);

        return play;
    }

    SoundPlay PlaySoundInternal(SoundInfo[] sounds, float time, float tick, int count, bool loop)
    {
        if (sounds == null || sounds.Length == 0
#if !SH_ASSETBUNDLE
            || ConfigData.Instance.UseSound == false || ConfigData.Instance.IsMute == true
#endif
            )
            return null;

        if (tick == 0f)
            count = 1;

        SoundPlay play = new SoundPlay();
        for (int i = 0; i < count; ++i)
        {
            SoundInfo info = sounds[sounds.Length==1?0:MNS.Random.Instance.NextRange(0, sounds.Length - 1)];
            AudioSource source = GetFreeSource(info.sound);
            source.priority = 128 + System.Math.Max(100, count * 10);
            source.volume = volume;
            source.loop = loop;

            float local_time = info.time + time + i * tick;

            if (local_time > 0f)
            {
                source.PlayDelayed(local_time);
            }
            else if (local_time == 0f)
                source.Play();
            else if (source.clip.length > -local_time)
            {
                source.Play();
                source.time = -local_time;
            }
            play.sources.Add(source);
        }

        return play;
    }

    static public int SortByEndTime(AudioSource a, AudioSource b) { return (a.clip.length - a.time).CompareTo(b.clip.length - b.time); }

    AudioSource NewSource()
    {
        GameObject go = new GameObject("_pool", typeof(AudioSource));
        go.transform.SetParent(transform, false);
        go.gameObject.SetActive(false);
        AudioSource source = go.GetComponent<AudioSource>();
        source.hideFlags = HideFlags.DontSave;
        source.reverbZoneMix = 0f;
        source.dopplerLevel = 0f;
        source.playOnAwake = false;
        return source;
    }
    bool m_NeedToNormalize = false;

    AudioSource GetFreeSource(AudioClip clip)
    {
        AudioSource source = null;
        if (FreeList.Count > 0)
        {
            source = FreeList.Pop();
            source.enabled = true;
        }
        else
        {
            source = NewSource();
        }

        source.gameObject.SetActive(true);
        source.volume = volume;
        source.clip = clip;
        source.name = clip.name;

        PlayingList.Add(source);
        PlayingList.Sort(SortByEndTime);
        m_NeedToNormalize = true;
        return source;
    }

    string m_BGMName = "";
    public void PlayBGM(string name, bool force = false)
    {
#if !SH_ASSETBUNDLE
        if (name == m_BGMName && force == false)
            return;

        m_BGMName = name;
        if (ConfigData.Instance.UseMusic && ConfigData.Instance.IsMute == false)
        {
            m_BGM.Stop();

            if (string.IsNullOrEmpty(name) == false)
            {
                m_BGM.clip = AssetManager.GetSound(name);
                m_BGM.Play();
            }
        }
#endif
    }

    public void PlayBGM()
    {
        PlayBGM(m_BGMName, true);
    }

    public void StopBGM()
    {
        m_BGM.Stop();
    }

    public bool IsPlayingBGM { get { return m_BGM.isPlaying; } }
}
