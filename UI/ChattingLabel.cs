using UnityEngine;
using System.Collections;
using System;

public class ChattingLabel : MonoBehaviour
{
    public UILabel text_label;
    public GameObject bg;

    DateTime m_last_written_at;

    public void Init()
    {
        gameObject.SetActive(false);
    }

    public bool CheckLabel()
    {
        if (gameObject.activeSelf && (DateTime.Now - m_last_written_at).TotalSeconds > GameConfig.Get<int>("keep_visiable_time"))
            gameObject.SetActive(false);

        if ((DateTime.Now - m_last_written_at).TotalMilliseconds < GameConfig.Get<int>("label_slide_time"))
            return false;
        
        return true;
    }

    public void SetLabel(Color color, string msg)
    {   
        gameObject.SetActive(true);
        text_label.color = color;
        text_label.text = msg;

        m_last_written_at = DateTime.Now;
    }

    
}