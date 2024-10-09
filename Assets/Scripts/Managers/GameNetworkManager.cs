using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetworkManager : NetworkManager
{
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
    public void CreatePlayers_ServerRpc()
    {
        for (int i = 0; i < ConnectedClientsList.Count + 1; i++)
        {
            GameObject g = Instantiate(PrefabRefManager.Instance.playerPawn);
            NetworkObject no = g.GetComponent<NetworkObject>();
            no.SpawnWithOwnership(ConnectedClientsIds[i]);

        }
    }

}
