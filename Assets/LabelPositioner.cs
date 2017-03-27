using UnityEngine;
using System.Collections;

public class LabelPositioner : MonoBehaviour {

    private Vector3? targetPosition;
    private int speed;

	// Use this for initialization
	void Start () {
        targetPosition = null;
        speed = 1;
	}

    // Update is called once per frame
    void Update()
    {
        if (targetPosition != null)
        {
            if (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards((Vector3)transform.position, (Vector3)targetPosition, speed * Time.deltaTime);
            }
            else
            {
                targetPosition = null;
            }
        }
	}

    public void setTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }
}
