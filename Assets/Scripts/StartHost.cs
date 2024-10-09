using Unity.Netcode;
using UnityEngine;

public class StartHost : MonoBehaviour
{
    private void Start()
    {
        NetworkManager.Singleton.StartHost();
    }

}
