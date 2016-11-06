using UnityEngine;
using System.Collections;

public class TrackingManagerScript : MonoBehaviour {

    private GameObject[] trackedObjs;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
    }

    /*void OnGUI()
    {
        if (isOnScreen && aTexture)
        {
            //GUI.DrawTexture(new Rect(screenPos.x, Screen.height - screenPos.y, 60, 60), aTexture);
            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.25f);
            GUI.DrawTexture(bounds, aTexture);
        }
    }*/
}
