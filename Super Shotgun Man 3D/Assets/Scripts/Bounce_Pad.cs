using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce_Pad : MonoBehaviour
{
    public bool bounced;

    private void OnCollisionEnter(Collision collision)
    {
        
    }
    // Start is called before the first frame update
    void Start()
    {
        bounced = false;
    }
}
