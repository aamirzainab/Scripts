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
    public LineRenderer lineRenderer;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float lastTapTime = 0;  
    private const float doubleTapDelay = 0.3f;  


    void Update()
    {
        HandleTouches();
        // UpdateTransformations();
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

        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        InitializeGizmo();
    }
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
        StartCoroutine(SnapBorderToScreenPlane());
    }

    IEnumerator SnapBorderToScreenPlane()
    {
        yield return new WaitForEndOfFrame(); 

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2); 
        Debug.Log("screencenter " + screenCenter); 
        Vector3 planePosition = Vector3.zero;
        Quaternion planeRotation = Quaternion.identity;
        Ray ray = arCamera.ViewportPointToRay(screenCenter);
        RaycastHit raycastHit;
        float rayLength = 10.0f; 
        // VisualizeRaycast(ray.origin, ray.direction * rayLength);

        lineRenderer.SetPosition(0, ray.origin);
        lineRenderer.SetPosition(1, ray.origin + ray.direction * rayLength);

        

        // if (raycastManager.Raycast(screenCenter, hits, TrackableType.Planes))
        // {
        //     foreach (var hit in hits)
        //     {
        //         ARPlane plane = planeManager.GetPlane(hit.trackableId);
        //         // Debug.Log("Zainab did you get here? "); 
        //         if (plane.alignment == PlaneAlignment.Vertical)
        //             // plane.size.x <= 1.0f && plane.size.y <= 0.5f)
        //         {
        //             planePosition = hit.pose.position;
        //             planeRotation = hit.pose.rotation;
        //             var renderer = plane.GetComponent<MeshRenderer>();
        //             Debug.Log("Zainab this is the plane " + plane); 


        //             // Instantiate or move your plane prefab
        //             // if (planeObjectScript != null)
        //             // {
        //             //     planeObjectScript.transform.position = planePosition;
        //             //     // centralObject.transform.rotation = planeRotation;
        //             //     Debug.Log("Zainab Plane snapped to vertical surface at: " + planePosition);
        //             // }
        //             // Vector3 start = screenCenter;
        //             // start = planeObjectScript.transform.position ; 
        //             // Vector3 end = hit.pose.position;
        //             // arCamera.ViewportPointToRay(screenCenter); 
        //             break; 
        //         }
        //     }
        // }
        // else
        // {
        //     Debug.Log("Zainab No suitable vertical plane detected.");
        // }
    }

    void VisualizeRaycast(Vector3 start, Vector3 end)
    {
        if (raycastLine == null)
            raycastLine = Instantiate(raycastLinePrefab, transform);
        var lineRenderer = raycastLine.GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { start, end });
    }
}
