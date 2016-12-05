using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(AtlasSprite))]
public class AtlasSpriteAnimation : MonoBehaviour {
    public enum AnimationType
    {
        None,
        Once,
        Loop,
    }
    public AnimationType m_AnimationType;
    public string[] Animations;
    public float AnimationLength = 1f, AnimationDelay = 0f;
    public bool IsPlayAnimationOnStart = true;
    public bool IsPlayAnimationOnEnable = true;
    public Action<AtlasSpriteAnimation> OnAnimationEnd = null, OnAnimationStart = null;

    AtlasSprite mSprite = null;
    public AtlasSprite Sprite
    {
        get
        {
            if (mSprite == null)
                mSprite = GetComponent<AtlasSprite>();
            return mSprite;
        }
    }

    void Update()
    {
        UpdateAnimation();
    }

    void Start()
    {
        if (IsPlayAnimationOnStart && CanAnimation)
            IsPlayingAnimation = true;
    }

    void OnEnable()
    {
        if (IsPlayAnimationOnEnable && CanAnimation)
            PlayAnimation();
    }

    bool CanAnimation { get { return m_AnimationType != AnimationType.None && AnimationLength > 0f || Animations.Length > 0; } }
    public bool IsPlayingAnimation { get; private set; }
    float PlaybackTime = 0f;
    void UpdateAnimation()
    {
        if (CanAnimation == false)
            return;

        PlaybackTime += Time.deltaTime;

        if (PlaybackTime < AnimationDelay)
        {
            return;
        }
        if (Sprite.Renderer.enabled == false)
        {
            Sprite.Renderer.enabled = true;
        }

        float playback_time = PlaybackTime - AnimationDelay;

        float animation_tick = AnimationLength / Animations.Length;

        int sprite_index = 0;
        switch (m_AnimationType)
        {
            case AnimationType.Loop:
                sprite_index = Mathf.FloorToInt(playback_time / animation_tick) % Animations.Length;
                break;

            case AnimationType.Once:
                sprite_index = Mathf.FloorToInt(playback_time / animation_tick);
                break;
        }

        if (sprite_index >= Animations.Length)
        {
            if (OnAnimationEnd != null)
                OnAnimationEnd(this);
            gameObject.SetActive(false);
            return;
        }

        string sprite_name = Animations[sprite_index];

        if (sprite_name != Sprite.spriteName)
            Sprite.spriteName = sprite_name;
    }

    public void PlayAnimation()
    {
        PlaybackTime = 0f;
        IsPlayingAnimation = true;
        Sprite.Renderer.enabled = false;
    }
}
