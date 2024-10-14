using System.Collections.Generic;
using Unity.Netcode;

using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerNetworkController : NetworkBehaviour
{
    public PawnNetworkController pawnObject;

    // 0 = Red Team, 1 = Blue Team
    public NetworkVariable<int> playerTeam = new(
    0,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Owner);



    private void Update()
    {
        if (!IsOwner) return;
    }

    [ClientRpc]
    public void SetUpPawn_ClientRpc(NetworkBehaviourReference pawnRef, int Team, ClientRpcParams clientRpcParams)
    {
        if(pawnRef.TryGet(out PawnNetworkController pawn))
        {
            pawnObject = pawn;
            pawn.controller = this;
        }

        pawn.ToggleCam(true);

        playerTeam.Value = Team == 7 ? 0 : 1;

        pawn.backpackMat.sharedMaterial = new Material(pawn.backpackMat.sharedMaterial);
        pawn.bodyMat.sharedMaterial = pawn.backpackMat.sharedMaterial;

        if (Team == 7) pawn.backpackMat.sharedMaterial.SetColor("_Color", Color.red);
        if (Team == 6) pawn.backpackMat.sharedMaterial.SetColor("_Color", Color.blue);

        SetLayerAllChildren(pawn.transform, Team);

        MainMenu_UI.Instance.StartGameHUD();
    }

    [ClientRpc]
    public void SetUpPawnOnDifferentClients_ClientRpc(NetworkBehaviourReference pawnRef, int Team, ClientRpcParams clientRpcParams)
    {
        if (pawnRef.TryGet(out PawnNetworkController pawn))
        {
            pawnObject = pawn;
            pawn.controller = this;
        }

        pawn.backpackMat.sharedMaterial = new Material(pawn.backpackMat.sharedMaterial);
        pawn.bodyMat.sharedMaterial = pawn.backpackMat.sharedMaterial;

        if (Team == 7) pawn.backpackMat.sharedMaterial.SetColor("_Color", Color.red);
        if (Team == 6) pawn.backpackMat.sharedMaterial.SetColor("_Color", Color.blue);

        SetLayerAllChildren(pawn.transform, Team);

    }

    void SetLayerAllChildren(Transform root, int layer)
    {
        var children = root.GetComponentsInChildren<Transform>(includeInactive: true);
        foreach (var child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
