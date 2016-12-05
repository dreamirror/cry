using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipTargetMessage : TooltipBase
{
    public UILabel m_LabelMessage;

    public UISprite m_BG;

    float margin = 10f;

    //RewardItem rewardItem;
    //UIButton target;
    SHTooltip target;

    void Update()
    {
        if (target != null && target.Pressed == false || target.gameObject.activeInHierarchy == false)
        {
            gameObject.SetActive(false);
            base.OnFinished();
        }
    }

    public override void Init(params object[] parms)
    {
        if (parms == null || parms.Length < 2)
        {
            throw new System.Exception("invalid params : TooltipTargetMessage");
        }

        m_LabelMessage.text = parms[0] as string;
        target = parms[1] as SHTooltip;

        if (target != null)
        {
            Vector3 pos = target.Collider.transform.position;
            //pos.z = transform.position.z;
            transform.position = pos;
            pos = transform.localPosition;

            pos.y += (m_LabelMessage.localSize.y + target.Collider.size.y) / 2 + margin;
            pos.z = -500;
            transform.localPosition = pos;
        }
        else
            throw new System.Exception("invalid params : TooltipTargetMessage");

        MoveScreenArea();
    }

    void MoveScreenArea()
    {
        float _margin = 2 * margin;
        Vector3 pos = UICamera.currentCamera.WorldToScreenPoint(transform.position);
        Vector2 size = m_LabelMessage.localSize;
        size.x *= Screen.width / 1280f;
        size.y *= Screen.height / 720f;

        if (pos.x - _margin - size.x / 2 < 0) pos.x = size.x / 2 + _margin;
        else if (pos.x + _margin + size.x / 2 > Screen.width) pos.x = Screen.width - size.x / 2 - _margin;

        if (pos.y - _margin - size.y / 2 < 0) pos.y = size.y / 2 + _margin;
        else if (pos.y + _margin + size.y / 2 > Screen.height) pos.y = Screen.height - size.y /2 - _margin;

        transform.position = UICamera.currentCamera.ScreenToWorldPoint(pos);
    }

    public override void Play()
    {
        gameObject.SetActive(true);
    }
}
