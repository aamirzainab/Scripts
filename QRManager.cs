using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;


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
    private Dictionary<string, ARTrackedImage> trackedImages = new Dictionary<string, ARTrackedImage>();
    
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
            // SpawnQRPrefab(trackedImage);
            Debug.Log("Detected zainab with this name  " + trackedImage.referenceImage.name ); 
            SetupDigitalScreen( trackedImage ); 
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

        if (_anchors.Count == 4) 

        {
            Debug.Log("Zainab lets start with the new set up "); 
            // SetupDigitalScreen(); 
            // CalculateCenterAndSetupPolygon();
        }
    }

    private void SpawnQRPrefab(ARTrackedImage trackedImage)
    {
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;
        Debug.Log("Zainab Initial Position: " + position + "\nZainab Initial Rotation: " + rotation.eulerAngles);

        if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
            float width = 1.7f;            
            // float height = 0.726f;     
            // 32 inches forward, 29.5 - height to bottom right,  34 inches right 
            float forward = 1.1f; // 0.818
            float upward =  1.1f; // 1.1936 
            float right = width / 2.0f ;  

            List<ARRaycastHit> hits = new List<ARRaycastHit>(); 
            Vector3 raycastDirection = Vector3.down;

            if (trackedImage.referenceImage.name == "two" || trackedImage.referenceImage.name == "one"  )
            {
                    Vector3 originalSize = planePrefab.GetComponent<BoxCollider>().size;
                    SetARWorldOrigin();

                 if (raycastManager.Raycast(position + Vector3.up * 0.01f, hits, TrackableType.Planes)) {
                    ARRaycastHit hit = hits[0];



                    Vector3 forwardOffset = Vector3.forward * forward ;  
                    Vector3 upwardOffset =  Vector3.up * upward ; 
                    Vector3 rightOffset =   Vector3.right * right ; 
                    Debug.DrawRay(hit.pose.position, forwardOffset, Color.blue, 5f);
                    Debug.DrawRay(hit.pose.position, upwardOffset, Color.green, 5f);
                    Debug.DrawRay(hit.pose.position, rightOffset, Color.red, 5f);
                    Debug.DrawRay(hit.pose.position, hit.pose.up * 2, Color.magenta, 5f);
                    Vector3 planePosition = hit.pose.position + forwardOffset + upwardOffset + rightOffset;
                    Quaternion flatRotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);

                    GameObject planePrefabInstance = Instantiate(planePrefab, planePosition, flatRotation);
                    planePrefabInstance.transform.localScale = new Vector3(1.7f, 0.71f, 0.02f); 
                    ARAnchor anchor = planePrefabInstance.AddComponent<ARAnchor>();
                    _anchors.Add(anchor);
                    PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
                    if (polygonDrawer != null)
                    {
                        polygonDrawer.InitializeObjects(planePrefabInstance); 
                    }

                 }
            }
        }
    }
    // void SetupDigitalScreen()
    // {
    //     Vector3 averagePosition = Vector3.zero;
    //     Quaternion averageRotation = Quaternion.identity;
    //     int count = 0;

    //     foreach (var image in trackedImages.Values)
    //         {
    //             // averagePosition += image.transform.position;
    //             averagePosition = image.transform.position; 
    //             averageRotation = image.transform.rotation; 
    //         //     if (count == 0)
    //         //     {
    //         //         averageRotation = image.transform.rotation; // Start with the first rotation
    //         //     }
    //         //     else
    //         //     {
    //         //         float step = 1.0f / (count + 1);
    //         //         averageRotation = Quaternion.Slerp(averageRotation, image.transform.rotation, step);
    //         //     }
    //         //     count++;
    //         // }

    //         // if (count > 0)
    //         // {
    //         //     averagePosition /= count;
    //         // }
    //         }

    //     // Instantiate or update the position and rotation of the digital screen
    //     Debug.Log("Zainab this is now instantiating  " ); 
    //     GameObject screen = Instantiate(planePrefab, averagePosition, averageRotation);
    //     screen.transform.localScale = new Vector3(1.86f, 0.726f, 0.02f); // assuming these are the desired dimensions
    //     ARAnchor anchor = screen.AddComponent<ARAnchor>();
    // }


void SetupDigitalScreen(ARTrackedImage trackedImage)
{
    ARTrackedImage image = trackedImage;

    // Assuming 'trackedImages' is a dictionary and you know the key, or it only contains one entry
    // if (trackedImages.Count > 0)
    // {
    //     image = trackedImages["oneSmall"];// Grabbing the first image assuming there's only one
    // }
    // image = trackedImages["oneSmall"]; 
    // image

    if (image != null)
    {
        Vector3 position = image.transform.position;
        Quaternion rotation = image.transform.rotation;
        Quaternion desiredRotation = image.transform.rotation;

        // Adjust the plane's rotation so that its forward vector matches the image's outward normal
        desiredRotation *= Quaternion.Euler(90, 0, 0);

        // Log the position and rotation for debugging
        Debug.Log("Zainab this is now instantiating at position: " + position + " with rotation: " + rotation.eulerAngles);

        // Instantiate or update the position and rotation of the digital screen
        GameObject screen = Instantiate(planePrefab, position, desiredRotation);
        screen.transform.localScale = new Vector3(0.54f, 0.325f, 0.02f); // Assuming these are the desired dimensions
        ARAnchor anchor = screen.AddComponent<ARAnchor>();
    }
    // else
    // {
    //     Debug.LogError("No tracked image available for instantiation.");
    // }
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


    private void SetARWorldOrigin()
    {
        ARSessionOrigin arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        if (arSessionOrigin != null)
        {
            // Reset world origin to the new calculated center position
            // arSessionOrigin.MakeContentAppearAt(arSessionOrigin.transform, newOrigin, Quaternion.identity);
            arSessionOrigin.transform.rotation = Quaternion.identity;
        }
    }
}
