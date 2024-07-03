using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class QRManager : MonoBehaviour
{
    private ARTrackedImageManager _trackedImageManager;
    public GameObject qrCodePrefab;
    public GameObject cubePrefab;  
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

        // if (_trackedPositions.Count == 3)
        // {
        //     CalculateCenterAndSetupPolygon();
        // }
        if (_anchors.Count == 3) 

        {
            CalculateCenterAndSetupPolygon();
        }
    }

    // private void SpawnQRPrefab(ARTrackedImage trackedImage)
    // {
    //     Vector3 position = trackedImage.transform.position;
    //     Quaternion rotation = trackedImage.transform.rotation; 

    //     if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
    //     {
    //         // GameObject qrPrefab = Instantiate(qrCodePrefab, position, Quaternion.identity);
    //         GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
    //         _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;
    //         _trackedPositions.Add(position);

    //         if (_trackedPositions.Count > 3)
    //         {
    //             _trackedPositions.RemoveAt(0);  
    //         }
    //     }
    // }
private void SpawnQRPrefab(ARTrackedImage trackedImage)
{
    Vector3 position = trackedImage.transform.position;
    Quaternion rotation = trackedImage.transform.rotation; 

    if (!_spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
    {
        GameObject qrPrefab = Instantiate(qrCodePrefab, position, rotation);
        _spawnedPrefabs[trackedImage.referenceImage.name] = qrPrefab;

        GameObject anchorObject = new GameObject("Anchor-" + trackedImage.referenceImage.name);
        anchorObject.transform.position = position;
        anchorObject.transform.rotation = rotation;

        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();
        _anchors.Add(anchor);
        Debug.Log("Zainab Anchor position at creation: " + position + " for " + trackedImage.referenceImage.name);
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
            polygonDrawer.SetupPolygon(centerPosition, Quaternion.identity); 
            // polygonDrawer.SetupPolygon(centerPosition,averageRotation);
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
