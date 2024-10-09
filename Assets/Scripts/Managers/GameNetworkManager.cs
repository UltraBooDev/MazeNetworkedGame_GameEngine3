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


    private void Update()
    {
        if (!IsServer) return;

        switch (gamemodeState.Value)
        {
            case GameState.Lobby:
                break;

            case GameState.InGame:
                break;

            case GameState.End:
                break;
        }
    }


}
