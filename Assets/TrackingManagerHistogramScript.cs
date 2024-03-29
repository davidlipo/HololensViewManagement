﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectLabelHistogram : MonoBehaviour
{
    public GameObject label;
    public GameObject trackedObject;

    public ObjectLabelHistogram(GameObject trackedObject, GameObject label)
    {
        this.trackedObject = trackedObject;
        this.label = label;
    }
}

public class TrackingManagerHistogramScript : MonoBehaviour
{
    private int width;
    private int height;
    private int[,] pixels;
    public GameObject label;
    private List<List<ObjectLabelHistogram>> objectLabels;
    private Camera cam;
    private int delay = 0;

    private const int DELAY_BETWEEN_CHECK = 10;

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        GameObject[] trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
        objectLabels = new List<List<ObjectLabelHistogram>>();
        for (int i = 0; i < trackedObjs.Length; i++)
        {
            // Create a label for each target object, taking the priority into account
            int index = trackedObjs[i].GetComponent<TargetScript>().getPriority();
            int newIndex = 0;
            while (objectLabels.Count < index) objectLabels.Add(new List<ObjectLabelHistogram>());
            if (objectLabels.Count == index)
            {
                objectLabels.Add(new List<ObjectLabelHistogram>());
            }
            else
            {
                newIndex = objectLabels[index].Count;
            }
            ObjectLabelHistogram objectLabelWithLabel = new ObjectLabelHistogram(trackedObjs[i], (GameObject)Instantiate(label, new Vector3(0, 0, 0), Quaternion.identity));
            objectLabels[index].Add(objectLabelWithLabel);
            objectLabels[index][newIndex].label.GetComponentInChildren<TextMesh>().text = trackedObjs[i].GetComponent<TargetScript>().getLabelMessage();
            objectLabels[index][newIndex].label.GetComponent<Renderer>().enabled = false;
        }
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>();
    }

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
                if (rect.Equals(new Rect()))
                {
                    // If the object isn't on screen, hide the label
                    if (currLabel.GetComponent<Renderer>().enabled)
                    {
                        currLabel.GetComponent<Renderer>().enabled = false;
                    }
                }
                else if (currLabel.GetComponentInChildren<TextMesh>().text != "")
                {
                    Rect labelRect = TargetScript.GetScreenBounds(currLabel.GetComponentInChildren<Renderer>(), cam);
                    if (!currLabel.GetComponent<Renderer>().enabled ||
                        (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmtpy(pixels, labelRect)))
                    {
                        Vector2 size = new Vector2(120, 80);
                        if (currLabel.GetComponent<Renderer>().enabled)
                        {
                            // Find the label size if it's already been placed
                            size = new Vector2(labelRect.width, labelRect.height);
                        }
                        // Find the 2D screen location to use
                        Vector2 locationToUse = placeLabelByLargestRectangle((int)size.y, (int)size.x, rect.center, rect.size);
                        // Check how far away the target object is from the camera
                        float distanceToPlace = Vector3.Distance(cam.transform.position, currObj.transform.position);
                        // Place the label in the same z-axis as the object and in the equivalent 3D location as the 2D point
                        Vector3 worldLocation = cam.ScreenToWorldPoint(new Vector3(locationToUse.x, locationToUse.y, distanceToPlace));
                        currLabel.GetComponent<LabelPositioner>().setTargetPosition(worldLocation);
                        currLabel.transform.parent = currObj.transform;
                        currLabel.GetComponent<Renderer>().enabled = true;
                        labelRect = TargetScript.GetScreenBounds(currLabel.GetComponentInChildren<Renderer>(), cam);
                    }
                    // Add the label to the pixel map so future labels don't overlap it
                    pixels = addToPixelMap(pixels, labelRect);
                    // Rotate the label to face the camera
                    currLabel.transform.rotation = Quaternion.LookRotation(-cam.transform.up, -cam.transform.forward);

                    // Draw the line from the target object to the label
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

    bool isCurrentLocationEmtpy(int[,] map, Rect rect)
    {
        // Mark a location as taken if the rectangle goes off the screen
        if (rect == null || rect.Equals(new Rect()) || rect.yMin < 0 || rect.yMax > height || rect.xMin < 0 || rect.xMax > width)
        {
            return false;
        }

        // Check if any pixel within the target rectangle is taken
        for (int i = (int)rect.yMin; i < rect.yMax; i++)
        {
            for (int j = (int)rect.xMin; j < rect.xMax; j++)
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

        // Mark every pixel as empty
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                pixels[i, j] = 0;
            }
        }

        for (int i = 0; i < objectLabels.Count; i++)
        {
            for (int j = 0; j < objectLabels[i].Count; j++)
            {
                trackedRects.Add(objectLabels[i][j].trackedObject.GetComponent<TargetScript>().getBounds());
            }
        }

        // Mark each pixel within each rectangle as taken
        foreach (Rect rect in trackedRects)
        {
            int rectYMin = Mathf.Max((int)rect.yMin, 0);
            int rectYMax = Mathf.Min((int)rect.yMax, height - 1);
            int rectXMin = Mathf.Max((int)rect.xMin, 0);
            int rectXMax = Mathf.Min((int)rect.xMax, width - 1);
            for (int i = rectYMin; i <= rectYMax; i++)
            {
                for (int j = rectXMin; j < rectXMax; j++)
                {
                    pixels[i, j] = 1;
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

        // Mark every pixel within the rectangle as taken
        for (int i = Mathf.Max((int)rect.y, 0); i < Mathf.Min(rect.y + rect.height, map.GetLength(0)); i++)
        {
            for (int j = Mathf.Max((int)rect.x, 0); j < Mathf.Min(rect.x + rect.width, map.GetLength(1)); j++)
            {
                map[i, j] = 1;
            }
        }
        return map;
    }

    Vector2 placeLabelByLargestRectangle(int aimHeight, int aimWidth, Vector2 objLoc, Vector2 objSize)
    {
        int[,] histogram = new int[height, width];
        // Generate the histogram
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (pixels[i, j] == 0)
                {
                    // Mark as 0 if the pixel is taken
                    // Mark as 1 more than the pixel value above otherwise
                    histogram[i, j] = 1 + (j > 0 ? histogram[i, j - 1] : 0);
                }
                else
                {
                    histogram[i, j] = 0;
                }
            }
        }

        Vector2 currentAim = new Vector2(objLoc.x, objLoc.y);
        float halfHeightPlusLabelHeight = aimHeight + objSize.y / 2;
        float halfWidthPlusLabelWidth = aimWidth + objSize.x / 2;
        if (height > currentAim.y + halfHeightPlusLabelHeight)
        {
            // Place the label above the object
            Vector2? spaceAvailable =
                trySpaceWithSetY(new Vector2(currentAim.x, currentAim.y + halfHeightPlusLabelHeight), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (currentAim.y - objSize.y / 2 > aimHeight)
        {
            // Place the label below the object
            Vector2? spaceAvailable =
                trySpaceWithSetY(new Vector2(currentAim.x, currentAim.y - objSize.y / 2 - 10), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (currentAim.x > halfWidthPlusLabelWidth)
        {
            // Place the label to the left of the object
            Vector2? spaceAvailable =
                trySpaceWithSetX(new Vector2(currentAim.x - halfWidthPlusLabelWidth, currentAim.y), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        if (width > currentAim.x + halfHeightPlusLabelHeight)
        {
            // Place the label to the right the object
            Vector2? spaceAvailable =
                trySpaceWithSetX(new Vector2(currentAim.x + objSize.x / 2, currentAim.y), histogram, aimWidth, aimHeight);
            if (spaceAvailable != null)
            {
                return (Vector2)spaceAvailable;
            }
        }
        return objLoc;
    }

    object trySpaceWithSetYOrReturnMinHeight(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight)
    {
        int countOnRow = 0;
        int maxMinNum = 0;

        if (currentAim.x < 0 || currentAim.x > width)
        {
            return 0;
        }

        for (int j = Mathf.Max(0, (int)currentAim.x - aimWidth); j < Mathf.Min(width, (int)currentAim.x + aimWidth); j++)
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
                    // Location is available
                    return new Vector2(j - aimWidth, currentAim.y);
                }
                else if (j > currentAim.x)
                {
                    // Location is taken so return how many pixels to skip
                    return maxMinNum > 0 ? maxMinNum : aimHeight - 1;
                }
                countOnRow = 0;
            }
        }
        return currentAim;
    }

    Vector2? trySpaceWithSetY(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight)
    {
        // Check if location is available
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
            else
            {
                return (Vector2)isCurrentXEmpty;
            }
        }
        return null;
    }
}