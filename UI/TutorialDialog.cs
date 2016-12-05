using UnityEngine;
using System.Collections;

public class TutorialDialog : MonoBehaviour
{
    public UILabel m_LabelName;
    public UILabel m_LabelMessage;
    public UILabel m_LabelRightName;
    public UILabel m_LabelRightMessage;
    public GameObject m_Left;
    public GameObject m_Right;

    public UI2DSprite m_LeftCharacter;
    public UI2DSprite m_RightCharacter;

    public UICharacterCutSceneContainer m_LeftCutSceneCharacter;
    public UICharacterCutSceneContainer m_RightCutSceneCharacter;

    public TypewriterEffect m_LeftEffect;
    public TypewriterEffect m_RightEffect;
    public void Init(TargetInfo info)
    {
        Init(info.creature_id, info.animation, info.is_shadow, info.Name, info.Desc, info.position);
    }

    void Start()
    {
        m_LeftEffect.ResetToBeginning();
        m_RightEffect.ResetToBeginning();
    }

    public void Init(string character, string animation, bool is_shadow, string name, string msg, string position)
    {
        gameObject.SetActive(true);
        switch (position)
        {
            case "left":
                {
                    m_Left.SetActive(true);
                    m_Right.SetActive(false);
                    m_LabelName.text = name;
                    m_LabelMessage.text = msg;
                    if (AssetManager.ContainsCharacterCutSceneData(string.Format("{0}_cutscene", character)) == false)
                    {
                        m_LeftCutSceneCharacter.gameObject.SetActive(false);
                        m_LeftCharacter.material.mainTexture = AssetManager.LoadBG(string.Format("cutscene_{0}_{1}", character, animation));
                        m_LeftCharacter.gameObject.SetActive(true);
                    }
                    else
                    {
                        m_LeftCharacter.gameObject.SetActive(false);
                        m_LeftCutSceneCharacter.Init(AssetManager.GetCharacterCutSceneAsset(string.Format("{0}_cutscene", character)), animation, is_shadow);
                        m_LeftCutSceneCharacter.gameObject.SetActive(true);
                    }
                }
                break;

            case "right":
                {
                    m_Left.SetActive(false);
                    m_Right.SetActive(true);
                    m_LabelRightName.text = name;
                    m_LabelRightMessage.text = msg;
                    if (AssetManager.ContainsCharacterCutSceneData(string.Format("{0}_cutscene", character)) == false)
                    {
                        m_RightCutSceneCharacter.gameObject.SetActive(false);
                        m_RightCharacter.material.mainTexture = AssetManager.LoadBG(string.Format("cutscene_{0}_{1}", character, animation));
                        m_RightCharacter.gameObject.SetActive(true);
                    }
                    else
                    {
                        m_RightCharacter.gameObject.SetActive(false);
                        m_RightCutSceneCharacter.Init(AssetManager.GetCharacterCutSceneAsset(string.Format("{0}_cutscene", character)), animation, is_shadow);
                        m_RightCutSceneCharacter.gameObject.SetActive(true);
                    }
                }
                break;
        }
    }
}
