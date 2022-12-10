using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemyBehavior : MonoBehaviour
{
    [System.Serializable]
    private struct Animation
    {
        public int starting_index;
        public int frame_count;
        public bool looping;
        public float speed_fps;
    }

    public int current_frame;
    public int current_animation;

    public int step_count_max, step_count_min;

    public float step_distance, step_frequency;

    private int last_animation;

    [SerializeField]
    private int step_count, targeting_threshold;

    public Vector3 lookdir;
    public Texture2DArray spritesheet;

    private float anim_tick;

    private float step_frequency_max;

    private Material visual_mat;
    [SerializeField]
    private List<Animation> animations;
    private BoxCollider col;

    bool CheckNextPostion(Vector3 direction)
    {
        Vector3 half_extents = new Vector3(col.size.x, col.size.y, col.size.z) * 0.99f;
        return !Physics.BoxCast(transform.position, half_extents, direction, transform.rotation, step_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Player"));
    }

    void UpdateAnimationViewAngle()
    {
        //First get the player's viewing angle and take the dot product with the enemy's look direction
        Vector2 eviewangle = new Vector2(lookdir.x, lookdir.z).normalized;
        Vector2 pviewangle = new Vector2(transform.position.x - Camera.main.transform.position.x, transform.position.z - Camera.main.transform.position.z).normalized;
        float frame_lerp = Vector2.Dot(eviewangle, pviewangle);

        //interpolate the current frame of animation
        int calculated_frame = animations[current_animation].starting_index + current_frame * 5;

        //looking at:
        //front
        if (frame_lerp < -0.75f)
            calculated_frame += 0;
        //3/4 front
        else if (frame_lerp < -0.25f)
            calculated_frame += 1;
        //side
        else if (frame_lerp < .25f)
            calculated_frame += 2;
        //3/4 back
        else if (frame_lerp < .75f)
            calculated_frame += 3;
        //back
        else
            calculated_frame += 4;
        
        visual_mat.SetFloat("_SpriteIndex", calculated_frame);

        //now handle flipping the sprite if viewed from the right or left
        Vector2 erightangle = -new Vector2(Vector3.Cross(lookdir, Vector3.up).x, Vector3.Cross(lookdir, Vector3.up).z);
        frame_lerp = Vector2.Dot(erightangle, pviewangle);

        if (frame_lerp > 0.0f)
            visual_mat.SetFloat("_Flip", 0);
        else
            visual_mat.SetFloat("_Flip", 1);
    }

    void Animate()
    {
        if (animations.Count == 0) return;
        //first get the active animation
        Animation anim = animations[current_animation];

        if(last_animation != current_animation)
        {
            anim_tick = 0.0f;
            last_animation = current_animation;
        }

        if (anim.looping)
        {
            if ((int)anim_tick >= anim.frame_count)
                anim_tick = 0.0f;
            current_frame = (int)anim_tick;
        }
        else
        {
            if ((int)anim_tick >= anim.frame_count)
                anim_tick = anim.frame_count;
            current_frame = (int)anim_tick;
        }

        anim_tick += Time.deltaTime * anim.speed_fps;
    }

    void ChasePlayer(GameObject target)
    {
        //if ready to pick a new direction
        if (step_count == 0)
        {
            //decide randomly how many steps to take towards the target
            step_count = (int)MathUtils.GaussianRandom(step_count_min, step_count_max);

            //create 8 movement directions based off from cardinals and orthogonals
            List<Vector2> possible_movements = new List<Vector2>() { new Vector2(-1.0f, 1.0f), new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f),
                                                                     new Vector2(1.0f, 0.0f), new Vector2(1.0f, -1.0f), new Vector2(0.0f, -1.0f),
                                                                     new Vector2(-1.0f, -1.0f), new Vector2(-1.0f, 0.0f)
                                                                    };

            for (int i = 0; i < possible_movements.Count; i++)
                possible_movements[i].Normalize();

            //out of all of the possible movements, find which one brings the enemy closest to the player
            int closest_index = 0;
            float closest_distance = Mathf.Infinity;
            for (int i = 0; i < possible_movements.Count; i++)
            {
                Vector2 new_position = new Vector2(transform.position.x + possible_movements[i].x * step_distance * step_count, transform.position.z + possible_movements[i].y * step_distance * step_count);
                float distance = Vector3.Distance(target.transform.position, new Vector3(new_position.x, transform.position.y, new_position.y));
                if (distance < closest_distance)
                {
                    closest_distance = distance;
                    closest_index = i;
                }
            }

            Vector3 potential_lookdir = new Vector3(possible_movements[closest_index].x, 0.0f, possible_movements[closest_index].y);

            //if this direction is blocked, 1) check its orthogonals, 2) if both of those are blocked check if it's original direction is blocked, 3) if that is blocked then turn around
            //4) if that final direction is blocked, you're boned so set the step counter to 0

            //Not blocked, ignore everything
            if (CheckNextPostion(potential_lookdir))
            {
                lookdir = potential_lookdir;
                transform.position += lookdir * step_distance;
                return;
            }
            //1) ortho
            //corner cases where orthogonals don't follow a standard calculation
            if (closest_index == 0)
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[1].x, 0.0f, possible_movements[1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[7].x, 0.0f, possible_movements[7].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }
            }
            else if(closest_index == 7)
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[0].x, 0.0f, possible_movements[0].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[6].x, 0.0f, possible_movements[6].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }
            }
            else
            {
                //right ortho
                potential_lookdir = new Vector3(possible_movements[closest_index + 1].x, 0.0f, possible_movements[closest_index + 1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }

                //left ortho
                potential_lookdir = new Vector3(possible_movements[closest_index - 1].x, 0.0f, possible_movements[closest_index - 1].y);
                if (CheckNextPostion(potential_lookdir))
                {
                    lookdir = potential_lookdir;
                    transform.position += lookdir * step_distance;
                    return;
                }
            }

            //2) current look direction
            if (CheckNextPostion(lookdir))
            {
                transform.position += lookdir * step_distance;
                return;
            }

            //3) turn around!
            int opposite_index = closest_index + 4;
            if (opposite_index > 7) opposite_index -= 8;

            potential_lookdir = new Vector3(possible_movements[opposite_index].x, 0.0f, possible_movements[opposite_index].y);
            if (CheckNextPostion(potential_lookdir))
            {
                lookdir = potential_lookdir;
                transform.position += lookdir * step_distance;
                return;
            }

            //4) you tried!
            step_count = 0;

        }
        //new direction is known
        else
        {
            //first see if the desired step location is valid, if it is then take a step, lower the step count and lower the targeting threshold
            if (CheckNextPostion(lookdir))
            {
                transform.position += lookdir * step_distance;
                step_count--;
                targeting_threshold--;
            }
            //if it isn't, then set the step count to 0
            else
            {
                step_count = 0;
            }
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        targeting_threshold = 0;
        step_count = 0;
        anim_tick = 0.0f;
        step_frequency_max = step_frequency;
        lookdir = transform.forward;

        //create a new material instance so that other enemies are unaffected
        visual_mat = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        col = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimationViewAngle();
        Animate();
        if (step_frequency > 0.0f)
            step_frequency -= Time.deltaTime;
        else
        {
            ChasePlayer(GameObject.FindGameObjectWithTag("Player"));
            step_frequency = step_frequency_max;
        }
    }
}