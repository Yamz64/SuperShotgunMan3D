using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaceBehavior : MonoBehaviour
{
    public float idle_anim_chance;

    public float idle_unique_duration, weapon_pickup_duration, pain_duration, soy_duration, speed_duration, angry_duration;

    private float max_idle_unique_duration, max_weapon_pickup_duration, max_pain_duration, max_soy_duration, max_speed_duration, max_angry_duration;

    [SerializeField]
    private int animation_state, frame_offset, unique_frame;
    private PlayerStats stats;
    [SerializeField]
    private List<Sprite> animation_frames;

    private Image image;

    public void SetState(int state)
    {
        animation_state = state;
    }

    int GetAnimOffset()
    {
        if (stats.HP < 33)
            return (animation_frames.Count / 3) * 2;
        else if (stats.HP < 66)
            return animation_frames.Count / 3;
        else
            return 0;
    }

    int Idle()
    {
        //handle animating a unique animation
        if (idle_unique_duration > 0.0f)
        {
            idle_unique_duration -= Time.deltaTime;
            return frame_offset + unique_frame;
        }
        else
            idle_unique_duration = 0.0f;
        //roll a random number between 0 and 100 to see if a unique idle frame should be played, then roll a random frame
        //unique animation
        if (Random.Range(0.0f, 100.0f) < idle_anim_chance)
        {
            idle_unique_duration = max_idle_unique_duration;
            unique_frame = Random.Range(1, 7);
        }

        //normal animation, if a random frame wasn't met, then simply return the default idle frame
        return frame_offset;
    }

    int PickUpWeapon()
    {
        if (weapon_pickup_duration > 0.0f)
            weapon_pickup_duration -= Time.deltaTime;
        else
            weapon_pickup_duration = max_weapon_pickup_duration;

        if (weapon_pickup_duration >= max_weapon_pickup_duration / 2.0f)
            return 7 + frame_offset;
        else
            return 8 + frame_offset;
    }

    int Pain(bool alt)
    {
        if (pain_duration > 0.0f)
            pain_duration -= Time.deltaTime;
        else
            pain_duration = max_pain_duration;

        if (!alt)
        {
            if (pain_duration >= max_pain_duration / 2.0f)
                return 9 + frame_offset;
            else
                return 10 + frame_offset;
        }
        if (pain_duration >= max_pain_duration / 2.0f)
            return 11 + frame_offset;
        else
            return 12 + frame_offset;
    }

    int Soy()
    {
        if (soy_duration > 0.0f)
            soy_duration -= Time.deltaTime;
        else
            soy_duration = max_soy_duration;

        if (soy_duration >= max_soy_duration / 2.0f)
            return 13 + frame_offset;
        else
            return 14 + frame_offset;
    }

    int Speed()
    {
        if (speed_duration > 0.0f)
            speed_duration -= Time.deltaTime;
        else
            speed_duration = max_speed_duration;

        if (speed_duration >= max_speed_duration / 2.0f)
            return 15 + frame_offset;
        else
            return 16 + frame_offset;
    }

    int Angry()
    {
        if (angry_duration > 0.0f)
            angry_duration -= Time.deltaTime;
        else
            angry_duration = max_angry_duration;

        if (angry_duration >= max_angry_duration / 2.0f)
            return 17 + frame_offset;
        else
            return 18 + frame_offset;
    }

    void Animate()
    {
        int current_frame = 0;
        switch (animation_state)
        {
            case 0:
                current_frame = Idle();
                break;
            case 1:
                current_frame = PickUpWeapon();
                break;
            case 2:
                current_frame = Pain(false);
                break;
            case 3:
                current_frame = Pain(true);
                break;
            case 4:
                current_frame = Soy();
                break;
            case 5:
                current_frame = Speed();
                break;
            case 6:
                current_frame = Angry();
                break;
            default:
                current_frame = Idle();
                break;
        }
        image.sprite = animation_frames[current_frame];
    }

    private void Start()
    {
        stats = transform.root.GetComponent<PlayerStats>();

        max_idle_unique_duration = idle_unique_duration;
        idle_unique_duration = 0.0f;

        max_weapon_pickup_duration = weapon_pickup_duration;
        weapon_pickup_duration = 0.0f;

        max_pain_duration = pain_duration;
        pain_duration = 0.0f;

        max_soy_duration = soy_duration;
        soy_duration = 0.0f;

        max_speed_duration = speed_duration;
        speed_duration = 0.0f;

        max_angry_duration = angry_duration;
        angry_duration = 0.0f;

        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        frame_offset = GetAnimOffset();
        Animate();
    }
}
