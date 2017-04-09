using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ObjectLabel : MonoBehaviour
{
    public GameObject label;
    public GameObject trackedObject;

    public ObjectLabel(GameObject trackedObject, GameObject label)
    {
        this.trackedObject = trackedObject;
        this.label = label;
    }
}

    public class TrackingManagerRectangleScript : MonoBehaviour
{
    private Rect screenRect;
    private List<Rect> rectsTakenOnScreen;
    private List<Rect> emptyRects;
    public GameObject label;
    private List<List<ObjectLabel>> objectLabels;
    private Camera cam;
    private int delay = 0;
    private bool DEBUG_MODE = false;
    private Vector2 initLabelSize = new Vector2(120, 80);

    private const int DELAY_BETWEEN_CHECK = 10;

    void Start()
    {
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        GameObject[]  trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
        objectLabels = new List<List<ObjectLabel>>();
        for (int i = 0; i < trackedObjs.Length; i++)
        {
            int index = trackedObjs[i].GetComponent<TargetScript>().getPriority();
            int newIndex = 0;
            while (objectLabels.Count < index) objectLabels.Add(new List<ObjectLabel>());
            if (objectLabels.Count == index)
            {
                objectLabels.Add(new List<ObjectLabel>());
            }
            else
            {
                newIndex = objectLabels[index].Count;
            }
            ObjectLabel objectLabelWithLabel = new ObjectLabel(trackedObjs[i], (GameObject)Instantiate(label, new Vector3(0, 0, 0), Quaternion.identity));
            objectLabels[index].Add(objectLabelWithLabel);
            objectLabels[index][newIndex].label.GetComponentInChildren<TextMesh>().text = trackedObjs[i].GetComponent<TargetScript>().getLabelMessage();
            objectLabels[index][newIndex].label.GetComponent<Renderer>().enabled = false;
        }
        if (UnityEngine.VR.VRDevice.isPresent)
        {
            cam = GameObject.FindWithTag("ARCamera").transform.GetChild(1).GetComponent<Camera>();
        }
        else
        {
            cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>();
        }
    }

    private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
    private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

    void OnGUI()
    {
        if (DEBUG_MODE)
        {
            GUI.backgroundColor = new Color(1, 0, 0, 0.3f);
            foreach (Rect rect in rectsTakenOnScreen)
            {
                GUI.Box(rect, GUIContent.none, textureStyle);
            }

            GUI.backgroundColor = new Color(0, 1, 0, 0.3f);
            foreach (Rect rect in emptyRects)
            {
                GUI.Box(rect, GUIContent.none, textureStyle);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        delay++;
        rectsTakenOnScreen = findTakenSpaces();
        emptyRects = findEmptySpace(rectsTakenOnScreen);

        setScreenRect();

        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                GameObject currLabel = objectLabels[i][j].label;
                GameObject currObj = objectLabels[i][j].trackedObject;
                Rect rect = currObj.GetComponent<TargetScript>().getBounds();
                if (rect.Equals(new Rect()))
                {
                    if (currLabel.GetComponent<Renderer>().enabled)
                    {
                        currLabel.GetComponent<Renderer>().enabled = false;
                    }
                }
                else if (currLabel.GetComponentInChildren<TextMesh>().text != "")
                {
                    Rect labelRect = TargetScript.GetScreenBounds(currLabel.GetComponentInChildren<Renderer>(), cam);
                    if (!currLabel.GetComponent<Renderer>().enabled ||
                        (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmpty(rectsTakenOnScreen, labelRect)))
                    {
                        Vector2 size = initLabelSize;
                        if (currLabel.GetComponent<Renderer>().enabled)
                        {
                            size = new Vector2(labelRect.width, labelRect.height);
                        }
                        Vector2 locationToUse = placeLabel(size.y, size.x, rect.center, emptyRects, rectsTakenOnScreen);
                        float distanceToPlace = Vector3.Distance(cam.transform.position, currObj.transform.position);
                        Vector3 screenPoint = new Vector3(locationToUse.x + size.x/2, Screen.height - locationToUse.y - size.y / 2, distanceToPlace);
                        Vector3 worldLocation = cam.ScreenToWorldPoint(screenPoint);
                        currLabel.GetComponent<LabelPositioner>().setTargetPosition(worldLocation);
                        currLabel.transform.parent = currObj.transform;
                        currLabel.GetComponent<Renderer>().enabled = true;
                        labelRect = TargetScript.GetScreenBounds(currLabel.GetComponentInChildren<Renderer>(), cam);
                    }
                    rectsTakenOnScreen.Add(labelRect);
                    emptyRects = findEmptySpace(rectsTakenOnScreen);
                    currLabel.transform.rotation = Quaternion.LookRotation(-cam.transform.up, -cam.transform.forward);

                    LineRenderer lineRenderer = currLabel.GetComponentInChildren<LineRenderer>();
                    lineRenderer.SetPosition(0, currObj.transform.position);
                    lineRenderer.SetPosition(1, currLabel.transform.position);
                }
            }
        }

        if (delay > DELAY_BETWEEN_CHECK)
        {
            delay = 0;
        }
    }

    void setScreenRect()
    {
        screenRect = new Rect();
        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                GameObject currObj = objectLabels[i][j].trackedObject;
                Rect rect = currObj.GetComponent<TargetScript>().getBounds();
                if (rect.xMin < screenRect.xMin)
                {
                    screenRect.xMin = rect.xMin;
                }
                if (rect.yMin < screenRect.yMin)
                {
                    screenRect.yMin = rect.yMin;
                }
                if (rect.xMax > screenRect.xMax)
                {
                    screenRect.xMax = rect.xMax;
                }
                if (rect.yMax > screenRect.yMax)
                {
                    screenRect.yMax = rect.yMax;
                }
            }
        }
        int padding = 30;
        screenRect.xMin -= initLabelSize.x + padding;
        screenRect.xMax += initLabelSize.x + padding;
        screenRect.yMin -= initLabelSize.y + padding;
        screenRect.yMax += initLabelSize.y + padding;

        if (screenRect.width > Screen.width)
        {
            float xDiff = screenRect.width - Screen.width;
            screenRect.xMin += xDiff / 2;
            screenRect.xMax -= xDiff / 2;
        }

        if (screenRect.height > Screen.height)
        {
            float yDiff = screenRect.height - Screen.height;
            screenRect.yMin += yDiff / 2;
            screenRect.yMax -= yDiff / 2;
        }
    }

    bool isCurrentLocationEmpty(List<Rect> takenRects, Rect rect)
    {
        if (!screenRect.Overlaps(rect))
        {
            return false;
        }

        foreach (Rect currRect in takenRects)
        {
            if (currRect.Overlaps(rect)) {
                return false;
            }
        }
        return true;
    }

    Vector2 placeLabel(float height, float width, Vector2 targetPos, List<Rect> emptyRects, List<Rect> rectsTakenOnScreen)
    {
        emptyRects.Sort((a, b) => compareDistances(a, b, targetPos));
        foreach (Rect rect in emptyRects)
        {
            float[] xs = new float[] { rect.center.x, rect.xMin, rect.xMax - width };
            float[] ys = new float[] { rect.center.y, rect.yMin, rect.yMax - height };
            if (targetPos.x > rect.center.x)
            {
                xs = new float[] { rect.center.x, rect.xMax - width, rect.xMin };
            }
            if (targetPos.y > rect.center.y)
            {
                ys = new float[] { rect.center.y, rect.yMax - height, rect.yMin };
            }
            
            foreach (float x in xs)
            {
                foreach (float y in ys)
                {
                    bool isOk = true;
                    Rect potentialRect = new Rect(x, y, width, height);
                    if (!screenRect.Overlaps(potentialRect))
                    {
                        continue;
                    }
                    foreach (Rect takenRect in rectsTakenOnScreen)
                    {
                        if (takenRect.Overlaps(potentialRect))
                        {
                            isOk = false;
                            break;
                        }
                    }
                    if (isOk)
                    {
                        return new Vector2(x, y);
                    }
                }
            }
        }

        return new Vector2();
    }

    int compareDistances(Rect a, Rect b, Vector2 targetPos)
    {
        return (int)(minDistanceToRect(a, targetPos) - minDistanceToRect(b, targetPos));
    }

    float minDistanceToRect(Rect a, Vector2 targetPos)
    {
        float minDistance = -1;
        float currDistance;
        foreach (float x in new float[] { a.center.x, a.xMin, a.xMax })
        {
            foreach (float y in new float[] { a.center.y, a.yMin, a.yMax })
            {
                currDistance = Vector2.Distance(new Vector2(x, y), targetPos);
                minDistance = minDistance == -1? currDistance : Mathf.Min(currDistance, minDistance);
            }
        }

        return minDistance;
    }

    Rect intersects(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        if (xMax > xMin && yMax > yMin) {
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }
        return new Rect();
    }

    List<Rect> findInverseRectangles(Rect rect, Rect removeRect)
    {
        List<Rect> rects = new List<Rect>();

        removeRect = intersects(removeRect, rect);

        if (removeRect != new Rect())
        {
            rects.Add(new Rect(rect.x, rect.y, rect.width, removeRect.y - rect.y));
            rects.Add(new Rect(rect.x, removeRect.y + removeRect.height, rect.width, (rect.y + rect.height) - (removeRect.y + removeRect.height)));
            rects.Add(new Rect(rect.x, removeRect.y, removeRect.x - rect.x, removeRect.height));
            rects.Add(new Rect(removeRect.x + removeRect.width, removeRect.y, (rect.x + rect.width) - (removeRect.x + removeRect.width), removeRect.height));

            rects.RemoveAll(x => x == new Rect());
        }
        else
        {
            rects.Add(rect);
        }
        return rects;
    }

    List<Rect> findEmptySpace(List<Rect> rects)
    {
        List<Rect> inverseRects = new List<Rect>();
        inverseRects.Add(screenRect);

        foreach (Rect currRect in rects)
        {
            List<Rect> newInverseRects = new List<Rect>();

            foreach (Rect currInverseRects in inverseRects)
            {
                newInverseRects.AddRange(findInverseRectangles(currInverseRects, currRect));
            }

            inverseRects = newInverseRects;
        }
        return inverseRects;
    }

    List<Rect> findTakenSpaces()
    {
        List<Rect> rects = new List<Rect>();

        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                Rect bounds = objectLabels[i][j].trackedObject.GetComponent<TargetScript>().getBounds();
                if (bounds != new Rect())
                {
                    rects.Add(bounds);
                }
            }
        }

        return rects;
    }
}
