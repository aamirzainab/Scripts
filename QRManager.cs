using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;
using Unity.Collections;

public struct PlaneDisplayData
{
    public Plane _plane;
    // public string _cameraName;
    public ARAnchor _topLeft, _bottomLeft, _bottomRight, _topRight;
}

public class QRManager : MonoBehaviour
{

    public Camera arCamera;  
    private ARTrackedImageManager _trackedImageManager;
    public GameObject qrCodePrefab;
    public GameObject cubePrefab;  
    public GameObject planePrefab;  
    private AROcclusionManager _occlusionManager;
    private ARAnchorManager _anchorManager;
    [SerializeField] private ARRaycastManager raycastManager; 

    private Dictionary<string, GameObject> _spawnedPrefabs = new Dictionary<string, GameObject>();
    private List<ARAnchor> _anchors = new List<ARAnchor>();
    private Dictionary<string, ARTrackedImage> trackedImages = new Dictionary<string, ARTrackedImage>();
    
    private List<Vector3> _trackedPositions = new List<Vector3>();

    private GameObject _centerCube = null;
    private GameObject screen = null ; 
    private Vector2 screenPosition = new Vector2(); //
     private float timer = 0f; // Timer to keep track of elapsed time
    private const float interval = 0.50f; // Interval in seconds
    public GameObject raycastLinePrefab;
    private GameObject raycastLine;
    [SerializeField]
    private GameObject anchorMarker;
    private ARAnchor topLeft, bottomLeft, bottomRight, topRight;
    public PlaneDisplayData myPlane ; 

    private void Awake()
    {
        _trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        _anchorManager = FindObjectOfType<ARAnchorManager>();
        _occlusionManager = FindObjectOfType<AROcclusionManager>();
    }
    void Update()
    {
        // Update the timer
        timer += Time.deltaTime;

        // Check if the timer has reached the interval
        if (timer >= interval)
        {
            // Call the DetermineScreenCoordinates method
            DetermineScreenCoordinates();

            // Reset the timer
            timer = 0f;
        }
    }


    private void OnEnable()
    {
        _trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        _trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            SpawnQRPrefab(trackedImage);
            if (trackedImage.referenceImage.name == "one") 
                PlacePlaneFromImage(trackedImage);
            UdpSender udpSender = GetComponent<UdpSender>();
            
            // Debug.Log("Zainab did you come here  " + udpSender); 
            if (udpSender != null)
            {
                // Debug.Log("Zainab did you come here "); 
                string name = trackedImage.referenceImage.name;
                udpSender.sendSpawnData(name); 
            }
            // SetupDigitalScreen();
            // SetupDigitalScreen(trackedImage);
            trackedImages[trackedImage.referenceImage.name] = trackedImage;
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateQRPrefab(trackedImage);
            trackedImages[trackedImage.referenceImage.name] = trackedImage;
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RemoveQRPrefab(trackedImage);
            trackedImages.Remove(trackedImage.referenceImage.name);
        }

        if (trackedImages.Count == 4) 

        {
            Debug.Log("lets start with the new set up "); 
            SetupDigitalScreen(); 
        }
    }

    // private void SpawnQRPrefab(ARTrackedImage trackedImage)
    // {
    //     Vector3 position = trackedImage.transform.position;
    //     Quaternion rotation = trackedImage.transform.rotation;
    //     // Debug.Log("Zainab Initial Position: " + position + "\nZainab Initial Rotation: " + rotation.eulerAngles);

    //     if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
    //     {
    //         GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
    //         _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
    //     }
    // }

private void SpawnQRPrefab(ARTrackedImage trackedImage)
{
    Vector3 position = trackedImage.transform.position;
    Quaternion rotation = trackedImage.transform.rotation;

    if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
    {
        // Check if the placement position is occluded using depth information
        // if (CheckIfPositionIsVisible(position))
        // {
            // Debug.Log("did you come here zainab "); 
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            ARAnchor anchor = _anchorManager.AddAnchor(new Pose(trackedImage.transform.position, trackedImage.transform.rotation));

            if (anchor != null)
            {
                qrPrefab.transform.parent = anchor.transform;
            }
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
        // }
        // else
        // {
        //     Debug.Log("Placement position is occluded, not placing prefab.");
        // }
    }
}

// private bool CheckIfPositionIsVisible(Vector3 position)
// {
//     List<ARRaycastHit> hits = new List<ARRaycastHit>();
//     raycastManager.Raycast(position, hits, TrackableType.All);
//     return hits.Count == 0; 
// }
// private float GetDepthAtPosition(Vector3 screenPosition)
// {
//     if (_occlusionManager.TryAcquireEnvironmentDepthCpuImage(out XRCpuImage depthImage))
//     {
//         XRCpuImage.Plane plane = depthImage.GetPlane(0);
//         NativeArray<byte> data = plane.data;

//         // Calculate the index to access depth data
//         Vector2Int pos = new Vector2Int((int)screenPosition.x, (int)screenPosition.y);
//         int index = pos.y * plane.rowStride + pos.x * plane.pixelStride;

//         // Convert the byte at the index to a float to get the depth value
//         float depth = System.BitConverter.ToSingle(data.ToArray(), index);

//         depthImage.Dispose();
//         return depth;
//     }
//     return 0f;
// }


    void SetupDigitalScreen()
    {
        if (trackedImages.Count < 4)
        {
            Debug.LogError("Not enough images detected to form a screen.");
            return;
        }

        Vector3 averagePosition = Vector3.zero;
        List<Quaternion> rotations = new List<Quaternion>();
        foreach (var image in trackedImages.Values)
        {
            averagePosition += image.transform.position;
            rotations.Add(image.transform.rotation);
        }

        averagePosition /= trackedImages.Count;
        Quaternion averageRotation = AverageRotation(rotations);
        Quaternion desiredRotation = rotations[0];
        desiredRotation *= Quaternion.Euler(90,0,0);
        Vector3 forwardOffset = desiredRotation * Vector3.forward * -0.25f;
        Vector3 newPosition = averagePosition + forwardOffset;

        
        if ( screen == null ) { 
            Debug.Log("Zainab: Instantiating screen at average position: " + newPosition );
            Debug.Log("Zainab: Instantiating screen at average rotation: " + desiredRotation );
            screen = Instantiate(planePrefab, newPosition, desiredRotation);

            screen.transform.localScale = new Vector3(1.86f, 0.726f, 0.02f);  // Assuming these are the dimensions of the screen
            PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
            if (polygonDrawer != null)
            {
                polygonDrawer.InitializeObjects(screen); 
            }

        }
        // ARAnchor anchor = screen.AddComponent<ARAnchor>();
    }

//  public void DetermineScreenCoordinates()
//     {
//         if (screen == null) return;

//         Ray ray = Camera.main.ViewportPointToRay(new Vector2(0.5f, 0.5f));

//         // Instantiate or use existing line renderer for visualization
//         if (raycastLine == null)
//         {
//             raycastLine = Instantiate(raycastLinePrefab, transform);
//         }

//         var lineRenderer = raycastLine.GetComponent<LineRenderer>();
//         lineRenderer.positionCount = 2;

//         Plane screenPlane = new Plane(screen.transform.up, screen.transform.position);

//         if (screenPlane.Raycast(ray, out float enter))
//         {
//             Vector3 intersectionPoint = ray.GetPoint(enter);

//             // Set the positions for the line renderer to visualize the ray
//             lineRenderer.SetPositions(new Vector3[] { ray.origin, intersectionPoint });

//             // Find the intersection point in the local coordinates of the screen
//             Vector3 localIntersectionPoint = screen.transform.InverseTransformPoint(intersectionPoint);

//             // Normalize the intersection point coordinates
//             float xViewport = (localIntersectionPoint.x / screen.transform.localScale.x) + 0.5f;
//             float yViewport = (localIntersectionPoint.y / screen.transform.localScale.y) + 0.5f;

//             screenPosition = new Vector2(xViewport, yViewport);

//             Debug.Log($"Screen coordinates: {screenPosition}");
//         }
//         else
//         {
//             // Ensure the line is not visible if there is no intersection
//             lineRenderer.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
//             Debug.Log("Ray does not intersect with the screen plane.");
//         }
//     }

    float FindViewportFromIntersection(Vector3 point1, Vector3 point2, Vector3 intersectionPoint)
    {
        Vector3 screenLine = point2 - point1;
        Vector3 intersectionLine = intersectionPoint - point1;
        Vector3 intersectionLineProjected = Vector3.Project(intersectionLine, screenLine.normalized);
        float sign = Vector3.Dot(screenLine.normalized, intersectionLineProjected.normalized);
        return (intersectionLineProjected.magnitude / screenLine.magnitude) * sign;
    }

    private void UpdateQRPrefab(ARTrackedImage trackedImage)
    {
        if (_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = _spawnedPrefabs[trackedImage.referenceImage.name];
            qrPrefab.transform.position = trackedImage.transform.position;
            qrPrefab.transform.rotation = trackedImage.transform.rotation;
        }
    }

    private void RemoveQRPrefab(ARTrackedImage trackedImage)
    {
        if (_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = _spawnedPrefabs[trackedImage.referenceImage.name];
            Destroy(qrPrefab); 
            _spawnedPrefabs.Remove(trackedImage.referenceImage.name);
            _trackedPositions.Remove(trackedImage.transform.position);

            ARAnchor anchor = qrPrefab.GetComponent<ARAnchor>();
            if (anchor != null)
            {
                _anchors.Remove(anchor);
                Destroy(anchor.gameObject); 
            }
        }
    }
    void PlacePlaneFromImage(ARTrackedImage img)
{
    Debug.Log("Placing plane from image...");
    Vector3 normal = img.transform.rotation * Vector3.up;
    Vector3 image_up = img.transform.rotation * Vector3.forward;
    Vector3 pointOnPlane = img.transform.position;
    Plane plane = new Plane(normal, pointOnPlane);

    // DisplayDimensions dispDim = cameraDisplayDataDict[cameraName].displayDimensions;
    Vector3 right = Vector3.Cross(normal.normalized, Vector3.up);

    Vector3 halfRightVector = right.normalized * (73/ 2 / 39.3701f); // width in inches 
    Vector3 halfUpVector = image_up.normalized * (28 / 2 / 39.3701f); // height in inches 

    Vector3 botLeft = pointOnPlane - halfUpVector - halfRightVector;
    Vector3 botRight = pointOnPlane - halfUpVector + halfRightVector;
    Vector3 tLeft = pointOnPlane + halfUpVector - halfRightVector;
    Vector3 tRight = pointOnPlane + halfUpVector + halfRightVector;

    ARAnchor tLeftAnch = CreateARAnchor(tLeft);
    ARAnchor tRightAnch = CreateARAnchor(tRight);
    ARAnchor bLeftAnch = CreateARAnchor(botLeft);
    ARAnchor bRightAnch = CreateARAnchor(botRight);

    myPlane = new PlaneDisplayData
    {
        // _cameraName = cameraName,
        _plane = plane,
        _bottomLeft = bLeftAnch,
        _bottomRight = bRightAnch,
        _topLeft = tLeftAnch,
        _topRight = tRightAnch
    };

    // Optionally switch from plane selection to main menu, update UI, etc.
}

     public void DetermineScreenCoordinates()
    {
        Ray ray = arCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
        if (raycastLine == null)
        {
            raycastLine = Instantiate(raycastLinePrefab, transform);
        }

        var lineRenderer = raycastLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        bool dataFound = false;
        PlaneDisplayData dd = myPlane; 
        // foreach (PlaneDisplayData dd in displayData)
        // {
            Plane plane = dd._plane;
            bottomLeft = dd._bottomLeft;
            bottomRight = dd._bottomRight;
            topLeft = dd._topLeft;
            topRight = dd._topRight;
            if (!dataFound && plane.Raycast(ray, out float enter))
            {
                Vector3 intersectionPoint = ray.GetPoint(enter);

                // find the viewport point
                float xViewport = FindViewportFromIntersection(topLeft.transform.position, topRight.transform.position, intersectionPoint);
                float yViewport = FindViewportFromIntersection(bottomRight.transform.position, topRight.transform.position, intersectionPoint);
                 lineRenderer.SetPositions(new Vector3[] { ray.origin, intersectionPoint });

                screenPosition = new Vector2(xViewport, yViewport);
                  Debug.Log($"Screen coordinates: {screenPosition}");
                // if (dataSync == null)
                // {
                //     dataSync = GameObject.FindObjectOfType<DataSync>();
                // }
                // if (0 <= xViewport && xViewport <= 1.0f && 0 <= yViewport && yViewport <= 1.0f) // only care if it's in our plane
                // {
                //     dataSync.SendViewportCoordinatesToServerRpc(screenPosition, dd._cameraName);
                //     dataFound = true;
                // }
                
            }
        // }
        
    }


    private Quaternion AverageRotation(List<Quaternion> rotations)
    {
        if (rotations.Count == 0)
            return Quaternion.identity;

        Quaternion average = rotations[0];

        for (int i = 1; i < rotations.Count; i++)
        {
            float weight = 1.0f / (i + 1);
            average = Quaternion.Slerp(average, rotations[i], weight);
        }

        return average;
    }


        private void SetARWorldOrigin()
        {
            ARSessionOrigin arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            if (arSessionOrigin != null)
            {
                arSessionOrigin.transform.rotation = Quaternion.identity;
            }
        }


        ARAnchor CreateARAnchor(Vector3 pos)
    {
        GameObject newAnchorGObj = Instantiate(anchorMarker, pos, Quaternion.identity);
        return newAnchorGObj.AddComponent<ARAnchor>();
    }

    }
