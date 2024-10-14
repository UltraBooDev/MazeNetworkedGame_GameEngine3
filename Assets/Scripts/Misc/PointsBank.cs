using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using NaughtyAttributes;

public class PointsBank : PointsTrigger
{
    // 0 = Red Team, 1 = Blue Team
    [Dropdown("GetTeamValues")]
    public int TeamBank = 0;

    private DropdownList<int> GetTeamValues()
    {
        return new DropdownList<int>()
        {
            { "Red Team", 0},
            { "Blue Team", 1}
        };
    }

    protected override void Update()
    {
        if (!IsServer) return;

        if (GameNetVars.Instance.gamemodeState.Value != GameNetworkManager.GameState.InGame) return;

        if (inTrigger.Count <= 0) return;

        if (pointTimer >= extractionTime)
        {
            foreach (PawnNetworkController target in inTrigger)
            {
                if (!target.isAlive) continue;

                if(target.controller.playerTeam.Value == TeamBank)
                {
                    if (target.pointsOnPlayer.Value <= 0) continue;

                    target.pointsOnPlayer.Value = Mathf.Clamp(target.pointsOnPlayer.Value - pointValue,0, int.MaxValue);

                    if(TeamBank == 0) GameNetVars.Instance.redPoints.Value += pointValue;
                    else if (TeamBank == 1) GameNetVars.Instance.bluePoints.Value += pointValue;
                }
                else
                {
                    target.pointsOnPlayer.Value += pointValue;

                    if (TeamBank == 0) GameNetVars.Instance.redPoints.Value = Mathf.Clamp(GameNetVars.Instance.redPoints.Value - pointValue, 0, int.MaxValue);
                    else if (TeamBank == 1) GameNetVars.Instance.bluePoints.Value = Mathf.Clamp(GameNetVars.Instance.bluePoints.Value - pointValue, 0, int.MaxValue);
                }

            }

            pointTimer = 0f;
            return;
        }

        pointTimer += Time.deltaTime;
    }
}
