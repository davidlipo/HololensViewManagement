using UnityEngine;
using System.Collections;
using Vuforia;

public class TargetScript : MonoBehaviour {
    private Rect bounds;
    bool isOnScreen;

    void Start()
    {
        isOnScreen = true;
    }
	
	void Update() {
        if (isOnScreen)
        {
            bounds = GetScreenBounds(transform.FindChild("Cube").gameObject);
        }
    }

    public Rect getBounds()
    {
        return bounds;
    }

    //http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html
    public static Rect GetScreenBounds(GameObject go)
    {
        if (go)
        {
            Vector3 cen = go.GetComponentInChildren<Renderer>().bounds.center;
            Vector3 ext = go.GetComponentInChildren<Renderer>().bounds.extents;
            Vector2[] extentPoints = new Vector2[8]
             {
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
                   Camera.main.WorldToScreenPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
             };
            Vector2 min = extentPoints[0];
            Vector2 max = extentPoints[0];
            foreach (Vector2 v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        } else
        {
            return new Rect();
        }
    }
}