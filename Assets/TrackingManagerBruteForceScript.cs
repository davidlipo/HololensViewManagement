using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackingManagerBruteForceScript : MonoBehaviour
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

    void Start()
    {
        width = Screen.width;
        height = Screen.height;
        trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
        objectLabels = new GameObject[trackedObjs.Length];
        for (int i = 0; i < objectLabels.Length; i++)
        {
            // Create a label for each target object
            objectLabels[i] = (GameObject)Instantiate(label, new Vector3(0, 0, 0), Quaternion.identity);
            objectLabels[i].GetComponentInChildren<TextMesh>().text = trackedObjs[i].GetComponent<TargetScript>().getLabelMessage();
            objectLabels[i].GetComponent<Renderer>().enabled = false;
        }
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>();
    }

    void Update()
    {
        delay++;
        pixels = generatePixelMap();

        for (int i = 0; i < objectLabels.Length; i++)
        {
            Rect rect = trackedObjs[i].GetComponent<TargetScript>().getBounds();
            if (rect.Equals(new Rect()))
            {
                // If the object isn't on screen, hide the label
                objectLabels[i].GetComponent<Renderer>().enabled = false;
            }
            else
            {
                if (!objectLabels[i].GetComponent<Renderer>().enabled ||
                    (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmtpy(100, 200, cam.WorldToScreenPoint(objectLabels[i].transform.position))))
                {
                    // Find the 2D screen location to use
                    Vector2 locationToUse = placeLabel(100, 200, rect.center);
                    // Check how far away the target object is from the camera
                    float distanceToPlace = Vector3.Distance(Camera.main.transform.position, trackedObjs[i].transform.position);
                    // Place the label in the same z-axis as the object and in the equivalent 3D location as the 2D point
                    Vector3 worldLocation = cam.ScreenToWorldPoint(new Vector3(locationToUse.x, locationToUse.y, distanceToPlace));
                    objectLabels[i].transform.position = worldLocation;
                    objectLabels[i].transform.parent = trackedObjs[i].transform;
                    objectLabels[i].GetComponent<Renderer>().enabled = true;
                }
            }
            // Rotate the label to face the camera
            objectLabels[i].transform.rotation = Quaternion.LookRotation(-Camera.main.transform.up, -Camera.main.transform.forward);
        }

        if (delay > DELAY_BETWEEN_CHECK)
        {
            delay = 0;
        }
    }

    bool isCurrentLocationEmtpy(int aimHeight, int aimWidth, Vector2 locationToUse)
    {
        // Mark a location as taken if the rectangle goes off the screen
        if (locationToUse == null ||
           locationToUse.x > Screen.width - aimWidth ||
           locationToUse.x < 0 ||
           locationToUse.y > Screen.height - aimHeight ||
           locationToUse.y < 0)
        {
            return false;
        }

        // Check if any pixel within the target rectangle is taken
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

        // Mark every pixel as empty
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                pixels[i, j] = 0;
            }
        }

        for (int k = 0; k < trackedObjs.Length; k++)
        {
            trackedRects.Add(trackedObjs[k].GetComponent<TargetScript>().getBounds());
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
                // For each potential label location, check how many taken pixels it will cover
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
                // On top of the taken pixels, add on the distance from the target object
                inBlock += distanceFromObjX * aimWidth + distanceFromObjY * aimHeight;
                fullnessOfBlock[blockNo] = inBlock;
                // If this location is better than all previous ones, store it as the best so far
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
}