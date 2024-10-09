using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkBullet : NetworkBehaviour
{
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] float bulletDamage = 10f;
    [SerializeField] float bulletLifeTime = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DestroyBullet());
    }

    private void Update()
    {
        rb.velocity = transform.forward * bulletSpeed;
    }

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(bulletLifeTime);
        Destroy(gameObject);
    }
}
