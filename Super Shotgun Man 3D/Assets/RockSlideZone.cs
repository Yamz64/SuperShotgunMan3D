using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockSlideZone : MonoBehaviour
{
    bool activated;
    //add container to hold hazards up

    private void OnTriggerEnter(Collider other)
    {
        if (!activated)
        {
            //Release hazards
            activated = true;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        activated = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
