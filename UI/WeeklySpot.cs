using UnityEngine;
using System.Collections;
using System;

public class WeeklySpot : SubMenuSpot
{
    public UILabel m_Title;
    public UILabel m_Desc;
    public UIToggle m_Toggle;
    public GameObject m_Nofity;
    public GameObject m_Event;
    public string m_MapID;

    override public void Init()
    {
        var map_infos = MapInfoManager.Instance.GetWeeklyDungeons();

        m_Nofity.SetActive(false);
        foreach (var map_info in map_infos)
        {
            var condition = map_info.CheckCondition();
            if (condition == null && MapClearDataManager.Instance.GetMapDailyClearCount(map_info.IDN) < map_info.TryLimit)
            {
                m_Nofity.SetActive(true);
            }
        }
        m_Event.SetActive(false);
    }

    public void OnBtnClicked()
    {
        Popup.Instance.Show(ePopupMode.WeeklyDungeon);
    }
}
