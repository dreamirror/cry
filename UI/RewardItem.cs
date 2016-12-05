using UnityEngine;
using System.Collections.Generic;
using PacketInfo;

public delegate void OnClickItemDelegate(ItemInfoBase info);

public class RewardItem : MonoBehaviour
{
    public Transform m_contents;
    public GameObject m_creature;
    public UISprite m_creature_sprite;

    public UISprite m_icon;
    public UISprite m_IconStore;
    public UISprite m_IconGray;
    public UILabel m_count, m_level;
    public GameObject m_StuffGrade;
    public UILabel m_LabelGrade;
    public OnClickItemDelegate OnClickItem = null;
    public UIPlayTween m_TWTargetItem;
    public GameObject[] m_Notifies;
    public GameObject m_Puzzle;

    public SHTooltip m_Tooltip;

    public UIToggleSprite character_border;

    bool bPlayTween = false;
    public ItemInfoBase Info = null;

    //bool bShowTooltip = false;
    public CreatureInfo m_RewardCreature = null;
    void Start()
    {
    }

    void Clear()
    {
        Info = null;
        m_RewardCreature = null;
        m_creature.SetActive(false);

        for (int i = 0; i < m_contents.childCount; ++i)
        {
            m_contents.GetChild(i).gameObject.SetActive(false);
        }
        System.Array.ForEach(m_Notifies, e => e.SetActive(false));
    }

    // Use this for initialization
    void OnEnable()
    {
        if (bPlayTween)
        {
            bPlayTween = false;
            m_TWTargetItem.Play(true);
        }
    }

    void InitInternal(int count)
    {
        switch (Info.ItemType)
        {
            case eItemType.SoulStone:
                InitSoulStoneInternal(Info as SoulStoneInfo, count);
                break;

            case eItemType.Token:
                InitIcon(false);
                break;

            default:
                InitIcon(count > 0);
                break;
        }

        m_count.text = count.ToString();
        m_count.gameObject.SetActive(count > 0);

        if (Network.TargetItemInfo != null && Network.TargetItemInfo.IDN == Info.IDN)
        {
            m_Notifies[1].SetActive(true);
        }
    }

    public void InitReward(RewardBase reward)
    {
        if (reward.CreatureInfo != null)
            InitCreature(reward.CreatureInfo, (short)reward.Value);
        else
            InitReward(reward.ItemInfo.IDN, reward.Value);
    }

    public void InitReward(string id, int count)
    {
        Clear();
        if (id.StartsWith("token_") == true)
        {
            Info = TokenInfoManager.Instance.GetInfoByID(id);
            InitInternal(count);
        }
        else if (id.StartsWith("icon_gacha_"))
        {
            gameObject.SetActive(true);
            m_StuffGrade.SetActive(true);

            m_icon.gameObject.SetActive(true);

            m_icon.spriteName = id;

            m_LabelGrade.gameObject.SetActive(true);
            m_LabelGrade.text = count.ToString();

            gameObject.SetActive(true);
        }
    }

    public void InitReward(int idn, int count)
    {
        Clear();
        Info = ItemInfoManager.Instance.GetInfoByIdn(idn);

        InitInternal(count);

        EventParamItemMade itemMade = ItemManager.Instance.ItemMadeList.Find(e => e.item.Info.IDN == idn);
        m_Notifies[0].SetActive(itemMade != null);

        var item = ItemManager.Instance.GetItemByIdn(idn);
        m_Notifies[2].SetActive(item != null && item.Notify);
    }

    public void InitRefreshItem(string refresh_icon)
    {
        Clear();

        m_icon.gameObject.SetActive(true);
        m_icon.spriteName = refresh_icon;
        
    }

    public void InitStoreItem(StoreLootItem item)
    {
        Clear();

        if (item.LootCount < 1)
            m_count.gameObject.SetActive(false);

        m_IconStore.gameObject.SetActive(true);
        m_count.text = item.LootCount.ToString();
        m_IconStore.spriteName = item.Image;
    }

    public void InitStoreItem(StoreGoodsItem item)
    {
        Clear();

        m_IconStore.gameObject.SetActive(true);
        m_IconStore.spriteName = item.Image;
        m_count.text = (item.Target.goods_value + item.bonus).ToString();
    }    

    void InitCreatureInternal(CreatureInfo creature_info, short grade)
    {
        gameObject.SetActive(true);
        m_creature.SetActive(true);

        string sprite_name = string.Format("cs_{0}", creature_info.ID);
        string new_sprite_name = "_cut_" + sprite_name;
        UISpriteData sp = m_creature_sprite.atlas.CloneCustomSprite(sprite_name, new_sprite_name);
        if (sp != null)
            sp.height = sp.width;

        m_creature_sprite.spriteName = new_sprite_name;

        m_StuffGrade.SetActive(true);
        m_LabelGrade.gameObject.SetActive(true);
        m_LabelGrade.text = grade.ToString();

        m_RewardCreature = creature_info;

        character_border.SetSpriteActive(creature_info != null && creature_info.TeamSkill != null);
    }
    public void InitCreature(CreatureInfo creature_info, short grade)
    {
        Clear();

        InitCreatureInternal(creature_info, grade);
    }
    public void InitCreature(Creature creature)
    {
        InitCreature(creature.Info, creature.Grade);
    }

    public void InitSoulStone(SoulStoneInfo soulstone_info, int count)
    {
        Clear();
        InitSoulStoneInternal(soulstone_info, count);
    }
    void InitSoulStoneInternal(SoulStoneInfo soulstone_info, int count)
    {
        Info = soulstone_info;
        InitCreatureInternal(soulstone_info.Creature, soulstone_info.Grade);

        m_Puzzle.SetActive(true);

        m_count.text = count.ToString();
        m_count.gameObject.SetActive(true);
    }

    public void InitStoreItem(pd_StoreItem item)
    {
        switch (item.item_type)
        {
            case pe_StoreItemType.Item:
            case pe_StoreItemType.Stuff:
            case pe_StoreItemType.Rune:
            case pe_StoreItemType.Token:
                Clear();
                Info = ItemInfoManager.Instance.GetInfoByIdn(item.item_idn);
                InitIcon(true);
                break;

            case pe_StoreItemType.SoulStone:
                Clear();
                Info = ItemInfoManager.Instance.GetInfoByIdn(item.item_idn);
                InitSoulStoneInternal((Info as SoulStoneInfo), item.item_count);
                return;
            case pe_StoreItemType.Creature:
                m_RewardCreature = CreatureInfoManager.Instance.GetInfoByIdn(item.item_idn);
                InitCreature(m_RewardCreature, item.item_count);
                return;
        }
        m_count.text = item.item_count.ToString();
        m_count.gameObject.SetActive(true);
    }

    public void InitRune(Rune rune)
    {
        Clear();
        Info = rune.Info;
        InitIcon(false);

        m_level.gameObject.SetActive(true);
        m_level.text = Localization.Format("HeroLevel", rune.Level);
    }

    public void InitGold(int gold)
    {
        Clear();

        int count = gold;

        m_icon.spriteName = "token_gold";
        m_IconGray.spriteName = "token_gold";
        gameObject.SetActive(true);

        m_count.text = count.ToString();
        m_count.gameObject.SetActive(true);
    }


    private void InitIcon(bool use_puzzle)
    {
        m_icon.gameObject.SetActive(true);

        m_icon.spriteName = Info.IconID;
        m_IconGray.spriteName = Info.IconID;

        ItemInfoGradeBase grade_info = Info as ItemInfoGradeBase;
        if (grade_info != null)
        {
            m_StuffGrade.SetActive(true);
            m_LabelGrade.gameObject.SetActive(true);
            m_LabelGrade.text = grade_info.Grade.ToString();
        }

        gameObject.SetActive(true);
        m_Puzzle.SetActive(use_puzzle && Info.PieceCountMax > 1);
        character_border.SetSpriteActive(false);
    }

    public void Init(Item item)
    {
        Clear();
        Info = item.Info;

        InitIcon(false);

        m_icon.gameObject.SetActive(item.Count > 0);
        m_IconGray.gameObject.SetActive(item.Count <= 0);

        if (item.StuffInfo.PieceCountMax > 1)
        {
            if (item.Count > 0)
            {
                m_count.text = item.Count.ToString();
                m_count.gameObject.SetActive(true);
                m_count.color = Color.white;
            }
            else
            {
                m_count.text = string.Format("[FF0000]{0}[-]/{1}", item.PieceCount, item.StuffInfo.PieceCountMax);
                m_count.gameObject.SetActive(true);
                m_count.color = Color.white;
            }
        }
        else
        {
            m_count.text = item.Count.ToString();
            m_count.gameObject.SetActive(true);
            m_count.color = item.Count == 0 ? Color.red : Color.white;
        }
    }
    //---------------------------------------------------------------------------

    public void OnClick()
    {
        if (m_Tooltip.Showed) return;

        if (OnClickItem != null)
        {
            OnClickItem(Info);
        }
    }

    public void OnShowTooltip(SHTooltip tooltip)
    {
        //if (OnClickItem != null && tooltip.Pressed == false)
        //{
        //    tooltip.CancelShow();
        //    return;
        //}

        if (Info != null && Info.IDN != 40003)//40003 energy
        {
            Tooltip.Instance.ShowRewardItem(this);
        }
        else if (m_RewardCreature != null)
        {
            Tooltip.Instance.ShowTarget(m_RewardCreature.GetTooltip(), tooltip);
        }
    }

    public string GetTooltip()
    {
        return Info.GetTooltip();
    }
}
