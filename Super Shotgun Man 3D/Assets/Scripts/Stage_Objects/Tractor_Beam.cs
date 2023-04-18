using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor_Beam : MonoBehaviour
{
    GameObject emitter;                 //Object emitting the tractor beam (point to pull towards)
    public float magnitude;             //Force to pull player with

    public bool cycle = false;          //Whether the beam cycles between on and off
    bool on;                            //Whether the tractor beam is on or off (true = on, false = off)
    public float cycle_time;            //Time for tractor beam to cycle between on/off
    float current;                      //Current time in the cycle

    // Start is called before the first frame update
    void Start()
    {
        on = true;
        current = cycle_time;
        emitter = this.transform.parent.gameObject;         //Define emitter immediately
    }

    private void OnTriggerEnter(Collider other)
    {
        //Instantly kill enemies on contact
        if (other.gameObject.tag.Equals("Enemy"))
        {
            other.gameObject.GetComponent<BaseEnemyBehavior>().TakeDamage(other.gameObject.GetComponent<BaseEnemyBehavior>().HP);
        }
        else if (other.gameObject.tag.Equals("Player"))
        {
            other.gameObject.GetComponent<PlayerMovement>()._movespeed = other.gameObject.GetComponent<PlayerMovement>()._movespeed / 2;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //If the tractor beam is active
        if (on)
        {
            //Turn off player gravity and pull
            if (other.gameObject.tag.Equals("Player"))
            {
                other.gameObject.GetComponent<Rigidbody>().useGravity = false;
                other.transform.position += (emitter.transform.position - other.transform.position).normalized * (Time.deltaTime * magnitude);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Turn gravity back on
        if (other.gameObject.tag.Equals("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
            other.gameObject.GetComponent<PlayerMovement>()._movespeed = other.gameObject.GetComponent<PlayerMovement>()._movespeed * 2;
        }
    }

    private void Update()
    {
        if (cycle)
        {
            current -= Time.deltaTime;
            if (current <= 0)
            {
                current = cycle_time;
                on = !on;
            }
        }
    }
}
