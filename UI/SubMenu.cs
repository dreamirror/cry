using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SubMenu : MenuBase
{
    public SubMenuSpot[] m_Spots;

    override public bool Init(MenuParams parms)
    {
        foreach (var spot in m_Spots)
        {
            spot.Init();
        }

        return true;
    }
}
