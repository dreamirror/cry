using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class PopupHeroDetail : PopupBase {

    public UILabel m_DetailTitle;
    public UILabel m_Description;

    public UILabel m_DetailStat;
    public UILabel m_RuneStat;

    public UILabel m_TagLabel;

    Creature m_Creature = null;
    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);
        m_Creature = parms[0] as Creature;
        Init();
    }

    void Init()
    {
        m_DetailTitle.text = string.Format("{0} {1}", m_Creature.GetLevelText(), m_Creature.Info.Name);
        if (m_Creature.Enchant > 0)
            m_DetailTitle.text += " " + m_Creature.GetEnchantText();

        m_Description.text = m_Creature.Info.Desc;
        m_Description.gameObject.GetComponent<BoxCollider2D>().size = m_Description.printedSize;
        m_Description.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0, -m_Description.printedSize.y / 2);
        
        m_DetailStat.text = GetDetailString();
        m_RuneStat.text = GetDetailRuneInfo();

        m_TagLabel.text = string.Empty;
        m_TagLabel.text = "[url=][/url]";
        var tags = m_Creature.Info.CreatureTags;
        foreach (string tag in tags)
            m_TagLabel.text += string.Format("[url={0}]{1}[/url] ", "Tag_" + tag, Localization.Get("Tag_" + tag));
    }

    string GetDetailString()
    {
        Dictionary<eStatType, float> stat_dic = new Dictionary<eStatType, float>();

        foreach (Rune rune in m_Creature.Runes)
        {
            foreach (SkillInfo.Action action in rune.Info.Skill.Actions)
            {
                if (stat_dic.ContainsKey(action.statType) == false)
                    stat_dic.Add(action.statType, action.increasePerLevel * rune.Level);
                else
                    stat_dic[action.statType] += action.increasePerLevel * rune.Level;
            }
        }

        var stat_string = "";
        foreach (eStatType type in Enum.GetValues(typeof(eStatType)))
        {
            if ((int)type >= 100)
                continue;

            if (StatInfo.IsDefaultValue(m_Creature.Info.AttackType, type) == false)
                continue;

            int value = m_Creature.StatTotal.GetValue(type);
            if (value == 0) continue;

            if (string.IsNullOrEmpty(stat_string) == false)
                stat_string += "\n";

            if (StatInfo.IsPercentValue(type) == true)
            {
                if (stat_dic.ContainsKey(type) == true)
                {
                    stat_string += string.Format("[c][F6D79F]{0}[-][/c] [c][E8AD05]{1}%[-][/c] [c][00B050](+{2}%)[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value / 100f, stat_dic[type]);
                    stat_dic.Remove(type);
                }
                else
                    stat_string += string.Format("[c][F6D79F]{0}[-][/c] [c][E8AD05]{1}%[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value / 100f);
            }
            else
            {
                if (stat_dic.ContainsKey(type) == true)
                {
                    stat_string += string.Format("[c][F6D79F]{0}[-][/c] [c][E8AD05]{1}[-][/c] [c][00B050](+{2})[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value, stat_dic[type]);
                    stat_dic.Remove(type);
                }
                else
                    stat_string += string.Format("[c][F6D79F]{0}[-][/c] [c][E8AD05]{1}[-][/c]", Localization.Get(string.Format("StatType_{0}", type)), value);
            }
        }

        stat_string += "\n";

        foreach(KeyValuePair<eStatType, float> values in stat_dic)
        {
            if (StatInfo.IsPercentValue(values.Key) == true)
                stat_string += string.Format("[c][00B050]{0} +{1}%[-][/c]\n", Localization.Get(string.Format("StatType_{0}", values.Key)), values.Value);
            else
                stat_string += string.Format("[c][00B050]{0} +{2}%[-][/c]\n", Localization.Get(string.Format("StatType_{0}", values.Key)), values.Value);
        }
        

        return stat_string;
    }

    string GetDetailRuneInfo()
    {
        string rune_string = string.Empty;

        int count = 0;
        foreach (Rune rune in m_Creature.Runes)
        {
            string rune_info = string.Empty;
            foreach (SkillInfo.Action action in rune.Info.Skill.Actions)
            {   
                if(string.IsNullOrEmpty(rune_info) == true)
                    rune_info += string.Format("[c][F6D79F]{0}. {1}[-][/c] [c][00B050]+{2}[-][/c]", ++count, Localization.Get(string.Format("StatType_{0}", action.statType)), action.increasePerLevel * rune.Level);
                else
                    rune_info += string.Format("   \n[c][F6D79F]{0}[-][/c] [c][00B050]+{1}[-][/c]", Localization.Get(string.Format("StatType_{0}", action.statType)), action.increasePerLevel * rune.Level);
            }

            rune_string += rune_info + "\n";
        }
        return rune_string;
    }


    public override void OnClose()
    {
        base.OnClose();
    }
}
