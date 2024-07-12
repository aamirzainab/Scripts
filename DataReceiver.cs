using UnityEngine;
using Mirror;

public class DataReceiver : NetworkBehaviour
{
    [ClientRpc]
    public void RpcReceiveData(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"Zainab Received data from iPad - Position: {position}, Rotation: {rotation}");
    }
}
