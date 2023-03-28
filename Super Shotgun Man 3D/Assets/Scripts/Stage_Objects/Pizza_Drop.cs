using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza_Drop : MonoBehaviour
{
    public int damage;          //Damage the droplet does to the player on impact
    public GameObject goop;     //Pizza goop/splatter to spawn when impacting the floor

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
        }
        else
        {
            if (collision.gameObject.tag != "Hazard")
                spawnGoop();
        }
        Destroy(this.gameObject);
    }

    public void spawnGoop()
    {
        GameObject splatter = Instantiate(goop, this.transform.position, this.transform.rotation);
        splatter.GetComponent<Pizza_Goop>().lifetime = 1.0f;
    }
}
