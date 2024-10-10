using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum PointType
{
    Extract,
    Deposit
}
public class PointsTrigger : NetworkBehaviour
{
    public PointType pointType;
    public int pointValue = 1;
    [SerializeField] float extractionTime = 1f;

    private bool extracting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            print(other.name);
            PawnNetworkController player = other.gameObject.GetComponentInParent<PawnNetworkController>();
            if (pointType == PointType.Extract)
            {
                if (player == null)
                {
                    Debug.Log("Player is null");
                    return;
                }
                ulong clientId = player.OwnerClientId;
                OnTriggerClientRpc(clientId, new ClientRpcParams()
                {
                    Send = new ClientRpcSendParams()
                    {
                        TargetClientIds = new List<ulong>() { clientId }
                    }
                });
                extracting = true;
                StartCoroutine(AddPointsToPlayer(player.GetComponent<PawnNetworkController>()));
            }
            else if (pointType == PointType.Deposit)
            {
                GetComponent<PointsBank>().AllocatePoints(player.pointsOnPlayer.Value, player.team);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (pointType == PointType.Extract)
        {
            extracting = false;
        }
    }

    private IEnumerator AddPointsToPlayer(PawnNetworkController player)
    {
        while (extracting)
        {
            yield return new WaitForSeconds(extractionTime);
            if (player != null)
            {
                player.pointsOnPlayer.Value += pointValue;
            }
        }
    }

    [ClientRpc]
    private void OnTriggerClientRpc(ulong clientId, ClientRpcParams rpcParams)
    {
        print($"Client {clientId}: points awarded");
    }
}
