using UnityEngine;
using System.Collections;
using PacketInfo;
using PacketEnums;

public class GuildCreate : GuildContentsBase
{
    public GameObject m_Left;
    public UISprite m_SpriteEmblem;
    public UILabel m_LabelName;
    public UILabel m_LabelLimitLevel;
    public UILabel m_LabelCreateCost;

    public UILabel m_LabelIntro;
    public UILabel m_LabelNotification;

    public GameObject m_LevelLimitPanel;
    public UILabel[] m_LabelFilters;

    public UIToggle m_ToggleAuto;

    public UIInput m_LabelGuildName;
    public UIInput m_LabelGuildIntro;
    public UIInput m_LabelGuildNotification;

    short m_JoinLevelLimit = 10;
    string m_Emblem;
    //string m_GuildName;
    override public void Init(Guild _parent)
    {
        base.Init(_parent);

        SetLimitLevel(10);
        m_Emblem = "";
        //m_GuildName = "";

        int limit_level = m_JoinLevelLimit;
        foreach(var label in m_LabelFilters)
        {
            label.text = Localization.Format("GuildJoinLimitLevelFormat", limit_level);
            limit_level += 10;
        }

        m_ToggleAuto.value = true;

        m_LabelGuildName.label.text = "";
        m_LabelGuildIntro.label.text = "";
        m_LabelGuildNotification.label.text = "";
        m_LabelCreateCost.text = Localization.Format("GoodsFormat", GuildInfoManager.Config.CreateGuildCost);

        m_SpriteEmblem.gameObject.SetActive(false);
    }
    public void OnClickCreate()
    {
        if (m_SpriteEmblem.isActiveAndEnabled == false || string.IsNullOrEmpty(m_Emblem) == true)
        {
            Tooltip.Instance.ShowMessageKey("ConfirmSelectGuildEmblem");
            return;
        }
        if (string.IsNullOrEmpty(m_LabelGuildName.label.text) == true)
        {
            Tooltip.Instance.ShowMessageKey("CheckGuildName");
            return;
        }
        if (Network.Instance.CheckGoods(pe_GoodsType.token_gem, GuildInfoManager.Config.CreateGuildCost) == false)
            return;

        C2G.GuildCreate packet = new C2G.GuildCreate();
        packet.guild_emblem = m_Emblem;
        packet.guild_limit_level = m_JoinLevelLimit;
        packet.guild_name = m_LabelGuildName.label.text;
        packet.guild_intro = m_LabelGuildIntro.label.text;
        packet.guild_notification = m_LabelGuildNotification.label.text;
        packet.is_auto = m_ToggleAuto.value;
#if DEBUG
        //packet.account_idx = 1;//for test
#endif
        Network.GameServer.JsonAsync<C2G.GuildCreate, C2G.GuildAck>(packet, OnGuildCreate);
    }

    public void OnClickEmblem()
    {
        Popup.Instance.Show(ePopupMode.GuildEmblem, "Create", new System.Action<string>(OnGuildEmblem));
    }

    void OnGuildEmblem(string emblem)
    {
        m_Emblem = emblem;
        m_SpriteEmblem.gameObject.SetActive(true);
        m_SpriteEmblem.spriteName = emblem;
    }
    public void OnClickAuto()
    {
        m_ToggleAuto.value = true;
    }
    public void OnClickNoAuto()
    {
        m_ToggleAuto.value = false;
    }
    public void OnClickGuildName()
    {
        m_LabelGuildName.isSelected = true;
    }
    public void OnClickIntro()
    {
        m_LabelGuildIntro.isSelected = true;
    }
    public void OnClickNotification()
    {
        m_LabelGuildNotification.isSelected = true;
    }
    public void OnClickLevelLimit()
    {
        m_LevelLimitPanel.SetActive(true);
    }
    void SetLimitLevel(int level)
    {
        m_JoinLevelLimit = 10;
        m_LabelLimitLevel.text = Localization.Format("GuildJoinLimitLevelFormat", level);
        m_LevelLimitPanel.SetActive(false);
    }
    public void OnClickLevel10() { SetLimitLevel(10); }
    public void OnClickLevel20() { SetLimitLevel(20); }
    public void OnClickLevel30() { SetLimitLevel(30); }
    public void OnClickLevel40() { SetLimitLevel(40); }
    public void OnClickLevel50() { SetLimitLevel(50); }
    public void OnClickLevel60() { SetLimitLevel(60); }
    public void OnClickLevel70() { SetLimitLevel(70); }
    public void OnClickLevel80() { SetLimitLevel(80); }
    //////////////////////////////////////////////////////////////////////////

    void OnGuildCreate(C2G.GuildCreate packet, C2G.GuildAck ack)
    {
        switch(ack.result)
        {
            case pe_GuildResult.Success:
                Network.PlayerInfo.UseGoods(ack.use_goods);
                GuildManager.Instance.SetGuildInfo(ack.guild_info);
                Network.ChatServer.JoinGuildChannel();
                GameMain.Instance.ChangeMenu(GameMenu.Guild);
                break;
            case pe_GuildResult.SameGuildName:
                Tooltip.Instance.ShowMessageKey("AlreadyUseGuildName");
                break;
            case pe_GuildResult.GuildJoinTimeDelay:
                Tooltip.Instance.ShowMessageKey("GuildJoinTimeDelay");
                break;
            default:
                Tooltip.Instance.ShowMessageKey("UnknownErrorGuildCreate");
                break;
        }
    }
}
