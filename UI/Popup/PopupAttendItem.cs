using UnityEngine;
using System.Collections;

public class PopupAttendItem : MonoBehaviour {

    public GameObject Indicate;
    public UILabel DayLabel;
    public UILabel ItemLabel;

    public GameObject Rewarded;
    public GameObject Block_Panel;
    public GameObject Disable_Panel;
    public GameObject Enable_Panel;

    public TweenScale StampRewardTween;

    public PrefabManager RewardPrefabManager;

    RewardItem m_Reward;
    public bool IsRewarded { get; private set; }
    public bool IsEnabled { get; private set; }

    public void Init(short day_index, bool is_enable, bool is_rewarded, RewardBase reward)
    {
        IsEnabled = is_enable;
        IsRewarded = is_rewarded;

        m_Reward = RewardPrefabManager.GetNewObject<RewardItem>(Indicate.transform, Vector3.zero);
        m_Reward.InitReward(reward);

        DayLabel.text = Localization.Format("AttendRewardDay", day_index + 1);
        if (reward.ItemInfo != null)
            ItemLabel.text = Localization.Format("AttendRewardItemCount", reward.GetName(), reward.Value);
        else
            ItemLabel.text = "";

        Enable_Panel.SetActive(IsEnabled);
        Disable_Panel.SetActive(!IsEnabled);
        
        Rewarded.SetActive(is_rewarded);
        Block_Panel.SetActive(is_rewarded);

        m_Reward.m_Notifies[0].SetActive(IsEnabled && is_rewarded == false);
    }

    public void SetReward()
    {
        m_Reward.m_Notifies[0].SetActive(false);
        Enable_Panel.SetActive(true);
        Disable_Panel.SetActive(false);
        Rewarded.SetActive(true);
        Block_Panel.SetActive(true);
        StampRewardTween.gameObject.SetActive(true);
        StampRewardTween.PlayForward();
    }

    public void FinishTween()
    {   
    }

}
