using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleGEOBehavior : MonoBehaviour
{
    private Transform particle_transform;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.GetChild(0) != null)
            particle_transform = transform.GetChild(0);

        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb;
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }
    
    public void Explode()
    {
        if(particle_transform != null)
        {
            particle_transform.GetComponent<ParticleSystem>().Play();
            particle_transform.parent = null;
        }
        Destroy(gameObject);
    }
}
