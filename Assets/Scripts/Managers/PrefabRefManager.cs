using UnityEngine;
using System.Collections.Generic;

public class PrefabRefManager : Singleton<PrefabRefManager>
{
    public GameObject playerPawn;
    public List<Transform> spawnPos_TeamRed, spawnPos_TeamBlue;
    public GameNetVars serverVars;
}
