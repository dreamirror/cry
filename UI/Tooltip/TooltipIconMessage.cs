using UnityEngine;
using System.Collections;

public class TooltipIconMessage : TooltipBase
{
    public UILabel m_LabelMessage;
    public UISprite m_Icon;

    public override string CompareValue { get { return m_LabelMessage.text; } }

    public override void Init(params object[] parms)
    {
        if (parms == null || parms.Length != 2)
        {
            throw new System.Exception("invalid params : TooltipMessage");
        }

        string sprite_name = parms[0] as string;
        string messgae = parms[1] as string;

        m_Icon.spriteName = sprite_name;
        m_LabelMessage.text = messgae;
    }
}
