using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameNetVars : Singleton_Network<GameNetVars>
{
    public NetworkVariable<GameNetworkManager.GameState> gamemodeState = new(
   GameNetworkManager.GameState.Lobby,
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

    public Camera LobbyCam;
}
