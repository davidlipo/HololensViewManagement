using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackingManagerScript : MonoBehaviour
{

    private GameObject[] trackedObjs;
    private int width;
    private int height;
    private int[,] pixels;
    public GameObject label;
    private GameObject[] objectLabels;
    private Camera cam;
    private int delay = 0;

    private const int DELAY_BETWEEN_CHECK = 20;

    // Use this for initialization
    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
        objectLabels = new GameObject[trackedObjs.Length];
        for (int i = 0; i < objectLabels.Length; i++)
        {
            objectLabels[i] = (GameObject)Instantiate(label, new Vector3(0, 0, 0), Quaternion.identity);
            objectLabels[i].GetComponentInChildren<TextMesh>().text = trackedObjs[i].GetComponent<TargetScript>().getLabelMessage();
            objectLabels[i].GetComponent<Renderer>().enabled = false;
        }
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>(); // 0 if single camera, 1 is dual camera
    }

    // Update is called once per frame
    void Update()
    {
        delay++;
        pixels = generatePixelMap();

        for (int i = 0; i < objectLabels.Length; i++)
        {
            Rect rect = trackedObjs[i].GetComponent<TargetScript>().getBounds();
            if (rect.Equals(new Rect()))
            {
                objectLabels[i].GetComponent<Renderer>().enabled = false;
            }
            else
            {
                if (!objectLabels[i].GetComponent<Renderer>().enabled ||
                    (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmtpy(100, 200, cam.WorldToScreenPoint(objectLabels[i].transform.position))))
                {
                    // Vector2 locationToUse = placeLabel(100, 200, rect.center);
                    Vector2 locationToUse = placeLabelByLargestRectangle(50, 100, rect.center, rect.size);
                    float distanceToPlace = Vector3.Distance(Camera.main.transform.position, trackedObjs[i].transform.position);
                    Vector3 worldLocation = cam.ScreenToWorldPoint(new Vector3(locationToUse.x, locationToUse.y, distanceToPlace));
                    objectLabels[i].transform.position = worldLocation;
                    objectLabels[i].transform.parent = trackedObjs[i].transform;
                    objectLabels[i].GetComponent<Renderer>().enabled = true;
                }
            }
            objectLabels[i].transform.rotation = Quaternion.LookRotation(-Camera.main.transform.up, -Camera.main.transform.forward);
        }

        if (delay > DELAY_BETWEEN_CHECK)
        {
            delay = 0;
        }
    }

    bool isCurrentLocationEmtpy(int aimHeight, int aimWidth, Vector2 locationToUse)
    {
        if (locationToUse == null ||
           locationToUse.x > Screen.width - aimWidth ||
           locationToUse.x < 0 ||
           locationToUse.y > Screen.height - aimHeight ||
           locationToUse.y < 0)
        {
            return false;
        }

        for (int k = (int)locationToUse.y; k < (int)locationToUse.y + aimHeight; k++)
        {
            for (int l = (int)locationToUse.x; l < (int)locationToUse.x + aimWidth; l++)
            {
                if (pixels[k, l] == 1)
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

        for (int k = 0; k < trackedObjs.Length; k++)
        {
            trackedRects.Add(trackedObjs[k].GetComponent<TargetScript>().getBounds());
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

    Vector2 placeLabel(int aimHeight, int aimWidth, Vector2 objLoc)
    {
        int noColumns = Mathf.FloorToInt(width / aimWidth);
        int noRows = Mathf.FloorToInt(height / aimHeight);

        int[] fullnessOfBlock = new int[noColumns * noRows];
        int emptiestBlock = 0;
        int emptiestValue = aimHeight * aimWidth + 1;

        for (int i = 0; i < noRows; i++)
        {
            for (int j = 0; j < noColumns; j++)
            {
                int inBlock = 0;
                for (int k = i * aimHeight; k < (i + 1) * aimHeight; k++)
                {
                    for (int l = j * aimWidth; l < (j + 1) * aimWidth; l++)
                    {
                        inBlock += pixels[k, l];
                    }
                }
                int blockNo = i * noColumns + j;
                int distanceFromObjX = Mathf.Abs(Mathf.FloorToInt(objLoc.x / aimWidth) - j);
                int distanceFromObjY = Mathf.Abs(Mathf.FloorToInt(objLoc.y / aimHeight) - i);
                inBlock += distanceFromObjX * aimWidth + distanceFromObjY * aimHeight;
                fullnessOfBlock[blockNo] = inBlock;
                if (inBlock < emptiestValue)
                {
                    emptiestValue = inBlock;
                    emptiestBlock = blockNo;
                }
            }
        }

        int emptiestBlockColumn = emptiestBlock % noColumns;
        int emptiestBlockRow = emptiestBlock / noColumns;

        return new Vector2(emptiestBlockColumn * aimWidth + aimWidth / 2, emptiestBlockRow * aimHeight + aimHeight / 2);
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
            return (Vector2)trySpaceWithSetYOrReturnMinHeight(currentAim, histogram, aimWidth, aimHeight);
        }
    }

    Vector2? trySpaceWithSetX(Vector2 currentAim, int[,] histogram, int aimWidth, int aimHeight)
    {
        // Add width / 2 so that the center of the rectangle is passed to trySpaceWithSetYOrReturnMinHeight
        Vector2 aim = new Vector2(currentAim.x + aimWidth / 2, currentAim.y);
        for (int i = Mathf.Max(0, (int)currentAim.y - aimHeight / 2); i < Mathf.Min(height, (int)currentAim.y + aimHeight / 2); i++)
        {
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
