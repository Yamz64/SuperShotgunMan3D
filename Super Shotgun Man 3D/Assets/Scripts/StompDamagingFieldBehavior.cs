using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompDamagingFieldBehavior : MonoBehaviour
{
    public bool active;
    public float min_size, max_size, min_damage, max_damage;
    public float min_speed, max_speed, damage_lerp;

    private SphereCollider col;

    private void OnTriggerEnter(Collider other)
    {
        if (!active)
            return;
        if (other.tag != "Enemy")
            return;

        int damage = (int)Mathf.Lerp(min_damage, max_damage, damage_lerp);
        col.radius = Mathf.Lerp(min_size, max_size, damage_lerp);
        other.GetComponent<BaseEnemyBehavior>().TakeDamage(damage, Vector3.up);
        other.GetComponent<BaseEnemyBehavior>().TargetingThreshold = 100;
        FXUtils.InstanceFXObject(2, other.transform.position, Quaternion.identity, null, false, 0, 1.0f, 1.0f);
    }

    private void Start()
    {
        col = GetComponent<SphereCollider>();
    }
}
