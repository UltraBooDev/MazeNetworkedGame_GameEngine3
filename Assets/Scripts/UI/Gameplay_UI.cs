using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Gameplay_UI : Singleton<Gameplay_UI>
{
    [SerializeField] TMP_Text Timer;
    [SerializeField] TMP_Text BlueScore, RedScore;
    public TMP_Text PlayerHoldAmount;
    public TMP_Text RespawnTimer;
    public GameObject RespawnPanel;
    public GameObject GameEndPanel, BlueWins, RedWins, TieWins;

    // Update is called once per frame
    void Update()
    {
        Timer.text = $"{Mathf.Ceil(GameNetVars.Instance.matchTimer.Value)}";
        BlueScore.text = $"{GameNetVars.Instance.bluePoints.Value}";
        RedScore.text = $"{GameNetVars.Instance.redPoints.Value}";
    }
}
