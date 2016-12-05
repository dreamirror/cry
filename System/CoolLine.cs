using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CoolLine : MonoBehaviour {
    public Image image;
    public LineRenderer line;
    float m_Fill = -1f;

	// Use this for initialization
	void Awake ()
    {
        image = transform.parent.GetComponent<Image>();
        line = gameObject.GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        gameObject.SetActive(image.isActiveAndEnabled);
	    if (image != null && line != null)
        {
            float fill = image.fillAmount;
            if (fill == m_Fill)
                return;

            m_Fill = fill;

            Vector3 pos = Vector3.zero;
            pos = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -360f * fill), Vector3.one).MultiplyVector(Vector3.up);
            pos = pos / Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y));
            line.SetPosition(1, pos);
        }
	}
}
