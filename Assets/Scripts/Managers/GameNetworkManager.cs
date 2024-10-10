using System.Collections.Generic;
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
                break;

            case GameState.InGame:

                if(PrefabRefManager.Instance.serverVars.matchStarted.Value)
                {
                    PrefabRefManager.Instance.serverVars.matchTimer.Value -= Time.deltaTime;
                    if (PrefabRefManager.Instance.serverVars.matchTimer.Value <= 0)
                    {
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
    public bool CreatePlayers_ServerRpc(ServerRpcParams rpcParams)
    {
        if (rpcParams.Receive.SenderClientId != LocalClientId) return false;

        PrefabRefManager.Instance.serverVars.matchStarted.Value = true;
        PrefabRefManager.Instance.serverVars.gamemodeState.Value = GameState.InGame;

        bool isBlueTeam = false;

        Dictionary<ulong, NetworkClient> clients =  new Dictionary<ulong, NetworkClient>(ConnectedClients);
        List<Transform> spawnPos_TeamRed = new List<Transform>(PrefabRefManager.Instance.spawnPos_TeamRed);
        List<Transform> spawnPos_TeamBlue = new List<Transform>(PrefabRefManager.Instance.spawnPos_TeamBlue);

        clients.OrderBy(x => Random.value);

        foreach (KeyValuePair<ulong, NetworkClient> kvp in clients)
        {
            int i = Random.Range(0, !isBlueTeam ? spawnPos_TeamRed.Count - 1 : spawnPos_TeamBlue.Count - 1);

            GameObject g = Instantiate(PrefabRefManager.Instance.playerPawn, !isBlueTeam ? spawnPos_TeamRed[i]: spawnPos_TeamBlue[i]);
            NetworkObject no = g.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(kvp.Key);

            if (!isBlueTeam) spawnPos_TeamRed.RemoveAt(i);
            else spawnPos_TeamBlue.RemoveAt(i);

            PlayerNetworkController[] playControls = FindObjectsOfType<PlayerNetworkController>();

            foreach(PlayerNetworkController ply in playControls)
            {
                if(ply.OwnerClientId == kvp.Key)
                {
                    var infoToSend = g.GetComponent<PawnNetworkController>();

                    ply.SetUpPawn_ClientRpc(infoToSend, !isBlueTeam ? 7 : 6, new ClientRpcParams()
                    {
                        Send = new ClientRpcSendParams()
                        {
                            TargetClientIds = new ulong[] {kvp.Key}
                        }
                    });

                    List<ulong> otherClients = new List<ulong> (ConnectedClientsIds);
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

            isBlueTeam = !isBlueTeam;
        }

        return true;

    }

    [ServerRpc]
    public void GameEnd_Calculate_ServerRpc(ServerRpcParams rpcParams)
    {
        if (rpcParams.Receive.SenderClientId != LocalClientId) return;



    }

}
