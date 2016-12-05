using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PrefabPoolManager : MonoBehaviour {

    static PrefabPoolManager m_Instance;
    public static PrefabPoolManager Instance
    {
        get
        {
            if (m_Instance == null)
            {
                GameObject obj = new GameObject("PrefabPoolManager");
                m_Instance = obj.AddComponent<PrefabPoolManager>();
                GameObject.DontDestroyOnLoad(obj);
            }

            return m_Instance;
        }
    }

    Dictionary<GameObject, PrefabPool> m_Pool = new Dictionary<GameObject, PrefabPool>();
    static public System.Action OnChanged = null;
    public int Count { get { return m_Pool.Count; } }
    public IEnumerable<PrefabPool> Pool { get { return m_Pool.Values; } }

    void OnPoolChanged()
    {
        if (OnChanged != null)
            OnChanged();
    }

//     void OnDestroy()
//     {
//         foreach (var pool in m_Pool.Values)
//         {
//             pool.DestroyAll();
//             GameObject.Destroy(pool.gameObject);
//         }
//         m_Pool.Clear();
//     }

    public PrefabPool GetPool(GameObject prefab, string name)
    {
        if (prefab == null)
        {
            Debug.LogWarningFormat("prefab should not be null : {0}", name);
            return null;
        }

        PrefabPool pool = null;
        if (m_Pool.TryGetValue(prefab, out pool) == false)
        {
            GameObject obj = new GameObject(prefab.name);
            obj.transform.SetParent(transform, false);
            pool = obj.AddComponent<PrefabPool>();

            pool.Init(prefab, OnPoolChanged);
            m_Pool.Add(prefab, pool);
        }
        return pool;
    }
}
