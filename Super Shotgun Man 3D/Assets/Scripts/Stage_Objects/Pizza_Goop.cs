using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza_Goop : MonoBehaviour
{
    public int damage;                  //The damage the player will take when in the goop
    public float delay;                 //The time between each bit of damage the player takes (either small/continuous or large and spaced out)
    public float current;               //The current time until the delay is reached for damage

    public float lifetime = -10.0f;     //The amount of time the goop should stay active
    public float age;                   //The current time until the lifetime limit is reached

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
        age = lifetime;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifetime != -10.0f)
        {
            if (age > 0)
            {
                age -= Time.deltaTime;
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
}
