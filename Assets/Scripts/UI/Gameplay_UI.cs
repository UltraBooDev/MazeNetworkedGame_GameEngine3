using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Gameplay_UI : MonoBehaviour
{
    [SerializeField] TMP_Text Timer;

    // Update is called once per frame
    void Update()
    {
        Timer.text = $"{Mathf.Ceil(GameNetVars.Instance.matchTimer.Value)}";
    }
}
