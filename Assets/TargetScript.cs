using UnityEngine;
using System.Collections;
using Vuforia;

public class TargetScript : MonoBehaviour {
    public string labelMessage;
    public int priority;
    private Rect bounds;
    private Camera cam;
    private GameObject trackingCube;
    private Renderer trackingCubeRenderer;

    void Start()
    {
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>(); // 0 if single camera, 1 is dual camera
        trackingCube = transform.FindChild("Cube").gameObject;
        trackingCubeRenderer = trackingCube.GetComponent<Renderer>();
    }
	
	void Update() {
        if (trackingCubeRenderer.enabled)
        {
            bounds = GetScreenBounds(trackingCube, cam);
        }
        else
        {
            bounds = new Rect();
        }
    }

    public Rect getBounds()
    {
        return bounds;
    }

    public string getLabelMessage()
    {
        return labelMessage;
    }

    public int getPriority()
    {
        return priority;
    }

    //http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html
    public static Rect GetScreenBounds(GameObject go, Camera cam)
    {
        if (go)
        {
            Vector3 cen = go.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 ext = go.GetComponentInChildren<Renderer>().bounds.extents;
            Vector2[] extentPoints = new Vector2[8]
             {
                   cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
                   cam.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
             };
            Vector2 min = extentPoints[0];
            Vector2 max = extentPoints[0];
            foreach (Vector2 v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        } else
        {
            return new Rect();
        }
    }
}