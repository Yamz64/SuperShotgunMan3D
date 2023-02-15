using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce_Pad : MonoBehaviour
{
    public bool bounced;
    public float bounceForce;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            if (!bounced)
            {
                collision.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * bounceForce);
                bounced = true;
                StartCoroutine(resetBounce());
            }
        }
    }
    private IEnumerator resetBounce()
    {
        yield return new WaitForSeconds(0.5f);
        bounced = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        bounced = false;
        if (bounceForce == 0) bounceForce = 500;
    }
}
