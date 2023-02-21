using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Wind_Tunnel_Point : MonoBehaviour
{
    public float wind_force;                //Force that the tunnel pulls with

    public Vector3 wind_direction;          //Direction this point in the wind tunnel pushes in

    public Vector3 player_speed;            //Speed of the player before entering the tunnel

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
           player_speed = other.gameObject.GetComponent<Rigidbody>().velocity;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.AddForce(wind_direction * wind_force);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            float max = player_speed.magnitude;//Math.Max(player_speed.x, Math.Max(player_speed.y, player_speed.z));
            Vector3 temp = wind_direction * max;
            other.gameObject.GetComponent<Rigidbody>().velocity += temp;
            player_speed.Set(0,0,0);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
