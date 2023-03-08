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

        //Return if no rigid body to affect
        if (rb == null) return;

        //Add force to object in the specified direction
        rb.AddForce(wind_direction * wind_force);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            //Get player speed amount
            float max = player_speed.magnitude;

            //Multiply by wind direction (only applies force in direction of tunnel)
            Vector3 temp = wind_direction * max;

            //Add to the object's velocity
            other.gameObject.GetComponent<Rigidbody>().velocity += temp;

            //Reset the speed var for next pass
            player_speed.Set(0,0,0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
