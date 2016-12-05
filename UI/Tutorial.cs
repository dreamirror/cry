using UnityEngine;
using System.Collections.Generic;
using LinqTools;
using PacketEnums;
using System;

public delegate bool TutorialCallback();

public class Tutorial : MonoBehaviour
{
    static Tutorial m_Instance = null;
    static public Tutorial Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = ((GameObject)GameObject.Instantiate(Resources.Load("Prefab/Menu/Tutorial"))).GetComponent<Tutorial>();
                m_Instance.gameObject.SetActive(true);
                m_Instance.PlayerInfo = Network.PlayerInfo;
                if (m_Instance.CurrentInfo == null && TutorialInfoManager.Instance.ContainsIdn(Network.PlayerInfo.tutorial_state))
                {
                    m_Instance.CurrentInfo = TutorialInfoManager.Instance.GetInfoByIdn(Network.PlayerInfo.tutorial_state) as TutorialInfo;
                }
#if UNITY_EDITOR
                if (m_Instance.m_TutorialAtlas.spriteMaterial == null)
                    m_Instance.m_TutorialAtlas.replacement = AssetManager.LoadTutorialAtlas();
#else
                m_Instance.m_TutorialAtlas.replacement = AssetManager.LoadTutorialAtlas();
#endif
                if (Instance.Completed == false)
                {
                    Instance.PreloadCharacters();
                }
                DontDestroyOnLoad(m_Instance.gameObject);
            }
            return m_Instance;
        }
    }
    public UIAtlas m_TutorialAtlas;
    public GameObject IndicatorPrefab;
    public GameObject DialogPrefab;
    public GameObject block;
    public GameObject contents;
    public GameObject BtnSkip;

    public bool Completed { get { return PlayerInfo != null && PlayerInfo.tutorial_state >= TutorialInfoManager.Instance.CompletedState; } }
    public int CurrentState { get { return CurrentInfo != null ? CurrentInfo.IDN : TutorialInfoManager.Instance.CompletedState; } }
    public int NextState { get { return TutorialInfoManager.Instance.GetNextTutorialState(CurrentInfo); } }

    List<Collider2D> m_TargetsCollider = new List<Collider2D>();
    List<GameObject> m_Prefabs = new List<GameObject>();

    ClientPlayerData PlayerInfo = null;

    float showTime = 0f;
    bool bShowed = false;
    public TutorialInfo CurrentInfo = null;
    public TutorialInfo CutsceneInfo = null;
    TargetInfo CurrentDialogTarget = null;

    void Start()
    {
        if (Completed)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
            block.SetActive(true);
            contents.SetActive(true);
        }

#if SH_DEV || UNITY_EDITOR
        BtnSkip.SetActive(Completed == false);
#endif
    }

    void Update()
    {
#if UNITY_EDITOR
        if ((CurrentDialogTarget != null || CutsceneInfo != null) && TimeManager.Instance.IsBoost == true)
        {
            OnBtnClick();
            return;
        }
#endif

        if (CutsceneInfo != null && CutsceneInfo.CutSceneInfo != null)
        {
            return;
        }

        if (Completed)
        {
            TutorialComplete();
            return;
        }
        if(bShowed == false && showTime < Time.realtimeSinceStartup)
        {
            if (CurrentInfo.Condition != null && CurrentInfo.Condition.IsConditionOK == false)
            {
                return;
            }
            Show();
        }
    }

    private void TutorialComplete()
    {
        gameObject.SetActive(false);
        if (GameMain.Instance.gameObject.activeSelf == false)
        {
            TimeManager.Instance.SetPause(false);
        }
        C2G.TutorialState tutorial_packet = new C2G.TutorialState();
        tutorial_packet.tutorial_state = (short)TutorialInfoManager.Instance.CompletedState;
        tutorial_packet.next_tutorial_state = (short)TutorialInfoManager.Instance.CompletedState;
        Network.GameServer.JsonAsync<C2G.TutorialState, C2G.TutorialStateAck>(tutorial_packet, OnTutorialEndHandler);
    }

    void OnTutorialEndHandler(C2G.TutorialState packet, C2G.TutorialStateAck ack)
    {
        Network.PlayerInfo.tutorial_state = TutorialInfoManager.Instance.CompletedState;
        Network.Instance.ProcessReward3Ack(ack.rewards_ack);

        TutorialInfo info = TutorialInfoManager.Instance.GetInfoByIdn(TutorialInfoManager.Instance.CompletedState) as TutorialInfo;
        Popup.Instance.Show(ePopupMode.Reward, info.rewards, Localization.Get("TutorialRewardTitle"), Localization.Get("GetThisRewards"), ack.rewards_ack);

        MetapsAnalyticsScript.TrackEvent("Tutorial", "Finish");
#if SH_DEV || UNITY_EDITOR
        BtnSkip.SetActive(Completed == false);
#endif
    }


    GameMenu CurrentMenu;
    void Show()
    {
        if (GameMain.Instance == null) return;
        bShowed = true;
        CurrentMenu = GameMain.Instance.CurrentGameMenu;
        if (GameMain.Instance.gameObject.activeSelf == false)
        {
            CurrentMenu = GameMenu.Battle;  // 삭제하면 튜토 1-3에서 클릭안되는 버그생김

            if (CurrentInfo.Condition == null || CurrentInfo.Condition.Type == eConditionType.ManaFull || CurrentInfo.Condition.Type == eConditionType.BattleStart)
            {
                TimeManager.Instance.SetPause(true);
            }
        }

        //bool bIndicator = CurrentInfo.Targets.Exists(t => t.Menu == CurrentMenu && t.type == eTutorialType.Indicator);
        
        List<TargetInfo> targets = CurrentInfo.Targets.Where(t=>t.Menu == CurrentMenu && t.type == eTutorialType.Click || t.type == eTutorialType.Indicator).ToList();
        bool is_drag = CurrentInfo.Targets.Exists(t => t.Menu == CurrentMenu && t.type == eTutorialType.Drag);
        if (targets != null && targets.Count > 0)
        {
            //Camera uiCamera = UICamera.currentCamera;
            foreach (var target in targets)
            {
                GameObject obj = NGUITools.AddChild(contents.gameObject, IndicatorPrefab);
                obj.SetActive(false);

                Collider2D collider = null;
                if (string.IsNullOrEmpty(target.gameobject) == false)
                {
                    GameObject target_obj = GameObject.Find(target.gameobject);
                    obj.transform.localPosition = target_obj.transform.localPosition;
                    collider = target_obj.GetComponent<Collider2D>();
                }
                else
                {
                    obj.transform.localPosition = target.pos;
                    collider = RayCast(obj.transform.position, target.confirm_tag);
                }

                if (collider == null)
                {
                    bShowed = false;
                    showTime = Time.realtimeSinceStartup+ 0.2f;
                    Destroy(obj);
                    return;
                }

                if(collider.GetComponent<UIEventTrigger>() == null)
                    obj.transform.position = collider.transform.position;

                m_TargetsCollider.Add(collider);


                Vector3 localPosition = obj.transform.localPosition;
                localPosition.z = 0;
                obj.transform.localPosition = localPosition;

                obj.GetComponent<TutorialIndicator>().Init(is_drag?TutorialIndicator.IndigatorType.Drag: TutorialIndicator.IndigatorType.Touch, localPosition.y < -200);
                obj.SetActive(true);
                m_Prefabs.Add(obj);
            }
        }

        if (is_drag)
        {
            ShowDragTargetIndicator();
            return;
        }


        TargetInfo info = CurrentInfo.Targets.Find(e => e.Menu == CurrentMenu && e.type == eTutorialType.Dialog);
        if(info != null)
        {
            SetDialogTarget(info);
        }

        if (m_TargetsCollider.Count == 0 && m_Prefabs.Count == 0)
        {
            PlayerInfo.tutorial_state = (short)CurrentInfo.IDN;
            CurrentInfo = TutorialInfoManager.Instance.GetNextTutorial(PlayerInfo.tutorial_state);
            if (CurrentInfo == null)
            {
                TutorialComplete();
                return;
            }
            showTime = Time.realtimeSinceStartup + CurrentInfo.delay;
            bShowed = false;
        }
    }

    public void OnBtnClick()
    {
        if (bShowed == false || (CurrentInfo != null && CurrentInfo.Targets.Exists(e => e.type == eTutorialType.Drag)) || m_TargetsCollider.Count == 0 && m_Prefabs.Count == 0)
        {
            return;
        }

        CheckTargetCollider();

        if (m_TargetsCollider.Count == 0)
        {
            while (m_Prefabs.Count > 0)
            {
                Destroy(m_Prefabs[0]);
                m_Prefabs.RemoveAt(0);
            }
            if (CutsceneInfo != null)
            {//Cut Scene
                if (CutsceneInfo.Targets.Count > 0)
                {
                    TutorialDialog obj = NGUITools.AddChild(contents.gameObject, DialogPrefab).GetComponent<TutorialDialog>();
                    obj.Init(CutsceneInfo.Targets[0]);
                    CutsceneInfo.Targets.RemoveAt(0);
                    m_Prefabs.Add(obj.gameObject);
                    bShowed = true;
                }
                else
                {//Cut Scene end
                    TutorialInfo _cut_scene = CutsceneInfo;
                    CutsceneInfo = null;
                    if ((_cut_scene.CutSceneInfo.SceneType == eSceneType.PreAll || _cut_scene.CutSceneInfo.SceneType == eSceneType.PreAll_Wave3) && CheckBattleStart() == true)
                    {
                        SetConditionOK();
                    }
                    else
                    {
                        if (Completed == true
                            || (CurrentMenu == GameMenu.Battle && CurrentInfo.Condition != null && CurrentInfo.Condition.Type == eConditionType.BattleEndPopup)
                            )
                        {
                            gameObject.SetActive(false);
                        }
                        TimeManager.Instance.SetPause(false);
                    }

                }
            }
            else
            {//Tutorial 
                CurrentInfo.Targets.Remove(CurrentDialogTarget);
                TargetInfo info = CurrentInfo.Targets.Find(e => e.Menu == CurrentMenu && e.type == eTutorialType.Dialog);
                if (info != null)
                {
                    SetDialogTarget(info);
                    return;
                }
                CurrentDialogTarget = null;

                if(CurrentInfo.AfterNetworking == true)
                {
                    gameObject.SetActive(false);
                    return;
                }
                else if(CurrentInfo.Condition == null || CurrentInfo.Condition.IsConditionOK == true)
                    SetNextTutorial();
            }
        }
    }

    private void SetNextTutorial()
    {
        PlayerInfo.tutorial_state = (short)CurrentInfo.IDN;
        CurrentInfo = TutorialInfoManager.Instance.GetNextTutorial(CurrentState);
        if (CurrentInfo == null)
        {
            TutorialComplete();
            return;
        }
        //Debug.LogFormat("Current info : {0} : {1}", CurrentInfo.IDN, CurrentMenu);
        //        CurrentInfo.Targets.ForEach(e => Debug.LogFormat("{0}", e.Menu));

        MetapsAnalyticsScript.TrackEvent("Tutorial", CurrentMenu.ToString(), CurrentInfo.IDN);

        if (CurrentInfo.Condition != null && CurrentInfo.Condition.Type == eConditionType.BattleEndPopup)
            gameObject.SetActive(false);
        else
        {
            if (CurrentInfo.Condition == null)
            {
                showTime = Time.realtimeSinceStartup + CurrentInfo.delay;
                bShowed = false;
            }
            gameObject.SetActive(true);
        }

        if (GameMain.Instance.gameObject.activeSelf == false && (CurrentInfo.Condition == null || CurrentInfo.Condition.IsConditionOK == false))
        {
            TimeManager.Instance.SetPause(false);
        }
        BtnSkip.SetActive(false);
#if SH_DEV || UNITY_EDITOR
        if(CurrentInfo.Targets.Exists(t=>t.type == eTutorialType.Dialog))
            BtnSkip.SetActive(Completed == false);
#endif

    }

    private void CheckTargetCollider()
    {
        Camera uiCamera = UICamera.currentCamera;
        Vector3 mouse_pos = Input.mousePosition;
        for (int i = 0; i < m_TargetsCollider.Count; ++i)
        {
            Collider2D collider = m_TargetsCollider[i];

            Vector3 pos = uiCamera.WorldToScreenPoint(collider.transform.position);
            Vector2 size = collider.GetComponent<BoxCollider2D>().size;

            UIEventTrigger target_trigger = collider.GetComponent<UIEventTrigger>();
            if (target_trigger != null)
            {
                pos = uiCamera.WorldToScreenPoint(m_Prefabs[i].transform.position);
                size = new Vector2(100, 100);
            }

            //Debug.LogFormat("{0}, {1}", mouse_pos, uiCamera.ScreenToViewportPoint(mouse_pos));
            //Debug.LogFormat("{0}, {1} : {2}", pos, mouse_pos, collider.name);
            //Debug.LogFormat("{0}", size);
            if (pos.x - size.x / 2 <= mouse_pos.x
                && mouse_pos.x <= pos.x + size.x / 2
                && pos.y - size.y / 2 <= mouse_pos.y
                && mouse_pos.y <= pos.y + size.y / 2
                )
            {
                bool bClicked = false;

                UIButton target_btn = collider.GetComponent<UIButton>();
                UIToggle target_tg = collider.GetComponent<UIToggle>();
                if (target_btn != null && target_btn.onClick.Count > 0)
                {
                    target_btn.onClick[0].Execute();
                    bClicked = true;
                }
                else if (target_tg != null)
                {
                    target_tg.value = true;
                    bClicked = true;
                }
                else if (target_trigger != null && target_trigger.onClick.Count > 0)
                {
                    target_trigger.onClick[0].Execute();
                    bClicked = true;
                }

                if (bClicked)
                {
                    m_TargetsCollider.RemoveAt(i);
                    Destroy(m_Prefabs[i]);
                    m_Prefabs.RemoveAt(i);
                }

                break;
            }
        }
    }

    public void AfterNetworking()
    {
        if (CurrentInfo == null || CurrentInfo.AfterNetworking == false)
        {
            Debug.LogError("CurrentInfo.AfterNetworking == false");
            return;
        }
        SetNextTutorial();
    }
    private void SetDialogTarget(TargetInfo info)
    {
        Debug.LogFormat("SetDialogTarget", info.Desc);
        CurrentDialogTarget = info;
        TutorialDialog obj = NGUITools.AddChild(contents.gameObject, DialogPrefab).GetComponent<TutorialDialog>();
        obj.Init(info);
        m_Prefabs.Add(obj.gameObject);
    }

    Collider2D RayCast(Vector2 pos, string tag = "")
    {
        block.SetActive(false);
        //Vector2 pos2 = UICamera.currentCamera.ViewportToWorldPoint(pos);
        //Vector2 pos2 = UICamera.currentCamera.ScreenToWorldPoint(pos);
        RaycastHit2D[] hitInfos = Physics2D.RaycastAll(pos, Vector2.down);
        block.SetActive(true);

        foreach (var hitInfo in hitInfos)
        {
            if (hitInfo.collider == null) continue;
            if (string.IsNullOrEmpty(tag) == false && hitInfo.transform.tag != tag) continue;
            UIButton target_btn = hitInfo.collider.GetComponent<UIButton>();
            UIToggle target_tg = hitInfo.collider.GetComponent<UIToggle>();
            UIEventTrigger target_trigger = hitInfo.collider.GetComponent<UIEventTrigger>();
            if (target_btn != null && target_btn.onClick.Count > 0)
            {
                return hitInfo.collider;
            }
            else if (target_tg != null)
            {
                return hitInfo.collider;
            }
            else if (target_trigger != null && target_trigger.onClick.Count > 0)
            {
                return hitInfo.collider;
            }

        }

        return null;
    }

    public void Check()
    {
        if (CurrentInfo == null)
            CurrentInfo = TutorialInfoManager.Instance.GetNextTutorial(PlayerInfo.tutorial_state);
        if (CurrentInfo != null && Completed == false)
        {
            if (CurrentInfo.AfterNetworking == true)
                AfterNetworking();
            else
            {
                if(CurrentInfo.Condition != null && GameMain.Instance.CurrentGameMenu != GameMenu.Battle)
                    SetConditionOK();
                showTime = Time.realtimeSinceStartup + CurrentInfo.delay;
                bShowed = false;
                gameObject.SetActive(true);

                if(CurrentInfo.IDN < 100)
                    MetapsAnalyticsScript.TrackEvent("Tutorial", "Begin");
            }
        }
    }

    MainLayout m_DragLayout= null;
    public void OnPress(bool isPressed)
    {
        if(CurrentInfo != null)
        {
            TargetInfo target = CurrentInfo.Targets.Find(e => e.type == eTutorialType.Drag);
            if(target != null)
            {
                if (string.IsNullOrEmpty(target.gameobject) == false)
                {
                    GameObject target_obj = GameObject.Find(target.gameobject);
                    if(target_obj != null)
                    {
                        m_DragLayout = target_obj.GetComponentInParent<MainLayout>();
                        if (m_DragLayout != null)
                        {
                            m_DragLayout.ProcessPress();
                            if(m_DragLayout.DragContainer == null || m_DragLayout.DragContainer.CharacterAsset.Asset.name != target.gameobject)
                            {
                                m_DragLayout.ProcessRelease();
                                m_DragLayout = null;
                            }
                            else
                            {
                                //TweenPosition tween = m_Prefabs[0].GetComponent<TweenPosition>();
                                //Vector3 pos = m_Prefabs[0].transform.localPosition;
                                //tween.from = pos;
                                //pos.x += target.drag_x;
                                //pos.y += target.drag_y;
                                //tween.to = pos;
                                //tween.PlayForward();
                            }
                        }
                    }
                }
            }
        }
    }
    public void OnRelease()
    {
        if (m_DragLayout != null)
        {
            TargetInfo target = CurrentInfo.Targets.Find(e => e.type == eTutorialType.Drag);
            Vector3 pos = CoreUtility.WorldPositionToUIPosition(UICamera.lastWorldPosition);
//            Debug.Log(pos);

            if (DungeonInfoMenu.IsRectContainsPoint(target.pos, target.size, pos) == true)
            {
                m_DragLayout.ProcessRelease();
                while (m_Prefabs.Count > 0)
                {
                    Destroy(m_Prefabs[0]);
                    m_Prefabs.RemoveAt(0);
                }
                m_DragLayout = null;
                SetNextTutorial();                
            }
            else
            {
                m_DragLayout.DragContainer = null;
                //m_DragLayout.Rebatch();
                m_DragLayout.Init(TeamDataManager.Instance.GetTeam(pe_Team.Main));
                m_DragLayout.ProcessRelease();
                m_DragLayout = null;

                //TweenPosition tween = m_Prefabs[0].GetComponent<TweenPosition>();
                //tween.enabled = false;
                //Vector3 indicator_pos = m_Prefabs[0].transform.localPosition;
                //m_Prefabs[0].transform.localPosition = tween.from;

            }
        }
    }

    private void ShowDragTargetIndicator()
    {
        TargetInfo drag_target = CurrentInfo.Targets.Find(t => t.Menu == CurrentMenu && t.type == eTutorialType.Drag);
        if (drag_target != null)
        {
            GameObject target_obj = GameObject.Find(drag_target.gameobject);
            if (target_obj != null)
            {
                GameObject obj = NGUITools.AddChild(contents.gameObject, IndicatorPrefab);
                obj.transform.position = target_obj.transform.position;
                Vector3 localPosition = obj.transform.localPosition;
                localPosition.z = 0;
                localPosition.y += 100;
                obj.transform.localPosition = localPosition;

                obj.GetComponent<TutorialIndicator>().Init(TutorialIndicator.IndigatorType.Drag, false);

                obj.SetActive(true);
                m_Prefabs.Add(obj);

                TweenPosition tween = m_Prefabs[0].GetComponent<TweenPosition>();
                Vector3 pos = m_Prefabs[0].transform.localPosition;
                tween.delay = 0.2f;
                tween.from = pos;
                pos.x += drag_target.drag_x;
                pos.y += drag_target.drag_y;
                tween.to = pos;
                tween.PlayForward();
            }
        }
    }

    public void DragOver()
    {
        if (m_DragLayout != null)
            m_DragLayout.ProcessDragOver();
    }
    //////////////////////////////////////////////////////////////////////////////////
    //CutScene
    public void ShowCutScene(eSceneType type)
    {
        if (Network.BattleStageInfo == null || Network.BattleStageInfo.Difficulty == pe_Difficulty.Hard) return ;
        if (CutsceneInfo != null)
        {
            CurrentMenu = GameMenu.Battle;  // 삭제하면 튜토 1-3에서 클릭안되는 버그생김

            if (CutsceneInfo.CutSceneInfo.SceneType == type && CutsceneInfo.Targets.Count > 0)
            {
                TimeManager.Instance.SetPause(true);
                bShowed = true;
                TutorialDialog obj = NGUITools.AddChild(contents.gameObject, DialogPrefab).GetComponent<TutorialDialog>();
                obj.Init(CutsceneInfo.Targets[0]);
                CutsceneInfo.Targets.RemoveAt(0);
                m_Prefabs.Add(obj.gameObject);
                gameObject.SetActive(true);
                return;
            }
            else
                CutsceneInfo = null;
            return;
        }
        if ((type == eSceneType.PreAll || type == eSceneType.PreAll_Wave3) && CheckBattleStart() == true)
        {
            SetConditionOK();
        }
    }
    public bool first_clear = false;
    public bool CheckCutScene(eSceneType type)
    {
        if (Network.BattleStageInfo == null || Network.BattleStageInfo.Difficulty == pe_Difficulty.Hard) return false;
        if (type != eSceneType.Post)
        {
            var clear_data = MapClearDataManager.Instance.GetData(Network.BattleStageInfo);
            if (clear_data != null && clear_data.clear_rate > 0)
                return false;
        }
        else
        {
            if (first_clear == false)
                return false;
        }
        TutorialInfo _CutsceneInfo = TutorialInfoManager.Instance.GetCutScene(type, Network.BattleStageInfo);
        if(_CutsceneInfo != null && _CutsceneInfo.Targets.Count > 0)
        {
            CutsceneInfo = _CutsceneInfo;
            return true;
        }

        return false;
    }

    public void SetConditionOK()
    {
        showTime = Time.realtimeSinceStartup + CurrentInfo.delay;
        CurrentInfo.Condition.IsConditionOK = true;
        bShowed = false;
        gameObject.SetActive(true);
    }

    public bool IsWaitManaFull()
    {
        return CurrentInfo != null && CurrentInfo.Condition != null && CurrentInfo.Condition.Type == eConditionType.ManaFull;
    }

    public bool CheckConditionManaFull(CreatureInfo creature)
    {
        if (CurrentInfo != null && CurrentInfo.Condition !=null 
            && CurrentInfo.Condition.Type == eConditionType.ManaFull
            && CurrentInfo.Condition.CreatureID == creature.ID)
        {
            return true;
        }
        return false;
    }


    public bool CheckConditionBattleEndPopup()
    {
        if (CurrentInfo != null && CurrentInfo.Condition != null
            && CurrentInfo.Condition.Type == eConditionType.BattleEndPopup)
        {
            return true;
        }
        return false;
    }

    public bool CheckBattleStart()
    {
        if (CurrentInfo != null && CurrentInfo.Condition != null
            && CurrentInfo.Condition.Type == eConditionType.BattleStart)
        {
            return true;
        }

        return false;
    }

    public void OnClickSkip()
    {
        TutorialComplete();
    }

    void PreloadCharacters()
    {
        Debug.Log("Tutorial.PreloadCharacters()");
        foreach (string preload_character_id in TutorialInfoManager.Instance.PreloadCharacters)
        {
            var container = AssetManager.GetCharacterAsset(preload_character_id, "default");
            container.Alloc();
            container.Free();
        }
    }
}