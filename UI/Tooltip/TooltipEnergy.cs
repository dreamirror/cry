using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class TooltipEnergy : TooltipBase
{
    public UILabel m_LabelMessage;

    public UISprite m_BG;

    float margin = 15f;

    //RewardItem rewardItem;
    //UIButton target;
    SHTooltip target;

    int m_energy_regen_time = 0;
    void Start()
    {
        m_energy_regen_time = GameConfig.Get<int>("energy_regen_time");
    }
    void Update()
    {
        if (target != null && target.Pressed == false)
        {
            gameObject.SetActive(false);
            base.OnFinished();
        }

        int energy = Network.PlayerInfo.GetEnergy();
        DateTime next_regen_energy_time = DateTime.MinValue;
        DateTime all_regen_energy_time = DateTime.MinValue;
        if (energy < Network.PlayerInfo.energy_max)
        {
            next_regen_energy_time = Network.PlayerInfo.energy_time.AddSeconds(m_energy_regen_time * (energy + 1));
            all_regen_energy_time = Network.PlayerInfo.energy_time.AddSeconds(m_energy_regen_time * Network.PlayerInfo.energy_max);
        }

        string regen_time;
        if (m_energy_regen_time % 60 == 0)
            regen_time = Localization.Format("Minute", m_energy_regen_time / 60);
        else
            regen_time = Localization.Format("MinuteSeconds", m_energy_regen_time / 60, m_energy_regen_time % 60);

        string remain_time = "-";
        string all_remain_time = "-";
        if (next_regen_energy_time != DateTime.MinValue)
        {
            int second = (int)(next_regen_energy_time - Network.Instance.ServerTime).TotalSeconds;

            remain_time = Localization.Format("Seconds", second);

            if (second > 60)
                remain_time = Localization.Format("MinuteSeconds", second / 60, second % 60);

            remain_time = Localization.Format("RemainsTime", remain_time);
        }
        if (all_regen_energy_time != DateTime.MinValue)
        {
            int second = (int)(all_regen_energy_time - Network.Instance.ServerTime).TotalSeconds;

            all_remain_time = Localization.Format("Seconds", second);

            if (second >= 3600)
                all_remain_time = Localization.Format("HourMinute", second / 3600, second % 3600 / 60);
            else if (second >= 60)
                all_remain_time = Localization.Format("MinuteSeconds", second / 60, second % 60);
            all_remain_time = Localization.Format("RemainsTime", all_remain_time);
        }

        m_LabelMessage.text = Localization.Format("TooltipEnergy", regen_time, remain_time, all_remain_time);
    }

    public override void Init(params object[] parms)
    {
        if(parms == null || parms.Length < 1)
        {
            throw new System.Exception("invalid params : TooltipEnergy");
        }

        target = parms[0] as SHTooltip;

        if (target != null)
        {
            Vector3 pos = target.Collider.transform.position;
            pos.z = transform.position.z;
            transform.position = pos;
            pos = transform.localPosition;

            pos.x -= (m_LabelMessage.localSize.x + target.Collider.size.x) / 2 + margin * 2;
            pos.y -= (m_LabelMessage.localSize.y + target.Collider.size.y) / 2 + margin * 2;
            transform.localPosition = pos;
        }
        else
            throw new System.Exception("invalid params : TooltipEnergy");

        MoveScreenArea();
    }

    void MoveScreenArea()
    {
        float _margin = 2 * margin;
        Vector3 pos = UICamera.currentCamera.WorldToScreenPoint(transform.position);

        if (pos.x - _margin - m_LabelMessage.printedSize.x / 2 < 0) pos.x = m_LabelMessage.printedSize.x / 2 + _margin;
        else if (pos.x + _margin + m_LabelMessage.printedSize.x / 2 > Screen.width) pos.x = Screen.width - m_LabelMessage.printedSize.x / 2 - _margin;

        if (pos.y - _margin - m_LabelMessage.printedSize.y / 2 < 0) pos.y = m_LabelMessage.printedSize.y / 2 + _margin;
        else if (pos.y + _margin + m_LabelMessage.printedSize.y / 2 > Screen.height) pos.y = Screen.height - m_LabelMessage.printedSize.y / 2 - _margin;

        transform.position = UICamera.currentCamera.ScreenToWorldPoint(pos);


    }

    public override void Play()
    {
        gameObject.SetActive(true);
    }

}
