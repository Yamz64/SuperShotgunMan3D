using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchHitboxBehavior : MonoBehaviour
{
    public int damage;
    public bool active;
    public Vector3 knockback_dir;

    private List<GameObject> punched;
    private PlayerMovement move;

    //function plays a voiceline if the player kills an enemy with a punch (index 28)
    public void PlayPunchVoiceline()
    {
        //roll random chance to not say a voiceline
        if (Random.Range(0, 10) != 0) return;

        AudioUtils.InstanceVoice(28, transform.position, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        //first check if the punch hitbox is active and it hit an enemy
        if (!active) return;

        if (other.tag != "Enemy") return;

        //if it isn't then punch enemies in the field
        punched = move.Punched;


        if (punched.Contains(other.gameObject)) return;

        other.GetComponent<BaseEnemyBehavior>().TakeDamage(damage, knockback_dir);
        other.GetComponent<BaseEnemyBehavior>().TargetingThreshold = 100;
        FXUtils.InstanceFXObject(1, other.transform.position, Quaternion.FromToRotation(Vector3.forward, -knockback_dir));
        AudioUtils.InstanceSound(3, transform.position, this, transform.root, false, 1.0f, .85f);
        move.AddPunched(other.gameObject);

        //if the enemy has died, play a voiceline
        if (other.GetComponent<BaseEnemyBehavior>().HP <= 0)
            PlayPunchVoiceline();
    }

    private void Start()
    {
        punched = new List<GameObject>();
        move = transform.root.GetComponent<PlayerMovement>();
    }
}
