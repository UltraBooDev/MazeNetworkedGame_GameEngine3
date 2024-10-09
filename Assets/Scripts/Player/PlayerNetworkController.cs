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

    public PawnNetworkController pawnObject;



    private void Update()
    {
        if (!IsOwner) return;
    }

    [ClientRpc]
    public void SetUpPawn_ClientRpc(NetworkBehaviourReference pawnRef, ClientRpcParams clientRpcParams)
    {
        if(pawnRef.TryGet(out PawnNetworkController pawn))
        {
            pawnObject = pawn;
        }

        MainMenu_UI.Instance.StartGameHUD();

    }
}
