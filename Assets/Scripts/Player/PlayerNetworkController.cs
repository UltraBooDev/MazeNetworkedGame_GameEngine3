using System.Collections.Generic;
using System.Collections;
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
    public void SetUpPawn_ClientRpc(NetworkBehaviourReference pawnRef, int Team, Vector3 pos,ClientRpcParams clientRpcParams)
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

        pawn.transform.SetPositionAndRotation(pos, Quaternion.identity);

        SetLayerAllChildren(pawn.transform, Team);

        MusicManager.Instance.ToggleMusic(true);

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


    [ClientRpc]
    public void EndServerOnClients_ClientRpc(ClientRpcParams clientRpcParams)
    {
        StartCoroutine(ServerEnd());
    }

    IEnumerator ServerEnd()
    {
        Gameplay_UI.Instance.GameEndPanel.SetActive(true);

        pawnObject.ToggleCam(false);
        MusicManager.Instance.ToggleMusic(false);

        if (GameNetVars.Instance.redPoints.Value > GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.RedWins.SetActive(true);
        if (GameNetVars.Instance.redPoints.Value < GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.BlueWins.SetActive(true);
        if (GameNetVars.Instance.redPoints.Value == GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.TieWins.SetActive(true);

        yield return new WaitForSeconds(6f);

        if (GameNetVars.Instance.redPoints.Value > GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.RedWins.SetActive(false);
        if (GameNetVars.Instance.redPoints.Value < GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.BlueWins.SetActive(false);
        if (GameNetVars.Instance.redPoints.Value == GameNetVars.Instance.bluePoints.Value) Gameplay_UI.Instance.TieWins.SetActive(false);

        Gameplay_UI.Instance.GameEndPanel.SetActive(false);

        MainMenu_UI.Instance.gameObject.SetActive(true);
        MainMenu_UI.Instance.PNL_Lobby.SetActive(true);
        MainMenu_UI.Instance.PNL_HostRoom.SetActive(false);
        MainMenu_UI.Instance.PNL_WaitRoom.SetActive(false);
        MainMenu_UI.Instance.PNL_GameUI.SetActive(false);

        GameNetVars.Instance.gamemodeState.Value = GameNetworkManager.GameState.Lobby;

        yield return new WaitForSeconds(0.05f);

        //Tried to make a return to lobby but clients were not returning to normal
        Application.Quit();

    }
}
