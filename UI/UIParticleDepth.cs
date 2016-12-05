using UnityEngine;
using System.Collections;
using HeroFX;

[AddComponentMenu("SmallHeroes/UI/UIParticleDepth")]
public class UIParticleDepth : MonoBehaviour
{
    public enum eUIParticlePlay
    {
        DeltaTime,
        RealTimeSinceStartup,
    }

    public bool ContinueTime = true;
    public bool IsAutoPlay = true;
    public string particle_name = "";
    public int depth = 0;
    public HFX_ParticleSystem ParticleAsset { get; private set; }
    public bool IsInit { get; private set; }

    public eUIParticlePlay play_type;

    float playback_time = 0f;
    void OnEnable()
    {
        if (IsAutoPlay == false)
            return;

        Play();
    }

    public void Stop()
    {
        if (IsInit == true)
        {
            if (ParticleAsset != null)
            {
                ParticleAsset.Stop();
            }
            return;
        }
    }

    public void Play()
    {
        if (IsInit == true)
        {
            if (ParticleAsset != null)
            {
                ParticleAsset.Stop();
                if (play_type == eUIParticlePlay.DeltaTime)
                    ParticleAsset.Play(false, 0);
                else
                {
                    if (ContinueTime == false)
                        ParticleAsset.Delay = Time.realtimeSinceStartup;
                    ParticleAsset.Play(true, 0);
                }

                ParticleAsset.SetLightingMax(1f);
                if (IsAutoPlay == true && ContinueTime == true)
                    ParticleAsset.PlaybackTime = playback_time;
            }
            return;
        }

        IsInit = true;

        if (string.IsNullOrEmpty(particle_name))
            return;

        ParticleAsset = GameObject.Instantiate<HFX_ParticleSystem>(AssetManager.GetParticleSystem(particle_name));
        if (ParticleAsset != null)
        {
            ParticleAsset.transform.SetParent(transform, false);
            ParticleAsset.gameObject.SetActive(true);
            if (ContinueTime == false && play_type == eUIParticlePlay.RealTimeSinceStartup)
                ParticleAsset.Delay = Time.realtimeSinceStartup;
            ParticleAsset.Play(play_type == eUIParticlePlay.RealTimeSinceStartup, 0);
            ParticleAsset.SetLightingMax(1f);
        }
        else
            Debug.LogWarningFormat("particle not found : {0}", particle_name);
    }

    void LateUpdate()
    {
        if (ParticleAsset == null || ParticleAsset.IsPlaying == false)
            return;

        UIPanel panel = CoreUtility.GetParentComponent<UIPanel>(transform);
        int renderQueue = panel.startingRenderQueue;
        ParticleAsset.SetRenderQueue(renderQueue+depth);
        playback_time = ParticleAsset.PlaybackTime;

        if (play_type == eUIParticlePlay.RealTimeSinceStartup)
            ParticleAsset.UpdatePlay(Time.realtimeSinceStartup);
    }

    void OnDisable()
    {
        if (ParticleAsset != null)
            ParticleAsset.Stop();
    }

    void OnDestroy()
    {
        if (ParticleAsset != null)
        {
            GameObject.Destroy(ParticleAsset.gameObject);
            ParticleAsset = null;
        }
    }
}
