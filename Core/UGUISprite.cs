using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RawImage))]
[AddComponentMenu("UI/UGUISprite")]
public class UGUISprite : MonoBehaviour
{
    public enum AnimationType
    {
        None,
        Once,
        Loop,
    }

    [HideInInspector][SerializeField]
    UIAtlas mAtlas;
    [HideInInspector][SerializeField]
    string mSpriteName;

    [System.NonSerialized]
    protected UISpriteData mSprite;
    [System.NonSerialized]
    bool mSpriteSet = false;
    public Vector2 m_Pivot = Vector2.one * 0.5f;

    public bool UseNativeSize = false;
    public bool UsePivot = false;

    public AnimationType m_AnimationType;
    public string[] Animations;
    public float AnimationLength = 1f, AnimationDelay = 0f;
    public bool IsPlayAnimationOnStart = true;
    public bool IsPlayAnimationOnEnable = true;
    RawImage m_RawImage = null;
    RectTransform m_RectTransform = null;

    public Action<UGUISprite> OnAnimationEnd = null, OnAnimationStart = null;

    public RawImage Raw
    {
        get
        {
            if (m_RawImage == null)
                m_RawImage = gameObject.GetComponent<RawImage>();
            return m_RawImage;
        }
    }

    RectTransform Rect
    {
        get
        {
            if (m_RectTransform == null)
                m_RectTransform = gameObject.GetComponent<RectTransform>();
            return m_RectTransform;
        }
    }

    public UIAtlas atlas
    {
        get
        {
            return mAtlas;
        }
        set
        {
            if (mAtlas != value)
            {
                mAtlas = value;
                mSpriteSet = false;
                mSprite = null;

                // Automatically choose the first sprite
                if (string.IsNullOrEmpty(mSpriteName))
                {
                    if (mAtlas != null && mAtlas.spriteList.Count > 0)
                    {
                        SetAtlasSprite(mAtlas.spriteList[0]);
                        mSpriteName = mSprite.name;
                    }
                }

                // Re-link the sprite
                if (!string.IsNullOrEmpty(mSpriteName))
                {
                    string sprite = mSpriteName;
                    mSpriteName = "";
                    spriteName = sprite;
                    MarkAsChanged();
                }
            }
        }
    }

    /// <summary>
    /// Sprite within the atlas used to draw this widget.
    /// </summary>

    public string spriteName
    {
        get
        {
            return mSpriteName;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                // If the sprite name hasn't been set yet, no need to do anything
                if (string.IsNullOrEmpty(mSpriteName)) return;

                // Clear the sprite name and the sprite reference
                mSpriteName = "";
                mSprite = null;
                mSpriteSet = false;
            }
            else if (mSpriteName != value)
            {
                // If the sprite name changes, the sprite reference should also be updated
                mSpriteName = value;
                mSprite = null;
                mSpriteSet = false;
            }
            if (GetAtlasSprite() == null)
            {
                Raw.texture = null;
            }
        }
    }

    /// <summary>
    /// Is there a valid sprite to work with?
    /// </summary>

    public bool isValid { get { return GetAtlasSprite() != null; } }

    /// <summary>
    /// Sliced sprites generally have a border. X = left, Y = bottom, Z = right, W = top.
    /// </summary>

    public Vector4 border
    {
        get
        {
            UISpriteData sp = GetAtlasSprite();
            if (sp == null) return Vector4.zero;
            return new Vector4(sp.borderLeft, sp.borderBottom, sp.borderRight, sp.borderTop);
        }
    }

    /// <summary>
    /// Size of the pixel -- used for drawing.
    /// </summary>

    public float pixelSize { get { return mAtlas != null ? mAtlas.pixelSize : 1f; } }

    /// <summary>
    /// Whether the texture is using a premultiplied alpha material.
    /// </summary>

    /// <summary>
    /// Retrieve the atlas sprite referenced by the spriteName field.
    /// </summary>

    public UISpriteData GetAtlasSprite()
    {
        if (!mSpriteSet) mSprite = null;

        if (mSprite == null && mAtlas != null)
        {
            if (!string.IsNullOrEmpty(mSpriteName))
            {
                UISpriteData sp = mAtlas.GetSprite(mSpriteName);
                if (sp == null) return null;
                SetAtlasSprite(sp);
            }

            if (mSprite == null && mAtlas.spriteList.Count > 0)
            {
                UISpriteData sp = mAtlas.spriteList[0];
                if (sp == null) return null;
                SetAtlasSprite(sp);

                if (mSprite == null)
                {
                    Debug.LogError(mAtlas.name + " seems to have a null sprite!");
                    return null;
                }
                mSpriteName = mSprite.name;
            }
        }
        return mSprite;
    }

    /// <summary>
    /// Set the atlas sprite directly.
    /// </summary>

    protected void SetAtlasSprite(UISpriteData sp)
    {
        mSpriteSet = true;

        if (sp != null)
        {
            mSprite = sp;
            mSpriteName = mSprite.name;
        }
        else
        {
            mSpriteName = (mSprite != null) ? mSprite.name : "";
            mSprite = sp;
        }

        RawImage image = Raw;
        Texture tex = atlas.texture;
        image.texture = tex;
        if (mSprite == null) mSprite = atlas.GetSprite(spriteName);
        if (mSprite == null) return;

        Rect inner = new Rect(mSprite.x + mSprite.borderLeft, mSprite.y + mSprite.borderTop,
            mSprite.width - mSprite.borderLeft - mSprite.borderRight,
            mSprite.height - mSprite.borderBottom - mSprite.borderTop);

        inner = NGUIMath.ConvertToTexCoords(inner, tex.width, tex.height);
        image.uvRect = inner;
        if (UseNativeSize)
            image.SetNativeSize();
        if (UsePivot)
            SetPivot();
    }

    public void MarkAsChanged()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void SetPivot()
    {
        RectTransform rect_transform = Rect;
        Vector2 pivot = m_Pivot;
        float width = mSprite.width + mSprite.paddingLeft + mSprite.paddingRight;
        float height = mSprite.height + mSprite.paddingTop + mSprite.paddingBottom;
        pivot.x = (pivot.x * width - mSprite.paddingLeft) / mSprite.width;
        pivot.y = (pivot.y * height - mSprite.paddingBottom) / mSprite.height;
        rect_transform.pivot = pivot;
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

    bool CanAnimation   { get { return m_AnimationType != AnimationType.None && AnimationLength > 0f || Animations.Length > 0; } }
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
        if (Raw.enabled == false)
        {
            Raw.enabled = true;
            if (OnAnimationStart != null)
                OnAnimationStart(this);
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

        if (sprite_name != mSpriteName)
            spriteName = sprite_name;
    }

    public void PlayAnimation()
    {
        PlaybackTime = 0f;
        IsPlayingAnimation = true;
        Raw.enabled = false;
    }
}
