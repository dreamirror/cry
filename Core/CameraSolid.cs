using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CameraSolid : MonoBehaviour {
    public Color color;

    Material m_Material = null;
    Color m_MaterialColor = Color.black;

    Material GetMaterial()
    {
        if (m_Material == null)
        {
            m_Material = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
            m_MaterialColor = m_Material.color;
        }
        return m_Material;
    }

    void Awake()
    {

    }

	// Update is called once per frame
	void Update ()
    {
        Material mat = GetMaterial();
        if (m_MaterialColor != color)
        {
            m_MaterialColor = color;
            mat.color = color;
        }
	}
}
