using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using HeroFX;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainLayout : MonoBehaviour
{
    public float horizontal = 0f;
    public HFX_ParticleSystem m_BoostFx { get; set; }

    public List<Creature> Creatures { get; private set; }

    public bool IsDragOut { get; private set; }
    UICharacterContainer m_DragContainer = null;
    public UICharacterContainer DragContainer
    {
        get { return m_DragContainer; }
        set
        {
            if (m_DragContainer != null)
            {
                m_DragContainer.IsDrag = false;
                DragContainer.Character.PlayAction("social");
            }
            m_DragContainer = value;
            if (m_DragContainer != null)
            {
                IsDragOut = false;
                m_DragContainer.IsDrag = true;

                if (m_BoostFx == null)
                {
                    m_BoostFx = GameObject.Instantiate<HFX_ParticleSystem>(AssetManager.GetParticleSystem("boost"));
                }
                m_BoostFx.transform.SetParent(m_DragContainer.Character.transform, false);
                m_BoostFx.gameObject.SetActive(true);
                m_BoostFx.Play(false, 0);
                Debug.LogFormat("Boost Play : {0}", m_DragContainer.Character.name);
            }
        }
    }

    public UICharacterContainer[] m_Characters;

    public bool IsBatching { get; private set; }

    void Start()
    {
    }

    public void Batch()
    {
        Vector3 pos = Vector3.zero;

        for (int i = 0; i < m_Characters.Length; ++i)
        {
            pos.x = GetPosition(i);
            m_Characters[i].Batch(pos);
        }
    }

    public void Batch(int index)
    {
        Vector3 pos = Vector3.zero;
        pos.x = GetPosition(index);
        m_Characters[index].Batch(pos);
    }

    public float GetPosition(int index)
    {
        return (index - m_Characters.Length / 2) * -horizontal;
    }

    public void Rebatch()
    {
        List<UICharacterContainer> list = m_Characters.Where(c => c.IsInit == true && c != DragContainer).ToList();
        list.AddRange(m_Characters.Where(c => c.IsInit == false || c == DragContainer));

        m_Characters = list.ToArray();
        IsBatching = true;
    }

    void Update()
    {
        if (IsBatching == true)
            UpdateBatch();
    }

    public void UpdateBatch()
    {
        Vector3 pos = Vector3.zero;
        float deltaTime = Time.deltaTime;
        IsBatching = false;
        float move_limit = 50f;

        for (int i = 0; i < m_Characters.Length; ++i)
        {
            if (m_Characters[i] == DragContainer)
                continue;

            pos.y = 0f;

            Vector3 local_pos = m_Characters[i].transform.localPosition;
            pos.x = GetPosition(i);
            pos.z = local_pos.z;

            if (local_pos.x != pos.x)
            {
                Vector3 move_vec = (pos - local_pos);
                float length = move_vec.magnitude;
                float move_length = Mathf.Clamp(length, -deltaTime * move_limit, deltaTime * move_limit);
                float move_length2 = deltaTime * length * 10f;
                move_length = Mathf.Max(move_length, move_length2);
                pos = local_pos + move_vec.normalized * move_length;
                m_Characters[i].Batch(pos);
                IsBatching = true;
            }
        }
        if (IsBatching == false && DragContainer == null)
        {
            if (m_BoostFx)
            {
                m_BoostFx.Finish();
                Debug.Log("Boost Finish");
            }

            for (int i = 0; i < m_Characters.Length; ++i)
            {
                Vector3 local_pos = m_Characters[i].transform.localPosition;
                local_pos.z = 0f;
                m_Characters[i].transform.localPosition = local_pos;
            }
            Creatures = m_Characters.Where(c => c.Character != null && c.Character.Creature != null).Select(c => c.Character.Creature as Creature).ToList();
        }
    }

    public void Init(TeamData team_data)
    {
        for (int i = 0; i < m_Characters.Length; i++)
        {
            if (team_data != null && i < team_data.Creatures.Count)
            {
                Creature creature = team_data.Creatures[i].creature; //得到角色的信息
                m_Characters[i].Init(AssetManager.GetCharacterAsset(creature.Info.ID, creature.SkinName), UICharacterContainer.Mode.UI_Normal); //初始化角色的容器
                if (m_Characters[i].Character != null)
                {
                    m_Characters[i].Character.Creature = creature;
                    m_Characters[i].CharacterAsset.Asset.name = string.Format("asset_{0}", creature.Info.ID);
                }
                m_Characters[i].gameObject.SetActive(true);
            }
            else
            {
                m_Characters[i].Uninit(); //析构掉角色的容器
            }
        }
        if(team_data != null)
            Creatures = team_data.Creatures.Select(c => c.creature).ToList();
        Batch();
    }

    public void Uninit()
    {
        for (int i = 0; i < m_Characters.Length; ++i)
            m_Characters[i].gameObject.SetActive(false);
    }

    public bool CheckDragIndex()
    {
        if (DragContainer == null)
            return false;

        bool drag_out = true;
        int active_count = m_Characters.Count(c => c.IsInit == true);
        int drag_index = active_count-1;

        Vector3 local_pos = DragContainer.transform.localPosition;

        if (local_pos.y < 8f)
        {
            drag_index = Mathf.Clamp(2 - Mathf.RoundToInt(local_pos.x / horizontal), 0, active_count - 1);
            drag_out = false;
        }

        if (m_Characters[drag_index] != DragContainer)
        {
            List<UICharacterContainer> list = m_Characters.Where(c => c.IsInit == true && c != DragContainer).ToList();
            list.Insert(drag_index, DragContainer);
            list.AddRange(m_Characters.Where(c => c.IsInit == false && c != DragContainer));

            if (drag_out == false)
            {
                UICharacterContainer container = m_Characters[drag_index];
                if (container != list[drag_index] && container.IsInit == true && container != DragContainer)
                    container.Character.PlayAction("damage");
            }
            m_Characters = list.ToArray();
        }

        return drag_out;
    }

    public Action _OnClick = null;
    public void ProcessClick() { if (_OnClick != null) _OnClick(); }

    public Action _OnPress = null;
    public void ProcessPress() { if (_OnPress != null) _OnPress(); }

    public Action _OnRelease = null;
    public void ProcessRelease() { if (_OnRelease != null) _OnRelease(); }

    public Action _OnDragOver = null;
    public void ProcessDragOver() { if (_OnDragOver != null) _OnDragOver(); }

    public Action _OnDragOut = null;
    public void ProcessDragOut() { if (_OnDragOut != null) _OnDragOut(); }

    public Action _OnDeepTouch = null;
    public void ProcessDeepTouch() { if (_OnDeepTouch != null) _OnDeepTouch(); }

    public void UpdateDrag()
    {
        if (DragContainer == null)
        {
            return;
        }

        float delta_time = Time.deltaTime;
        Vector3 pos = UICamera.lastWorldPosition;

        Vector3 move = pos - DragContainer.transform.position;
        pos.z = DragContainer.transform.parent.position.z - 2f;
        DragContainer.transform.position = pos;

        IsDragOut = CheckDragIndex();

        DragContainer.SetRotation(move.x / delta_time * 10f);

        //if (Mathf.Abs(move.y) > Mathf.Abs(move.x))
        //{
        //    float speed = move.y / delta_time;
        //    if (speed > 2f)
        //        IsDragOut = true;
        //}

        if (IsBatching == false)
            UpdateBatch();

    }

    public void Reposition(int target_index, int insert_index)
    {
        UICharacterContainer target = m_Characters[target_index];
        List<UICharacterContainer> list = m_Characters.ToList();
        list.Remove(target);
        list.Insert(insert_index, target);
        m_Characters = list.ToArray();
        IsBatching = true;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MainLayout), true)]
public class MainLayoutInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Batch"))
        {
            ((MainLayout)target).Batch();
        }
    }
}
#endif

