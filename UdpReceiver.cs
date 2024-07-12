using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    public int listenPort = 9000; // Port to listen on
    private Thread receiveThread;

    void Start()
    {
        udpClient = new UdpClient(listenPort); // Bind the UDP client to the listening port
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start(); // Start the receiving thread
    }

    private void ReceiveData()
    {
        try
        {
            while (true)
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint); // Blocking call
                string receivedText = Encoding.ASCII.GetString(receivedBytes);
                Debug.Log("Received vector: " + receivedText);
                ParseAndUseData(receivedText);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving UDP: " + e.Message);
        }
    }

    void ParseAndUseData(string data)
    {
        string[] parts = data.Split(',');
        if (parts.Length == 3)
        {
            try
            {
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                Vector3 receivedVector = new Vector3(x, y, z);
                Debug.Log("Zainab Parsed vector: " + receivedVector);
                // Use the vector as needed in your application
            }
            catch (Exception e)
            {
                Debug.LogError("Error parsing vector data: " + e.Message);
            }
        }
    }

    void OnDestroy()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort(); // Stop the thread when the GameObject is destroyed
        }
        udpClient.Close(); // Close the UDP client
    }
}
