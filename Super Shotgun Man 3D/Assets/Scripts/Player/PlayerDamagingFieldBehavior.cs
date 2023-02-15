using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamagingFieldBehavior : MonoBehaviour
{
    public bool active;
    public int damage;
    public float damage_interval;
    private bool cause_damage;
    private float damage_interval_max;


    // Start is called before the first frame update
    void Start()
    {
        damage_interval_max = damage_interval;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!active) return;
        if (damage_interval > 0) {
            damage_interval -= Time.deltaTime;
            cause_damage = false;
        }
        else
        {
            damage_interval = damage_interval_max;
            cause_damage = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!active) return;
        if (!cause_damage) return;
        if(other.tag == "Enemy")
        {
            BaseEnemyBehavior stats = other.GetComponent<BaseEnemyBehavior>();
            stats.TakeDamage(damage, (other.transform.position - transform.position).normalized);
        }
    }
}
