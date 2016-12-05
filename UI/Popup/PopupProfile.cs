using UnityEngine;
using System.Collections;

public class PopupProfile : PopupBase
{
    public UISprite m_SpriteCharacter;
    public UILabel m_LabelNickname;
    public UILabel m_LabelLevel;
    public UILabel m_LabelExp;

    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_LabelLevel.text = Localization.Format("Level", Network.PlayerInfo.player_level);
        m_LabelExp.text = Localization.Format("ExpValue", Network.PlayerInfo.player_exp, LevelInfoManager.Instance.GetPlayerExpMax(Network.PlayerInfo.player_level));

        UpdateProfile();
    }

    void UpdateProfile()
    {
        m_SpriteCharacter.spriteName = Network.PlayerInfo.leader_creature.GetProfileName();
        m_LabelNickname.text = Network.PlayerInfo.nickname;
    }
    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnClickChangeLeader()
    {
        //Tooltip.Instance.ShowMessageKey("NotImplement");
        Tooltip.Instance.ShowLeaderCharacter(new EventDelegate(OnLeaderCharacter));
    }
    public void OnClickChangeNickname()
    {
        Popup.Instance.Show(ePopupMode.Nickname);
    }

    void OnLeaderCharacter()
    {
        UpdateProfile();
    }
    public void OnClickHelp()
    {
        Tooltip.Instance.ShowHelp(Localization.Get("Help_Profile_Title"), Localization.Get("Help_Profile"));
    }
}
