using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;
using System.Collections;
using TMPro;

public class HostScreen_UI : MonoBehaviour
{

    [SerializeField] TMP_Text plyAmount, waitText;
    [SerializeField] Button BTNStart;

    public bool hasStarted;

    private void OnEnable()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        StopAllCoroutines();
        StartCoroutine(CheckForPlayers());
    }

    IEnumerator CheckForPlayers()
    {
        int dots = 0;

        float timer = 0;

        while(!hasStarted)
        {
            timer += 0.05f;
            yield return new WaitForSeconds(0.05f);

            if(timer > 0.25f)
            {
                dots++;
                if (dots > 2) dots = 0;

                string dotString = "";

                for (int i = 0; i < dots + 1; i++)
                {
                    dotString += ".";
                }

                waitText.text = $"Waiting for players{dotString}";

                timer = 0f;
            }

            if(NetworkManager.Singleton.ConnectedClients.Count == 1) plyAmount.text = $"{NetworkManager.Singleton.ConnectedClients.Count} player connected";
            else plyAmount.text = $"{NetworkManager.Singleton.ConnectedClients.Count} players connected";

            if (NetworkManager.Singleton.ConnectedClients.Count > 1) BTNStart.gameObject.SetActive(true);
            else BTNStart.gameObject.SetActive(false);

            yield return new WaitForEndOfFrame();
        }

        NetworkManager.Singleton.gameObject.GetComponent<GameNetworkManager>().CreatePlayers_ServerRpc();

    }
}
