using PacketInfo;
using UnityEngine;

public class PopupMoveStore : PopupBase
{
    public UILabel m_title;
    public UILabel m_message;


    short notEnoughType = (short)pe_GoodsType.invalid;
    override public void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        notEnoughType = (short)(pe_GoodsType)parms[0];
        Init();
    }

    void Enable()
    {
    }

    void Init()
    {
        m_message.text = string.Format("{0}\n{1}",Localization.Get(string.Format("NotEnough{0}", (pe_GoodsType)notEnoughType)),
            Localization.Get("MoveStoreConfirm")
            );
    }

    public void OnCancel()
    {
        parent.Close();
    }

    public void OnClickOK()
    {
        parent.Close(true, true);
        switch((pe_GoodsType)notEnoughType)
        {
            case pe_GoodsType.token_mileage:
            case pe_GoodsType.token_gem:
                GameMain.MoveStore("Gem");
                break;
            case pe_GoodsType.token_gold:
                GameMain.MoveStore("Gold");
                break;
            case pe_GoodsType.token_energy:
                GameMain.MoveStore("Energy");
                break;
            default:
                break;
        }
    }
}
