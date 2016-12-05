using UnityEngine;
using PacketInfo;
using PacketEnums;
using System;

public class PopupGuildSearch : PopupBase {

    public UILabel m_LabelMessage;
    public UILabel m_InputMessage;
    public UIInput m_Input;

    Action<pd_GuildInfo> OnSearchComplete = null;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        OnSearchComplete = parms[0] as Action<pd_GuildInfo>;

        m_Input.value = "";
        
    }

    public void OnClickSearch()
    {
        if(string.IsNullOrEmpty(m_InputMessage.text))
        {
            Tooltip.Instance.ShowMessageKey("CheckGuildName");
            return;
        }

        C2G.GuildSearch packet = new C2G.GuildSearch();
        packet.guild_name = m_InputMessage.text;
        Network.GameServer.JsonAsync<C2G.GuildSearch, C2G.GuildAck>(packet, OnGuildSearchHandler);
    }

    void OnGuildSearchHandler(C2G.GuildSearch packet, C2G.GuildAck ack)
    {
        switch (ack.result)
        {
            case pe_GuildResult.Success:
                if(ack.guild_info == null)
                {
                    Tooltip.Instance.ShowMessageKey("NotExistGuildForSearch");
                    return;
                }
                base.OnClose();
                if (OnSearchComplete != null)
                    OnSearchComplete(ack.guild_info);
                break;
            case pe_GuildResult.NotExistGuild:
                Tooltip.Instance.ShowMessageKey("NotExistGuildForSearch");
                break;
        }
    }
}
