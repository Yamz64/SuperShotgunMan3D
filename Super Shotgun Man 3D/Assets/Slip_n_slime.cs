using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slip_n_slime : MonoBehaviour
{
    public float slip_factor;         //Integer to decrease player friction to

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponent<PlayerMovement>().friction = slip_factor;
            collision.gameObject.GetComponent<PlayerMovement>().c_friction = slip_factor / 10;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponent<PlayerMovement>().friction = 25;
            collision.gameObject.GetComponent<PlayerMovement>().c_friction = 2.5f;
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
