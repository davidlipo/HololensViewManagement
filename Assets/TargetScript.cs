using UnityEngine;
using System.Collections;
using Vuforia;

public class TargetScript : MonoBehaviour {
    private Rect bounds;
    bool isOnScreen;
    Camera cam;

    void Start()
    {
        cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        isOnScreen = true;
    }
	
	void Update() {
        //Debug.Log("cube pos " + cam.WorldToScreenPoint(GameObject.FindWithTag("Cube").transform.position));
        //Debug.Log(cam.transform.position);
        if (isOnScreen)
        {
            bounds = GetScreenBounds(transform.FindChild("Cube").gameObject, cam);
            //Debug.Log("Obj" + bounds);
        }
    }

    public Rect getBounds()
    {
        return bounds;
    }

    //http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html
    public static Rect GetScreenBounds(GameObject go, Camera cam)
    {
        if (go)
        {
            Vector3 cen = go.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 ext = go.GetComponentInChildren<Renderer>().bounds.extents;
            //Debug.Log("cen" + cen + " ext" + ext);
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
                //Debug.Log(v);
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            //Debug.Log("min" + min + " max" + max);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        } else
        {
            return new Rect();
        }
    }
}