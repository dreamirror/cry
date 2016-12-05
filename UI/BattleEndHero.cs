using UnityEngine;
using System.Collections;

public class BattleEndCreature
{
    public Creature Creature { get; private set; }
    public short Level { get; private set; }
    public short Enchant { get; private set; }
    public int Exp { get; private set; }

    public int AddExp { get; set; }
    public bool IsLevelUp {get; set;}

    public BattleEndCreature() { }
    public BattleEndCreature(Creature creature)
    {
        this.Creature = creature;
        Level = creature.Level;
        Enchant = creature.Enchant;
        Exp = creature.Exp;
    }
}

public class BattleEndHero : MonoBehaviour {

    public GameObject m_DungeonHeroPrefab;
    public GameObject m_HeroIndicator;
    public UIProgressBar m_ProgressExp;
    public UILabel m_LabelExp;
    public UILabel m_LabelLevelUP;

    DungeonHero m_DungeonHero = null;

    BattleEndCreature m_Creature;
    float m_Delay = 1f;
    float m_InitTime = 0f;
    int m_TotalProgressValue;
    int m_ProgressCurrentValue;
    int m_ProgressFinalValue;
    // Update is called once per frame
    void Update ()
    {
        if(Time.time - m_InitTime > m_Delay && m_ProgressFinalValue != m_ProgressCurrentValue)
        {
            int delta = m_TotalProgressValue/20;
            if (delta < 1) delta = 1;
            if (m_ProgressFinalValue > m_ProgressCurrentValue)
            {
                m_ProgressCurrentValue = System.Math.Min(m_ProgressFinalValue, m_ProgressCurrentValue + delta);
            }
            else if (m_ProgressCurrentValue == m_ProgressExp.numberOfSteps)
            {
                m_ProgressCurrentValue = 0;
                m_DungeonHero.m_level.text = m_Creature.Creature.GetLevelText();
                m_ProgressExp.numberOfSteps = LevelInfoManager.Instance.GetCharacterExpMax(m_Creature.Creature.Level);
                m_LabelLevelUP.gameObject.SetActive(true);
            }
            else
                m_ProgressCurrentValue = System.Math.Min(m_ProgressExp.numberOfSteps, m_ProgressCurrentValue + delta);

            m_ProgressExp.value = (float)m_ProgressCurrentValue / m_ProgressExp.numberOfSteps;
        }
    }

    public void Init(BattleEndCreature creature)
    {
        m_Creature = creature;
        m_InitTime = Time.time;
        if(m_DungeonHero == null)
            m_DungeonHero = NGUITools.AddChild(m_HeroIndicator, m_DungeonHeroPrefab).GetComponent<DungeonHero>();

        m_DungeonHero.Init(creature);

        m_TotalProgressValue = m_Creature.AddExp;
        m_ProgressExp.numberOfSteps = LevelInfoManager.Instance.GetCharacterExpMax(m_Creature.Level);
        m_ProgressCurrentValue = m_Creature.Exp;
        m_ProgressExp.value = (float)m_ProgressCurrentValue / m_ProgressExp.numberOfSteps;
        m_LabelLevelUP.gameObject.SetActive(false);

        m_ProgressFinalValue = m_Creature.Creature.Exp;
        m_LabelExp.text = Localization.Format("AddExp", m_Creature.AddExp);

        if(m_Creature.IsLevelUp && m_ProgressCurrentValue <= m_ProgressFinalValue)
        {
            m_ProgressCurrentValue = m_ProgressFinalValue + 1;
        }
        gameObject.SetActive(true);
    }
}
