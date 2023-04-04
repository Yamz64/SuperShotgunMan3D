using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tractor_Beam : MonoBehaviour
{
    GameObject emitter;         //Object emitting the tractor beam (point to pull towards)
    public float magnitude;     //Force to pull player with

    // Start is called before the first frame update
    void Start()
    {
        emitter = this.transform.parent.gameObject;         //Define emitter immediately
    }

    private void OnTriggerStay(Collider other)
    {
        //Turn off player gravity and pull
        if (other.gameObject.tag.Equals("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = false;
            other.transform.position += (emitter.transform.position - other.transform.position).normalized * (Time.deltaTime * magnitude);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Turn gravity back on
        if (other.gameObject.tag.Equals("Player"))
        {
            other.gameObject.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
