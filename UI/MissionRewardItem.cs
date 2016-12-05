using UnityEngine;
using System.Collections;
using SharedData;

public class MissionRewardItem : MonoBehaviour
{
    public UISprite m_SpriteImage;
    public UILabel m_Text;

    public void Init(RewardBase reward)
    {
        m_SpriteImage.spriteName = reward.ItemInfo.IconID;
        m_Text.text = string.Format("x{0}", reward.Value);
    }
}
