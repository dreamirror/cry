using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
//[AddComponentMenu("Physics 2D/Box Collider 2D")]
public class UIDisableButton : UIButton{
    static Dictionary<string, ManualLock> Locks = new Dictionary<string, ManualLock>();
    public static void Unlock(string manual_reset_id)
    {
        ManualLock lock_value;
        if (Locks.TryGetValue(manual_reset_id, out lock_value) == true)
        {
            lock_value.is_lock = false;
            Locks.Remove(manual_reset_id);
        }
    }

    class ManualLock
    {
        public bool is_lock;
    }

    public float disableDuration = 0.5f;
    public bool manual_reset = false;
    public string manual_reset_id;
    ManualLock manual_lock = null;

    float m_DisableTime = 0f;

    public void ResetDisable()
    {
        m_DisableTime = 0f;
        if (manual_lock != null)
            manual_lock.is_lock = false;
    }

    protected override void OnClick()
    {
        if (manual_reset == true && manual_lock != null && manual_lock.is_lock == true || disableDuration > 0f && m_DisableTime > Time.realtimeSinceStartup)
        {
            Debug.LogFormat("disabled button clicked at {0}", m_DisableTime);
            return;
        }

        if (manual_reset == true && string.IsNullOrEmpty(manual_reset_id) == false)
        {
            if (manual_lock == null)
                manual_lock = new ManualLock();
            manual_lock.is_lock = true;
            Locks.Add(manual_reset_id, manual_lock);
        }
        else if (disableDuration > 0f)
            m_DisableTime = Time.realtimeSinceStartup + disableDuration;

        base.OnClick();
    }
}
