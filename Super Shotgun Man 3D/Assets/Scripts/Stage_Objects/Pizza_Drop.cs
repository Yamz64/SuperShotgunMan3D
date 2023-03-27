using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pizza_Drop : MonoBehaviour
{
    public int damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
        }
        Destroy(this.gameObject);
    }
}
