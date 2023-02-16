using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce_Pad : MonoBehaviour
{
    public bool bounced;                //If the bounce pad has just been used
    public float bounceForce;           //Force to launch the player (or actor/object) with

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag.Equals("Player"))  //Check that the collision object should be launched
        {
            if (!bounced)                               //Make sure they aren't launched too much in a short time
            {
                collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * bounceForce);
                bounced = true;
                StartCoroutine(resetBounce());
            }
        }
    }

    //Reset the bounce pad for future use
    private IEnumerator resetBounce()
    {
        yield return new WaitForSeconds(0.5f);
        bounced = false;
    }

    void Start()
    {
        bounced = false;
        if (bounceForce == 0) bounceForce = 500;            //Set a default launch force if necessary
    }
}
