using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class BattleLayout : MonoBehaviour {
    public CharacterLayout m_Mine, m_Enemy;

    public bool TopFirst = true;
    public float center = 8f;
    public float horizontal = 5f;
    public float vertical = 6f;

    void Awake()
    {
        m_Mine.Init(this);
        m_Enemy.Init(this);
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void Batch()
    {
        m_Mine.transform.localPosition = new Vector3(-center, 0f, 0f);
        m_Enemy.transform.localPosition = new Vector3(center, 0f, 0f);

        m_Mine.Batch(this);
        m_Enemy.Batch(this);
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(BattleLayout), true)]
public class BattleLayoutInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Batch"))
        {
            ((BattleLayout)target).Batch();
        }
    }
}
#endif

