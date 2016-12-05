using PacketInfo;
using System;
using System.Collections.Generic;

public class GameConfig : MNS.Singleton<GameConfig>
{
    Dictionary<string, pd_GameConfigValue> Values;

    public void Init(List<pd_GameConfigValue> values)
    {
        Values = new Dictionary<string, pd_GameConfigValue>();
        values.ForEach(v => Values.Add(v.id, v));
    }

    public void Update(List<pd_GameConfigValue> values)
    {
        foreach (var value in values)
        {
            Values.Remove(value.id);
            Values.Add(value.id, value);
        }
    }

    static public T Get<T>(string id)
    {
        pd_GameConfigValue config_value;
        if (Instance.Values.TryGetValue(id, out config_value) == true)
        {
            return (T)Convert.ChangeType(config_value.value, typeof(T));
        }
        else
            throw new System.Exception(string.Format("not found {0} in GameConfig", id));
    }
}
