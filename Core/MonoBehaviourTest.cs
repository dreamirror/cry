#define PRINT_UPDATE

using UnityEngine;
using System.Collections;


public class MonoBehaviourTest : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Debug.LogFormat("{0} : Start", gameObject.name);
	}
	
	// Update is called once per frame
	void Update ()
    {
#if PRINT_UPDATE
        Debug.LogFormat("{0} : Update", gameObject.name);
#endif
    }

    void OnEnable()
    {
        Debug.LogFormat("{0} : OnEnable", gameObject.name);
    }

    void OnDisable()
    {
        Debug.LogFormat("{0} : OnDisable", gameObject.name);
    }

    void OnDestroy()
    {
        Debug.LogFormat("{0} : OnDestroy", gameObject.name);
    }
}
