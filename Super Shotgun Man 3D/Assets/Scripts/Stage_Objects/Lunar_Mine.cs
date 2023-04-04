using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lunar_Mine : MonoBehaviour
{
    public int damage;          //Damage to do upon explosion
    public float waitTime;      //Time to wait for the explosion
    private List<Collider> objs;    //List of objects that exist in the mine's radius

    private void OnTriggerEnter(Collider other)
    {
        //Add object to the collider list
        if (!objs.Contains(other))
        {
            objs.Add(other);
        }

        //If the player is detected, detonate
        if (other.gameObject.tag.Equals("Player"))
        {
            StartCoroutine(explode());
        }
    }
    private void OnTriggerExit(Collider other)
    {
        objs.Remove(other);
    }

    //Blow up the mine in a fiery display
    IEnumerator explode()
    {
        //Wait for detonation to execute
        yield return new WaitForSeconds(waitTime);

        //Go through detected entities to check if damage should be done
        for (int i = 0; i < objs.Count; i++)
        {
            if (objs[i].gameObject.tag.Equals("Player"))
            {
                //Do stuff to the player
                objs[i].gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
            }
        }
        Destroy(this.gameObject);

        yield return new WaitForSeconds(0.0f);
    }

    // Start is called before the first frame update
    void Start()
    {
        objs = new List<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
