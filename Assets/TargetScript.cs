using UnityEngine;
using System.Collections;
using UnityEditor;
using Vuforia;

public class TargetScript : MonoBehaviour {
    public Vector3 screenPos;
    public Rect bounds;
    public Texture aTexture;
    bool isOnScreen;

    void Start()
    {
        isOnScreen = true;
    }
	
	// Update is called once per frame
	void Update() {
        if (isOnScreen)
        {
            screenPos = Camera.main.WorldToScreenPoint(transform.position);
            bounds = GUIRectWithObject(gameObject);
            print(bounds);
            print(screenPos);
        }
    }

    void OnGUI()
    {
        if(isOnScreen && aTexture)
        {
            //GUI.DrawTexture(new Rect(screenPos.x, Screen.height - screenPos.y, 60, 60), aTexture);
            GUI.DrawTexture(bounds, aTexture);
        }
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