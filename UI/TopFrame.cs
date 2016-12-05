using System;
using UnityEngine;

public class TopFrame : MonoBehaviour
{

    public GameObject m_BtnBack;
    public UIToggle m_Toggle;
    public UILabel m_LabelGold, m_LabelCash, m_LabelEnergy, m_LabelBackMenu;
    public UIButton[] m_BtnShortCut;
    public GameObject[] m_Notify;

    public GameObject m_Center;
    public GameObject m_Right;

    public UIPlayTween m_Tween;
    ClientPlayerData PlayerInfo = null;

    short CurrentEnergy;
    DateTime EnergyRegenTime = DateTime.MaxValue;
    int m_energy_regen_time;

    DateTime CheckQuestTime = DateTime.MaxValue;

    void Update()
    {
        if(EnergyRegenTime < Network.Instance.ServerTime)
        {
            UpdatePlayerInfo();
        }

        if(CheckQuestTime < Network.Instance.ServerTime)
        {
            CheckQuestTime = Network.Instance.ServerTime.AddMinutes(1f);
            QuestManager.Instance.CheckComplete();
            UpdateNotify();
        }
    }
    public void UpdatePlayerInfo()
    {
        if (PlayerInfo != null)
        {
            CurrentEnergy = Network.PlayerInfo.GetEnergy();
            if(CurrentEnergy < Network.PlayerInfo.energy_max)
            {
                EnergyRegenTime = Network.PlayerInfo.energy_time.AddSeconds(m_energy_regen_time * (CurrentEnergy + 1));
            }
            else
                EnergyRegenTime = DateTime.MaxValue;
            m_LabelGold.text = string.Format("{0:n0}", PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gold));
            m_LabelCash.text = string.Format("{0:n0}", PlayerInfo.GetGoodsValue(PacketInfo.pe_GoodsType.token_gem));
            m_LabelEnergy.text = string.Format("{0}/{1}", CurrentEnergy, PlayerInfo.energy_max);
        }
    }
    public void Init()
    {
        CheckQuestTime = Network.Instance.ServerTime.AddMinutes(1f);
        if (GameMain.Instance == null)
            return;
        PlayerInfo = Network.PlayerInfo;
        m_energy_regen_time = GameConfig.Get<int>("energy_regen_time");

        GameMenu game_menu = GameMain.Instance.CurrentGameMenu;
        switch (game_menu)
        {
            case GameMenu.MainMenu:
                m_BtnBack.SetActive(false);
                m_Toggle.value = true;
                m_Center.SetActive(true);
                m_Right.SetActive(true);
                break;

            case GameMenu.HeroesEval:
                m_Center.SetActive(false);
                m_Right.SetActive(false);
                break;

            default:
                m_BtnBack.SetActive(true);
                m_LabelBackMenu.text = Localization.Get("Menu_" + game_menu);
                m_Toggle.value = false;
                m_Center.SetActive(true);
                m_Right.SetActive(true);
                break;
        }
        UpdatePlayerInfo();
        UpdateNotify();
    }

    public void OnClickBack()
    {
        TutorialCardSet();

        GameMain.Instance.BackMenu();
    }

    private static void TutorialCardSet()
    {
        if (Tutorial.Instance.Completed == false && Tutorial.Instance.CurrentState == 910)
        {
            int creature_idx = -1;
            foreach (var reward_base in Tutorial.Instance.CurrentInfo.rewards)
            {
                if (reward_base.CreatureInfo == null) continue;
                CreatureInfo info = reward_base.CreatureInfo;
                Creature enchant_creature = new Creature(creature_idx--, info.IDN, 0, (short)reward_base.Value, (short)reward_base.Value3, (short)reward_base.Value2);
                CreatureManager.Instance.AddTutorialCard(enchant_creature);
            }
        }
    }

    public void OnClickHeroesInfo()
    {
        TutorialCardSet();

        GameMain.MoveShortCut(GameMenu.HeroesInfo);
    }

    public void OnClickInventory()
    {
        GameMain.MoveShortCut(GameMenu.Inventory);
    }

    public void OnClickMission()
    {
        GameMain.MoveShortCut(GameMenu.Mission);
        //Popup.Instance.Show(ePopupMode.Mission);
    }
    
    public void OnMailInfoRequestHandler(C2G.MailGet send, C2G.MailGetAck recv)
    {   
        MailManager.Instance.Init(recv.info);
        Popup.Instance.Show(ePopupMode.MailBox);
                
        Network.Instance.SetUnreadMail(MailManager.Instance.GetUnreadState());
        UpdateNotify();
    }

    public void OnClickMail()
    {
        C2G.MailGet _MailInfoRequest = new C2G.MailGet();
        Network.GameServer.JsonAsync<C2G.MailGet, C2G.MailGetAck>(_MailInfoRequest, OnMailInfoRequestHandler);

        //Tooltip.Instance.ShowMessageKey("NotImplement");

        //if (Network.PlayerInfo.can_cheat == true || Application.isEditor == true)
        //    Popup.Instance.Show(ePopupMode.Cheat);

    }

    public void OnClickSetup()
    {
        Popup.Instance.Show(ePopupMode.Setting);
    }

    public void OnClickChat()
    {
        ChattingMain.Instance.ShowChattingPopup();
    }

    public void UpdateNotify()
    {
        m_Notify[0].SetActive(TeamDataManager.Instance.IsNotify || ItemManager.Instance.IsPieceNotify || CreatureBookManager.Instance.IsNotify);
        m_Notify[1].SetActive(false);//inventory
        m_Notify[2].SetActive(QuestManager.Instance.IsNotify);//mission
        m_Notify[3].SetActive(false);//setup

        m_Notify[4].SetActive(Network.Instance.UnreadMailState != PacketEnums.pe_UnreadMailState.None);//mail
        m_Notify[5].SetActive(ChattingMain.Instance.is_notify_icon);//chat

        bool bNoty = false;
        int i;
        for (i = 0; i < 4; ++i)
        {
            if (m_Notify[i].activeSelf == true)
                bNoty = true;
        }
        m_Notify[6].SetActive(bNoty);

        Network.Instance.UpdateMail = false;
    }

    public void OnClickEnergy()
    {
        GameMain.MoveStore("Energy");
    }
    public void OnClickGem()
    {
        GameMain.MoveStore("Gem");

    }
    public void OnClickGold()
    {
        GameMain.MoveStore("Gold");

    }

    public void OnClickToggle()
    {
        m_Toggle.value = !m_Toggle.value;
    }
    public void OnToggleValueChanged()
    {
        //Debug.LogFormat("OnToggleValueChanged : {0}", m_Toggle.value);
        //m_Tween.Play(!m_Toggle.value);
    }

    public void OnShowTooltipEnergy(SHTooltip tooltip)
    {
        Tooltip.Instance.ShowEnergy(tooltip);
    }
}
