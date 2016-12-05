using UnityEngine;
using System.Collections;
using PacketInfo;
using System.Collections.Generic;
using System.Linq;
using SharedData;
using System;

public class Quest
{
    public QuestInfo Info { get; private set; }
    public pd_QuestData Data { get; private set; }

    public bool IsIndexMatch
    {
        get
        {
            return Info.Type == eQuestType.Achievement || Info.Type == eQuestType.AchievementBase 
                || (Info.Type == eQuestType.Daily && Data.daily_index == Network.DailyIndex) 
                //|| (Info.Type == eQuestType.Weekly && Data.weekly_index == Network.WeeklyIndex)
                ;
        }
    }
    public long Progress
    {
        get
        {
            return IsIndexMatch == true ? Data.quest_progress : 0;
        }
    }

    public bool IsRewarded
    {
        get
        {
            return IsIndexMatch == true ? Data.rewarded : false;
        }
    }

    public bool IsComplete { get; private set; }
    public bool IsShow { get; private set; }

    public Quest(QuestInfo info, pd_QuestData data)
    {
        Info = info;
        if (data != null)
            Data = data;
        else
        {
            Data = new pd_QuestData();
            Data.quest_idn = Info.IDN;
            Data.daily_index = Network.DailyIndex;
            Data.weekly_index = Network.WeeklyIndex;
        }
        CheckComplete();
    }

    public void Update(long quest_progress)
    {
        Data.quest_progress = quest_progress;
        CheckComplete();
    }
    public void UpdateAndNotify(long quest_progress)
    {
        if (IsComplete || IsRewarded) return;
        Update(quest_progress);
        if(IsShow)
            ShowQuestCompleteTooltip();
    }

    private void ShowQuestCompleteTooltip()
    {
        if (IsComplete)
            Tooltip.Instance.ShowTooltip(eTooltipMode.MissionProgress, Localization.Format("MissionCompleted", Info.Title));
        else if (GameConfig.Get<bool>("show_quest_progress") == true || Info.ProgressShow == true)
            Tooltip.Instance.ShowTooltip(eTooltipMode.MissionProgress, Localization.Format("MissionProgress", Info.Title, Data.quest_progress, Info.Condition.ProgressMax));
    }

    public void Update(pd_QuestDataUpdate update)
    {
        Data.quest_progress = update.quest_progress;
        Data.daily_index = Network.DailyIndex;
        Data.weekly_index = Network.WeeklyIndex;
        CheckComplete();

        if (Info.Type == eQuestType.AchievementBase)
        {
            QuestManager.Instance.GetTriggerQuests(this).ForEach(q => {
                if (q.IsComplete)
                    q.Update(Data.quest_progress);
                else
                    q.UpdateAndNotify(Data.quest_progress);
            }
            );
            return;
        }

        ShowQuestCompleteTooltip();
    }

    public void CheckComplete()
    {
        if (IsRewarded == true)
        {
            IsShow = false;
            IsComplete = false;
            return;
        }

        switch (Info.Condition.ConditionType)
        {
            case eQuestCondition.time:
                {
                    QuestConditionTime condition_time = Info.Condition as QuestConditionTime;
                    TimeSpan tod = Network.Instance.ServerTime.TimeOfDay;

                    // 0시 걸친 경우
                    bool over_day = condition_time.time_begin > condition_time.time_end;

                    if (over_day)
                        IsComplete = condition_time.time_begin < tod || tod < condition_time.time_end;
                    else
                        IsComplete = condition_time.time_begin < tod && tod < condition_time.time_end;

                    // ToDo : IsShow for over day
                    IsShow = tod < condition_time.time_end && !QuestManager.Instance.Data.Any(d => d != this && d.Info.Condition.ConditionType == eQuestCondition.time && d.IsShow == true);
                }
                break;

            case eQuestCondition.progress:
                {
                    IsShow = true;
                    IsComplete = Progress >= Info.Condition.ProgressMax;
                    if (Info.PrevQuestInfo != null)
                    {
                        Quest quest = QuestManager.Instance.Data.Find(e => e.Info.IDN == Info.PrevQuestInfo.IDN);
                        if (quest != null && quest.IsRewarded == false)
                        {
                            IsShow = false;
                            IsComplete = false;
                        }
                    }
                }
                break;
        }
    }
}

public class QuestManager : MNS.Singleton<QuestManager>
{
    public bool IsNotify { get; set; }
    public bool IsUpdateNotify { get; set; }

    public List<Quest> Data { get; private set; }

    ////////////////////////////////////////////////////////////////
    public void Init(List<pd_QuestData> datas)
    {
        Data = new List<Quest>();
        foreach (var info in QuestInfoManager.Instance.Values)
        {
            var data = datas.Find(d => d.quest_idn == info.IDN);
            Data.Add(new Quest(info, data));
        }

        Data.Where(e => e.Info.FireTriggerType != eQuestTrigger.none).ToList().ForEach(q => UpdateAchivementProgress(q));
        SetUpdateNotify();
    }
    public List<Quest> GetTriggerQuests(Quest quest)
    {
        return Data.Where(e => e.Info.Trigger != null && e.Info.Type == eQuestType.Achievement && e.Info.Trigger.TriggerType == quest.Info.FireTriggerType).ToList();
    }
    void UpdateAchivementProgress(Quest quest)
    {
        List<Quest> trigger_quest = GetTriggerQuests(quest);
        trigger_quest.ForEach(q => q.Update(quest.Progress));
    }

    public void UpdateData(List<pd_QuestDataUpdate> updates)
    {
        foreach (var update in updates)
        {
            Quest quest = Data.Find(d => d.Info.IDN == update.quest_idn);
            if (quest == null)
                Debug.LogWarningFormat("can't find quest : {0}", update.quest_idn);
            else
            {
                quest.Update(update);
            }
        }
        //Data.Where(e => e.Info.FireTriggerType != eQuestTrigger.none).ToList().ForEach(q => UpdateAchivementProgress(q));
        SetUpdateNotify();
    }

    public void SetUpdateNotify()
    {
        IsUpdateNotify = true;
    }

    public void UpdateNotify()
    {
        if (IsUpdateNotify == false || Data == null)
            return;

        IsUpdateNotify = false;

        IsNotify = Data.Any(d => d.Info.Type != eQuestType.AchievementBase && d.IsComplete == true);
    }

    public void CheckComplete()
    {
        Data.ForEach(d => d.CheckComplete());
        SetUpdateNotify();
    }

    public Quest GetShowQuest()
    {
        List<Quest> candidates = Data.FindAll(e => e.IsShow && e.IsComplete == false && e.Info.Type == eQuestType.Daily && e.Info.Condition.ConditionType == eQuestCondition.progress);
        if (candidates != null && candidates.Count > 0)
            return candidates[MNS.Random.Instance.Next() % candidates.Count];

        candidates = Data.FindAll(e => e.IsShow && e.Info.Type == eQuestType.Achievement && e.Info.Condition.ProgressMax != 1 && e.Info.Condition.ProgressMax - e.Progress == 1).ToList();
        if (candidates != null && candidates.Count > 0)
            return candidates[MNS.Random.Instance.Next() % candidates.Count];

        candidates = Data.FindAll(e => e.IsShow && e.Info.Type == eQuestType.Achievement && e.Info.Condition.ProgressMax == 1 && e.Info.Condition.ProgressMax - e.Progress == 1).ToList();
        if (candidates != null && candidates.Count > 0)
            return candidates[MNS.Random.Instance.Next() % candidates.Count];

        candidates = Data.FindAll(e => e.IsShow && e.IsComplete == false && e.Info.Type == eQuestType.Achievement).OrderByDescending(q => (float)q.Progress / q.Info.Condition.ProgressMax).ToList();
        if (candidates != null && candidates.Count > 0)
            return candidates[0];
        return null;
    }
}
