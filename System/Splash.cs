using UnityEngine;
using System.Collections;

public class Splash : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Localization.language = ConfigData.Instance.Language;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            PushManager.Instance.Init();

        StartCoroutine(PlayVideo());
    }

    bool load_scene = false;
    IEnumerator PlayVideo()
    {
        yield return new WaitForEndOfFrame();
        Handheld.PlayFullScreenMovie("MonsterSmile_CI.mp4", Color.white, FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.AspectFit);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        load_scene = true;
    }

    // Update is called once per frame
    void Update ()
    {
        if (load_scene)
            UnityEngine.SceneManagement.SceneManager.LoadScene("ResourceDownload", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
