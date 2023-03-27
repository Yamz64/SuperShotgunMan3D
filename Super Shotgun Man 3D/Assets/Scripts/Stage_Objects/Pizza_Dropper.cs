using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza_Dropper : MonoBehaviour
{
    public float delay;                     //Delay between dropping each blob
    public float current;
    public GameObject blob;                 //Pizza blob object to drop

    //Drops a Pizza blob 1 unit below the dropper
    public void dropPizza()
    {
        Vector3 dropSpot = new Vector3(this.transform.position.x, this.transform.position.y - 1.0f, this.transform.position.z);
        Instantiate(blob, dropSpot, this.transform.rotation);
    }

    // Start is called before the first frame update
    void Start()
    {
        current = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (current <= 0)
        {
            dropPizza();
            current = delay;
        }
        current -= Time.deltaTime;
    }
}
