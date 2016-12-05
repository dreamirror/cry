using UnityEngine;

public class TooltipHelp : TooltipBase
{
    public UIScrollView m_Scroll;
    public UILabel m_TextTitle;
    public UILabel m_Text;

    //////////////////////////////////////////////////////////////////////////////////////

    void Start()
    {
    }

    public override void Init(params object[] parms)
    {
        if (parms == null || parms.Length < 2)
        {
            throw new System.Exception("invalid params : TooltipHelp");
        }

        gameObject.SetActive(true);
        m_TextTitle.text = parms[0] as string;
        m_Text.text = parms[1] as string;

        m_Scroll.ResetPosition();
    }

    public void Close()
    {
        OnFinished();        
    }
}
