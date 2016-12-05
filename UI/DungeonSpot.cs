using UnityEngine;
using System.Collections;


public class DungeonSpot : MonoBehaviour {

    public UIParticleContainer m_BossParticle;
    public UIToggleSprite m_ToggleAvailable;
    public GameObject m_Stars;
    public UIToggleSprite[] m_ToggleRating;

    public delegate void _OnClickStage(DungeonSpot spot);
    public _OnClickStage OnClickStage = null;
    public MapStageDifficulty StageInfo { get; private set; }

    public bool Availble { get { return m_ToggleAvailable.ActiveSprite; } }

    public void Init(MapStageDifficulty info, short rating)
    {
        StageInfo = info;
        bool bAvailable = MapClearDataManager.Instance.AvailableStage(StageInfo);
        if (bAvailable == false)
            rating = 0;

        if (bAvailable == false)
            m_ToggleAvailable.SetSpriteActive(bAvailable);
        else
        {
            if (StageInfo.Difficulty == PacketEnums.pe_Difficulty.Normal)
                m_ToggleAvailable.spriteName = "dungeon_in_in_on";
            else
                m_ToggleAvailable.spriteName = "dungeon_in_in_on_hard";
        }

        gameObject.GetComponent<BoxCollider2D>().enabled = bAvailable;

        if (rating <= 0)
            m_Stars.SetActive(false);
        else
        {
            m_Stars.SetActive(true);

            for (int i = 0; i < m_ToggleRating.Length; ++i)
            {
                m_ToggleRating[i].SetSpriteActive(rating > i);
            }
        }
        gameObject.SetActive(true);

        if (info.StageType == eStageType.Boss)
            m_BossParticle.Play();
        else
            m_BossParticle.Stop();
    }

    public void OnClick()
    {
        if (OnClickStage != null)
            OnClickStage(this);
    }
}
