using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using RuntimeHandle;
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

    // private GameObject _centerCube = null;
    private GameObject screen = null ; 
    private Vector2 screenPosition = new Vector2(); //
    //  private float timer = 0f; // Timer to keep track of elapsed time
    private const float interval = 0.50f; // Interval in seconds
    public GameObject raycastLinePrefab;
    private GameObject raycastLine;
    [SerializeField]
    private GameObject anchorMarker;
    private ARAnchor topLeft, bottomLeft, bottomRight, topRight;
    public PlaneDisplayData myPlane ; 
    private float lastTapTime = 0;  
    public RuntimeTransformHandle transformHandle;
    private const float doubleTapDelay = 0.3f; 
     bool calibrated = false ; 

    private void Awake()
    {
        _trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        _anchorManager = FindObjectOfType<ARAnchorManager>();
        _occlusionManager = FindObjectOfType<AROcclusionManager>();
    }

    void Update()
    {
        HandleTouches();

        if (calibrated)
        {
            // timer += Time.deltaTime;

            // if (timer >= interval) 
            // {
                DetermineScreenCoordinates(); 

                // timer = 0f; 
            // }
        }
    }

    void HandleTouches()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                if (Time.time - lastTapTime  < doubleTapDelay)
                {
                    ToggleGizmoMode();
                }
                lastTapTime = Time.time;
            }
        }
    }

     public void InitializeGizmo()
    {   

        if (transformHandle != null && screen != null)
        {
            transformHandle.target = screen.transform;
            transformHandle.gameObject.SetActive(true);
        }
    }

      void ToggleGizmoMode()
    {
        if (transformHandle.type == HandleType.POSITION)
        {
            transformHandle.type = HandleType.ROTATION;
        }
        else if (transformHandle.type == HandleType.ROTATION)
        {
            transformHandle.type = HandleType.POSITION;
        }
    }

    public void FinalizeCalibration()
    {
        if (transformHandle != null)
        {
            transformHandle.gameObject.SetActive(false);
            calibrated = true ; 
            UpdatePlaneAndAnchors(screen.transform.position, screen.transform.rotation);

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
            if (udpSender != null)
            {
                string name = trackedImage.referenceImage.name;
                udpSender.sendSpawnData(name); 
            }
            trackedImages[trackedImage.referenceImage.name] = trackedImage;
        }

        // foreach (ARTrackedImage trackedImage in eventArgs.updated)
        // {
        //     UpdateQRPrefab(trackedImage);
        //     trackedImages[trackedImage.referenceImage.name] = trackedImage;
        // }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RemoveQRPrefab(trackedImage);
            trackedImages.Remove(trackedImage.referenceImage.name);
        }

    }

    private void SpawnQRPrefab(ARTrackedImage trackedImage)
    {
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;

        if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
        }
    }
// where from line between point1 and point2 is the intersection point, b/w 0-1 means its in b/w p1 and p2!
    float FindViewportFromIntersection(Vector3 point1, Vector3 point2, Vector3 intersectionPoint)
    {
        Vector3 screenLine = point2 - point1; // calculating direction vector 
        Vector3 intersectionLine = intersectionPoint - point1; //calculating intersection vector
        Vector3 intersectionLineProjected = Vector3.Project(intersectionLine, screenLine.normalized); //project intersection on direction???
        float sign = Vector3.Dot(screenLine.normalized, intersectionLineProjected.normalized); // find sign of projection
        return (intersectionLineProjected.magnitude / screenLine.magnitude) * sign; // calculate final intersection position
    }

    void PlacePlaneFromImage(ARTrackedImage img)
    {
        Vector3 normal = img.transform.rotation * Vector3.up;  // Normal to the plane
        Vector3 image_up = img.transform.rotation * Vector3.forward;  // Up direction for the image
        Vector3 pointOnPlane = img.transform.position;  // This is now the top-left corner

        Vector3 right = Vector3.Cross(normal, Vector3.up);  // Right direction perpendicular to the normal

        float widthInMeters = 73 / 39.3701f;  // Convert width from inches to meters
        float heightInMeters = 28 / 39.3701f;  // Convert height from inches to meters
        float adjustment = 10.5f / 100f;
        float forwardAdjustment = 0.1f;

        // Adjust pointOnPlane to be the top-left corner shifted up and left by 10.5 cm
        // pointOnPlane += -right * adjustment + image_up * adjustment;
        pointOnPlane += -right * adjustment + image_up * adjustment + normal * forwardAdjustment;


        // Since pointOnPlane is the top-left corner, calculate other corners based on this
        Vector3 topRight = pointOnPlane + right * widthInMeters;
        Vector3 bottomLeft = pointOnPlane - image_up * heightInMeters;
        Vector3 bottomRight = bottomLeft + right * widthInMeters;

        ARAnchor tLeftAnchor = CreateARAnchor(pointOnPlane);  // Top-left 
        ARAnchor tRightAnchor = CreateARAnchor(topRight);  // Top-right 
        ARAnchor bLeftAnchor = CreateARAnchor(bottomLeft);  // Bottom-left 
        ARAnchor bRightAnchor = CreateARAnchor(bottomRight);  // Bottom-right 

        myPlane = new PlaneDisplayData
        {
            _plane = new Plane(normal, pointOnPlane),
            _topLeft = tLeftAnchor,
            _topRight = tRightAnchor,
            _bottomLeft = bLeftAnchor,
            _bottomRight = bRightAnchor
        };
        if (screen == null)
        {
            Vector3 centerPosition = (myPlane._topLeft.transform.position + myPlane._topRight.transform.position + myPlane._bottomLeft.transform.position + myPlane._bottomRight.transform.position) / 4;
            Quaternion rotation = Quaternion.LookRotation(Vector3.Cross(myPlane._topRight.transform.position - myPlane._topLeft.transform.position, myPlane._bottomLeft.transform.position - myPlane._topLeft.transform.position));

            Debug.Log("Zainab: Instantiating screen at calculated position and rotation.");
            screen = Instantiate(planePrefab, centerPosition, rotation);
            screen.transform.localScale = new Vector3(1.86f, 0.726f, 0.02f);  
            InitializeGizmo();
        }
    }

    void UpdatePlaneAndAnchors(Vector3 newPosition, Quaternion newRotation)
    {
        if (screen != null)
        {
            screen.transform.position = newPosition;
            screen.transform.rotation = newRotation;

            // Correctly calculate the right and up vectors based on the new rotation
            Vector3 right = newRotation * Vector3.right; // Assuming the original right direction = local right ?? 
            Vector3 up = newRotation * Vector3.up; // Local up has to be adjusted by rotation

            float widthInMeters = 73 / 39.3701f; // Convert width from inches to meters
            float heightInMeters = 28 / 39.3701f; // Convert height from inches to meters

            // float adjustment = 10.5f / 100f;

            // Calculate half dimensions to find corners from the center
            float halfWidth = widthInMeters / 2;
            float halfHeight = heightInMeters / 2;

            // Adjust the anchor points from the center
            Vector3 topLeft = newPosition - right * halfWidth + up * halfHeight;
            Vector3 topRight = newPosition + right * halfWidth + up * halfHeight;
            Vector3 bottomLeft = newPosition - right * halfWidth - up * halfHeight;
            Vector3 bottomRight = newPosition + right * halfWidth - up * halfHeight;

            myPlane._topLeft.transform.position = topLeft;
            myPlane._topRight.transform.position = topRight;
            myPlane._bottomLeft.transform.position = bottomLeft;
            myPlane._bottomRight.transform.position = bottomRight;
        }
    }


    public void DetermineScreenCoordinates()
    {
        SetARWorldOrigin(); 
        Ray ray = arCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));  
        if (raycastLine == null)
        {
            raycastLine = Instantiate(raycastLinePrefab, transform);
        }
        var lineRenderer = raycastLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        PlaneDisplayData dd = myPlane; 
        Plane plane = dd._plane;
        ARAnchor topLeft = dd._topLeft;
        ARAnchor topRight = dd._topRight;
        ARAnchor bottomLeft = dd._bottomLeft;
        ARAnchor bottomRight = dd._bottomRight;
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 intersectionPoint = ray.GetPoint(enter);
            
            //Zainab I'm calculating coordinates relative to top-left corner and finding  the right and down vectors using the corner anchors 
            Vector3 relativePosition = intersectionPoint - topLeft.transform.position;
            Vector3 rightVector = (topRight.transform.position - topLeft.transform.position).normalized;
            // rightVector = (topLeft.transform.position - topRight.transform.position).normalized;
            Vector3 downVector = (bottomLeft.transform.position - topLeft.transform.position).normalized;
            // Vector3 downVector = (topLeft.transform.position - bottomLeft.transform.position).normalized;

            // Now projecting the relative position onto the right and down vectors
            float xProjected = Vector3.Project(relativePosition, rightVector).magnitude;
            float yProjected = Vector3.Project(relativePosition, downVector).magnitude;

            // Have to account for direction relative to the top-left origin, accordingly adjust it and find the projected x and y coordinates and normalize them
            if (Vector3.Dot(relativePosition, rightVector) < 0) xProjected *= -1;
            if (Vector3.Dot(relativePosition, downVector) < 0) yProjected *= -1;

            float normalizedX = xProjected / Vector3.Distance(topLeft.transform.position, topRight.transform.position);
            float normalizedY = yProjected / Vector3.Distance(topLeft.transform.position, bottomLeft.transform.position);
            float adjustedX = 1.0f - normalizedX;
            float adjustedY = normalizedY;
            // Checking that they fall on our screen, otherwise we don't send the coordinates 
            // if (0 <= normalizedX && normalizedX <= 1.0f && 0 <= normalizedY && normalizedY <= 1.0f)
            // {
            //     screenPosition = new Vector2(normalizedX, normalizedY);
            //     lineRenderer.SetPositions(new Vector3[] { ray.origin, intersectionPoint });
            //     Debug.Log($"Screen coordinates: {screenPosition}");

            //     UdpSender udpSender = GetComponent<UdpSender>();
            //     if (udpSender != null)
            //     {
            //         udpSender.sendCastData(screenPosition); 
            //     }
            // }
                if (0 <= adjustedX && adjustedX <= 1.0f && 0 <= adjustedY && adjustedY <= 1.0f)
            {
                screenPosition = new Vector2(adjustedX, adjustedY);
                lineRenderer.SetPositions(new Vector3[] { ray.origin, intersectionPoint });
                Debug.Log($"Screen coordinates: {screenPosition}");

                UdpSender udpSender = GetComponent<UdpSender>();
                if (udpSender != null)
                {
                    udpSender.SendCastCoordData(screenPosition, ray.origin, ray.direction, arCamera.transform.position, arCamera.transform.rotation);
                    // udpSender.sendCastData(screenPosition); 
                    // udpSender.sendRayCastData(ray.origin,ray.direction);
                }
            }
        }
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
