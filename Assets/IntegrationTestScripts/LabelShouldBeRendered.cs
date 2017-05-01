using UnityEngine;
using System.Collections;

public class LabelShouldBeRendered : MonoBehaviour {

    int counter = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GameObject currLabel;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.tag == "Label")
            {
                currLabel = child.gameObject;
                if (currLabel.GetComponent<Renderer>().enabled)
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
