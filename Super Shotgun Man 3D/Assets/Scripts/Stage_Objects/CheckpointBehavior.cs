using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointBehavior : MonoBehaviour
{
    public float animation_speed;
    private float animation_tick;
    private bool is_active_checkpoint;
    private Transform end_point;
    private Material mat, end_mat;


    //checks if player is within the checkpoint
    bool CheckForPlayer()
    {
        Vector3 half_extents = GetComponent<BoxCollider>().bounds.extents;
        return Physics.BoxCast(transform.position, half_extents, (end_point.position - transform.position).normalized, transform.rotation, (end_point.position - transform.position).magnitude, LayerMask.GetMask("Player"));
    }

    public void ResetCheckpoint()
    {
        is_active_checkpoint = false;
        animation_tick = 0.0f;
        mat.SetFloat("_SpriteIndex", 0);
        end_mat.SetFloat("_SpriteIndex", 0);
    }

    void ResetAllCheckpoints()
    {
        GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("Checkpoint");

        for(int i=0; i<checkpoints.Length; i++)
        {
            checkpoints[i].GetComponent<CheckpointBehavior>().ResetCheckpoint();
        }
    }

    void Animate()
    {
        if (animation_tick < 3.0f)
            animation_tick += animation_speed * Time.deltaTime;
        else
            animation_tick = 3.0f;
        mat.SetFloat("_SpriteIndex", (int)animation_tick);
        end_mat.SetFloat("_SpriteIndex", (int)animation_tick);
    }

    // Start is called before the first frame update
    void Start()
    {
        is_active_checkpoint = false;
        end_point = transform.GetChild(1);
        mat = transform.GetChild(0).GetComponent<Renderer>().material;
        end_mat = end_point.GetChild(0).GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_active_checkpoint)
            Animate();
        if (CheckForPlayer())
        {
            if (is_active_checkpoint)
                return;
            ResetAllCheckpoints();
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().SetActiveCheckpoint((end_point.position - transform.position) / 2.0f + transform.position);
            is_active_checkpoint = true;
        }
    }
}
