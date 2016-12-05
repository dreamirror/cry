using UnityEngine;
using System.Collections;

public class PopupNations : MonoBehaviour {

    public UILabel NationLabel;
    public UISprite NationFlag;
    public UISprite ActiveIcon;
    public UIToggle Toggle;

    string nation_name;
    bool is_implement = true;

    public void Init(string label_name, string nation_name, bool is_active)
    {
        this.nation_name = nation_name;
        NationFlag.spriteName = "setup_" + nation_name.ToLower();
        NationLabel.text = label_name;
        Toggle.value = is_active;

        if (!nation_name.Equals("Korean"))
        {
            Toggle.enabled = false;
            is_implement = false;
        }
    }

    public void OnClickThisNation()
    {   
        if (ConfigData.Instance.Language.Equals(nation_name))
            return;

        if (!is_implement)
        {
            Tooltip.Instance.ShowMessage(Localization.Get("NotImplement"));
            return;
        }

        ConfigData.Instance.Language = nation_name;
        ActiveIcon.gameObject.SetActive(true);

        Tooltip.Instance.ShowMessage(NationLabel.text);
    }
}
