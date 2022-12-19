using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingCollapseScript : MonoBehaviour
{
    public GameObject barrel_observer;
    private bool triggered;
    private Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (barrel_observer == null && !triggered)
        {
            triggered = true;
            anim.SetBool("BuildingFall", true);

            for(int i=0; i<transform.childCount; i++)
            {
                transform.GetChild(i).GetChild(0).GetComponent<ParticleSystem>().Play();
            }
        }
    }
}
