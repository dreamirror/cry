using UnityEngine;
using System.Collections;
using HeroFX;
using System.Collections.Generic;

public class CutSceneCamera : MonoBehaviour
{
    List<ICreature> creatures = new List<ICreature>();

    public HFX_ParticleSystem particle;
    HFX_ParticleSystem m_Particle;

    public void AddCreature(ICreature creature)
    {
        if (creature.IsTeam == false)
            return;

        creatures.Add(creature);

        if (gameObject.activeInHierarchy == false)
        {
            gameObject.SetActive(true);
            PlayParticle();
        }
    }

    void PlayParticle()
    {
        if (particle != null)
        {
            if (m_Particle == null)
            {
                m_Particle = GameObject.Instantiate<HFX_ParticleSystem>(particle);
                m_Particle.SetLightingMax(1f);
                m_Particle.transform.SetParent(transform, false);
            }
            m_Particle.Play(false, 0);
        }
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void RemoveCreature(ICreature creature)
    {
        creatures.Remove(creature);
        if (creatures.Count == 0)
        {
            gameObject.SetActive(false);
        }
    }
}
