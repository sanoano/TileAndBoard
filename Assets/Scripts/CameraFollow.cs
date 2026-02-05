using System;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }


    private void Update()
    {

        if (IsOwner)
        {
            transform.position = cam.gameObject.transform.position;
            transform.rotation = cam.gameObject.transform.rotation;
            
            foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientIds == NetworkManager.LocalClientId) continue;
                SendPositionRpc(transform.position, transform.rotation, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
            }
        }
        
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendPositionRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams = default)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
