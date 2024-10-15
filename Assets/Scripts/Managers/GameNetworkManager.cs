using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public class GameNetworkManager : NetworkManager
{
    [SerializeField]
    public enum GameState
    {
        Lobby,
        InGame,
        End
    }

    public NetworkList<int> playersInfo; //gonna make this into a class/struct;


    private void Update()
    {
        if (!IsServer) return;

        switch (PrefabRefManager.Instance.serverVars.gamemodeState.Value)
        {
            case GameState.Lobby:
                if (PrefabRefManager.Instance.serverVars.matchTimer.Value != 90f) PrefabRefManager.Instance.serverVars.matchTimer.Value = 90f;
                if (PrefabRefManager.Instance.serverVars.matchStarted.Value) PrefabRefManager.Instance.serverVars.matchStarted.Value = false;
                PrefabRefManager.Instance.serverVars.redPoints.Value = 0;
                PrefabRefManager.Instance.serverVars.bluePoints.Value = 0;
                break;

            case GameState.InGame:

                if(PrefabRefManager.Instance.serverVars.matchStarted.Value)
                {
                    PrefabRefManager.Instance.serverVars.matchTimer.Value -= Time.deltaTime;
                    if (PrefabRefManager.Instance.serverVars.matchTimer.Value <= 0)
                    {
                        PawnNetworkController[] playerPawns = FindObjectsOfType<PawnNetworkController>();

                        foreach (PawnNetworkController ply in playerPawns)
                        {
                            ply.controller.EndServerOnClients_ClientRpc(new ClientRpcParams()
                            {
                                Send = new ClientRpcSendParams()
                                {
                                    TargetClientIds = new List<ulong>(ConnectedClientsIds)
                                }
                            });
                        }

                        PrefabRefManager.Instance.serverVars.gamemodeState.Value = GameState.End;
                    }
                }

                break;

            case GameState.End:
                PrefabRefManager.Instance.serverVars.matchTimer.Value = 0f;
                break;
        }
    }

    [ServerRpc]
    public void KillPlayer_ServerRpc(ulong playerID, ServerRpcParams rpcParams)
    {
        PawnNetworkController[] playerPawns = FindObjectsOfType<PawnNetworkController>();

        foreach (PawnNetworkController ply in playerPawns)
        {
            if(ply.OwnerClientId == playerID)
            {
                ply.isAlive = false;

                ply.pointsOnPlayer.Value = 0;
                break;
            }
        }
    }

    [ServerRpc]
    public void CreatePlayers_ServerRpc(ServerRpcParams rpcParams)
    {
        //if (rpcParams.Receive.SenderClientId != LocalClientId) return false;

        StartCoroutine(ServerStart());
    }

    IEnumerator ServerStart()
    {
        PrefabRefManager.Instance.serverVars.gamemodeState.Value = GameState.InGame;

        bool isBlueTeam = false;

        Dictionary<ulong, NetworkClient> clients = new Dictionary<ulong, NetworkClient>(ConnectedClients);
        List<Transform> spawnPos_TeamRed = new List<Transform>(PrefabRefManager.Instance.spawnPos_TeamRed);
        List<Transform> spawnPos_TeamBlue = new List<Transform>(PrefabRefManager.Instance.spawnPos_TeamBlue);

        clients.OrderBy(x => Random.value);

        foreach (KeyValuePair<ulong, NetworkClient> kvp in clients)
        {
            yield return new WaitForFixedUpdate();

            spawnPos_TeamRed.OrderBy(x => Random.value);
            spawnPos_TeamBlue.OrderBy(x => Random.value);

            int i = Random.Range(0, !isBlueTeam ? spawnPos_TeamRed.Count : spawnPos_TeamBlue.Count);

            yield return new WaitForFixedUpdate();

            GameObject g = Instantiate(PrefabRefManager.Instance.playerPawn, !isBlueTeam ? spawnPos_TeamRed[i] : spawnPos_TeamBlue[i]);
            NetworkObject no = g.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(kvp.Key, true);

            yield return new WaitForFixedUpdate();

            //no.GetComponent<Rigidbody>().position = !isBlueTeam ? spawnPos_TeamRed[i].position : spawnPos_TeamBlue[i].position;

            yield return new WaitForFixedUpdate();

            PlayerNetworkController[] playControls = FindObjectsOfType<PlayerNetworkController>();

            yield return new WaitForSeconds(0.05f);

            foreach (PlayerNetworkController ply in playControls)
            {
                if (ply.OwnerClientId == kvp.Key)
                {
                    var infoToSend = g.GetComponent<PawnNetworkController>();

                    ply.SetUpPawn_ClientRpc(infoToSend, !isBlueTeam ? 7 : 6, !isBlueTeam ? spawnPos_TeamRed[i].position : spawnPos_TeamBlue[i].position, new ClientRpcParams()
                    {
                        Send = new ClientRpcSendParams()
                        {
                            TargetClientIds = new ulong[] { kvp.Key }
                        }
                    });

                    List<ulong> otherClients = new List<ulong>(ConnectedClientsIds);
                    otherClients.Remove(kvp.Key);

                    ply.SetUpPawnOnDifferentClients_ClientRpc(infoToSend, !isBlueTeam ? 7 : 6, new ClientRpcParams()
                    {
                        Send = new ClientRpcSendParams()
                        {
                            TargetClientIds = otherClients
                        }
                    });

                    break;
                }
            }

            if (!isBlueTeam) spawnPos_TeamRed.RemoveAt(i);
            else spawnPos_TeamBlue.RemoveAt(i);

            isBlueTeam = !isBlueTeam;
        }

        PrefabRefManager.Instance.serverVars.matchStarted.Value = true;
    }

}
