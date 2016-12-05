using UnityEngine;
using System.Collections;

public class PopupMenuItem : MonoBehaviour
{
    public UILabel m_Title, m_Description;
    public UISprite m_SpriteImage, m_SpriteLabel;
    public UIDisableButton m_btnHelp;

    System.Action<string> _OnClick;
    string m_Help;

    public void OnClick()
    {
        if (_OnClick != null)
            _OnClick(this.name);
    }

    public void OnHelp()
    {
        Tooltip.Instance.ShowHelp(m_Title.text, m_Help);
        //Popup.Instance.ShowImmediately(ePopupMode.Help, m_Help);
    }

    public void Init(string name, string title, string description, string image_name, string help, System.Action<string> clickCallback)
    {
        this.name = name;
        m_Help = help;

        _OnClick = clickCallback;
        m_btnHelp.gameObject.SetActive(string.IsNullOrEmpty(help)==false);

        m_Title.text = title;
        if (string.IsNullOrEmpty(description) == false)
        {
            m_Description.text = description;
            m_SpriteLabel.gameObject.SetActive(true);
        }
        else
        {
            m_Description.text = "";
            m_SpriteLabel.gameObject.SetActive(true);
        }

        m_SpriteImage.spriteName = image_name;
    }
}
