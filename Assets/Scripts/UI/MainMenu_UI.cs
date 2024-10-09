using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class MainMenu_UI : MonoBehaviour
{
    [SerializeField] Button BTN_Host, BTN_Join;

    void Start()
    {
        BTN_Host.onClick.AddListener(StartHost);
        BTN_Join.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
