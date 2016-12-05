using UnityEngine;
using System.Collections;
using UnityEditor;
using HeroFX;

[CustomEditor(typeof(ColorContainer), true)]
public class ColorContainerInspector : Editor
{
    static InspectorUtil s_Util = new InspectorUtil("ColorContainerInspector", new Color(0.6f, 0.9f, 0.6f));

    class ColorDataList : InspectorList<ColorData>
    {
        public ColorContainer container { get; set; }
        public ColorDataList(ColorContainer container)
            : base(s_Util, "Colors", true)
        {
            this.container = container;
        }

        override public ColorData[] Datas { get { if (container.Colors == null) container.Colors = new ColorData[0]; return container.Colors; } set { container.Colors = value; } }

        override public string GetDataName(int index) { return Datas[index].name; }
        override public void SetDataName(int index, string name) { Datas[index].name = name; }

        override public ColorData CreateNewData() { ColorData new_data = new ColorData(); return new_data; }
        override public ColorData CloneData(int index) { ColorData clone_data = Datas[index].Clone(); return clone_data; }

        override protected void OnInspectorItem(int index, ColorData selected)
        {
            EditorGUILayout.BeginHorizontal();
            selected.color = EditorGUILayout.ColorField(selected.color);
            EditorGUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = 0f;
        }
    }
    ColorDataList color_data_list = new ColorDataList(null);

    public override void OnInspectorGUI()
    {
        ColorContainer manager = (ColorContainer)target;

        color_data_list.container = manager;
        color_data_list.OnInspectorGUI();

        EditorUtility.SetDirty(target);
    }

}
