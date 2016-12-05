using UnityEngine;
using System.Collections;

public class PopupStuffReserveItem : MonoBehaviour
{

    public UISprite character;
    public UISprite character_type;
    public UISprite m_SpriteWeapon, m_SpriteArmor;
    public UIToggleSprite []m_ToggleWeaponStuff, m_ToggleArmorStuff;
    public UIToggle m_ToggleWeaponFull, m_ToggleArmorFull;
    public UIToggleSprite m_SpriteWeaponFull, m_SpriteArmorFull;
    public UIGrid gradeGrid;
    public GameObject[] stars;

    public UILabel m_LabelName, m_LabelLevel;
    public GameObject m_Notify;

    public Creature CreatureInfo { get; private set; }

    public bool bDragDropEnable;

    bool bActiveGrade = false;

    // Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (bActiveGrade == false)
        {
            gradeGrid.gameObject.SetActive(bActiveGrade = true);
            gradeGrid.Reposition();
        }

	}
    //---------------------------------------------------------------------------
    public void Init(Creature creature)
    {
        CreatureInfo = creature;

        character.spriteName = string.Format("cs_{0}", CreatureInfo.Info.ID);
        name = character.spriteName;
        character_type.spriteName = string.Format("hero_info_hero_{0}", CreatureInfo.Info.ShowAttackType);
        for (int i = 0; i < stars.Length; ++i)
            stars[i].SetActive(i < CreatureInfo.Grade);
        gradeGrid.gameObject.SetActive(bActiveGrade = false);

        m_LabelName.text = creature.Info.Name;
        m_LabelLevel.text = string.Format("Lv{0}", creature.Level);

        m_SpriteWeapon.spriteName = creature.Weapon.Info.ID;
        m_SpriteArmor.spriteName = creature.Armor.Info.ID;

        m_ToggleWeaponFull.value = creature.Weapon.EnchantLevel >= 5;
        m_ToggleArmorFull.value = creature.Armor.EnchantLevel >= 5;

        if (m_ToggleArmorFull.value == true)
            m_SpriteArmorFull.SetSpriteActive(creature.Weapon.AvailableUpgrade());
        else
        {
            for (int i = 0; i < 3; ++i)
            {
                m_ToggleArmorStuff[i].SetSpriteActive(creature.Armor.Stuffs[i].Count > 0);
            }
        }


        if (m_ToggleWeaponFull.value == true)
            m_SpriteWeaponFull.SetSpriteActive(creature.Weapon.AvailableUpgrade());
        else
        {
            for (int i = 0; i < 3; ++i)
            {
                m_ToggleWeaponStuff[i].SetSpriteActive(creature.Weapon.Stuffs[i].Count > 0);
            }
        }

        SetDragDrop(bDragDropEnable);
        m_Notify.SetActive(CreatureInfo.IsNotify);

        gameObject.SetActive(true);
    }
    //---------------------------------------------------------------------------
    public void SetCreature(Creature info)
    {
        CreatureInfo = info;
        //Init();
        //character.spriteName = string.Format("cs_{0}", info.Info.ID);
        //name = character.spriteName;
        //characterLine.spriteName = string.Format("cs_character_line_{0}", info.Info.AttackType);
        //for (int i = 0; i < stars.Length; ++i)
        //    stars[i].SetActive(i < info.Grade);
        //gradeGrid.gameObject.SetActive(bActiveGrade = false);

        //toggle.value = false;
        //SetDragDrop(bDragDropEnable);
        m_Notify.SetActive(CreatureInfo.IsNotify);
    }    
    
    //---------------------------------------------------------------------------
    public void SetDragDrop(bool enable)
    {
//         drag.enabled = bDragDropEnable = enable;
//         drag.cloneOnDrag = enable;
    }

    //---------------------------------------------------------------------------

    public void OnClick()
    {
        MenuParams menu = new MenuParams();
        menu.AddParam<Creature>(CreatureInfo);
        GameMain.Instance.ChangeMenu(GameMenu.HeroInfoDetail, menu);
    }
}
