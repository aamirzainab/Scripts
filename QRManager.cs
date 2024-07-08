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

        if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
        {
            GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
            _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
            float width = 1.86f;            
            float height = 0.726f;         
            if (trackedImage.referenceImage.name == "one")
            {
                Quaternion uprightRotation = Quaternion.Euler(90, 0, 0) * rotation;
                Vector3 centerPosition = position + new Vector3(1.4986f, 0.8182f, 0);
                Debug.Log("Zainab this is the centerPosition being used to move the cube " + centerPosition);
                GameObject planePrefabInstance = Instantiate(planePrefab, centerPosition, rotation);
                planePrefabInstance.transform.localScale = new Vector3(width, height, 0.02f);
                // planePrefabInstance.transform.SetParent(qrPrefab.transform, false); // not sure about this 
                ARAnchor anchor = planePrefabInstance.AddComponent<ARAnchor>();

                _anchors.Add(anchor);
                Debug.Log("Zainab Anchor position at creation: " + position + " for " + trackedImage.referenceImage.name);
                PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
                if (polygonDrawer != null)
                {
                    Debug.Log("Nanabooboo"); 
                    //  polygonDrawer.SetupPlanePrefab(position, uprightRotation, planePrefabInstance);
                    // polygonDrawer.SetupPolygon(position, uprightRotation); // Adjust arguments if necessary for your setup
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


    // private void CalculateCenterAndSetupPolygon()
    // {
    //     Vector3 centerPosition = Vector3.zero;
    //     Quaternion averageRotation = AverageRotation(_anchors);
    //     foreach (var anchor in _anchors)
    //     { 
    //         centerPosition += anchor.transform.position;
    //     }
    //     Debug.Log("Zainab Average Rotation: " + averageRotation);
    //     centerPosition /= 3;
    //     Debug.Log("Zainab Center Position: " + centerPosition);
    //     // centerPosition.y = _trackedPositions[0].y;  
    //     if (_centerCube == null)
    //     {
    //         _centerCube = Instantiate(cubePrefab, centerPosition, Quaternion.identity);
    //         // _centerCube = Instantiate(cubePrefab, centerPosition, averageRotation);
    //         // SetARWorldOrigin(centerPosition);
    //     }
    //     else
    //     {
    //         _centerCube.transform.position = centerPosition;
    //     }
    //     PolygonDrawer polygonDrawer = GetComponent<PolygonDrawer>();
    //     if (polygonDrawer != null)
    //     {
    //     // {   Debug.Log("Zainab drawing polygon here"); 
    //         polygonDrawer.SetupPolygon(centerPosition, averageRotation); 
    //     }
    // }


    // private void SetARWorldOrigin(Vector3 newOrigin)
    // {
    //     ARSessionOrigin arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
    //     if (arSessionOrigin != null)
    //     {
    //         // Reset world origin to the new calculated center position
    //         arSessionOrigin.MakeContentAppearAt(arSessionOrigin.transform, newOrigin, Quaternion.identity);
    //         Debug.Log("World origin set to new center position.");
    //     }
    // }
}
