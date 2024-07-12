using UnityEngine;
using Mirror;

public class DataSender : NetworkBehaviour
{
    // Assuming the iPad acts as a server
    private void Update()
    {
        if (isServer)
        {
            Debug.Log("Did ya make it herw zainab "); 
            SendDataToClients();
        }
    }

    // This method is called to send data to all clients
    [ClientRpc]
    void RpcSendDataToClients()
    {
        Vector3 dummyPosition = new Vector3(1.0f, 2.0f, 3.0f); // Example position
        Quaternion dummyRotation = Quaternion.Euler(45.0f, 30.0f, 60.0f); // Example rotation
        Debug.Log($"Zainab Sending data to clients - Position: {dummyPosition}, Rotation: {dummyRotation}");
    }

    // Method to invoke the RPC
    void SendDataToClients()
    {
        RpcSendDataToClients();
    }
}
