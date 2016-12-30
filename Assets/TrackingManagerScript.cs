using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrackingManagerScript : MonoBehaviour {

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
    void Start () {
        width = Screen.width;
        height = Screen.height;
        trackedObjs = GameObject.FindGameObjectsWithTag("TrackedObj");
        objectLabels = new GameObject[trackedObjs.Length];
        for (int i = 0; i < objectLabels.Length; i++)
        {
            objectLabels[i] = (GameObject)Instantiate(label, new Vector3(0, 0, 0), Quaternion.identity);
            objectLabels[i].GetComponentInChildren<TextMesh>().text = trackedObjs[i].GetComponent<TargetScript>().getLabelMessage();
        }
        cam = GameObject.FindWithTag("ARCamera").transform.GetChild(0).GetComponent<Camera>(); // 0 if single camera, 1 is dual camera
    }

    // Update is called once per frame
    void Update() {
        delay++;
        pixels = generatePixelMap();

        for (int i = 0; i < objectLabels.Length; i++)
        {
            if (delay > DELAY_BETWEEN_CHECK && !isCurrentLocationEmtpy(100, 200, cam.WorldToScreenPoint(objectLabels[i].transform.position)))
            {
                Rect rect = trackedObjs[i].GetComponent<TargetScript>().getBounds();
                Vector2 locationToUse = placeLabel(100, 200, rect.center);
                float distanceToPlace = Vector3.Distance(Camera.main.transform.position, trackedObjs[i].transform.position);
                Vector3 worldLocation = cam.ScreenToWorldPoint(new Vector3(locationToUse.x, locationToUse.y, distanceToPlace));
                objectLabels[i].transform.position = worldLocation;
                objectLabels[i].transform.parent = trackedObjs[i].transform;
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
        if(locationToUse == null ||
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
}
