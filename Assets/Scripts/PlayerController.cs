using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private float speed = 0;

    private void FixedUpdate()
    {
        //Player movement
        if (!IsOwner) return;
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        rb.AddForce(movement * speed);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            //Make Camera follow the client
            Camera.main.GetComponent<CameraFollow>().setTarget(gameObject.transform);
        }
    }
}
