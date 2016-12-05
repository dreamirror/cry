using UnityEngine;
using System.Collections;

public class PopupDifficultyItem : MonoBehaviour
{
    public UILabel m_Title;
    public UISprite m_SpriteImage, m_SpriteImageBack;

    System.Action<string> _OnClick;

    public void OnClick()
    {
        if (_OnClick != null)
            _OnClick(this.name);
    }

    public void Init(string name, string title, string image_name, string image_back_name, System.Action<string> clickCallback)
    {
        this.name = name;

        _OnClick = clickCallback;

        m_Title.text = title;
        m_SpriteImage.spriteName = image_name;
        m_SpriteImageBack.spriteName = image_back_name;
    }
}
