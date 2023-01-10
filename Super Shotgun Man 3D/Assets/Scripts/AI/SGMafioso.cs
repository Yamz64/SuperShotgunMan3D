using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SGMafioso : BrickMafioso
{
    public int pellet_count;

    public Object shotgun;
    private bool dropped_shotgun;
    private PlayerStats stats;

    //attack sound is index 15 on the sound table
    public override void PlayAtkSound()
    {
        AudioUtils.InstanceSound(15, transform.position, this, null, true, 1f, Random.Range(0.95f, 1.05f));
    }

    public override void Fire()
    {
        //if no projectile is specified, fire a hitscan shot forward at a random spread angle
        if (projectile == null)
        {
            for (int i = 0; i < pellet_count; i++)
            {
                Vector3 shot_dir = lookdir;
                shot_dir = Quaternion.AngleAxis(spread * Random.Range(-1.0f, 1.0f), Vector3.up) * shot_dir;
                shot_dir = Quaternion.AngleAxis(spread * Random.Range(-1.0f, 1.0f), Vector3.right) * shot_dir;
                Debug.DrawRay(transform.position, shot_dir * Mathf.Infinity, Color.blue, 1.0f);
                FireHitscan(shot_dir);
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
