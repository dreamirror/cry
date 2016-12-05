using UnityEngine;
using System.Collections;
using LinqTools;

public class HottimeEventIconContainer : MonoBehaviour
{
    public PrefabManager EventIconPrefab;

    void Start()
    {

    }

    void OnEnable()
    {
        UpdateHottime();
    }

    void OnDisable()
    {
        EventIconPrefab.Clear();
    }
    private void UpdateHottime()
    {
        EventIconPrefab.Clear();
        foreach (var event_info in EventHottimeManager.Instance.Events.Where(e => e.OnGoing))
        {
            if (event_info.end_date < Network.Instance.ServerTime) continue;
            var item = EventIconPrefab.GetNewObject<HottimeEventIcon>(EventIconPrefab.transform, Vector3.zero);
            item.Init(event_info, OnItemDisable);
        }

        EventIconPrefab.GetComponent<UIGrid>().Reposition();
    }

    void OnItemDisable()
    {
        EventIconPrefab.GetComponent<UIGrid>().Reposition();
    }

    public void Clear()
    {
        EventIconPrefab.Clear();
    }
}
