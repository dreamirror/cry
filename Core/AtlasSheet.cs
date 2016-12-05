using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(UIAtlas))]
public class AtlasSheet : MonoBehaviour {

    UIAtlas mAtlas;
    public UIAtlas Atlas
    {
        get
        {
            if (mAtlas == null)
                mAtlas = gameObject.GetComponent<UIAtlas>();
            return mAtlas;
        }
    }

    public float pixelSize { get { return Atlas.pixelSize; } }
    Dictionary<string, Sprite> m_Sprites = new Dictionary<string, Sprite>();

    public Vector2 m_Pivot = Vector2.one * 0.5f;
    public float m_PixelPerUnit = 100f;

    public Sprite GetSprite(string name)
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying == false)
        {
            return CreateSprite(name);
        }
#endif
        Sprite sprite;
        if (m_Sprites.TryGetValue(name, out sprite) == false)
        {
            sprite = CreateSprite(name);
            m_Sprites.Add(name, sprite);
        }
        return sprite;
    }

    Sprite CreateSprite(string name)
    {
        Sprite sprite = null;
        UISpriteData sp = Atlas.GetSprite(name);
        if (sp != null)
        {
            float width = sp.width + sp.paddingLeft + sp.paddingRight;
            float height = sp.height + sp.paddingTop + sp.paddingBottom;
            Vector2 pivot = m_Pivot;
            pivot.x = (pivot.x * width - sp.paddingLeft) / width;
            pivot.y = (pivot.y * height - sp.paddingBottom) / height;
            sprite = Sprite.Create(mAtlas.texture as Texture2D, new Rect(sp.x, Atlas.texture.height - sp.y - sp.height, sp.width, sp.height), pivot, m_PixelPerUnit);
            sprite.name = name;
            return sprite;
        }
        return null;
    }
}
