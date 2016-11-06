using UnityEngine;
using System.Collections;
using UnityEditor;
using Vuforia;

public class TargetScript : MonoBehaviour {
    private Vector3 screenPos;
    private Rect bounds;
    bool isOnScreen;

    void Start()
    {
        isOnScreen = true;
    }
	
	void Update() {
        if (isOnScreen)
        {
            // DO WE NEED THIS?
            screenPos = Camera.main.WorldToScreenPoint(transform.position);
            bounds = GUIRectWithObject(transform.FindChild("Cube").gameObject);
        }
    }

    Rect getBounds()
    {
        return bounds;
    }

    //http://answers.unity3d.com/questions/49943/is-there-an-easy-way-to-get-on-screen-render-size.html
    public static Rect GUIRectWithObject(GameObject go)
    {
        Vector3 cen = go.GetComponentInChildren<Renderer>().bounds.center;
        Vector3 ext = go.GetComponentInChildren<Renderer>().bounds.extents;
        Vector2[] extentPoints = new Vector2[8]
         {
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
               WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
               WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
         };
        Vector2 min = extentPoints[0];
        Vector2 max = extentPoints[0];
        foreach (Vector2 v in extentPoints)
        {
            min = Vector2.Min(min, v);
            max = Vector2.Max(max, v);
        }
        return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
    }

    public static Vector2 WorldToGUIPoint(Vector3 world)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(world);
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }
}