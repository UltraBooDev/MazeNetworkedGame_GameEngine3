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

    public NetworkVariable<GameState> gamemodeState = new(
        GameState.Lobby,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<float> matchTimer = new(
        90f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> matchStarted = new(
    false,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);

    public NetworkList<int> playersInfo; //gonna make this into a class/struct;


    private void Update()
    {
        if (!IsServer) return;

        switch (gamemodeState.Value)
        {
            case GameState.Lobby:
                if (matchTimer.Value != 90f) matchTimer.Value = 90f;
                if (matchStarted.Value) matchStarted.Value = false;
                break;

            case GameState.InGame:

                if(matchStarted.Value)
                {
                    matchTimer.Value -= Time.deltaTime;
                    if (matchTimer.Value < 0) matchTimer.Value = 0f;
                }

                break;

            case GameState.End:
                break;
        }
    }

    [ServerRpc]
    public bool CreatePlayers_ServerRpc(ServerRpcParams rpcParams)
    {
        if (rpcParams.Receive.SenderClientId != LocalClientId) return false;

        //gamemodeState.Value = GameState.InGame;

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

                    ply.SetUpPawn_ClientRpc(infoToSend, new ClientRpcParams()
                    {
                        Send = new ClientRpcSendParams()
                        {
                            TargetClientIds = new ulong[] {kvp.Key}
                        }
                    });

                    break;
                }
            }

            isBlueTeam = !isBlueTeam;
        }

        return true;

    }

}
