using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehavior : MonoBehaviour
{
    public bool animated;
    public int frame_count;
    public float life_time, starting_velocity, animation_speed, animation_tick;
    public GameObject ignore_collisions;

    private int current_frame;
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

    void Animate()
    {
        animation_tick += Time.deltaTime * animation_speed;

        if(animation_tick >= 1.0f)
        {
            current_frame++;
            if (current_frame >= frame_count)
                current_frame = 0;

            transform.GetChild(0).GetComponent<Renderer>().material.SetFloat("_SpriteIndex", current_frame);
            animation_tick = 0.0f;
        }
    }

    private void Awake()
    {
        current_frame = 0;
        StartCoroutine(StartSequence());
    }

    private void Update()
    {
        if (animated) Animate();
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
            other.GetComponent<PlayerStats>().TakeDamage(damage, (other.transform.position - transform.position).normalized, ignore_collisions);
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
