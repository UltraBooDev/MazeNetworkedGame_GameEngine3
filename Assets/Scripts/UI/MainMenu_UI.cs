using UnityEngine.UI;
using UnityEngine;
using Unity.Netcode;

public class MainMenu_UI : Singleton<MainMenu_UI>
{
    [SerializeField] Button BTN_Host, BTN_Join;
    [SerializeField] GameObject PNL_Lobby, PNL_WaitRoom, PNL_HostRoom;

    void Start()
    {
        BTN_Host.onClick.AddListener(StartHost);
        BTN_Join.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        if(NetworkManager.Singleton.StartHost())
        {
            PNL_Lobby.SetActive(false);
            PNL_HostRoom.SetActive(true);
        }
        else
        {

        }
    }

    private void StartClient()
    {
        if(NetworkManager.Singleton.StartClient())
        {
            PNL_Lobby.SetActive(false);
            PNL_WaitRoom.SetActive(true);
        }
    }
}
