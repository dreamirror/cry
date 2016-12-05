using UnityEngine;
using System.Collections;

public class TooltipMessage : TooltipBase
{
    public UILabel m_LabelMessage;

    public override string CompareValue { get { return m_LabelMessage.text; } }

    public override void Init(params object[] parms)
    {
        if(parms == null || parms.Length < 1)
        {
            throw new System.Exception("invalid params : TooltipMessage");
        }

        string messgae = parms[0] as string;
        m_LabelMessage.text = messgae;

        if (parms.Length > 1 && parms[1] != null)
        {
            Vector3 pos = (Vector3)parms[1];

            transform.localPosition = pos;
        }
    }
}
