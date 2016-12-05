using UnityEngine;
using System;
using System.Collections;


public class ActionSkillInfo
{
    public string name;
    public int skillIdx;
    public ActionSkillInfo(string _name, int _skillIdx)
    {
        name = _name;
        skillIdx = _skillIdx;
    }
}
public class UISkillInfo : MonoBehaviour {

    public UISprite mSkillBG;
    public UISprite mSkillBorder;
    public UILabel mSkillName;

    public UIPlayTween mTWShow;
    public UIPlayTween mTWHide;

    float fShowTime = 1000f;
    DateTime mShowTime = DateTime.MinValue;

    SkillInfo mSkill;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if(mShowTime != DateTime.MinValue && DateTime.Now - mShowTime > TimeSpan.FromMilliseconds(fShowTime))
        {
            mShowTime = DateTime.MinValue;
            Hide();
        }
	}

    public void Init(SkillInfo skillInfo)
    {
        gameObject.SetActive(true);
        SetSkill(skillInfo);
        //bShow = false;
        //bInit = true;
    }

    public void Show()
    {
        mTWShow.Play(true);
        //bShow = true;
        mShowTime = DateTime.Now;
    }

    public void Hide()
    {
        mTWHide.Play(true);
        //bShow = false;
        //bInit = false;
    }

    void SetSkill(SkillInfo info)
    {
        if (mSkill == info) return;
        mSkill = info;
        mSkillName.text = info.Name;
    }

    void SetSkillName(string name)
    {
        mSkillName.text = name;
    }
}
