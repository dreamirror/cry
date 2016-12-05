using UnityEngine;
using System.Collections;

public class PopupBossDetail : PopupBase {

    public UILabel TitleLabel;

    public UILabel DescTitleLabel;
    public UILabel DescLabel;

    public UIGrid SkillGrid;
    public PrefabManager SkillItemPrefabmMnager;

    public UIGrid Recommend;
    public PrefabManager RecommendPrefabManager;

    MapStageDifficulty m_StageInfo = null;

    public override void SetParams(bool is_new, object[] parms)
    {
        base.SetParams(is_new, parms);

        m_StageInfo = parms[0] as MapStageDifficulty;

        Init();
    }

    public override void OnClose()
    {
        base.OnClose();
    }

    void Init()
    {
        MapCreatureInfo map_creature = m_StageInfo.Waves[0].Creatures.Find(c => c.CreatureType == eMapCreatureType.Boss || c.CreatureType == eMapCreatureType.WorldBoss);
        TitleLabel.text = map_creature.CreatureInfo.Name;
        DescLabel.text = map_creature.CreatureInfo.Desc;
        DescLabel.gameObject.GetComponent<BoxCollider2D>().size = DescLabel.printedSize;
        DescLabel.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0, -DescLabel.printedSize.y / 2);

        foreach (SkillInfo skill in map_creature.CreatureInfo.Skills)
        {
            if (skill.Name.Equals("-"))
                continue;
            var item = SkillItemPrefabmMnager.GetNewObject<SkillMidItem>(SkillGrid.transform, Vector3.zero);
            item.Init(skill);
        }

        foreach (CreatureInfo hero in m_StageInfo.Recommends)
        {
            var item = RecommendPrefabManager.GetNewObject<DungeonHeroRecommend>(Recommend.transform, Vector3.zero);
            item.Init(hero);
        }


        SkillGrid.Reposition();

        Recommend.Reposition();

    }
}
