using UnityEngine;
using System.Collections;
using HeroFX;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("SmallHeroes/UI/UIParticleContainer")]
public class UIParticleContainer : UIWidget
{
    public enum eUIParticlePlay
    {
        DeltaTime,
        RealTimeSinceStartup,
    }

    public string mDepthGroupID;
    public override string DepthID
    {
        get
        {
            return mDepthGroupID;
        }
    }

    public bool ContinueTime = true;
    public bool IsAutoPlay = true;
    public string particle_name = "";
    public HFX_ParticleSystem ParticleAsset { get; private set; }
    public bool IsInit { get; private set; }

    public eUIParticlePlay play_type;

    override public eWidgetDepth DepthType { get { return eWidgetDepth.Particle; } }

    public bool IsPlaying { get { return ParticleAsset == null ? false : ParticleAsset.IsPlaying; } }
    public bool IsFinish { get { return ParticleAsset == null ? false : ParticleAsset.IsFinish; } }
    public float PlaybackTime { get; private set; }

    override protected void OnEnable()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            base.OnEnable();
            return;
        }
#endif
        base.OnEnable();

        if (IsAutoPlay)
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
        gameObject.SetActive(true);
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
                    ParticleAsset.PlaybackTime = PlaybackTime;
                else
                    PlaybackTime = 0f;
            }
            return;
        }

        IsInit = true;

        if (string.IsNullOrEmpty(particle_name))
            return;

        PlaybackTime = 0f;
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
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            return;
        }
#endif

        if (ParticleAsset == null || ParticleAsset.IsPlaying == false)
        {
            return;
        }

        if (drawCall == null)
            return;

        ParticleAsset.SetRenderQueue(drawCall.renderQueue);
        PlaybackTime = ParticleAsset.PlaybackTime;

        if (play_type == eUIParticlePlay.RealTimeSinceStartup)
        {
            ParticleAsset.UpdatePlay(Time.realtimeSinceStartup);
        }
    }

    override protected void OnDisable()
    {
        base.OnDisable();
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
