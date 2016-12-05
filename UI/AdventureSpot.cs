using UnityEngine;
using System.Collections.Generic;
using PacketInfo;

public class AdventureSpot : SubMenuSpot
{
    public UILabel m_Title;
    public UILabel m_Desc;
    public UIToggle m_Toggle;
    public GameObject m_Nofity;
    public GameObject m_Event;
    public string m_MapID;

    override public void Init()
    {
        m_Nofity.SetActive(false);
        m_Title.text = AdventureInfoManager.Instance.Title;
        m_Desc.text = AdventureInfoManager.Instance.Desc;

        C2G.AdventureInfoDetail packet = new C2G.AdventureInfoDetail();
        Network.GameServer.JsonAsync<C2G.AdventureInfoDetail, C2G.AdventureInfoDetailAck>(packet, OnAdventureInfoDetail);
        m_Event.SetActive(false);
    }

    public void OnBtnClicked()
    {
        Popup.Instance.Show(ePopupMode.Adventure);
    }


    void OnAdventureInfoDetail(C2G.AdventureInfoDetail packet, C2G.AdventureInfoDetailAck ack)
    {
        AdventureInfoManager.Instance.SetInfoDetails(ack.adventure_infos);
        foreach(var info in ack.adventure_infos)
        {
            if(info.is_rewarded == false && info.end_at < Network.Instance.ServerTime)
            {
                m_Nofity.SetActive(true);
                return;
            }
        }
    }
}
