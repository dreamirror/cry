using UnityEngine;
using System.Collections;
using SharedData;

public class TooltipMission : TooltipBase
{
    public UILabel m_LabelMessage;

    public override string CompareValue { get { return m_LabelMessage.text; } }

    Quest m_Quest;
    void Update()
    {
        if(GameMain.Instance.CurrentGameMenu != GameMenu.MainMenu)
        {
            OnFinished();
        }
    }
    public override void Init(params object[] parms)
    {
        if(parms == null || parms.Length < 1)
        {
            throw new System.Exception("invalid params : TooltipMission");
        }

        m_Quest = parms[0] as Quest;
        m_LabelMessage.text = Localization.Format("MissionTooltip", m_Quest.Info.Description, m_Quest.Data.quest_progress, m_Quest.Info.Condition.ProgressMax);

        if (parms.Length > 1 && parms[1] != null)
        {
            Vector3 pos = (Vector3)parms[1];

            transform.localPosition = pos;
        }
        transform.localPosition = new Vector3(0, 280);
    }

    public void OnClickMission()
    {
        Mission.MissionMove(m_Quest);

        //OnFinished();
    }
}
