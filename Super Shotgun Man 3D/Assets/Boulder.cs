using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            collision.gameObject.GetComponent<PlayerStats>().TakeDamage(damage);
        }
    }

    public int damage;

    // Start is called before the first frame update
    void Start()
    {
        if (damage == 0)
            damage = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
