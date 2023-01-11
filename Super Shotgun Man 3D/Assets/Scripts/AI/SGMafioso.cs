using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGMafioso : BrickMafioso
{
    public int pellet_count;

    public Object shotgun;
    private bool dropped_shotgun;
    private PlayerStats stats;

    class DamageBatch
    {
        public GameObject target;
        public Vector3 direction;
        public int damage;
        public bool is_player;
    }

    //attack sound is index 15 on the sound table
    public override void PlayAtkSound()
    {
        AudioUtils.InstanceSound(15, transform.position, this, null, true, 1f, Random.Range(0.95f, 1.05f));
    }

    public GameObject FireHitscan(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, LayerMask.GetMask("Ground") | LayerMask.GetMask("Player") | LayerMask.GetMask("Enemy")))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                FXUtils.InstanceFXObject(0, hit.point, Quaternion.identity);
            else
            {
                FXUtils.InstanceFXObject(1, hit.point, Quaternion.FromToRotation(Vector3.forward, -direction));

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    return hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<BaseEnemyBehavior>().GetEnemytag() != enemy_tag)
                {
                    return hit.collider.gameObject;
                }

            }

            MathUtils.DrawPoint(hit.point, 0.04f, Color.cyan, Mathf.Infinity);
        }
        return null;
    }

    public override void Fire()
    {
        //if no projectile is specified, fire a hitscan shot forward at a random spread angle
        if (projectile == null)
        {
            List<DamageBatch> batches = new List<DamageBatch>();
            for (int i = 0; i < pellet_count; i++)
            {
                Vector3 shot_dir = lookdir;
                shot_dir = Quaternion.AngleAxis(spread * Random.Range(-1.0f, 1.0f), Vector3.up) * shot_dir;
                shot_dir = Quaternion.AngleAxis(spread * Random.Range(-1.0f, 1.0f), Vector3.right) * shot_dir;
                Debug.DrawRay(transform.position, shot_dir * Mathf.Infinity, Color.blue, 1.0f);

                //fire a hitscan and batch damage
                GameObject hit = FireHitscan(shot_dir);
                if(hit != null)
                {
                    //first see if the batches list contains the object hit, if it does, then increment the damage dealt
                    bool found_object = false;
                    for(int j=0; j<batches.Count; j++)
                    {
                        if (hit.gameObject == batches[j].target)
                        {
                            batches[j].damage += (int)MathUtils.GaussianRandom(min_damage, max_damage);
                            batches[j].direction += shot_dir;
                            found_object = true;
                            break;
                        }
                    }

                    //if the object wasn't found then add it to the list
                    if (!found_object)
                    {
                        DamageBatch batch = new DamageBatch();
                        batch.target = hit;
                        batch.direction = shot_dir;
                        batch.damage = (int)MathUtils.GaussianRandom(min_damage, max_damage);
                        batch.is_player = hit.GetComponent<PlayerStats>() != null;
                        batches.Add(batch);
                    }
                }
            }

            //finally go through the damage batches and assign damage to everything that took damage
            for(int i=0; i<batches.Count; i++)
            {
                if (batches[i].is_player)
                {
                    batches[i].target.GetComponent<PlayerStats>().TakeDamage(batches[i].damage, batches[i].direction.normalized, gameObject);
                    if (batches[i].target.GetComponent<PlayerMovement>().GetDead() && batches[i].target.GetComponent<PlayerStats>().HP <= 0)
                        batches[i].target.GetComponent<PlayerStats>().AnnounceText = death_message;
                }
                else
                {
                    if (batches[i].target.GetComponent<BaseEnemyBehavior>() == null)
                        return;
                    batches[i].target.GetComponent<BaseEnemyBehavior>().TakeDamage(batches[i].damage, batches[i].direction.normalized);
                    if (batches[i].target.GetComponent<BaseEnemyBehavior>().GetTargetingThreshold() <= 0)
                        batches[i].target.GetComponent<BaseEnemyBehavior>().SetTarget(gameObject);
                }
            }
        }
        else
        {
            Vector3 shot_dir = lookdir;
            shot_dir = Quaternion.AngleAxis(spread * MathUtils.GaussianRandom(-1.0f, 1.0f), Vector3.up) * shot_dir;
            shot_dir = Quaternion.AngleAxis(spread * MathUtils.GaussianRandom(-1.0f, 1.0f), Vector3.right) * shot_dir;
            lookdir = shot_dir;
            GameObject instance = (GameObject)Instantiate(projectile, transform.position, Quaternion.LookRotation(lookdir));
            instance.GetComponent<ProjectileBehavior>().ignore_collisions = gameObject;
        }
    }

    public override void AI()
    {
        base.AI();
        if(HP <= 0.0f && !dropped_shotgun && !stats.HasShotgun)
        {
            Instantiate(shotgun, transform.position, transform.rotation);
            dropped_shotgun = true;
        }
    }

    public override void StartOverrides()
    {
        base.StartOverrides();
        stats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
    }
}
