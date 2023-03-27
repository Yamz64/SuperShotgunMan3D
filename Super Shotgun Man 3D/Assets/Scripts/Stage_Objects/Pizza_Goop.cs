using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza_Goop : MonoBehaviour
{
    public int damage;          //The damage the player will take when in the goop
    public float delay;         //The time between each bit of damage the player takes (either small/continuous or large and spaced out)
    public float current;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {

        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (current <= 0)
            {
                other.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
                current = delay;
            }
            current -= Time.deltaTime;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        current = delay;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
