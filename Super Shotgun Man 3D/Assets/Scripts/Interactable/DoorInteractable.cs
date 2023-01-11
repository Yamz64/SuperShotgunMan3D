using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteractable : Interactable
{
    public bool locked;
    public float interpolation_speed;
    public string locked_text;

    private float interpolation_factor;
    private bool direction_toggle, finished_animation;

    private Transform pivot;
    private Vector3 starting_rotation;
    private Collider col;   

    public override void Interact()
    {
        if (locked) {
            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>().AnnounceText = locked_text;
            return;
        }
        if (finished_animation)
        {
            direction_toggle = !direction_toggle;
            AudioUtils.InstanceSound(17, transform.position, this, null, true, 1f, Random.Range(0.75f, 1.1f));
        }
    }

    void LerpDoor()
    {
        col.enabled = finished_animation;
        Vector3 ending_rotation = starting_rotation + new Vector3(0.0f, 90.0f, 0.0f);
        pivot.transform.rotation = Quaternion.Euler(Vector3.Lerp(starting_rotation, ending_rotation, interpolation_factor));
    }

    private void Start()
    {
        col = GetComponent<Collider>();
        direction_toggle = false;
        interpolation_factor = 0.0f;
        pivot = transform.parent;
        starting_rotation = pivot.transform.rotation.eulerAngles;
    }

    private void Update()
    {
        LerpDoor();
        if(direction_toggle == false)
        {
            if (interpolation_factor > 0.0f)
            {
                finished_animation = false;
                interpolation_factor -= Time.deltaTime * interpolation_speed;
            }
            else
            {
                finished_animation = true;
                interpolation_factor = 0.0f;
            }
        }
        else
        {
            if (interpolation_factor < 1.0f)
            {
                finished_animation = false;
                interpolation_factor += Time.deltaTime * interpolation_speed;
            }
            else
            {
                finished_animation = true;
                interpolation_factor = 1.0f;
            }
        }
    }
}
