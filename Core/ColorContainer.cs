using UnityEngine;
using System.Collections;
using System.Linq;
using System;

[System.Serializable]
public class ColorData
{
    public string name;
    public Color32 color;

    public ColorData Clone()
    {
        var new_data = new ColorData();
        new_data.name = name + " (Clone)";
        new_data.color = color;
        return new_data;
    }
}

public class ColorContainer : MonoBehaviour {
    public ColorData[] Colors;

    public int GetSelectedIndex(string name)
    {
        return Math.Max(0, Array.FindIndex(Colors, c => c.name == name));
    }

    public Color32 GetColor(string name)
    {
        return Colors[GetSelectedIndex(name)].color;
    }
}
