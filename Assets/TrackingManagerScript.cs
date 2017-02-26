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

    public class TrackingManagerScript : MonoBehaviour
{
    private int width;
    private int height;
    private int[,] pixels;
    public GameObject label;
    private List<List<ObjectLabel>> objectLabels;
    private Camera cam;
    private int delay = 0;

    private const int DELAY_BETWEEN_CHECK = 10;

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
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
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>(); // 0 if single camera, 1 is dual camera
    }

    // Update is called once per frame
    void Update()
    {
        delay++;
        pixels = generatePixelMap();
        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                GameObject currLabel = objectLabels[i][j].label;
                GameObject currObj = objectLabels[i][j].trackedObject;
                Rect rect = currObj.GetComponent<TargetScript>().getBounds();
                Rect labelRect = TargetScript.GetScreenBounds(currLabel, cam);
                if (rect.Equals(new Rect()))
                {
                    if (currLabel.GetComponent<Renderer>().enabled)
                    {
                        currLabel.GetComponent<Renderer>().enabled = false;
                    }
                }
                else
                {
                    if (!currLabel.GetComponent<Renderer>().enabled ||
                        (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmtpy(pixels, labelRect)))
                    {
                        Vector2 size = new Vector2(120, 80);
                        if (currLabel.GetComponent<Renderer>().enabled)
                        {
                            size = new Vector2(labelRect.width, labelRect.height);
                        }
                        Vector2 locationToUse = placeLabelByLargestRectangle((int)size.y, (int)size.x, rect.center, rect.size);
                        float distanceToPlace = Vector3.Distance(Camera.main.transform.position, currObj.transform.position);
                        Vector3 worldLocation = cam.ScreenToWorldPoint(new Vector3(locationToUse.x, locationToUse.y, distanceToPlace));
                        currLabel.transform.position = worldLocation;
                        currLabel.transform.parent = currObj.transform;
                        currLabel.GetComponent<Renderer>().enabled = true;
                        labelRect = TargetScript.GetScreenBounds(currLabel, cam);
                    }
                }
                pixels = addToPixelMap(pixels, labelRect);
                currLabel.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.up, -Camera.main.transform.forward);
            }
        }

        if (delay > DELAY_BETWEEN_CHECK)
        {
            delay = 0;
        }
    }

    bool isCurrentLocationEmtpy(int[,] map, Rect rect)
    {
        if (rect == null || rect.Equals(new Rect()))
        {
            return false;
        }

        for (int i = Mathf.Max((int)rect.y, 0); i < Mathf.Min(rect.y + rect.height, map.GetLength(0)); i++)
        {
            for (int j = Mathf.Max((int)rect.x, 0); j < Mathf.Min(rect.x + rect.width, map.GetLength(1)); j++)
            {
                if (map[i, j] == 1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    int[,] generatePixelMap()
    {
        List<Rect> trackedRects = new List<Rect>();
        int[,] pixels = new int[height, width];

        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                trackedRects.Add(objectLabels[i][j].trackedObject.GetComponent<TargetScript>().getBounds());
            }
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector2 point = new Vector2(j, i);
                pixels[i, j] = 0;
                foreach (Rect rect in trackedRects)
                {
                    if (rect.Contains(point))
                    {
                        pixels[i, j] = 1;
                        break;
                    }
                }
            }
        }
        return pixels;
    }

    int[,] addToPixelMap(int[,] map, Rect rect)
    {
        if (rect == null || rect.Equals(new Rect()))
        {
            return map;
        }

        for (int i = Mathf.Max((int)rect.y, 0); i < Mathf.Min(rect.y + rect.height, map.GetLength(0)); i++)
        {
            for (int j = Mathf.Max((int)rect.x, 0); j < Mathf.Min(rect.x + rect.width, map.GetLength(1)) ; j++)
            {
                map[i, j] = 1;
            }
        }
        return map;
    }

    Vector2 placeLabelByLargestRectangle(int aimHeight, int aimWidth, Vector2 objLoc, Vector2 objSize)
    {
        int[,] histogram = new int[height, width];
        for (int i = 0; i < pixels.GetLength(0); i++)
        {
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                if (pixels[i, j] == 0) {
                    histogram[i, j] = 1 + (j > 0 ? histogram[i, j - 1] : 0);
                } else {
                    histogram[i, j] = 0;
                }
            }
        }

        Vector2 currentAim = new Vector2(objLoc.x, objLoc.y);
        float halfHeightPlusLabelHeight = aimHeight + objSize.y / 2;
        float halfWidthPlusLabelWidth = aimWidth + objSize.x / 2;
        if (height > currentAim.y + halfHeightPlusLabelHeight)
        {
            // Top
            Vector2? spaceAvailable =
                trySpaceWithSetY(new Vector2(currentAim.x, currentAim.y + halfHeightPlusLabelHeight), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (currentAim.y - objSize.y / 2 > aimHeight)
        {
            // Bottom
            Vector2? spaceAvailable =
                trySpaceWithSetY(new Vector2(currentAim.x, currentAim.y - objSize.y / 2), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (currentAim.x > halfWidthPlusLabelWidth)
        {
            // Left
            Vector2? spaceAvailable =
                trySpaceWithSetX(new Vector2(currentAim.x - halfWidthPlusLabelWidth, currentAim.y), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (width > currentAim.x + halfHeightPlusLabelHeight)
        {
            // Right
            Vector2? spaceAvailable =
                trySpaceWithSetX(new Vector2(currentAim.x + objSize.x / 2, currentAim.y), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        return objLoc;
    }

    object trySpaceWithSetYOrReturnMinHeight(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight) {
        int countOnRow = 0;
        int maxMinNum = 0;
        for (int j = Mathf.Max(0, (int)currentAim.x - aimWidth); j<Mathf.Min(width, (int)currentAim.x + aimWidth); j++)
        {
            if (histogram[(int)currentAim.y, j] >= aimHeight)
            {
                countOnRow += 1;
            }
            else
            {
                maxMinNum = Mathf.Max(maxMinNum, histogram[(int)currentAim.y, j]);
               
                if (countOnRow > aimWidth)
                {
                    return new Vector2(j - aimWidth, currentAim.y);
                }
                else if (j > currentAim.x)
                {
                    return maxMinNum > 0 ? maxMinNum : aimHeight - 1;
                }
                countOnRow = 0;
            }
        }
        return currentAim;
    }

    Vector2? trySpaceWithSetY(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight)
    {
        object ret = trySpaceWithSetYOrReturnMinHeight(currentAim, histogram, aimWidth, aimHeight);
        if (ret is int)
        {
            return null;
        }
        else
        {
            return (Vector2)ret;
        }
    }

    Vector2? trySpaceWithSetX(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight)
    {
        // Add width / 2 so that the center of the rectangle is passed to trySpaceWithSetYOrReturnMinHeight
        Vector2 aim = new Vector2(currentAim.x + aimWidth / 2, currentAim.y);
        for (int i = Mathf.Max(0, (int)currentAim.y - aimHeight / 2); i < Mathf.Min(height, (int)currentAim.y + aimHeight / 2); i++)
        {
            aim.y = i;
            object isCurrentXEmpty = trySpaceWithSetYOrReturnMinHeight(aim, histogram, aimWidth / 2, aimHeight);
            if (isCurrentXEmpty is int)
            {
                i += aimHeight - (int)isCurrentXEmpty - 1;
            }
            else {
                return (Vector2)isCurrentXEmpty;
            }
        }
        return null;
    }
}
