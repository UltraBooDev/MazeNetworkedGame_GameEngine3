using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PointsBank : PointsTrigger
{
    // 0 = Red Team, 1 = Blue Team
    public int TeamBank = 0;

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
                    if (TeamBank == 0)
                    {
                        if (GameNetVars.Instance.redPoints.Value <= 0) continue;
                        GameNetVars.Instance.redPoints.Value = Mathf.Clamp(GameNetVars.Instance.redPoints.Value - pointValue, 0, int.MaxValue);
                    }
                    else if (TeamBank == 1)
                    {
                        if (GameNetVars.Instance.bluePoints.Value <= 0) continue;
                        GameNetVars.Instance.bluePoints.Value = Mathf.Clamp(GameNetVars.Instance.bluePoints.Value - pointValue, 0, int.MaxValue);
                    }

                    target.pointsOnPlayer.Value += pointValue;
                }

            }

            pointTimer = 0f;
            return;
        }

        pointTimer += Time.deltaTime;
    }
}
