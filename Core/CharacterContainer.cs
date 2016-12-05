using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterContainer : MonoBehaviour
{
    Character m_Character = null;
    public Character Character
    {
        get
        {
            if (CharacterAsset != null)
                return CharacterAsset.Asset;
            if (m_Character == null)
                m_Character = transform.GetComponentInChildren<Character>();
            return m_Character;
        }
    }

    public Vector3 TargetPositionDelta = Vector3.zero;
    public bool IsActive { get { return CharacterAsset != null && CharacterAsset.Asset != null; } }

    public GameObject Info;

    public CharacterAssetContainer CharacterAsset { get; private set; }
    public bool IsInit { get { return CharacterAsset != null; } }

    public void Init(CharacterAssetContainer character)
    {
        if (IsInit == true)
            Uninit();

        CharacterAsset = character;
        //m_PlaybackTime = 0f;

        SetActive(false);
        if (gameObject.activeInHierarchy == true)
            SetActive(true);
    }

    public void Uninit()
    {
        SetActive(false);
        if (CharacterAsset != null && CharacterAsset.Asset != null && CharacterAsset.Asset.Creature != null)
            CharacterAsset.Asset.Creature.Container = null;
        CharacterAsset = null;
    }

    void SetActive(bool isActive)
    {
        if (CharacterAsset == null)
            return;

        if (isActive == true)
        {
            Character character = CharacterAsset.Alloc();
            CoreUtility.SetRecursiveLayer(character.gameObject, "Character");
            character.transform.SetParent(transform, false);
            character.transform.localPosition = Vector3.zero;
            character.IsPause = false;

            character.CharacterAnimation.SetBattleMode();
            character.SetUIMode(false);
            character.CharacterAnimation.SetRenderQueue(2999);
        }
        else if (CharacterAsset != null && CharacterAsset.Asset != null)
        {
            //m_PlaybackTime = Character.PlaybackTime;
            Character.Reset();
            CharacterAsset.Asset.Creature = null;
            CharacterAsset.Free();
        }
            
    }

    //float m_PlaybackTime = 0f;

    void OnEnable()
    {
        SetActive(true);
    }

    void OnDisable()
    {
        SetActive(false);
    }

    void OnDestroy()
    {
        SetActive(false);
    }

    public void Batch(Vector3 pos)
    {
        transform.localPosition = pos;
        if (Info != null)
        {
            Vector3 info_local_pos = Info.transform.localPosition;
            info_local_pos.x = pos.x;
            Info.transform.localPosition = info_local_pos;
        }
        Character character = Character;
        if (character != null)
            character.transform.localPosition = Vector3.zero;
    }
}
