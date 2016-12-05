using UnityEngine;
using System.Collections;

public class AudioTest : MonoBehaviour {
    public AudioSource m_AudioSource;

	// Use this for initialization
	void Start () {
        StartCoroutine(AudioStart());
	}

    IEnumerator AudioStart()
    {
        Debug.Log("start coroutine");
        if (m_AudioSource != null)
        {
            Debug.Log("m_AudioSource != null");
            //m_AudioSource.clip = SHResources.Load<AudioClip>("_Intro");
            m_AudioSource.clip = AssetManager.GetSound("Intro");
            if (m_AudioSource.clip != null)
            {
                Debug.Log("start audio");
                m_AudioSource.Play();
                //endTime = System.DateTime.Now + System.TimeSpan.FromSeconds(playingTime);
            }
        }
        yield break;
    }
    //System.DateTime endTime = System.DateTime.MinValue;
    float playingTime = 2f;
	// Update is called once per frame
	void Update () {
        //if(m_AudioSource != null && m_AudioSource.clip != null)
        //{
        //    if (m_AudioSource.isPlaying == true && System.DateTime.Now > endTime)
        //    {
        //        m_AudioSource.Stop();
        //        m_AudioSource.clip = null;
        //        endTime = System.DateTime.MinValue;
        //    }
        //}
	}
}
