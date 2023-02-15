using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeyserBehavior : MonoBehaviour
{
    public float error, geyser_force;

    private Rigidbody player_rb;
    private BoxCollider collision_box;

    void UpdateActiveCollisionBox()
    {
        if (player_rb.transform.position.y > collision_box.transform.position.y + error)
            collision_box.enabled = true;
        else
            collision_box.enabled = false;
    }

    private void Start()
    {
        player_rb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();
        collision_box = transform.GetChild(1).GetComponent<BoxCollider>();
    }

    private void Update()
    {
        UpdateActiveCollisionBox();
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.AddForce(transform.up * geyser_force);
    }
}
