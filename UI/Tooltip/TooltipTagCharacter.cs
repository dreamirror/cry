using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class TooltipTagCharacter : TooltipBase {

    public PrefabManager m_CreaturePrefabManager;

    public UILabel m_TagTitle;
    public UIGrid m_GridCharacter;

    public override void Init(params object[] parms)
    {
        gameObject.SetActive(true);

        string tag = parms[0].ToString();
        
        List<CreatureInfo> creatures = CreatureInfoManager.Instance.Values.Where(c => c.ContainsTag(tag.Split('_')[1])).ToList();

        m_TagTitle.text = Localization.Get(tag);

        foreach (var creature in creatures)
        {
            var item = m_CreaturePrefabManager.GetNewObject<DungeonHeroRecommend>(m_GridCharacter.transform, Vector3.zero);
            item.Init(creature);
        }

        m_GridCharacter.Reposition();
    }

    public void Close()
    {
        m_CreaturePrefabManager.Destroy();
        OnFinished();
    }
}
