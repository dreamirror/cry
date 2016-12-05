using PacketEnums;
using UnityEngine;

abstract public class PopupMainMenu : PopupBase
{
    public UIGrid m_Grid;
    public UIScrollView m_Scroll;
    public UISprite m_BG;
    public PrefabManager m_PrefabManager;

    protected int scroll_gap = 30;

    abstract protected void InitItem();

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        Init(is_new);
    }
    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
    }

    void OnEnable()
    {
        UIPanel scrollPanel = m_Scroll.GetComponent<UIPanel>();
        scrollPanel.ResetAndUpdateAnchors();
       m_Scroll.ResetPosition();

        m_BG.ResizeCollider();
    }

    public void Init(bool is_new)
    {
        //gameObject.SetActive(true);
        InitItem();

        m_Grid.Reposition();
        //
        int maxPerLine = System.Math.Max(1, m_Grid.maxPerLine);
        int item_count = (m_Grid.transform.childCount + maxPerLine - 1) / maxPerLine;
        float pos = 0;
        m_Scroll.movement = UIScrollView.Movement.Custom;
        if (item_count > 3)
        {
            m_BG.width = 28 + (int)(m_Grid.cellWidth*3) - scroll_gap * 2;
            m_Scroll.customMovement = new Vector2(1, 0);
            pos = -m_Grid.cellWidth + scroll_gap;
        }
        else
        {
            m_BG.width = 28 + (int)(m_Grid.cellWidth) * item_count;
            m_Scroll.customMovement = Vector2.zero;
            pos = -(item_count - 1) * m_Grid.cellWidth * 0.5f;
        }
        if (m_Scroll.transform.localPosition.x == 0f || is_new == true)
        {
            m_Scroll.GetComponent<UIPanel>().clipOffset = new Vector2(-pos, 0f);
            m_Scroll.transform.localPosition = new Vector3(pos, m_Scroll.transform.localPosition.y);
        }
    }


    public void OnCancel()
    {
        parent.Close();
    }
}

public class PopupColosseum : PopupMainMenu
{
    override protected void InitItem()
    {
        //[결투장 규칙 설명]\n
        string pvp_text = "1. 내 영웅 팀으로 다른 유저와 싸워 순위를 쟁탈합니다.\n2. 결투장 팀은 최대 5명으로 구성됩니다.\n3. 나보다 순위가 높은 적에게 승리하면 순위가 교체됩니다.\n4. 공격자는 전투 후 10분동안 재도전할 수 없습니다.\n5. 매일 오후 9시에 순위를 산정하여 보상을 지급합니다.\n6. 매일 오전 5시에 남은 횟수가 초기화됩니다.\n7. 수정을 이용하여 남은 횟수를 즉시 초기화할 수 있습니다.";
        var pvp = m_PrefabManager.GetNewObject<PopupMenuItem>(m_Grid.transform, Vector3.zero);
        pvp.Init("pvp", "결투장", "남은 도전 횟수 5/5", "main_menu_colosseum_pvp", pvp_text, OnClick);

        //[전력전 규칙 설명]\n
        string totalwar_text = "1. 대규모 팀을 구성하여 다른 유저와 싸워 순위를 쟁탈합니다.\n2. 자신의 모든 영웅으로 하나의 파티를 편성할 수 있습니다.\n3. 총 5개의 파티 슬롯에 영웅을 순서대로 배치합니다.\n4. 전투 중 영웅이 죽으면 해당 슬롯의 다음 순서 영웅이 난입합니다.\n5. 나보다 순위가 높은 적에게 승리하면 순위가 교체됩니다.\n6. 공격자는 전투 후 10분동안 재도전할 수 없습니다.\n7. 매일 오후 9시에 순위를 산정하여 보상을 지급합니다.\n8. 매일 오전 5시에 남은 횟수가 초기화됩니다.\n9. 수정을 이용하여 남은 횟수를 즉시 초기화할 수 있습니다.";
        var totalwar = m_PrefabManager.GetNewObject<PopupMenuItem>(m_Grid.transform, Vector3.zero);
        totalwar.Init("totalwar", "전력전", "남은 도전 횟수 5/5", "main_menu_colosseum_totalwar", totalwar_text, OnClick);
    }

    void OnClick(string menu_id)
    {
        switch (menu_id)
        {
            case "pvp":
                if(TeamDataManager.Instance.GetTeam(pe_Team.PVP_Defense) == null)
                {
                    MenuParams parm = new MenuParams();
                    parm.AddParam("deck_type", "defense");
                    GameMain.Instance.ChangeMenu(GameMenu.PVPDeckInfo, parm);
                }
                else
                {
                    GameMain.Instance.ChangeMenu(GameMenu.PVP);
                }
                OnCancel();

                return;
            case "totalwar":
                break;

        }

        Tooltip.Instance.ShowMessageKey("NotImplement");
    }
}
