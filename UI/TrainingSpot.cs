using UnityEngine;
using System.Collections;
using System;
using PacketInfo;

abstract public class SubMenuSpot : MonoBehaviour
{
    abstract public void Init();
}

public class TrainingSpot : SubMenuSpot
{
    public UILabel m_Title;
    public UILabel m_Desc;
    public UIToggle m_Toggle;
    public GameObject m_Nofity;
    public GameObject m_Event;
    MapCondition m_Condition;
    public string m_MapID;

    MapInfo m_Info;
    override public void Init()
    {
        m_Info = MapInfoManager.Instance.GetInfoByID(m_MapID);

        string leftTryCount = Localization.Format("TryCount", (m_Info.TryLimit - MapClearDataManager.Instance.GetMapDailyClearCount(m_Info.IDN, PacketEnums.pe_Difficulty.Normal)), m_Info.TryLimit);

        m_Title.text = string.Format("{0}({1})", m_Info.Name, leftTryCount);
        m_Desc.text = m_Info.Description;
        m_Condition = m_Info.CheckCondition();
        m_Toggle.value = m_Condition != null;

        int clear_rate = MapClearDataManager.Instance.GetTotalClearRate(m_Info.IDN, PacketEnums.pe_Difficulty.Normal);
        m_Nofity.SetActive(m_Toggle.value == false && clear_rate == 0);

        if (m_Toggle.value == false)
        {
            pd_EventHottime event_info = null;
            switch (m_Info.ID)
            {
                case "1001_event_gold":
                    event_info = EventHottimeManager.Instance.GetInfoByID("training_gold_reward_2x");
                    break;
                case "1002_event_exp":
                    event_info = EventHottimeManager.Instance.GetInfoByID("training_exp_reward_2x");
                    break;
                //case "1003_event_equip_enchant":
                //    event_info = EventHottimeManager.Instance.GetInfoByID("training_stuff_reward_2x");
                //    break;
                //case "1004_event_equip_evolve":
                //    event_info = EventHottimeManager.Instance.GetInfoByID("training_recipe_reward_2x");
                //    break;
                case "1005_event_rune":
                    event_info = EventHottimeManager.Instance.GetInfoByID("training_rune_reward_2x");
                    break;
                case "1006_event_creature":
                    event_info = EventHottimeManager.Instance.GetInfoByID("training_creature_reward_2x");
                    break;
                case "1007_event_tower":
                    break;

            }
                m_Event.SetActive(event_info != null);
        }
        else 
            m_Event.SetActive(false);

    }

    public void OnBtnClicked()
    {
        if (m_Toggle.value == true)
            Tooltip.Instance.ShowMessage(m_Condition.Condition);
        else
            Popup.Instance.Show(ePopupMode.Training, m_Info);
    }
}