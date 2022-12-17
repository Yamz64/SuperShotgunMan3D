using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public float life_time, starting_velocity;
    public GameObject ignore_collisions;

    private Rigidbody rb;

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(life_time);
        Destroy(gameObject);
    }

    IEnumerator StartSequence()
    {
        yield return new WaitUntil(() => ignore_collisions != null);
        Physics.IgnoreCollision(GetComponent<Collider>(), ignore_collisions.GetComponent<Collider>());
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * starting_velocity;
        StartCoroutine(DeathSequence());
    }

    private void Awake()
    {
        StartCoroutine(StartSequence());
    }

    private void Update()
    {
        transform.GetChild(0).transform.rotation = Quaternion.identity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ignore_collisions == null)
            return;
        if (other.gameObject == ignore_collisions)
            return;
        if (other.tag != "Enemy" && other.tag != "Player" && other.gameObject.layer != LayerMask.NameToLayer("Ground"))
            return;

        int damage = (int)MathUtils.GaussianRandom(ignore_collisions.GetComponent<BaseEnemyBehavior>().MinDamage, ignore_collisions.GetComponent<BaseEnemyBehavior>().MaxDamage);
        if(other.tag == "Player")
        {
            FXUtils.InstanceFXObject(1, transform.position, Quaternion.FromToRotation(Vector3.forward, -(other.transform.position - transform.position).normalized));
            other.GetComponent<PlayerStats>().TakeDamage(damage, (other.transform.position - transform.position).normalized, gameObject);
            if (!other.GetComponent<PlayerMovement>().GetDead() && other.GetComponent<PlayerStats>().HP <= 0)
                other.GetComponent<PlayerStats>().AnnounceText = ignore_collisions.GetComponent<BaseEnemyBehavior>().DeathMessage;
        }
        if (other.tag == "Enemy")
        {
            if(ignore_collisions.GetComponent<BaseEnemyBehavior>().EnemyTag != other.GetComponent<BaseEnemyBehavior>().EnemyTag)
            {
                FXUtils.InstanceFXObject(1, transform.position, Quaternion.FromToRotation(Vector3.forward, -(other.transform.position - transform.position).normalized));
                other.GetComponent<BaseEnemyBehavior>().TakeDamage(damage, (other.transform.position - transform.position).normalized);
                if(other.GetComponent<BaseEnemyBehavior>().TargetingThreshold <= 0)
                    other.GetComponent<BaseEnemyBehavior>().Target = ignore_collisions;

            }
        }

        if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            FXUtils.InstanceFXObject(0, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
