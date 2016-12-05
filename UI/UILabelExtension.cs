using UnityEngine;
using System.Collections;

static public class UILabelExtension
{
    static public void ShowTag(this UILabel label)
    {
        string msg = label.GetUrlAtPosition(UICamera.lastWorldPosition);

        if (string.IsNullOrEmpty(msg) == true)
            return;

        Tooltip.Instance.ShowTooltip(eTooltipMode.TagCharacter, msg);
    }

}
