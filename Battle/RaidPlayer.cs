using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class RaidPlayer : MonoBehaviour {
	public RaidCharacter CharacterPrefab;
	List<RaidCharacter> m_Characters;

	public long AccountIdx { get; private set; }
	public string ProfileName { get; private set; }
	public string Nickname { get; private set; }
	public int Level { get; private set; }

	// dummy
	public int Deal = 0;

	public RaidUIPlayer UIPlayer { get; set; }

	// Use this for initialization
	public void Init(long account_idx, string profile_name, string nickname, int level)
	{
		AccountIdx = account_idx;
		ProfileName = profile_name;
		Nickname = nickname;
		Level = level;

		m_Characters = new List<RaidCharacter>();
		Vector3 position = Vector3.zero;
		for (int i = 0; i < 5; ++i)
		{
			var character = GameObject.Instantiate<RaidCharacter>(CharacterPrefab);
			m_Characters.Add(character);
			position.z = (i % 2) == 1 ? -10f : 10f;
			character.Init(position, i < 3);
			position.x -= 4.5f;
		}
		m_Characters[0].transform.SetParent(transform, false);
		m_Characters[2].transform.SetParent(transform, false);
		m_Characters[4].transform.SetParent(transform, false);
		m_Characters[1].transform.SetParent(transform, false);
		m_Characters[3].transform.SetParent(transform, false);
	}

	public void UpdatePlayer()
	{
		m_Characters.ForEach(c => c.UpdateCharacter());
	}
}
