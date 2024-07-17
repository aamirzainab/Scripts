using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RuntimeHandle;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PolygonDrawer : MonoBehaviour
{
    [SerializeField] private Camera arCamera; 
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject centralObjectPrefab, raycastLinePrefab, planePrefab;
    private List<GameObject> lineObjects = new List<GameObject>();
    public Vector3[] vertices;
    private GameObject planeObjectScript, raycastLine;
    public RuntimeTransformHandle transformHandle;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float lastTapTime = 0;  
    private const float doubleTapDelay = 0.3f;  


 void Start()
{
    // Start the repeated sending of raycast data every 1 second
    // InvokeRepeating("SendRayCastData", 0, 0.50f);  // Starts immediately, repeats every 1 second
}

void Update()
{
    HandleTouches();
}

void SendRayCastData()
{
    Ray ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    float rayLength = 10.0f;

    // Set up the line renderer
    // lineRenderer.startWidth = 0.000001f;
    // lineRenderer.endWidth = 0.00001f;
    // lineRenderer.positionCount = 2;
    // lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
    // lineRenderer.startColor = Color.red;
    // lineRenderer.endColor = Color.red;
    // lineRenderer.SetPosition(0, ray.origin);
    // lineRenderer.SetPosition(1, ray.origin + ray.direction * rayLength);

    // Calculate endpoint and rotation
    Vector3 endpoint = ray.origin + ray.direction * rayLength;
    Quaternion rotation = Quaternion.LookRotation(ray.direction);
     UdpSender udpSender = GetComponent<UdpSender>();

    // Send the data
    if (udpSender != null)
    {
        // udpSender.sendRayCastData(endpoint, rotation);
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

    public void InitializeObjects(GameObject planeObject)
    {            
        planeObjectScript = planeObject;
        planeObjectScript.tag = "Selectable" ; 


        InitializeGizmo();
    }
    // public void InitializeObjects(ARAnchor planeObject)
    // {            
    //     // planeObjectScript = planeObject;
    //     planeObjectScript.tag = "Selectable" ; 


    //     InitializeGizmo();
    // }
    public void InitializeGizmo()
    {   

        if (transformHandle != null && planeObjectScript != null)
        {
            transformHandle.target = planeObjectScript.transform;
            transformHandle.gameObject.SetActive(true);
        }
    }

    void ClearLines()
    {
        foreach (var line in lineObjects)
            Destroy(line);
        lineObjects.Clear();
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
        }
    }

}
