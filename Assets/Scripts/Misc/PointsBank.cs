using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PointsBank : NetworkBehaviour
{
    public PlayerTeam team;

    [SerializeField] TextMeshProUGUI textRed;
    [SerializeField] TextMeshProUGUI textBlue;

    public void AllocatePoints(int points, PlayerTeam playerTeam)
    {
        if (playerTeam == team)
        {
            if (playerTeam == PlayerTeam.Red)
            {
                GameNetVars.Instance.redPoints.Value += points;
            }
            else if (playerTeam == PlayerTeam.Blue)
            {
                GameNetVars.Instance.bluePoints.Value += points;
            }
        }
        else if (playerTeam != team)
        {
            if (playerTeam == PlayerTeam.Red)
            {
                //TODO: Deduct points from blue and add to red
            }
            else if (playerTeam == PlayerTeam.Blue)
            {
                //TODO: Deduct points from red and add to blue
            }
        }
    }
}
