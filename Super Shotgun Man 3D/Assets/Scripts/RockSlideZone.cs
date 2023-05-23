using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockSlideZone : MonoBehaviour
{
    bool activated;
    public List<GameObject> rocks;
    //add container to hold hazards up

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (!activated)
            {
                //Release hazards
                for (int i = 0; i < rocks.Count; i++)
                {
                    rocks[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    rocks[i].GetComponent<Rigidbody>().useGravity = true;
                }
                activated = true;
            }
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
