using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class PopupGuildEmblem : PopupBase
{
    public UIScrollView m_ScrollView;
    public UIGrid m_Grid;

    public PrefabManager GuildEmblemItemPrefabManager;

    public UIToggle m_ToggleCreate;

    //string m_SelectedEmblem;
    System.Action<string> OnGuildEmblemDelegate = null;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        if(parms == null && parms.Length != 2)
        {
            throw new System.Exception("Invalid Param(PopupGuildEmblem)");
        }
        m_ToggleCreate.value = (string)parms[0] == "Create";
        OnGuildEmblemDelegate = parms[1] as Action<string>;

        for (int i=1; i<=GuildInfoManager.Config.GuildEmblemCount; ++i)
        {
            var item = GuildEmblemItemPrefabManager.GetNewObject<GuildEmblemItem>(m_Grid.transform, Vector3.zero);
            item.Init(string.Format("{0}{1:D2}", GuildInfoManager.Config.GuildEmblemPrefix, i), OnGuildEmblem);
        }

        //m_SelectedEmblem = "";
    }
    void OnGuildEmblem(string emblem)
    {
        //m_SelectedEmblem = emblem;
        if (OnGuildEmblemDelegate != null)
            OnGuildEmblemDelegate(emblem);
    }
    public override void OnFinishedShow()
    {        
        base.OnFinishedShow();

        m_Grid.Reposition();
        m_ScrollView.ResetPosition();
    }

    public void OnClickConfirm()
    {
        parent.Close();
    }
}
