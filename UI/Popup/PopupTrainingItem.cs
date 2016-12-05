using UnityEngine;
using System.Collections;

public class PopupTrainingItem : MonoBehaviour {

    public UILabel m_labelTitle;
    public UIToggle m_Toggle;
    public UISprite m_SpriteLock;

    System.Action<MapStageDifficulty> callback;

    MapStageDifficulty m_StageInfo;

    public bool IsLock { get { return m_SpriteLock.gameObject.activeSelf; } }

    public void Init(MapStageDifficulty stage_info, System.Action<MapStageDifficulty> callback)
    {
        m_StageInfo = stage_info;
        m_labelTitle.text = stage_info.Name;
        m_Toggle.value = false;
        m_Toggle.Set(false);
        this.callback = callback;

        m_SpriteLock.gameObject.SetActive(stage_info.Condition != null && stage_info.Condition.CheckCondition() != null);
    }

    public void Select()
    {
        m_Toggle.value = true;
    }

    public void OnValueChanged()
    {
        if (m_Toggle.value == true)
            this.callback(m_StageInfo);
    }
}
