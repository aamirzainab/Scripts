using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RuntimeHandle;

public class PolygonDrawer : MonoBehaviour
{
    [SerializeField] private Camera arCamera; 
    public GameObject linePrefab, centralObjectPrefab, raycastLinePrefab, planePrefab;
    private List<GameObject> lineObjects = new List<GameObject>();
    public Vector3[] vertices;
    private GameObject centralObject, planeObject, raycastLine;
    public RuntimeTransformHandle transformHandle;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float lastTapTime = 0;  
    private const float doubleTapDelay = 0.3f;  

    void Update()
    {
        HandleTouches();
        UpdateTransformations();
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

    void UpdateTransformations()
    {
        if (centralObject)
        {
            Vector3 currentPosition = centralObject.transform.position;
            Quaternion currentRotation = centralObject.transform.rotation;

            if (currentPosition != lastPosition)
            {
                MovePolygon(currentPosition - lastPosition);
                lastPosition = currentPosition;
            }
            if (currentRotation != lastRotation)
            {
                RotatePolygon(currentRotation * Quaternion.Inverse(lastRotation));
                lastRotation = currentRotation;
            }
        }
    }

    // public void SetupPolygon(Vector3 qrPosition, Quaternion qrRotation)
    // {
    //     CalculateVertices(qrPosition, qrRotation);
    //     InitializeObjects();
    //     lastPosition = centralObject.transform.position;
    //     lastRotation = centralObject.transform.rotation;
    // }

    public void SetupPolygon(Vector3 qrPosition, Quaternion qrRotation,GameObject planeObject)
    {
        // SetupPlanePrefab(qrPosition, qrRotation);
        InitializeGizmo(planeObject);
        lastPosition = centralObject.transform.position;
        lastRotation = centralObject.transform.rotation;
    }
    public void SetupPlanePrefab(Vector3 qrPosition, Quaternion qrRotation, GameObject existingPlaneObject)
    {
        Debug.Log("Zainab Rotation Angles: " + qrRotation.eulerAngles);

        // Offsets to position the center of the plane based on your specific needs
        float forwardOffset = 1.4986f;  // Distance forward from the QR code
        float upwardOffset = 0.8128f;   // Distance upward from the QR code
        float width = 1.86f;            // Width of the plane
        float height = 0.726f;          // Height of the plane

        // Drawing rays for visual debugging (helpful to see in the Scene view)
        // Debug.DrawRay(qrPosition, Vector3.forward * 10, Color.blue, 120.0f);
        // Debug.DrawRay(qrPosition, Vector3.up * 10, Color.green, 120.0f);
        // Debug.DrawRay(qrPosition, Vector3.right * 10, Color.red, 120.0f);

        // Calculate the center position of the plane using only the QR code's position and local offsets
        

        // Resetting rotation to make the plane face upward regardless of the QR code's rotation
        existingPlaneObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        Vector3 centerPosition = qrPosition + new Vector3(forwardOffset, upwardOffset, 0);

        Debug.Log("Zainab this is the centerPosition being used to move the cube " + centerPosition);
        existingPlaneObject.transform.SetPositionAndRotation(centerPosition, Quaternion.Euler(0, 0, 0));
        existingPlaneObject.transform.localScale = new Vector3(width, height, 20.00f);
    }

        // existingPlaneObject.transform.localPosition -= new Vector3(width / 2, 0, height / 2);
        // planeObject = Instantiate(planePrefab, centerPosition, qrRotation );
        // planeObject.transform.localScale = new Vector3(width, height, 5.00f);  
        // planeObject.transform.localPosition -= new Vector3(width / 2, 0, height / 2);


    void InitializeObjects()
    {
        InitializePlaneObject();
        DrawPolygon();
        PositionCentralObject();
        // InitializeGizmo();
    }
    public void InitializeGizmo(GameObject planeObject)
    {
        if (transformHandle != null && planeObject != null)
        {
            transformHandle.target = planeObject.transform;
            transformHandle.gameObject.SetActive(true);
        }
    }
    private void PositionCentralObject()
    {
        Vector3 centroid = CalculateCentroid();
        if (centralObject == null)
        {
            centralObject = Instantiate(centralObjectPrefab, centroid, Quaternion.identity, transform);
            centralObject.tag = "Selectable";
        }
        else
        {
            centralObject.transform.position = centroid;
        }
    }
    private void CalculateVertices(Vector3 qrPosition, Quaternion qrRotation)
    {   
        float forwardOffset = 1.4986f;
        float upwardOffset = 0.8128f;
        float width = 1.86f;
        float height = 0.726f;
        float rightOffset = width / 2.0f;

        Vector3 bottomRight = qrPosition + qrRotation * new Vector3(forwardOffset, upwardOffset, rightOffset);
        // Vector3 bottomRight = qrPosition + qrRotation * new Vector3(upwardOffset, forwardOffset, rightOffset);
        vertices = new Vector3[]
        {
            bottomRight,
            bottomRight + qrRotation * new Vector3(0, 0, -width),
            bottomRight + qrRotation * new Vector3(0, height, -width),
            bottomRight + qrRotation * new Vector3(0, height, 0)
        };
        
    }
    private Quaternion CalculatePlaneRotation()
    {
        if (vertices.Length >= 3)
        {
            Vector3 edge1 = vertices[1] - vertices[0];
            Vector3 edge2 = vertices[2] - vertices[0];
            Vector3 normal = Vector3.Cross(edge1, edge2).normalized;
            return Quaternion.LookRotation(normal);
        }
        return Quaternion.identity;
    }


    void InitializePlaneObject()
    {
        if (planeObject != null) Destroy(planeObject);
        Quaternion planeRotation = CalculatePlaneRotation();
        planeObject = Instantiate(planePrefab, CalculateCentroid(), planeRotation, transform);
        UpdateMesh(planeObject);
    }

    // void UpdateMesh(GameObject obj)
    // {
    //     Mesh mesh = new Mesh();
    //     obj.GetComponent<MeshFilter>().mesh = mesh;
    //     mesh.vertices = vertices;
    //     mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
    //     mesh.RecalculateNormals();
    //     mesh.RecalculateBounds();

    //     var meshCollider = obj.GetComponent<MeshCollider>() ?? obj.AddComponent<MeshCollider>();
    //     meshCollider.sharedMesh = mesh;
    // }

void UpdateMesh(GameObject obj)
    {
        Debug.Log("Zainab - Updating Mesh for GameObject: " + obj.name);

        Mesh mesh = new Mesh();
        Vector3[] adjustedVertices = new Vector3[vertices.Length];
        Quaternion inverseRotation = Quaternion.Inverse(obj.transform.rotation);

        for (int i = 0; i < vertices.Length; i++)
        {
            adjustedVertices[i] = inverseRotation * (vertices[i] - obj.transform.position);
            Debug.Log("Zainab - Original Vertex " + i + ": " + vertices[i] + ", Adjusted Vertex: " + adjustedVertices[i]);
        }

        mesh.vertices = adjustedVertices;
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };

        Debug.Log("Zainab - Mesh vertices and triangles set. Recalculating normals and bounds.");

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider meshCollider = obj.GetComponent<MeshCollider>() ?? obj.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

    }

    void DrawPolygon()
    {
        ClearLines();
        foreach (var vertexIndex in System.Linq.Enumerable.Range(0, vertices.Length))
        {
            GameObject line = Instantiate(linePrefab, transform);
            var lineRenderer = line.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[] { vertices[vertexIndex], vertices[(vertexIndex + 1) % vertices.Length] });
            lineObjects.Add(line);
        }
        if (planeObject) UpdateMesh(planeObject);
    }

    void MovePolygon(Vector3 translation)
    {
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] += translation;
        DrawPolygon();
    }

    void RotatePolygon(Quaternion rotation)
    {
        Vector3 centroid = CalculateCentroid();
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = rotation * (vertices[i] - centroid) + centroid;
        DrawPolygon();
    }

    Vector3 CalculateCentroid()
    {
        Vector3 centroid = Vector3.zero;
        foreach (var vertex in vertices)
            centroid += vertex;
        return centroid / vertices.Length;
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
            transformHandle.gameObject.SetActive(false);
        if (centralObject != null)
            centralObject.SetActive(false);
        StartCoroutine(SnapBorderToScreenPlane());
    }


    IEnumerator SnapBorderToScreenPlane()
    {
    yield return new WaitForEndOfFrame();
    Vector3 center = CalculateCentroid();
    Ray ray = new Ray(arCamera.transform.position, arCamera.transform.forward);
    Debug.DrawRay(arCamera.transform.position, arCamera.transform.forward * 100f, Color.red, 2f);

    if (Physics.Raycast(ray, out RaycastHit hit, 100f))
    {
        Debug.Log("zainab Raycast hit: " + hit.collider.name);
        VisualizeRaycast(ray.origin, hit.point);
    }
    else
    {
        Debug.Log("Zainab No hit detected");
    }
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
