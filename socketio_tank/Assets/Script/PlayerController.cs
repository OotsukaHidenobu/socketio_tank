using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform muzzle = default;
    public float bulletSpeed;
    public float moveSpeed;
    public float turnSpeed;
    public bool isLocaPlayer = false;
    private Rigidbody rb;
    private float movementInputValue;
    private float turnInputValue;

    Vector3 oldPosition;
    Vector3 currentPosition;
    Quaternion oldRotation;
    Quaternion currentRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        oldPosition = transform.position;
        currentPosition = oldPosition;
        oldRotation = transform.rotation;
        currentRotation = oldRotation;
    }

    void Update()
    {
        if (!isLocaPlayer)
        {
            return;
        }
        var y = Input.GetAxis("Horizontal") * Time.deltaTime * turnSpeed;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

        transform.Rotate(0, y, 0);
        transform.Translate(0, 0, z);

        currentPosition = transform.position;
        currentRotation = transform.rotation;

        if(currentPosition != oldPosition)
        {
            NetworkManager.instance.GetComponent<NetworkManager>().CommandMove(transform.position);
            oldPosition = currentPosition;
        }
        if(currentRotation != oldRotation)
        {
            NetworkManager.instance.GetComponent<NetworkManager>().CommandTurn(transform.rotation);
            oldRotation = currentRotation;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager>();
            n.CommandShoot();
        }
    }

    public void CmdFire()
    {
        var bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.identity) as GameObject;

        Bullet b = bullet.GetComponent<Bullet>();
        b.playerFrom = this.gameObject;
        bullet.GetComponent<Rigidbody>().velocity = transform.forward * bulletSpeed;
        Destroy(b, 5);
    }
}
