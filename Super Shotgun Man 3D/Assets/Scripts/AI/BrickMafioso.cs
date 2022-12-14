using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickMafioso : BaseEnemyBehavior
{
    [SerializeField]
    private float aggro_distance;

    [SerializeField]
    private bool silent_awake;

    private bool awake;

    public void SleepSequence()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 player_orientation = (player.transform.position - transform.position);
        //first check if the player is within aggro distance see if the enemy sees the player first, if not, then see if the player made a noise
        if (player_orientation.magnitude > aggro_distance)
            return;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, Mathf.Infinity, LayerMask.GetMask("Player") | LayerMask.GetMask("Ground")))
        {
            if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player"))
                return;
        }

        float dot = Vector3.Dot(lookdir, player_orientation.normalized);
        if(dot > 0.0f)
        {
            awake = true;
            target = GameObject.FindGameObjectWithTag("Player");
            PlayAwakeSound();
            return;
        }

        if(player.GetComponent<PlayerStats>().Shells > 0 && Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
        {
            awake = true;
            target = GameObject.FindGameObjectWithTag("Player");
            PlayAwakeSound();
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            awake = true;
            target = GameObject.FindGameObjectWithTag("Player");
            PlayAwakeSound();
            return;
        }
    }

    public void TargetCheck()
    {
        if (target == null)
            return;

        if(target.tag == "Enemy")
        {
            if (target.GetComponent<BaseEnemyBehavior>().HP <= 0)
            {
                target = null;
                awake = false;
            }
        }
    }

    //awake sounds are indices 6-9 on the sound table
    public void PlayAwakeSound()
    {
        if (silent_awake) return;
        int index = Random.Range(6, 10);
        AudioUtils.InstanceSound(index, transform.position, this, null, true, .75f, Random.Range(0.95f, 1.05f));
    }

    //pain sounds are indices 12 and 13 on the sound table
    public void PlayPainSound()
    {
        int index = Random.Range(12, 14);
        AudioUtils.InstanceSound(index, transform.position, this, null, true, .75f, Random.Range(0.95f, 1.05f));
    }

    //death sounds are indices 10 and 11 on the sound table
    public void PlayDeathSound()
    {
        int index = Random.Range(10, 12);
        AudioUtils.InstanceSound(index, transform.position, this, null, true, .50f, Random.Range(0.95f, 1.05f));
    }

    //attack sound is index 14 on the sound table
    public virtual void PlayAtkSound()
    {
        AudioUtils.InstanceSound(14, transform.position, this, null, true, 1f, Random.Range(0.95f, 1.05f));
    }

    public override void AI()
    {
        //in this scenario attack the player at greater frequencies at close range, must have LOS
        if (_hp > 0.0f)
        {
            if (in_pain)
            {
                awake = true;
                if (current_animation != 3) PlayPainSound();
                current_animation = 3;
                return;
            }
            if (!awake)
            {
                SleepSequence();
                return;
            }
            if (target == null) return;
            if (step_count == 0 && reaction_time <= 0)
            {
                TargetCheck();
                if (target == null) return;
                if (anim_completed)
                {
                    current_animation = 0;
                    ChasePlayer(target);
                    return;
                }
                //calculate attack probability
                float max_magnitude = max_attack_distance - min_attack_distance;
                float attack_magnitude = Vector3.Distance(target.transform.position, transform.position) - min_attack_distance;
                float distance_lerp = attack_magnitude / max_magnitude;

                float attack_probability = Mathf.Lerp(100.0f, 0.0f, distance_lerp);
                if (MathUtils.GaussianRandom(0.0f, 100.0f) <= attack_probability)
                {
                    current_animation = 1;
                }
                else if (current_animation != 1)
                {
                    current_animation = 0;
                    ChasePlayer(target);
                    return;
                }
            }
            else
            {
                if (step_frequency > 0.0f)
                    step_frequency -= Time.deltaTime;
                else
                {
                    ChasePlayer(target);
                    current_animation = 0;
                    step_frequency = step_frequency_max;
                }
            }
        }
        else
        {
            current_animation = 2;
            if (!collision_off)
            {
                //ignore collision with the player and all enemies in the map
                Physics.IgnoreCollision(col, GameObject.FindGameObjectWithTag("Player").GetComponent<Collider>());

                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                for (int i = 0; i < enemies.Length; i++)
                {
                    Physics.IgnoreCollision(col, GameObject.FindGameObjectWithTag("Enemy").GetComponent<Collider>());
                }
                col.center = death_col_offset;
                col.size /= 4;
                collision_off = true;
            }
        }
    }

    public override void StartOverrides()
    {
        awake = false;
        target = null;
    }
}
