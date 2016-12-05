using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(SpriteRenderer))]
[AddComponentMenu("UI/AtlasSprite")]
public class AtlasSprite : MonoBehaviour
{
    [HideInInspector][SerializeField]
    AtlasSheet mAtlas;
    [HideInInspector][SerializeField]
    string mSpriteName;

    [System.NonSerialized]
    bool mSpriteSet = false;
    public Vector2 m_Pivot = Vector2.one * 0.5f;

    SpriteRenderer m_Renderer = null;

    public SpriteRenderer Renderer
    {
        get
        {
            if (m_Renderer == null)
                m_Renderer = gameObject.GetComponent<SpriteRenderer>();
            return m_Renderer;
        }
    }

    public AtlasSheet atlas
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
                Renderer.sprite = null;

                // Automatically choose the first sprite
                if (string.IsNullOrEmpty(mSpriteName))
                {
                    if (mAtlas != null)
                    {
                        SetAtlasSprite(null);
                        mSpriteName = null;
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

    public bool isValid { get { return GetAtlasSprite() != null; } }

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
                Renderer.sprite = null;
                mSpriteSet = false;
            }
            else if (mSpriteName != value)
            {
                // If the sprite name changes, the sprite reference should also be updated
                mSpriteName = value;
                Renderer.sprite = null;
                mSpriteSet = false;
            }
            if (GetAtlasSprite() == null)
            {
                Renderer.sprite = null;
            }
        }
    }

    /// <summary>
    /// Retrieve the atlas sprite referenced by the spriteName field.
    /// </summary>

    public Sprite GetAtlasSprite()
    {
        if (!mSpriteSet) Renderer.sprite = null;

        if (Renderer.sprite == null && mAtlas != null)
        {
            if (!string.IsNullOrEmpty(mSpriteName))
            {
                Sprite sp = mAtlas.GetSprite(mSpriteName);
                if (sp == null) return null;
                SetAtlasSprite(sp);
            }
        }
        return Renderer.sprite;
    }

    /// <summary>
    /// Set the atlas sprite directly.
    /// </summary>

    protected void SetAtlasSprite(Sprite sp)
    {
        mSpriteSet = true;

        if (sp != null)
        {
            Renderer.sprite = sp;
            mSpriteName = sp.name;
        }
        else
        {
            Renderer.sprite = sp;
        }
    }

    public void MarkAsChanged()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
