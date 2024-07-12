using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpSender : MonoBehaviour
{
    private UdpClient udpClient;
    public string host = "127.0.0.1"; // The IP address of the receiver
    public int port = 9000; // The port on which data will be sent
    public float sendInterval = 0.1f; // Time interval in seconds between sends

    private float timeSinceLastSend = 0.0f;

    void Start()
    {
        udpClient = new UdpClient(); // Initialize the UDP client
        Input.gyro.enabled = true; // Enable the gyroscope
    }

    void Update()
    {
        timeSinceLastSend += Time.deltaTime;

        if (timeSinceLastSend >= sendInterval)
        {
            SendGyroData();
            timeSinceLastSend = 0;
        }
    }

    void SendGyroData()
    {
        Quaternion attitude = Input.gyro.attitude;
        Vector3 rotationRate = Input.gyro.rotationRate;

        string message = $"{attitude.x},{attitude.y},{attitude.z},{attitude.w},{rotationRate.x},{rotationRate.y},{rotationRate.z}";
        byte[] bytesToSend = Encoding.ASCII.GetBytes(message); // Convert string to byte array

        udpClient.SendAsync(bytesToSend, bytesToSend.Length, host, port); // Send the byte array to the host
        Debug.Log("Sent Gyro Data: " + message);
    }

    void OnDestroy()
    {
        udpClient.Close(); // Close the UDP client when the object is destroyed
        Input.gyro.enabled = false; // Disable the gyroscope when not needed
    }
}
