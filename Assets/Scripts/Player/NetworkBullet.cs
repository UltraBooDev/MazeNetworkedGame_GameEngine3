using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] float bulletLifeTime = 5f;

    private Rigidbody rb;
    [HideInInspector]public ulong bulletOwner;

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        if(IsServer)StartCoroutine(DestroyBullet());
    }

    private void Update()
    {
        //if (!IsServer) return;

        rb.velocity = transform.forward * bulletSpeed;
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(bulletLifeTime);
        KillBullet();
    }

    void KillBullet()
    {
        gameObject.GetComponent<NetworkObject>().Despawn(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        Debug.Log($"{other.gameObject.tag}");

        if(other.gameObject.tag == "Player")
        {
            PawnNetworkController player = other.gameObject.GetComponentInParent<PawnNetworkController>();

            if (player == null) return;
            if (player.OwnerClientId == bulletOwner) return;
            if (!player.isAlive) return;

            player.PlayerKill_ClientRpc(player.OwnerClientId, new ClientRpcParams()
            {
                Send = new ClientRpcSendParams()
                {
                    TargetClientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds)
                }
            });
            NetworkManager.Singleton.gameObject.GetComponent<GameNetworkManager>().KillPlayer_ServerRpc(player.OwnerClientId,new ServerRpcParams());

            KillBullet();
        }

        if (other.gameObject.tag == "Ground") KillBullet();

    }
}
