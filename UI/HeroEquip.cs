using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeroEquip : MonoBehaviour
{
    Creature m_Creature;
    Equip m_CreatureEquip;

    public GameObject EquipItemPrefab;

    public GameObject m_Equip;
    public GameObject[] m_Stuffs;

    public UIGrid m_GridStuff;
    //public UIToggle m_ToggleUpgrade;
    //public UILabel m_LabelUpgradeCondition;

    public UILabel m_Label, m_Title;

    public UIParticleContainer m_ParticleReady, m_ParticleEnchant, m_ParticleUpgrade;

    EquipItem EquipItem;
    public List<RewardItem> Stuffs { get; private set; }

    System.Action OnEquipEnchantCallback = null;

    public void Init(Creature creature, Equip equip, GameObject StuffItemPrefab, System.Action equipEnchantCallback)
    {
        gameObject.SetActive(true);
        OnEquipEnchantCallback = equipEnchantCallback;
        m_Creature = creature;
        m_CreatureEquip = equip;

        if (Stuffs == null)
        {
            Stuffs = new List<RewardItem>();
            for (int i = 0; i < m_Stuffs.Length; ++i)
            {
                RewardItem stuff;
                stuff = NGUITools.AddChild(m_GridStuff.gameObject, StuffItemPrefab).GetComponent<RewardItem>();
                //Vector3 scale = new Vector3(0.82f, 0.82f, 1f);
                //stuff.transform.localScale = scale;
                Stuffs.Add(stuff);
                stuff.OnClickItem = OnClickItem;
            }

        }
        Reinit();
    }

    public void Reinit()
    {
        m_Title.text = m_CreatureEquip.Info.Name;
        if (EquipItem == null) EquipItem = NGUITools.AddChild(m_Equip, EquipItemPrefab).GetComponent<EquipItem>();
        EquipItem.Init(m_CreatureEquip);
        for (int i = 0; i < m_Stuffs.Length; ++i)
        {
            if (m_CreatureEquip.Stuffs.Count > i)
            {
                Stuffs[i].Init(m_CreatureEquip.Stuffs[i]);
            }
            else
                Stuffs[i].gameObject.SetActive(false);
        }
        m_GridStuff.Reposition();
        //m_ToggleUpgrade.value = m_CreatureEquip.EnchantLevel == 5;
        //m_LabelUpgradeCondition.text = Localization.Format("ConditionEquipUpgrade", m_CreatureEquip.Info.NextEquipLevel);

        if (m_CreatureEquip.EnchantLevel == 5)
            m_Label.text = Localization.Get("Upgrade");
        else
            m_Label.text = Localization.Get("Enchant");

        if (m_ParticleReady != null)
            m_ParticleReady.gameObject.SetActive(m_CreatureEquip.IsNotify);
    }

    void OnClickItem(ItemInfoBase info)
    {
        Popup.Instance.Show(ePopupMode.Stuff, info);
    }

    public void OnClickEnchant()
    {//Enchant or Upgrade Weapon
        if (m_Label.text.Equals(Localization.Get("Upgrade")))
            EquipUpgrade(m_CreatureEquip);
        else
            EquipEnchant(m_CreatureEquip);
    }

    void EquipEnchant(Equip equip)
    {
        Popup.Instance.Show(ePopupMode.Enchant, equip, new OnEquipEnchantDelegate(OnEquipEnchant));
    }
    void OnEquipEnchant(Equip equip)
    {
        C2G.EquipEnchant packet = new C2G.EquipEnchant();
        packet.equip_idx = equip.EquipIdx;
        packet.creature_idx = equip.CreatureIdx;
        packet.equip_id = equip.Info.ID;
        packet.enchant_level = equip.EnchantLevel;

        if (Tutorial.Instance.Completed == false)
        {
            C2G.TutorialState tutorial_packet = new C2G.TutorialState();
            tutorial_packet.tutorial_state = (short)Tutorial.Instance.CurrentState;
            tutorial_packet.next_tutorial_state = (short)Tutorial.Instance.NextState;
            tutorial_packet.equip_enchant = packet;
            Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialEquipEnchant);
        }
        else
            Network.GameServer.JsonAsync<C2G.EquipEnchant, C2G.EquipEnchantAck>(packet, OnEquipEnchant);
    }
    void OnTutorialEquipEnchant(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        OnEquipEnchant(packet.equip_enchant, ack.equip_enchant);
        Tutorial.Instance.AfterNetworking();
    }
    void OnEquipEnchant(C2G.EquipEnchant packet, C2G.EquipEnchantAck ack)
    {
        ItemManager.Instance.Reset(ack.item);
        CreatureManager.Instance.UpdateEquip(m_Creature, ack.equip);
        Network.PlayerInfo.UseGoods(ack.use_gold);
        Tooltip.Instance.ShowMessageKey("EquipEnchantSuccess");


        m_ParticleEnchant.Play();

        GameMain.Instance.UpdateMenu();
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.UpdateNotify(false);

        //Reinit();

        OnEquipEnchantCallback();
    }

    void EquipUpgrade(Equip equip)
    {
        if (m_CreatureEquip.Info.NextEquipLevel == short.MaxValue)
        {
            Tooltip.Instance.ShowMessageKey("EquipUpgradeLimit");
            return;
        }

        Popup.Instance.Show(ePopupMode.EquipUpgrade, equip, new OnEquipUpgradeDelegate(OnEquipUpgradeCallback));
    }

    void OnEquipUpgradeCallback(Equip equip)
    {
        C2G.EquipUpgrade packet = new C2G.EquipUpgrade();
        packet.creature_idx = m_Creature.Idx;
        packet.equip_idx = equip.EquipIdx;
        packet.equip_grade = equip.Info.Grade;
        packet.equip_id = equip.Info.ID;

        Network.GameServer.JsonAsync<C2G.EquipUpgrade, C2G.EquipUpgradeAck>(packet, OnEquipUpgrade);
    }

    void OnEquipUpgrade(C2G.EquipUpgrade packet, C2G.EquipUpgradeAck ack)
    {
        ItemManager.Instance.Reset(ack.item);
        CreatureManager.Instance.UpdateEquip(m_Creature, ack.equip);
        Network.PlayerInfo.UseGoods(ack.use_gold);

        Tooltip.Instance.ShowMessageKey("EquipUpgradeSuccess");

        m_ParticleUpgrade.Play();

        GameMain.Instance.UpdateNotify(false);
        GameMain.Instance.UpdateMenu();
        GameMain.Instance.UpdatePlayerInfo();
        Reinit();
        OnEquipEnchantCallback();
    }
}
