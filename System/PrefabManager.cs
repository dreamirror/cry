using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[AddComponentMenu("SmallHeroes/Utility/PrefabManager")]
public class PrefabManager : MonoBehaviour
{
    public GameObject Prefab;
    PrefabPool m_Pool;

    public int Count { get { return m_Pool.Count; } }
    public int FreeCount { get { return m_Pool.FreeCount; } }

    public GameObject LastObject { get { return m_Pool.LastObject; } }

    List<GameObject> m_UsingList = new List<GameObject>();

    void CheckPool()
    {
        if (m_Pool != null)
            return;

        m_Pool = PrefabPoolManager.Instance.GetPool(Prefab, name);
    }

    void Awake()
    {
        CheckPool();
    }

    void OnApplicationQuit()
    {
        m_Pool = null;
    }

    public bool Contains(GameObject obj)
    {
        return m_UsingList.Contains(obj);
    }

    public T GetNewObject<T>(Transform parent, Vector3 localPosition) where T : Component
    {
        CheckPool();

        GameObject obj = m_Pool.Alloc();

        if (parent == null)
            obj.transform.SetParent(m_Pool.transform, false);
        else
            obj.transform.SetParent(parent, false);
        obj.SetActive(true);

        obj.transform.localPosition = localPosition;
        obj.transform.localScale = Vector3.one;

        m_UsingList.Add(obj);

        return obj.GetComponent<T>();
    }

    public void Free(GameObject obj)
    {
        obj.SetActive(false);
        if (m_Pool.Free(obj) == true)
            m_UsingList.Remove(obj);
    }

    public void Free(GameObject obj, Transform parent)
    {
        if (m_Pool.Free(obj) == true)
        {
            obj.SetActive(false);
            obj.transform.parent = parent;
            m_UsingList.Remove(obj);
        }
    }

    public void Clear()
    {
        if (m_Pool == null) return;

        m_Pool.Free(m_UsingList);
        m_UsingList.ForEach(o => { o.SetActive(false); });
        m_UsingList.Clear();
    }

    public void Destroy()
    {
        if (m_Pool == null) return;

        m_UsingList.ForEach(o => { o.SetActive(false); });
        m_Pool.Destroy(m_UsingList);
        m_UsingList.Clear();
    }

    void OnDisable()
    {
        if (m_Pool == null) return;
        Clear();
    }

    protected void OnDestroy()
    {
        if (m_Pool == null) return;

        if (Prefab != null)
        {
            Debug.Log(string.Format("[PrefabManager:{0}] Destroy", Prefab.name));

            Clear();
        }
    }

    public void FreeDisabled()
    {
        m_Pool.Free(m_UsingList.Where(o => o.activeSelf == false).ToList());
        m_UsingList.RemoveAll(o => o.activeSelf == false);
    }
}
