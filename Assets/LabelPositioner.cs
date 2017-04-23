using UnityEngine;
using System.Collections;

public class LabelPositioner : MonoBehaviour {

    public Vector3? targetPosition;
    private int speed;
    private bool isVisible;
    private float alpha;
    private float fadeSpeed;

	void Start () {
        targetPosition = null;
        speed = 1;
        fadeSpeed = 0.005f;
        isVisible = false;
    }

    // Update is called once per frame
    void Update()
    {
        alpha = getVisibility();
        if (isVisible)
        {
            if (alpha < 1)
            {
                // Fade in
                setVisibility(Mathf.Min(alpha + fadeSpeed, 1));
            }
        } else {
            if (alpha > 0)
            {
                // Fade out
                setVisibility(Mathf.Max(alpha - fadeSpeed, 0));
            } else if (alpha <= 0)
            {
                GetComponent<Renderer>().enabled = false;
            }
        }

        if (targetPosition != null)
        {
            if (transform.position != targetPosition)
            {
                // Move the label towards the new location if it's not there already
                transform.position = Vector3.MoveTowards((Vector3)transform.position, (Vector3)targetPosition, speed * Time.deltaTime);
            }
            else
            {
                targetPosition = null;
            }
        }
	}

    public bool isShown()
    {
        return isVisible;
    }

    public void hide()
    {
        isVisible = false;
    }

    public void show()
    {
        if (!isVisible)
        {
            isVisible = true;
            GetComponent<Renderer>().enabled = true;
            setVisibility(0);
        }
    }

    public void setTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    private void setVisibility(float alpha)
    {
        for (int i = 0; i < GetComponentsInChildren<MeshRenderer>().Length; i++)
        {
            MeshRenderer renderer = GetComponentsInChildren<MeshRenderer>()[i];
            Color color = renderer.material.color;
            renderer.material.color = new Color(color.r, color.g, color.b, alpha);
        }
    }

    private float getVisibility()
    {
        return GetComponent<Renderer>().material.color.a;
    }
}
