using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class PawnNetworkController : NetworkBehaviour
{
    [SerializeField] GameObject gunPivot;
    [SerializeField] GameObject modelObj;
    [HideInInspector] public bool isAlive = true;
    [SerializeField] float movementSpeed = 4f;
    [SerializeField] float gamepadDeadzone = 0.1f;
    [Header("Shoot Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPoint;
    [SerializeField] float fireRate = 0.5f;
    float fireRateTimer = 0f;
    [Space(10)]
    Vector2 moveDir, mouseDir;

    [SerializeField] Camera myCam;
    [SerializeField] Rigidbody myRB;

    [HideInInspector]public PlayerNetworkController controller;

    //bool isUsingController = false;
    [SerializeField] private string currentControlScheme;
    public PlayerInput playerInput;

    public NetworkVariable<int> pointsOnPlayer = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Update()
    {
        if (!IsOwner) return;

        MovePlayer();
        LookAtAngle();

        if (playerInput.currentControlScheme != currentControlScheme)
        {
            OnControlSchemeChanged();
            currentControlScheme = playerInput.currentControlScheme;
        }

        Gameplay_UI.Instance.PlayerHoldAmount.text = $"You are holding {pointsOnPlayer.Value} points";
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

        if (value.isPressed && Time.time > fireRateTimer)
        {
            Fire();
            fireRateTimer = Time.time + fireRate;
        }
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
        NetworkObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, gunPivot.transform.rotation).GetComponent<NetworkObject>();
        bullet.GetComponent<NetworkBullet>().bulletOwner = OwnerClientId;
        bullet.Spawn();
    }

    [ClientRpc]
    public void PlayerKill_ClientRpc(ulong playerHit, ClientRpcParams clientRpcParams)
    {
        isAlive = false;
        modelObj.SetActive(false);

        if(IsOwner)
        {
            StartCoroutine(PlayerRespawnTimer());
        }
    }

    IEnumerator PlayerRespawnTimer()
    {
        yield return new WaitForSeconds(6f);
    }

}
