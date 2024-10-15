using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class PawnNetworkController : NetworkBehaviour
{
    public GameObject gunPivot;
    public GameObject modelObj;
    [HideInInspector] public bool isAlive = true;
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float gamepadDeadzone = 0.1f;
    [Header("Shoot Settings")]
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    [SerializeField] float fireRate = 0.5f;
    float fireRateTimer = 0f;
    [Space(10)]
    Vector2 moveDir, mouseDir;

    public MeshRenderer bodyMat, backpackMat;

    [SerializeField] Camera myCam;
    [SerializeField] Rigidbody myRB;

    bool isFirePressed = false;

    [HideInInspector]public PlayerNetworkController controller;

    //bool isUsingController = false;
    [SerializeField] private string currentControlScheme;
    public PlayerInput playerInput;

    public NetworkVariable<int> pointsOnPlayer = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Update()
    {
        if (!IsOwner) return;
        if (!GameNetVars.Instance.matchStarted.Value) return;

        Gameplay_UI.Instance.PlayerHoldAmount.text = $"You are holding {pointsOnPlayer.Value} points";

        if (!isAlive) return;

        LookAtAngle();

        if(isFirePressed && Time.time > fireRateTimer)
        {
            Fire();
            fireRateTimer = Time.time + fireRate;
        }

        if (playerInput.currentControlScheme != currentControlScheme)
        {
            OnControlSchemeChanged();
            currentControlScheme = playerInput.currentControlScheme;
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (!isAlive) return;

        MovePlayer();
    }


    void OnLook(InputValue value)
    {
        if (!IsOwner) return;
        if (!isAlive) return;

        mouseDir = value.Get<Vector2>();
    }

    void OnMove(InputValue value)
    {
        if (!IsOwner) return;
        if (!isAlive) return;

        moveDir = value.Get<Vector2>() * -1;
    }

    void OnFire(InputValue value)
    {
        if (!IsOwner) return;
        if (!isAlive) return;

        isFirePressed = value.isPressed;
    }

    public void OnControlSchemeChanged()
    {
        if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            Debug.Log("Keyboard & Mouse");
        }
        else
        {
            Debug.Log("Gamepad");
        }
    }

    void MovePlayer()
    {
        myRB.AddForce((new Vector3(moveDir.x, 0f, moveDir.y) * movementSpeed), ForceMode.Force);
        //transform.Translate(new Vector3(moveDir.x, 0f, moveDir.y) * movementSpeed * Time.deltaTime, Space.World);
    }

    void LookAtAngle()
    {
        if (currentControlScheme == "Gamepad")
        {
            if (Mathf.Abs(mouseDir.x) > gamepadDeadzone || Mathf.Abs(mouseDir.y) > gamepadDeadzone)
            {
                Vector3 lookDir = Vector3.right * mouseDir.x + Vector3.forward * mouseDir.y;
                if (lookDir.sqrMagnitude > 0f)
                {
                    Quaternion newRot = Quaternion.Euler(Quaternion.LookRotation(lookDir, Vector3.up).eulerAngles + new Vector3(0, 180, 0));
                    gunPivot.transform.rotation = Quaternion.RotateTowards(gunPivot.transform.rotation, newRot, Time.deltaTime * 1000);
                }
            }
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(mouseDir);
            Plane testPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDist;

            if (testPlane.Raycast(ray, out rayDist))
            {
                Vector3 point = ray.GetPoint(rayDist);
                gunPivot.transform.LookAt(new Vector3(point.x, gunPivot.transform.position.y, point.z));
            }
        }
    }

    public void ToggleCam(bool turnOn)
    {
        GameNetVars.Instance.LobbyCam.gameObject.SetActive(!turnOn);
        myCam.gameObject.SetActive(turnOn);
    }

    void Fire()
    {
        SpawnBullet_ServerRpc(new ServerRpcParams()
        {
            Receive = new ServerRpcReceiveParams()
            {
                SenderClientId = OwnerClientId
            }
        });
    }

    [ServerRpc]
    public void SpawnBullet_ServerRpc(ServerRpcParams rpcParams)
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, gunPivot.transform.rotation);
        bullet.GetComponent<NetworkBullet>().bulletOwner = rpcParams.Receive.SenderClientId;
        bullet.GetComponent<NetworkBullet>().teamOwner = controller.playerTeam.Value;
        bullet.GetComponent<NetworkObject>().SpawnAsPlayerObject(rpcParams.Receive.SenderClientId, true);


        Debug.Log($"Spawned from {rpcParams.Receive.SenderClientId}");
    }

    [ClientRpc]
    public void PlayerKill_ClientRpc(ulong playerHit, ClientRpcParams clientRpcParams)
    {
        isAlive = false;
        modelObj.SetActive(false);

        if(IsOwner)
        {
            Gameplay_UI.Instance.RespawnPanel.SetActive(true);
            StartCoroutine(PlayerRespawnTimer());
        }
    }

    [ClientRpc]
    public void PlayerRevive_ClientRpc(ulong playerHit, ClientRpcParams clientRpcParams)
    {
        isAlive = true;
        modelObj.SetActive(true);

        if(IsOwner)
        {
            if (controller.playerTeam.Value == 0) myRB.position = PrefabRefManager.Instance.spawnPos_TeamRed[Random.Range(0, PrefabRefManager.Instance.spawnPos_TeamRed.Count)].position;
            else myRB.position = PrefabRefManager.Instance.spawnPos_TeamBlue[Random.Range(0, PrefabRefManager.Instance.spawnPos_TeamBlue.Count)].position;
        }

    }

    IEnumerator PlayerRespawnTimer()
    {
        int timer = 6;

        while(timer > 0)
        {
            Gameplay_UI.Instance.RespawnTimer.text = $"Respawn in {timer}...";
            yield return new WaitForSeconds(1f);
            timer--;
            yield return null;
        }

        Gameplay_UI.Instance.RespawnTimer.text = $"Respawn soon...";
        yield return new WaitForSeconds(0.05f);
        Gameplay_UI.Instance.RespawnPanel.SetActive(false);

        RevivePlayer_ServerRpc(OwnerClientId, new ServerRpcParams());
    }

    [ServerRpc]
    public void RevivePlayer_ServerRpc(ulong playerID, ServerRpcParams rpcParams)
    {
        pointsOnPlayer.Value = 0;

        PlayerRevive_ClientRpc(playerID, new ClientRpcParams()
        {
            Send = new ClientRpcSendParams()
            {
                TargetClientIds = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds)
            }
        });
    }

}
