using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TooltipRewardItem : TooltipBase
{
    public UILabel m_LabelMessage;

    public GameObject HeroFramePrefab;
    public UIGrid m_Grid;
    public UISprite m_BG;

    float margin = 10f;

    RewardItem rewardItem;
    UIButton target;

    void Update()
    {
        if (target.state != UIButtonColor.State.Pressed)
        {
            gameObject.SetActive(false);
            base.OnFinished();
        }
    }

    public override void Init(params object[] parms)
    {
        if (parms == null || parms.Length != 1)
        {
            throw new System.Exception("invalid params : TooltipRewardItem");
        }

        rewardItem = parms[0] as RewardItem;
        target = rewardItem.GetComponent<UIButton>();
        m_LabelMessage.text = rewardItem.GetTooltip();

        Vector3 pos = rewardItem.transform.position;
        pos.z = transform.position.z;

        transform.position = pos;
        pos = transform.localPosition;

        switch (rewardItem.Info.ItemType)
        {
            case eItemType.Item:
                {
                    Item item = ItemManager.Instance.GetItemByIdn(rewardItem.Info.IDN);
                    if (item != null)
                    {
                        var creatureidxs = item.GetUseFor();
                        Transform bottom_target = null;
                        Transform left_target = null;
                        Transform right_target = null;
                        float width = 0;
                        //float height = 0;
                        for (int i = 0; i < creatureidxs.Count; ++i)
                        {
                            var hero = NGUITools.AddChild(m_Grid.gameObject, HeroFramePrefab).GetComponent<HeroFrame>();
                            hero.Init(creatureidxs[i]);
                            bottom_target = hero.transform;
                            if (i == 0)
                            {
                                left_target = hero.transform;
                                width = hero.m_Sprite.width;
                                //height = hero.m_Sprite.height;
                            }
                            else if (i == 3 || i == 4)
                                right_target = hero.transform;

                        }
                        m_Grid.Reposition();

                        if (creatureidxs.Count > 0)
                        {
                            float additional_bottom = m_Grid.cellHeight * (creatureidxs.Count > 5 ? 2 : 1);
                            pos.y += additional_bottom;
                            pos.x -= m_Grid.cellWidth * (creatureidxs.Count >= 5 ? 5 : creatureidxs.Count) / 2;
                        }
                        else
                            pos.x -= m_LabelMessage.localSize.x / 2;


                        if (bottom_target != null)
                            m_BG.bottomAnchor.Set(bottom_target, 0, m_BG.bottomAnchor.absolute);
                        if (right_target != null)
                            m_BG.rightAnchor.Set(right_target, 0, m_BG.rightAnchor.absolute + width);
                        if (left_target != null)
                        {
                            m_LabelMessage.leftAnchor.Set(left_target, 0, 0);
                            m_BG.leftAnchor.Set(left_target, 0, m_BG.leftAnchor.absolute);
                        }
                    }
                    else
                        pos.x -= m_LabelMessage.localSize.x / 2;
                }
                break;
        }

        pos.y += (m_LabelMessage.localSize.y + rewardItem.m_icon.height) / 2 + margin;
        transform.localPosition = pos;
    }

    public override void Play()
    {
        gameObject.SetActive(true);
    }

}
