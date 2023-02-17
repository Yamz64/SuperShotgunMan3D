using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind_Tunnel_Point : MonoBehaviour
{
    public float wind_force;                //Force that the tunnel pulls with

    public Vector3 wind_direction;          //Direction this point in the wind tunnel pushes in

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.AddForce(wind_direction * wind_force);
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
