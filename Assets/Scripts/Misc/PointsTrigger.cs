using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class PointsTrigger : NetworkBehaviour
{
    [SerializeField] protected int pointValue = 1;
    [SerializeField] protected float extractionTime = 1.5f;

    protected float pointTimer = 0f;

    protected List<PawnNetworkController> inTrigger = new List<PawnNetworkController>();

    protected virtual void Update()
    {
        if (!IsServer) return;

        if (GameNetVars.Instance.gamemodeState.Value != GameNetworkManager.GameState.InGame) return;

        if (inTrigger.Count <= 0) return;

        if (pointTimer >= extractionTime)
        {
            foreach (PawnNetworkController target in inTrigger)
            {
                if (!target.isAlive) continue;

                target.pointsOnPlayer.Value += pointValue;
            }

            pointTimer = 0f;
            return;
        }

        pointTimer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PawnNetworkController player = other.gameObject.GetComponent<PawnNetworkController>();

        if (player == null) return;
        if (inTrigger.Find(x => x.OwnerClientId == player.OwnerClientId)) return;

        inTrigger.Add(player);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        PawnNetworkController player = other.gameObject.GetComponent<PawnNetworkController>();

        if (player == null) return;
        if (!inTrigger.Find(x => x.OwnerClientId == player.OwnerClientId)) return;

        inTrigger.Remove(inTrigger.Find(x => x.OwnerClientId == player.OwnerClientId));
    }
}
