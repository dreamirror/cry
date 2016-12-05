using UnityEngine;
using System.Collections;

public class CoreUtility
{
    static public string GetHierachy(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;

        return GetHierachy(transform.parent) + "/" + transform.name;
    }

    static public string GetHierachy(Transform transform, Transform root)
    {
        if (transform == null)
            return "";

        if (transform.parent == null)
            return "";

        if (transform.parent == root)
            return transform.name;

        string hierachy = GetHierachy(transform.parent, root);
        if (hierachy == "")
            return "";

        return hierachy + "/" + transform.name;
    }

    public static Vector3 WorldPositionToUIPosition(Vector3 pos)
    {
        Vector3 scale = UICamera.currentCamera.transform.lossyScale;
        pos.x /= scale.x;
        pos.y /= scale.y;
        pos.z /= scale.z;
        return pos;
    }

    public static void SetRecursiveLayer(GameObject obj, string layer_name)
    {
        int layer = LayerMask.NameToLayer(layer_name);
        SetRecursiveLayer(obj, layer);
    }

    public static void SetRecursiveLayer(GameObject obj, int layer)
    {
        obj.layer = layer;
        for (int i = 0; i < obj.transform.childCount; ++i)
            SetRecursiveLayer(obj.transform.GetChild(i).gameObject, layer);
    }

    public static T GetParentComponent<T>(Transform transform) where T : class
    {
        T comp = transform.GetComponent<T>();
        if (comp != null)
            return comp;

        if (transform.parent == null)
            return null;

        return GetParentComponent<T>(transform.parent);
    }

    public class ObjectSingleton<T> where T : class, new()
    {
        static T _Instance = null;

        static public T Instance
        {
            get
            {
                MakeInstance();
                return _Instance;
            }
        }

        static public void MakeInstance()
        {
            if (_Instance == null)
                _Instance = new T();
        }

        static public void ClearInstance()
        {
            if (_Instance != null)
                _Instance = null;
        }

        static public bool CheckInstance()
        {
            return _Instance != null;
        }
    }
}
