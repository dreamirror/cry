using UnityEngine;
using System.Collections;
using System.Linq;

public class PopupBossInfo : PopupBase {

    public PrefabManager RewardPrefabManager;

    public UICharacterContainer CharacterContainer;
    public UILabel LabelTitle;
    public UIGrid GridReward;

    MapStageDifficulty m_StageInfo = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_StageInfo = parms[0] as MapStageDifficulty;
                
        Init();
    }

    void Init()
    {   
        MapCreatureInfo map_creature = m_StageInfo.Waves[0].Creatures.Find(c => c.CreatureType == eMapCreatureType.Boss || c.CreatureType == eMapCreatureType.WorldBoss);
               
        LabelTitle.text = string.Format("Lv.{0} {1}", BossSpot.CalculateLevel(map_creature.Level, m_StageInfo), map_creature.CreatureInfo.Name);

        CharacterContainer.Init(AssetManager.GetCharacterAsset(map_creature.CreatureInfo.ID, "default"), UICharacterContainer.Mode.UI_Normal);
        CharacterContainer.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        CharacterContainer.SetPlay(UICharacterContainer.ePlayType.Social);
        
        foreach (RewardLoot reward in m_StageInfo.DropItems)
        {   
            var reward_item = RewardPrefabManager.GetNewObject<RewardItem>(GridReward.transform, Vector3.zero);
            if (reward.ItemInfo.ItemType == eItemType.SoulStone)
                reward_item.InitSoulStone(reward.ItemInfo as SoulStoneInfo, reward.Value);
            else
                reward_item.InitReward(reward);
        }

        GridReward.Reposition();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    public void OnClickEnter()
    {
        MapInfo map_info = MapInfoManager.Instance.GetInfoByID("10001_boss");
        if (MapClearDataManager.Instance.GetMapDailyClearCount(map_info.IDN, PacketEnums.pe_Difficulty.Normal) >= map_info.TryLimit)
        {
            Tooltip.Instance.ShowMessageKey("NotEnoughTryCount");
            return;
        }

        MenuParams parms = new MenuParams();
        parms.AddParam<MapStageDifficulty>(m_StageInfo);
        OnClose();
        GameMain.Instance.ChangeMenu(GameMenu.DungeonInfo, parms);
    }
    public void OnClickBossInfo()
    {
        Popup.Instance.Show(ePopupMode.BossDetail, m_StageInfo);
    }

    bool IsDraggingCharacter = false;
    Vector2 m_TouchPosition = Vector2.zero, m_FirstTouchPosition = Vector2.zero;

    public void OnCharacterPress()
    {
        m_FirstTouchPosition = m_TouchPosition = UICamera.lastTouchPosition;
        IsDraggingCharacter = true;
    }

    public void OnCharacterRelease()
    {
        if (m_FirstTouchPosition == UICamera.lastTouchPosition)
        {
            CharacterContainer.PlayRandomAction();
        }
        m_TouchPosition = Vector2.zero;
        IsDraggingCharacter = false;
    }

    void Update()
    {
        if (IsDraggingCharacter)
            UpdateDragCharacter();
    }

    void UpdateDragCharacter()
    {
        Vector2 pos = UICamera.lastTouchPosition;
        float delta = m_TouchPosition.x - pos.x;
        float speed = 0.5f;
        m_TouchPosition = pos;

        CharacterContainer.transform.localRotation *= Quaternion.Euler(0f, delta * speed, 0f);
    }
}
