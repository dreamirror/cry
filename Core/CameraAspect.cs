using UnityEngine;
using System.Collections;

public class CameraAspect : MonoBehaviour
{
    public Camera[] cameras;
    public float width = 1280, height = 720;
    public static float Ratio = 1f;
    public static float RatioInverse = 1f;

	// Use this for initialization
	void Start () {
        Ratio = (width / height) / ((float)Screen.width / Screen.height);
        RatioInverse = ((float)Screen.width / Screen.height) / (width / height);
        foreach (Camera camera in cameras)
        {
            if (camera == null)
                continue;
            if (camera.orthographic)
                camera.orthographicSize = Ratio * camera.orthographicSize;
            else
            {
                var cam_pe = camera.gameObject.GetComponent<CameraPerspectiveEditor>();
                if (cam_pe != null)
                {
                    cam_pe.aspectScale.x = RatioInverse;
                    cam_pe.aspectScale.y = RatioInverse;
                }
            }
        }
	}
	
}
