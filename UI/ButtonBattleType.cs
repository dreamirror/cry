using UnityEngine;
using System.Collections;

public delegate void OnToggleBattleTypeDelegate(int idx, bool bActive, bool initialized);

public class ButtonBattleType : MonoBehaviour {
    public UIToggle toggle;
    public UILabel label_active, label_deactive;

    public OnToggleBattleTypeDelegate OnToggleBattleType = null;
    bool initialized = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	    if(bDisable == true)
        {
            Disable();
        }
	}

    public void SetText(string text)
    {
        label_active.text = text;
        label_deactive.text = text;
    }

    public void SetStartingState()
    {
        toggle.startsActive = true;
    }

    public void OnValueChanged()
    {
        if(OnToggleBattleType != null)
        {
            OnToggleBattleType(int.Parse(this.name), toggle.value, initialized);
        }
        initialized = true;
    }

    bool bDisable = false;
    public void Disable()
    {
        if(bDisable == false)
        {
            bDisable = true;
            return;
        }
        bDisable = false;
        //gameObject.GetComponent<BoxCollider>().enabled = false;
        //GameObject[] objs= GetComponentsInChildren<GameObject>();
        //foreach(GameObject obj in  objs)
        //{
        //    foreach (UIWidget widget in obj.GetComponentsInChildren<UIWidget>(true))
        //    {
        //        widget.color = Color.grey;
        //    }
        //}
    }
}
