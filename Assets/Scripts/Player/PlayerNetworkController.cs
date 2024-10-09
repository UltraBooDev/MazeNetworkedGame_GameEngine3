using System.Collections.Generic;
using Unity.Netcode;

using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkController : NetworkBehaviour
{
    public NetworkVariable<int> playerScore = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);



    private void Update()
    {
        if (!IsOwner) return;
    }
}
