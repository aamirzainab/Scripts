using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Linq;

public class UdpSender : MonoBehaviour
{
    private UdpClient udpClient;
    public string host = "130.245.4.133"; //  IP address of the receiver
    public int port = 8081; //  port on which data will be sent
    public float sendInterval = 0.1f; //  interval in seconds between sends
    public Camera arCamera;  
    private float timeSinceLastSend = 0.0f;

    void Start()
    {
        udpClient = new UdpClient(); 
        Debug.Log("did ya alrady come here zainab "); 
        Input.gyro.enabled = true; 
    }

    void Update()
    {
        timeSinceLastSend += Time.deltaTime;

        if (timeSinceLastSend >= sendInterval)
        {
            SendCameraData();
            timeSinceLastSend = 0;
        }
    }

public void SendCameraData()
    {
        if (arCamera != null)
        {
            Vector3 position = arCamera.transform.position;
            Quaternion rotation = arCamera.transform.rotation;

            string message = $"{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w}";
            byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
            udpClient.SendAsync(bytesToSend, bytesToSend.Length, host, port); 
            Debug.Log("Sent AR Camera Data: " + message);
        }
    }

    public void sendSpawnData(string name)
    {
            Vector3 position = arCamera.transform.position;
            Quaternion rotation = arCamera.transform.rotation;

        // string message = $"SPAWN {name}: {attitude.x},{attitude.y},{attitude.z},{attitude.w},{rotationRate.x},{rotationRate.y},{rotationRate.z}";
        string message = $"SPAWN {name}:{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w}";
        byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
        udpClient.SendAsync(bytesToSend, bytesToSend.Length, host, port); 
        Debug.Log("Sent Spawn Data Zainab: " + message);
    }
    public void sendRayCastData(Vector3 position, Quaternion rotation)
    {
            // Vector3 position = lineRenderer.transform.position;
            // Quaternion rotation = arCamera.transform.rotation;

        // string message = $"SPAWN {name}: {attitude.x},{attitude.y},{attitude.z},{attitude.w},{rotationRate.x},{rotationRate.y},{rotationRate.z}";
        string message = $"RAYCAST {name}:{position.x},{position.y},{position.z},{rotation.x},{rotation.y},{rotation.z},{rotation.w}";
        byte[] bytesToSend = Encoding.ASCII.GetBytes(message);
        udpClient.SendAsync(bytesToSend, bytesToSend.Length, host, port); 
        Debug.Log("Sent Raycast Data Zainab: " + message);
    }
    void OnDestroy()
    {
        udpClient.Close(); 
        Input.gyro.enabled = false; 
    }
}
