using UnityEngine;
using System.Collections;

public class LootHeroItem : MonoBehaviour
{
    public UICharacterContainer m_Character;

    public UIGrid m_Grid;
    public UISprite[] m_Stars;

    public UIToggle m_Special;
    public UIToggle m_SpecialLeaderSkill;

    public Creature m_Creature;

    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame

    void Update () {
    }

    void OnDestroy()
    {
    }

    public void Init(PacketInfo.pd_CreatureLootData loot)
    {
        m_Character.Uninit();
        
        m_Creature = CreatureManager.Instance.GetInfoByIdx(loot.creature.creature_idx);
        m_Character.Init(AssetManager.GetCharacterAsset(m_Creature.Info.ID, m_Creature.SkinName), UICharacterContainer.Mode.UI_Normal, "win");
        m_Character.SetPlay(UICharacterContainer.ePlayType.Social);
        m_Character.transform.parent.gameObject.SetActive(true);

        m_SpecialLeaderSkill.value = m_Creature.TeamSkill != null;
        m_Special.value = m_SpecialLeaderSkill.value == false && m_Creature.Grade >= 4;

        for (int i = 0; i < m_Stars.Length; ++i)
        {
            m_Stars[i].gameObject.SetActive(i < m_Creature.Grade);
        }

        m_Grid.Reposition();
    }

    public void OnBtnClick()
    {
        Popup.Instance.Show(ePopupMode.LootCharacter, m_Creature.Idx, false, false);
    }
}
