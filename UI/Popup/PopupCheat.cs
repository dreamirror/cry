#define USE_TOUCHCONSOLE

using System;
using UnityEngine;
using System.Linq;
#if USE_TOUCHCONSOLE
using Opencoding.CommandHandlerSystem;
#endif

static class CheatCommandHandlers
{
    public static void Initialize()
    {
#if USE_TOUCHCONSOLE
        CommandHandlers.RegisterCommandHandlers(typeof(CheatCommandHandlers));
#endif
    }

    public static void Uninitialize()
    {
#if USE_TOUCHCONSOLE
        CommandHandlers.UnregisterCommandHandlers(typeof(CheatCommandHandlers));
#endif
    }

#if USE_TOUCHCONSOLE
    [CommandHandler]
#endif
    private static void OpenCheat()
    {
        Popup.Instance.Show(ePopupMode.Cheat);
#if USE_TOUCHCONSOLE
        Opencoding.Console.DebugConsole.IsVisible = false;
#endif
    }
}

public class PopupCheat : PopupBase
{
    public PrefabManager m_PrefabManager;
    public GameObject m_Commands, m_SelectCommands;
    public UILabel m_Description;
    public Texture2D m_TestTexture;


    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        AddCategories();
    }

    //////////////////////////////////////////////////////////////////////////////////////

    void AddCategories()
    {
        m_Category = null;
        m_Command = null;
        m_Param = null;

        m_Description.text = "";

        m_PrefabManager.Clear();
        m_PrefabManager.Destroy();
        bool isFirst = true;
        foreach (string category in CheatInfoManager.Instance.GetCategories())
        {
            AddCommand(m_Commands.transform, category, isFirst, CallbackExecute);
            isFirst = false;
        }
    }

    void AddSelected()
    {
        m_PrefabManager.Clear();
        m_PrefabManager.Destroy();

        AddCommand(m_SelectCommands.transform, "", true, CallbackClear);

        string description = "";
        if (m_Category != null)
        {
            AddCommand(m_SelectCommands.transform, m_Category, false, CallbackCategory);
            if (m_Command != null)
            {
                AddCommand(m_SelectCommands.transform, m_Command, false, CallbackCommand);

                CheatInfo cheat_info = CheatInfoManager.Instance.GetInfo(m_Category, m_Command);
                if (cheat_info != null)
                    description = cheat_info.Description;

                if (m_Param != null)
                    AddCommand(m_SelectCommands.transform, m_Param, false, null);
            }
        }

        m_Description.text = description;
    }

    void AddCommands()
    {
        AddSelected();

        bool isFirst = true;
        foreach (string command in CheatInfoManager.Instance.GetCommands(m_Category))
        {
            AddCommand(m_Commands.transform, command, isFirst, CallbackExecute);
            isFirst = false;
        }
    }

    void AddParams()
    {
        AddSelected();

        CheatInfo cheat_info = CheatInfoManager.Instance.GetInfo(m_Category, m_Command);

        bool isFirst = true;
        foreach (string command in cheat_info.Params)
        {
            AddCommand(m_Commands.transform, command, isFirst, CallbackExecute);
            isFirst = false;
        }
    }

    void OnDisable()
    {
        m_PrefabManager.Clear();
        m_PrefabManager.Destroy();
    }

    public void OnCancel()
    {
        parent.Close();
    }


    void AddCommand(Transform parent, string strCommand, bool isFirst, Action<string> callback)
    {
        GameObject last_object = isFirst==true?null:m_PrefabManager.LastObject;

        var command = m_PrefabManager.GetNewObject<CheatCommand>(parent, Vector3.zero);
        command.Init(strCommand, callback);
        float cur_width = command.label.width;

        Vector3 pos = Vector3.zero;
        if (last_object != null)
        {
            float width = last_object.GetComponent<CheatCommand>().label.width;
            pos += last_object.transform.localPosition;
            pos.x += width*0.5f + 30f;

            if (pos.x + (cur_width*0.5f + 20f)*2 > 1000f)
            {
                pos.x = 0f;
                pos.y -= 70f;
            }
        }

        pos.x += (cur_width*0.5f+20f);
        command.transform.localPosition = pos;
    }

    public void OnClear()
    {
        AddCategories();
    }

    string m_Category = null, m_Command = null, m_Param = null;
    public void CallbackExecute(string strCommand)
    {
        if (string.IsNullOrEmpty(m_Category))
        {
            m_Category = strCommand;

            AddCommands();
        }
        else if (string.IsNullOrEmpty(m_Command))
        {
            m_Command = strCommand;
            AddParams();
        }
        else
        {
            m_Param = strCommand;
            AddSelected();
        }
    }

    public void CallbackClear(string strCommand)
    {
        AddCategories();
    }

    public void CallbackCategory(string strCommand)
    {
        m_Param = null;
        m_Command = null;
        AddCommands();
    }

    public void CallbackCommand(string strCommand)
    {
        m_Param = null;
        AddParams();
    }

    void OnExecute()
    {
        if (m_Category == null || m_Command == null)
        {
            Tooltip.Instance.ShowMessage("Please select command.");
            return;
        }

        CheatInfo cheat_info = CheatInfoManager.Instance.GetInfo(m_Category, m_Command);
        if (m_Param == null)
        {
            if (cheat_info.Params.Count > 0)
            {
                Tooltip.Instance.ShowMessage("Please select param.");
                return;
            }
            m_Param = "";
        }

        if (BattleBase.Instance != null && m_Category != "Show")
        {
            Tooltip.Instance.ShowMessage("In the battle it can not be used.");
            return;
        }

        if (cheat_info.IsClient)
            OnExecuteClient();
        else
        {
            C2G.Cheat packet = new C2G.Cheat();
            packet.category = m_Category;
            packet.command = m_Command;
            packet.param = m_Param;
            Network.GameServer.JsonAsync<C2G.Cheat, C2G.CheatAck>(packet, OnExecuteAck);
        }
    }

    void OnExecuteAck(C2G.Cheat packet, C2G.CheatAck ack)
    {
        if (string.IsNullOrEmpty(ack.error) == false)
        {
            Tooltip.Instance.ShowMessage(ack.error);
            return;
        }

        Network.PlayerInfo.SetPlayerData(ack.player_info);
        SaveDataManger.Instance.InitFromData(ack.detail_data);

        while (GameMain.Instance.BackMenu(true) == true) ;
        GameMain.Instance.GetCurrentMenu().UpdateMenu();
        GameMain.Instance.UpdatePlayerInfo();
        GameMain.Instance.CheckNotify();
        CreatureManager.Instance.SetSort();

        Tooltip.Instance.ShowMessage("Command Executed");
        AddCategories();
    }

    public void OnExecuteClient()
    {
        switch (m_Category)
        {
            case "Show":
                OnExecuteClientShow();
                break;

            case "Set":
                switch(m_Command)
                {
                    case "Quality":
                        ConfigData.Instance.GraphicsOption = m_Param;
                        Tooltip.Instance.ShowMessage("Command Executed");
                        break;
                }
                break;
        }
    }

    void OnExecuteClientShow()
    {
        switch(m_Command)
        {
            case "FPS":
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.fpsCounter.Enabled = !CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.fpsCounter.Enabled;
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.SwitchCounter();
                break;

            case "Memory":
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.memoryCounter.Enabled = !CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.memoryCounter.Enabled;
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.SwitchCounter();
                break;

            case "DeviceInfo":
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.deviceInfoCounter.Enabled = !CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.deviceInfoCounter.Enabled;
                CodeStage.AdvancedFPSCounter.AFPSCounter.Instance.SwitchCounter();
                break;
        }
    }
}
