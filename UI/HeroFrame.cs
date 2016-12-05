using UnityEngine;
using System.Collections;

public class HeroFrame : MonoBehaviour {
    public UISprite m_Sprite;

    public void Init(string id)
    {
        InitInternal(id);
    }

    public void Init(long idx)
    {
        if(idx == 0)
        {
            m_Sprite.spriteName = "";
            return;
        }
        InitInternal(CreatureManager.Instance.GetInfoByIdx(idx).Info.ID);
    }

    void InitInternal(string id)
    {
        string sprite_name = string.Format("cs_{0}", id);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_Sprite.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_Sprite.spriteName = new_sprite_name;
    }
}
