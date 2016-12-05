using UnityEngine;
using System.Collections;

public class TutorialIndicator : MonoBehaviour
{
    public enum IndigatorType
    {
        Touch,
        Drag
    }

    public UIParticleContainer m_ParticleContainerTouch;
    public UIParticleContainer m_ParticleContainerDrag;

    public void Init(IndigatorType type, bool rotate)
    {
        UIParticleContainer particle_container = null;
        if (type == IndigatorType.Touch)
        {
            particle_container = m_ParticleContainerTouch;
            m_ParticleContainerDrag.gameObject.SetActive(false);
            m_ParticleContainerTouch.gameObject.SetActive(true);
            m_ParticleContainerDrag.Stop();
            m_ParticleContainerTouch.Play();
        }
        else
        {
            particle_container = m_ParticleContainerDrag;
            m_ParticleContainerDrag.gameObject.SetActive(true);
            m_ParticleContainerTouch.gameObject.SetActive(false);
            m_ParticleContainerTouch.Stop();
            m_ParticleContainerDrag.Play();
        }

        if (rotate == true)
        {
            Quaternion rotation = particle_container.ParticleAsset.transform.localRotation;
            rotation.z = 180f;
            particle_container.ParticleAsset.transform.localRotation = rotation;
        }
    }
}
