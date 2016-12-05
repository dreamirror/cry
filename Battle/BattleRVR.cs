using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using HeroFX;
using PacketEnums;

public class BattleRVR : BattleBase
{
    static new public BattleRVR Instance { get; protected set; }

    public UIAtlas m_AtlasCharacterShots;

    public MeshRenderer m_BG;

    // raid
    public PrefabManager m_RaidPlayerPrefabManager;
    public Canvas RVRCanvas;
    public GameObject PlayerContainer, EnemyContainer;
    List<RaidPlayer> m_Characters, m_Enemies;

    public int column_count = 4;
    public int row_count = 10;
    public Vector3 StartPosition = Vector3.zero;
    public float RowGap = 50f, ColumnGap = 30f;

    override protected void Awake()
    {
        base.Awake();
        BattleRVR.Instance = this;
    }

    protected override void Start()
    {
        GameMain.Instance.gameObject.SetActive(false);

        m_BG.material.mainTexture = AssetManager.LoadBG("2001_raid_golem");

        tween_system = AssetManager.GetCharacterPrefab("000_tween").GetComponent<HFX_TweenSystem>();
        color_container = tween_system.gameObject.GetComponent<ColorContainer>();

        InitRVR();

//        battleEndType = pe_EndBattle.Invalid;
        IsBattleEnd = false;
        PlaybackTime = 0f;

        Network.HideIndicator();
        SoundManager.Instance.PlayBGM("PVP");
    }

    protected override void Update()
    {
        base.Update();

        if (Time.timeScale == 0f) return;
        if (IsPause == ePauseType.Pause)
            return;

        m_Characters.ForEach(p => p.UpdatePlayer());
        m_Enemies.ForEach(p => p.UpdatePlayer());
    }

    protected void Clear()
    {
        m_RaidPlayerPrefabManager.Clear();
    }

    void InitRVR()
    {
        if (m_AtlasCharacterShots.spriteMaterial == null)
            m_AtlasCharacterShots.replacement = AssetManager.LoadCharacterShotsAtlas();

        //Vector3 enemyScale = new Vector3(-1f, 1f, 1f);

        m_Characters = new List<RaidPlayer>();
        m_Enemies = new List<RaidPlayer>();
        Vector3 position = StartPosition;
        long account_idx = 1000000;
        long enemy_account_idx = 2000000;
        for (int i = 0; i < row_count; ++i)
        {
            position.x = StartPosition.x;
            for (int j = 0; j < column_count; ++j)
            {
                var player = m_RaidPlayerPrefabManager.GetNewObject<RaidPlayer>(PlayerContainer.transform, position);

                player.Init(++account_idx, "profile", "Player" + MNS.Random.Instance.NextRange(1000, 2000), MNS.Random.Instance.NextRange(5, 20));
                m_Characters.Add(player);

                var enemy = m_RaidPlayerPrefabManager.GetNewObject<RaidPlayer>(EnemyContainer.transform, position);
                enemy.Init(++enemy_account_idx, "profile", "Enemy" + MNS.Random.Instance.NextRange(1000, 2000), MNS.Random.Instance.NextRange(5, 20));
                m_Enemies.Add(enemy);

                position.x -= ColumnGap;
            }
            position.z -= RowGap;
        }

    }

    public void OnPause()
    {
        if (IsBattleEnd == true)
            return;

        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }

        SetPause(false);
    }

    public void SetPause(bool bShowImmediately)
    {
        if (IsBattleEnd == true)
            return;

        Popup.PopupInfo popup = Popup.Instance.GetCurrentPopup();
        if (popup != null) return;

        Debug.Log("OnPause");
        if (bShowImmediately)
            Popup.Instance.ShowImmediately(ePopupMode.BattleOption);
        else
            Popup.Instance.Show(ePopupMode.BattleOption);
    }

    public void OnChat()
    {
        if (Tutorial.Instance.Completed == false)
        {
            Tooltip.Instance.ShowMessageKey("NotAvailableInTutorial");
            return;
        }
        if (GameConfig.Get<bool>("contents_chatting_maintenance") == true)
        {
            Tooltip.Instance.ShowMessage(Localization.Get("ChatMaintenance"));
            return;
        }
        ChattingMain.Instance.ShowChattingPopup();
    }
}
