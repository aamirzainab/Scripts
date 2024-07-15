using System.Collections.Generic;
using System.Collections;
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
    private GameObject screen = null ; 

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

    private void SpawnQRPrefab(ARTrackedImage trackedImage)
    {
        Vector3 position = trackedImage.transform.position;
        Quaternion rotation = trackedImage.transform.rotation;
        // Debug.Log("Zainab Initial Position: " + position + "\nZainab Initial Rotation: " + rotation.eulerAngles);

        if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
        }
    }
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

    // void SetupDigitalScreen(ARTrackedImage trackedImage)
    // {
    //     ARTrackedImage image = trackedImage;

    //     // Assuming 'trackedImages' is a dictionary and you know the key, or it only contains one entry
    //     // if (trackedImages.Count > 0)
    //     // {
    //     //     image = trackedImages["oneSmall"];// Grabbing the first image assuming there's only one
    //     // }
    //     // image = trackedImages["oneSmall"]; 
    //     // image

    //     if (image != null)
    //     {
    //         Vector3 position = image.transform.position;
    //         Quaternion rotation = image.transform.rotation;
    //         Quaternion desiredRotation = image.transform.rotation;

    //         // Adjust the plane's rotation so that its forward vector matches the image's outward normal
    //         desiredRotation *= Quaternion.Euler(90, 0, 0);

    //         // Log the position and rotation for debugging
    //         Debug.Log("Zainab this is now instantiating at position: " + position + " with rotation: " + rotation.eulerAngles);

    //         // Instantiate or update the position and rotation of the digital screen
    //         if (screen == null )
    //         {
    //         GameObject screen = Instantiate(planePrefab, position, desiredRotation);

    //         // screen.transform.localScale = new Vector3(0.54f, 0.325f, 0.02f); 
    //         screen.transform.localScale = new Vector3(1.7f, 0.71f, 0.02f); // Assuming these are the desired dimensions
    //         ARAnchor anchor = screen.AddComponent<ARAnchor>();
    //         }

    //     }
    //     // else
    //     // {
    //     //     Debug.LogError("No tracked image available for instantiation.");
    //     // }
    // }
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
    }
