using UnityEngine;
using System.Collections;

public class LabelShouldntTouchObject : MonoBehaviour {

    int counter = 0;
    Camera cam;
    Rect gameObjLoc;

    // Use this for initialization
    void Start()
    {
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>();
        gameObjLoc = TargetScript.GetScreenBounds(gameObject.GetComponentInChildren<Renderer>(), cam);
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
                if (!gameObjLoc.Overlaps(TargetScript.GetScreenBounds(currLabel.GetComponentInChildren<Renderer>(), cam)))
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
