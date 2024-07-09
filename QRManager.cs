using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRManager : MonoBehaviour
{
    private ARTrackedImageManager _trackedImageManager;
    public GameObject qrCodePrefab;
    public GameObject cubePrefab;  
    public GameObject planePrefab;  
    private ARAnchorManager _anchorManager;
    [SerializeField] private ARRaycastManager raycastManager; 

    private Dictionary<string, GameObject> _spawnedPrefabs = new Dictionary<string, GameObject>();
    private List<ARAnchor> _anchors = new List<ARAnchor>();
    private List<Vector3> _trackedPositions = new List<Vector3>();

    private GameObject _centerCube = null;

    private void Awake()
    {
        _trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
        _anchorManager = FindObjectOfType<ARAnchorManager>();
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
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateQRPrefab(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RemoveQRPrefab(trackedImage);
        }

        if (_anchors.Count == 3) 

        {
            CalculateCenterAndSetupPolygon();
        }
    }

    private void SpawnQRPrefab(ARTrackedImage trackedImage)
    {
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;
        Debug.Log("Zainab Initial Position: " + position + ", Initial Rotation: " + rotation.eulerAngles);

        if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
            float width = 1.86f;            
            float height = 0.726f;     
            // 32 inches forward, 29.5 - height to bottom right,  34 inches right 
            float forward = 0.818f; // 0.818
            float upward =  1.1938f;  // 1.1938
            float right = width /2.0f ;  

            List<ARRaycastHit> hits = new List<ARRaycastHit>(); 
            Vector3 raycastDirection = Vector3.down;

            if (trackedImage.referenceImage.name == "one")
            {
                    Vector3 originalSize = planePrefab.GetComponent<BoxCollider>().size;
                    Debug.Log("Zainab Original Size: " + originalSize);

                 if (raycastManager.Raycast(position + Vector3.up * 0.1f, hits, TrackableType.Planes)) {
                    // Debug.Log("Zainab did you enter this condition"); 

                    ARRaycastHit hit = hits[0];
                    
                    Vector3 forwardOffset = rotation * Vector3.forward * 1.8f; // 1.8 meters forward
                    Vector3 upwardOffset = rotation * Vector3.up * 0.8f; // 0.8 meters up

                    // Position of the plane prefab should take into account these offsets
                    Vector3 planePosition = hit.pose.position + forwardOffset + upwardOffset;

                    Quaternion flatRotation = Quaternion.LookRotation(Vector3.forward, hit.pose.up);


                    GameObject planePrefabInstance = Instantiate(planePrefab, planePosition, flatRotation);
                    planePrefabInstance.transform.localScale = new Vector3(1.86f, 0.726f, 0.02f); 
                    ARAnchor anchor = planePrefabInstance.AddComponent<ARAnchor>();
                    _anchors.Add(anchor);
                    PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
                    if (polygonDrawer != null)
                    {
                        // Debug.Log("Nanabooboo"); 
                        polygonDrawer.InitializeGizmo(planePrefabInstance); 
                        //  polygonDrawer.SetupPlanePrefab(position, uprightRotation, planePrefabInstance);
                        // polygonDrawer.SetupPolygon(position, uprightRotation); // Adjust arguments if necessary for your setup
                    }

                 }

                // Quaternion uprightRotation = Quaternion.Euler(90, 0, 0) * rotation;
                // // Vector3 centerPosition = position + new Vector3(forward, upward, 0);
                // Vector3 centerPosition = position + rotation * new Vector3(forward, upward, right);

                
                // Debug.Log("Zainab Upright Rotation: " + uprightRotation.eulerAngles);
                // Debug.Log("Zainab Center Position for Plane: " + centerPosition);
                // Debug.Log("Zainab this is the centerPosition being used to move the cube " + centerPosition);
                // Quaternion finalRotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0); // This keeps the rotation around the Y-axis but resets others.
                // Debug.Log("Zainab Final Rotation: " + finalRotation);
                // GameObject planePrefabInstance = Instantiate(planePrefab, centerPosition, finalRotation);
                // planePrefabInstance.transform.localScale = new Vector3(width, height, 0.02f);
                // // planePrefabInstance.transform.SetParent(qrPrefab.transform, false); // not sure about this 
                // ARAnchor anchor = planePrefabInstance.AddComponent<ARAnchor>();
                // _anchors.Add(anchor);
                // Debug.Log("Zainab Anchor position at creation: " + position + " for " + trackedImage.referenceImage.name);
                // PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
                // if (polygonDrawer != null)
                // {
                //     Debug.Log("Nanabooboo"); 
                //     //  polygonDrawer.SetupPlanePrefab(position, uprightRotation, planePrefabInstance);
                //     // polygonDrawer.SetupPolygon(position, uprightRotation); // Adjust arguments if necessary for your setup
                // }
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
    private Quaternion AverageRotation(List<ARAnchor> anchors)
    {
        Vector4 cumulative = Vector4.zero;
        foreach (var anchor in anchors)
        {
            cumulative += new Vector4(anchor.transform.rotation.x, anchor.transform.rotation.y, anchor.transform.rotation.z, anchor.transform.rotation.w);
        }
        cumulative /= anchors.Count;
        return new Quaternion(cumulative.x, cumulative.y, cumulative.z, cumulative.w).normalized;
    }


    private void CalculateCenterAndSetupPolygon()
    {
        Vector3 centerPosition = Vector3.zero;
        Quaternion averageRotation = AverageRotation(_anchors);
        foreach (var anchor in _anchors)
        { 
            centerPosition += anchor.transform.position;
        }
        Debug.Log("Zainab Average Rotation: " + averageRotation);
        centerPosition /= 3;
        Debug.Log("Zainab Center Position: " + centerPosition);
        // centerPosition.y = _trackedPositions[0].y;  
        if (_centerCube == null)
        {
            _centerCube = Instantiate(cubePrefab, centerPosition, Quaternion.identity);
            // _centerCube = Instantiate(cubePrefab, centerPosition, averageRotation);
            // SetARWorldOrigin(centerPosition);
        }
        else
        {
            _centerCube.transform.position = centerPosition;
        }
        PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
        if (polygonDrawer != null)
        {
        // {   Debug.Log("Zainab drawing polygon here"); 
            // polygonDrawer.SetupPolygon(centerPosition, averageRotation); 
        }
    }


    private void SetARWorldOrigin(Vector3 newOrigin)
    {
        ARSessionOrigin arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (arSessionOrigin != null)
        {
            // Reset world origin to the new calculated center position
            arSessionOrigin.MakeContentAppearAt(arSessionOrigin.transform, newOrigin, Quaternion.identity);
            Debug.Log("World origin set to new center position.");
        }
    }
}
