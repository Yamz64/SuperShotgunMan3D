using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GeyserEditor : MonoBehaviour
{
    public bool execute = false;
    public float height;
    private BoxCollider push_box, collision_box;
    private ParticleSystem visual;

    void UpdateHeight()
    {
        //first update the visuals
        //calculate the mean of the start and end velocities of the particle system
        AnimationCurve y_max_curve = visual.velocityOverLifetime.y.curveMax;
        AnimationCurve y_min_curve = visual.velocityOverLifetime.y.curveMin;

        float y_starting_velocity = (y_min_curve.keys[0].value + y_max_curve.keys[0].value) / 2.0f;
        float y_ending_velocity = (y_min_curve.keys[1].value + y_max_curve.keys[1].value) / 2.0f;

        //now set the particle lifetime based off from the height provided
        float life_time = ((2.0f * height) / (y_starting_velocity + y_ending_velocity)) * Mathf.Pow(10, -1.0f);
        ParticleSystem.MainModule main = visual.main;
        main.startLifetime = life_time;

        //next update the collision boxes
        //update the push box
        push_box.transform.localPosition = new Vector3(0.0f, height / 2.0f, 0.0f);
        push_box.size = new Vector3(1.0f, height, 1.0f);

        //update the collision box
        collision_box.transform.localPosition = new Vector3(0.0f, height, 0.0f);
        collision_box.size = new Vector3(height, Mathf.Clamp(0.1f * height, 0.0f, float.MaxValue), height);
    }

    private void Start()
    {
        push_box = transform.GetChild(0).GetComponent<BoxCollider>();
        collision_box = transform.GetChild(1).GetComponent<BoxCollider>();
        visual = transform.GetChild(2).GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (execute)
        {
            UpdateHeight();
        }
    }
}
