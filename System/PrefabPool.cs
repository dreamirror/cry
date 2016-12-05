using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PrefabPool : MonoBehaviour
{
    GameObject Prefab;
    List<GameObject> m_Objs = new List<GameObject>();
    List<GameObject> m_FreeObjs = new List<GameObject>();

    public string Name { get { return Prefab.name; } }
    public int Count { get { return m_Objs.Count; } }
    public int FreeCount { get { return m_FreeObjs.Count; } }
    public System.Action OnChanged = null;

    public void Init(GameObject prefab, System.Action OnChanged)
    {
        this.Prefab = prefab;
        this.OnChanged = OnChanged;
    }

    public GameObject Alloc()
    {
        GameObject obj;

        if (m_FreeObjs.Count > 0)
        {
            obj = m_FreeObjs[0];
            m_FreeObjs.RemoveAt(0);
        }
        else
        {
            obj = (GameObject)GameObject.Instantiate(Prefab);
        }

        m_Objs.Add(obj);

        if (OnChanged != null) OnChanged();

        return obj;
    }

    public bool Free(GameObject obj)
    {
        if (m_FreeObjs.Contains(obj))
        {
            Debug.LogError("Already free");
            return false;
        }

        m_FreeObjs.Add(obj);
        m_Objs.Remove(obj);
        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (OnChanged != null) OnChanged();

        return true;
    }

    public void Free(List<GameObject> objs)
    {
        objs.ForEach(o => { o.SetActive(false); o.transform.SetParent(transform, false); m_Objs.Remove(o); });
        m_FreeObjs.AddRange(objs);

        if (OnChanged != null) OnChanged();
    }

    public void Destroy(List<GameObject> objs)
    {
        objs.ForEach(o => { m_Objs.Remove(o); GameObject.DestroyImmediate(o); });
        if (OnChanged != null) OnChanged();
    }

    public void DestroyAll()
    {
        m_Objs.ForEach(o => GameObject.Destroy(o));
        m_FreeObjs.ForEach(o => GameObject.Destroy(o));
        m_Objs.Clear();
        m_FreeObjs.Clear();
    }

    public void OnQuit()
    {
        m_Objs.Clear();
        m_FreeObjs.Clear();
    }

    public GameObject LastObject { get { if (m_Objs.Count == 0) return null; return m_Objs[m_Objs.Count - 1]; } }
}

