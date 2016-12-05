using UnityEngine;
using System.Collections;
using SharedData;

public class TooltipMissionProgress : TooltipBase
{
    public UILabel m_LabelMessage;

    public override string CompareValue { get { return m_LabelMessage.text; } }

    public override void Init(params object[] parms)
    {
        if (parms == null || parms.Length < 1)
        {
            throw new System.Exception("invalid params : TooltipMissionProgress");
        }

        m_LabelMessage.text = parms[0] as string;
        transform.localPosition = new Vector3(0, 280);

        if (parms.Length > 1 && parms[1] != null)
        {
            Vector3 pos = (Vector3)parms[1];

            transform.localPosition = pos;
        }
    }
}
