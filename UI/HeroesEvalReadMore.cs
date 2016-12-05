using UnityEngine;
using System.Collections;

public class HeroesEvalReadMore : MonoBehaviour {

    System.Action OnClickReadMoreCallback = null;

    public void Init(System.Action OnClickReadMoreCallback)
    {
        this.OnClickReadMoreCallback = OnClickReadMoreCallback;
    }

    public void OnClickReadMore()
    {
        if(OnClickReadMoreCallback != null)
            OnClickReadMoreCallback();
    }
}
