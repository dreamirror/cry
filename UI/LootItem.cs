using UnityEngine;
using System.Collections;

public class LootItemInfo
{
    public LootItemInfo(int idn, int value, bool notify = true)
    {
        this.idn = idn;
        this.value = value;
        this.notify = notify;
    }
    public int idn;
    public int value;
    public bool notify;
}

public class LootItem : MonoBehaviour
{
    public GameObject RewardItemPrefab;
    public UILabel m_ItemName;
    public UIPlayTween m_Tween;

    RewardItem m_RewardItem = null;
    public void Init(LootItemInfo info)
    {
        if (m_RewardItem == null)
            m_RewardItem = NGUITools.AddChild(gameObject, RewardItemPrefab).GetComponent<RewardItem>();

        var item_info = ItemInfoManager.Instance.GetInfoByIdn(info.idn);
        m_RewardItem.InitReward(item_info.IDN, info.value);
        m_RewardItem.m_Notifies[0].SetActive(info.notify);

        m_ItemName.text = item_info.Name;

        m_Tween.Play(true);
    }
}
