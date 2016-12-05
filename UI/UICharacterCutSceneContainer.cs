using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("SmallHeroes/UI/UICharacterCutSceneContainer")]
public class UICharacterCutSceneContainer : UIWidget
{
    override public eWidgetDepth DepthType { get { return eWidgetDepth.Character; } }

    GameObject m_Container = null;

    CharacterCutScene m_Character = null;
    public CharacterCutScene Character
    {
        get
        {
            if (CharacterAsset != null)
                return CharacterAsset.Asset;
            if (m_Character == null)
                m_Character = transform.GetComponentInChildren<CharacterCutScene>();
            return m_Character;
        }
    }

    public bool IsActive { get { return CharacterAsset != null && CharacterAsset.Asset != null; } }

    public AssetContainer<CharacterCutScene> CharacterAsset { get; private set; }
    public bool IsInit { get { return CharacterAsset != null; } }

    string m_ActionName;
    public bool IsShadow { get; private set; }

    public void Init(AssetContainer<CharacterCutScene> character, string action_name, bool is_shadow = false)
    {
        if (IsInit == true)
            Uninit();

        IsShadow = is_shadow;
        CharacterAsset = character;
        if (CharacterAsset.Component.ContainsAnimation(action_name) == false)
            action_name = "default";
        m_ActionName = action_name;
        m_PlaybackTime = 0f;

        SetActive(false);
        if (gameObject.activeInHierarchy == true)
            SetActive(true);
    }

    public void Uninit()
    {
        SetActive(false);
        CharacterAsset = null;
    }

    void SetActive(bool isActive)
    {
        if (CharacterAsset == null)
            return;

        if (isActive == true)
        {
            if (m_Container == null)
            {
                m_Container = new GameObject("_Container");
                Vector3 scale = Vector3.one * 360f;
                m_Container.transform.localScale = scale;
                m_Container.transform.SetParent(transform, false);
            }
            CharacterCutScene character = CharacterAsset.Alloc();
            character.transform.SetParent(m_Container.transform, false);
            character.IsPause = false;
            character.SetShadow(IsShadow);

            if (string.IsNullOrEmpty(m_ActionName) == false)
            {
                character.PlayAnimation(m_ActionName);
                Character.UpdatePlay(m_PlaybackTime);
            }

            LateUpdate();
            //character.CharacterAnimation.Play(m_Pause);
        }
        else if (CharacterAsset != null && CharacterAsset.Asset != null)
        {
            m_PlaybackTime = Character.PlaybackTime;
            Character.Reset();
            CharacterAsset.Free();
        }

    }

    float m_PlaybackTime = 0f;

    override protected void OnEnable()
    {
        SetActive(true);
        base.OnEnable();
    }

    override protected void OnDisable()
    {
        base.OnDisable();
        SetActive(false);
    }

    void OnDestroy()
    {
        SetActive(false);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Character == null)
            return;

        if (drawCall != null)
            Character.SetRenderQueue(drawCall.renderQueue);
    }
}
