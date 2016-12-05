using UnityEngine;
using System.Collections;

public class PieceHeroItem : MonoBehaviour
{
    public GameObject EnchantHeroPrefab;
    public UIProgressBar m_Progress;
    public UILabel m_PieceCount, m_HeroName;
    public UISprite m_Notify;

    public Item m_SoulStone { get; private set; }
    SoulStoneInfo m_Info = null;

    void OnDestroy()
    {
        if (m_Hero != null)
        {
            Destroy(m_Hero.gameObject);
            m_Hero = null;
        }
    }


    // Use this for initialization
    EnchantHero m_Hero;
    public void Init(Item soul_stone)
    {
        if (soul_stone == null)
        {
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = false);
            gameObject.name = "dummy";
            return;
        }
        else
        {
            System.Array.ForEach(gameObject.GetComponentsInChildren(typeof(UIWidget), true), o => o.GetComponent<UIWidget>().enabled = true);
        }

        m_SoulStone = soul_stone;
        m_Info = m_SoulStone.SoulStoneInfo;

        gameObject.name = m_Info.ID;

        if (m_Hero == null)
        {
            m_Hero = NGUITools.AddChild(gameObject, EnchantHeroPrefab).GetComponent<EnchantHero>();
            m_Hero.GetComponent<BoxCollider2D>().enabled = false;
        }

        m_Hero.InitSoulStone(m_Info);
        m_HeroName.text = m_Info.Creature.Name;

        m_Progress.value = (float)m_SoulStone.Count/ m_Info.LootCount;
        m_PieceCount.text = string.Format("{0}/{1}", m_SoulStone.Count, m_Info.LootCount);
        m_Notify.gameObject.SetActive(false);
    }

    public void CheckNotify()
    {
        bool notify = m_SoulStone.Count >= m_Info.LootCount;
        m_Notify.gameObject.SetActive(notify);
    }

    public void OnBtnClick()
    {
        if (m_SoulStone == null)
            return;

        Popup.Instance.Show(ePopupMode.PieceCharacter, m_Info.Creature.ID, m_Info.Grade, m_SoulStone);
    }
}
