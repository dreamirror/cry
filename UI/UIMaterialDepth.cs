using UnityEngine;
using System.Collections;
using HeroFX;

[AddComponentMenu("SmallHeroes/UI/UIMaterialDepth")]
public class UIMaterialDepth : MonoBehaviour
{
    public int depth = 0;

    void Start()
    {
        SetRenderQueue();
    }

    void OnEnable()
    {
        SetRenderQueue();
    }

    void SetRenderQueue()
    {
        UIPanel panel = CoreUtility.GetParentComponent<UIPanel>(transform);
        int renderQueue = panel.startingRenderQueue;
        gameObject.GetComponent<Renderer>().material.renderQueue = renderQueue + depth;
    }

    void LateUpdate()
    {
        SetRenderQueue();
    }
}
