using System.Collections.Generic;
using Unity.Netcode;

using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkController : NetworkBehaviour
{
    private void Update()
    {
        if (!IsOwner) return;
    }
}
