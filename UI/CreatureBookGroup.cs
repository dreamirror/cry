using UnityEngine;
using System.Collections;

public class CreatureBookGroup : MonoBehaviour {

    public UILabel m_BookGroupLabel;
    public UIGrid m_CreatureGrid;
    public PrefabManager m_CreaturePrefabManager;

    [HideInInspector]
    public int grid_count = 0;

    public void Init(CreatureBookGroupInfo info)
    {   
        int available_count = 0;
        grid_count = 0;
        foreach (var creature in info.Books)
        {
            var item = m_CreaturePrefabManager.GetNewObject<CreatureBookItem>(m_CreatureGrid.transform, Vector3.zero);

            bool is_exist = CreatureBookManager.Instance.IsExistBook(creature.Creature.IDN);
            item.Init(creature.Creature, creature.Grade,!is_exist);
            item.SetNotifyIcon(CreatureBookManager.Instance.IsNotifyBook(creature.Creature.IDN));
            if(is_exist == true)
                available_count++;
            grid_count++;
        }

        if(grid_count % m_CreatureGrid.maxPerLine > 0)
            for (int i = 0; i < m_CreatureGrid.maxPerLine - (grid_count % m_CreatureGrid.maxPerLine); i ++)
            {
                var item = m_CreaturePrefabManager.GetNewObject<CreatureBookItem>(m_CreatureGrid.transform, Vector3.zero);
                item.GetComponent<UIDisableButton>().enabled = false;
                item.Init();
            }
        m_BookGroupLabel.text = string.Format("{0} ({1}/{2})", info.Name, available_count, info.Books.Count);

        m_CreatureGrid.Reposition();
    }
}
