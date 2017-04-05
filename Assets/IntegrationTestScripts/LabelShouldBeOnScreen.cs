using UnityEngine;
using System.Collections;

public class LabelShouldBeOnScreen : MonoBehaviour {

    int counter = 0;
    Camera cam;
    Rect screen;

    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>();
        screen = new Rect(0, 0, Screen.width, Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        GameObject currLabel;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.tag == "Label")
            {
                currLabel = child.gameObject;
                Rect obj = TargetScript.GetScreenBounds(currLabel, cam);
                if (screen.Contains(new Vector2(obj.xMin, obj.yMin)) && screen.Contains(new Vector2(obj.xMax, obj.yMax)))
                {
                    IntegrationTest.Pass();
                }
            }
        }

        if (counter > 20)
        {
            IntegrationTest.Fail();
        }
        counter++;
    }
}
