using UnityEngine;
using System.Collections;

public class RaidUIPlayer : MonoBehaviour {
    public UISprite player_bg, mine_bg, profile_sprite;
    public UILabel label_nickname, label_description, label_level, label_rank;

    public int Rank { get; set; }
    public bool IsPlayer { get; private set; }

    public UIPanel m_Panel;
    public TweenPosition m_MoveTween;
    public TweenScale m_ScaleTween;

    void OnDisable()
    {
        m_MoveTween.enabled = false;
        m_ScaleTween.enabled = false;
    }

    public void Init(bool is_init, bool is_player, bool use_player_bg, string profile_name, string nickname, int level, int rank, int damage, float damage_percent)
    {
        IsPlayer = is_player;
        Rank = 0;
        player_bg.gameObject.SetActive(!use_player_bg);
        mine_bg.gameObject.SetActive(use_player_bg);

        profile_sprite.spriteName = profile_name;
        label_nickname.text = nickname;
        label_level.text = level.ToString();
        SetRank(rank, damage, damage_percent, is_init);
    }

    readonly float move_gap = -72f;

    public void SetRank(int rank, int damage, float damage_percent, bool init = false)
    {
        label_description.text = Localization.Format("RaidPlayerDescription", damage, damage_percent);
        label_rank.text = rank.ToString();

        if (IsPlayer == true)
        {
            if (rank < Rank || Rank == 0)
            {
                m_ScaleTween.ResetToBeginning();
                m_ScaleTween.PlayForward();
            }
            Rank = rank;
            return;
        }

        Vector3 to_position = Vector3.zero;
        to_position.y = move_gap * rank;

        if (init == true)
        {
            transform.localPosition = to_position;
        }
        else if (rank != Rank)
        {
            if (rank < Rank || Rank == 0)
            {
                m_Panel.depth = 2;
                m_ScaleTween.ResetToBeginning();
                m_ScaleTween.PlayForward();
            }
            else
                m_Panel.depth = 1;
            if (Rank == 0)
                m_MoveTween.from.y = move_gap * 5.5f;
            else
                m_MoveTween.from = transform.localPosition;
            m_MoveTween.to = to_position;
            m_MoveTween.ResetToBeginning();
            m_MoveTween.PlayForward();
        }
        else
            m_Panel.depth = 1;

        Rank = rank;
    }
}
