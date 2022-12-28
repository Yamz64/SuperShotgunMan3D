using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public int pellet_count, min_pellet_damage, max_pellet_damage, punch_damage;
    public int hold_punch_min, hold_punch_max;

    public float hold_punch_min_speed, hold_punch_max_speed, kickback;

    public float spread_angle, punch_distance;

    public float friction, c_friction;
    public float _movespeed, _walkspeed, _accelspeed, horizontal_gravity;
    public float airmult_cap, jump_speed;

    public float slidespeed, sj_speed, max_sj_speed;

    public float ground_check_distance;

    public float max_sj_airspeed, slide_timer, current_airspeed, slope_accel, terminal_speed_threshold;

    public float viewbob_amplitude, viewbob_frequency, cam_roll_amount;
    public float shotgun_frequency, shotgun_amplitude, shotgun_sway_x;
    public float death_cam_interp;

    private bool grounded, sliding, aircrouching, set_slide_vector, set_slide_speed, terminal_speed_hit;
    private bool has_checkpoint;
    private float rot_x, rot_y;

    private float max_slide_timer, current_slide_speed;

    private float animation_tick, last_vertical_velocity;

    public Vector2 mouse_sens;

    public Vector3 death_cam_height;

    private RectTransform shotgun_root, shotgun_position;
    [SerializeField]
    private bool fired, reloading, landed;

    private bool dead, punching, started_respawn;

    private Animator anim, r_anim, p_anim;

    private Transform cam_transform;
    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector3 slide_vector;
    private Vector3 cam_pivot;
    private Vector3 active_checkpoint;

    private PlayerStats stats;

    private PunchHitboxBehavior hitbox;
    private StompDamagingFieldBehavior stompbox;
    private List<GameObject> punched_enemies;

    private PauseMenuBehavior p_menu;

    public List<GameObject> Punched
    {
        get { return punched_enemies; }
        set { punched_enemies = value; }
    }
    public void SetActiveCheckpoint(Vector3 position)
    {
        active_checkpoint = position;
        has_checkpoint = true;
    }

    public void AddPunched(GameObject other) { punched_enemies.Add(other); }

    public bool GetDead() { return dead; }

    public void PlayFireSound() { AudioUtils.InstanceSound(0, transform.position, this, transform); }
    public void PlayFatFireSound() { AudioUtils.InstanceSound(0, transform.position, this, transform, false, 1.5f, 0.8f); }
    public void PlayReloadSound() { AudioUtils.InstanceSound(1, transform.position, this, transform); }

    IEnumerator FireSequence()
    {
        Fire();
        rb.AddForce(-cam_transform.forward * kickback);
        fired = true;
        anim.SetInteger("ViewmodelState", 1);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 1);
        anim.SetInteger("ViewmodelState", 0);
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 0);
        fired = false;
        stats.Shells--;
    }

    IEnumerator BigFireSequence()
    {
        Fire(true);
        rb.AddForce(-cam_transform.forward * kickback * 2.0f);
        fired = true;
        anim.SetInteger("ViewmodelState", 3);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 3);
        anim.SetInteger("ViewmodelState", 0);
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 0);
        fired = false;
        stats.Shells--;
        stats.Shells--;
    }

    IEnumerator ReloadSequence()
    {
        reloading = true;
        anim.SetInteger("ViewmodelState", 2);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 2);
        anim.SetInteger("ViewmodelState", 0);
        yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
        yield return new WaitUntil(() => anim.GetInteger("ViewmodelState") == 0);
        reloading = false;
        stats.Shells = 2;
    }

    IEnumerator JumpSequence()
    {
        r_anim.SetInteger("RootState", 1);
        yield return new WaitUntil(() => r_anim.GetInteger("RootState") == 1);
        r_anim.SetInteger("RootState", 0);
        yield return new WaitUntil(() => r_anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
    }

    IEnumerator LandSequence()
    {
        r_anim.SetInteger("RootState", 2);
        yield return new WaitUntil(() => r_anim.GetInteger("RootState") == 2);
        r_anim.SetInteger("RootState", 0);
        yield return new WaitUntil(() => r_anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1);
    }

    IEnumerator RespawnSequence()
    {
        //if the player doesn't have an active checkpoint reload the scene
        if (!has_checkpoint)
        {
            stats.ToggleFade();
            yield return new WaitUntil(() => !stats.FadeFinished);
            yield return new WaitUntil(() => stats.FadeFinished);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            stats.ToggleFade();
            yield return new WaitUntil(() => !stats.FadeFinished);
            yield return new WaitUntil(() => stats.FadeFinished);
            transform.position = active_checkpoint;
            stats.ToggleFade();
            stats.HP = 100;
            stats.AP = 0;
            dead = false;
            started_respawn = false;
        }
    }

    void FirePellet(Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(cam_transform.position, direction, out hit, Mathf.Infinity, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy")))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                FXUtils.InstanceFXObject(0, hit.point, Quaternion.identity);
            else
            {
                if (hit.collider.GetComponent<BaseEnemyBehavior>().EnemyTag != "EXPLOSIVE_BARREL")
                    FXUtils.InstanceFXObject(1, hit.point, Quaternion.FromToRotation(Vector3.forward, -direction));
                else
                    FXUtils.InstanceFXObject(0, hit.point, Quaternion.identity);

                hit.collider.gameObject.GetComponent<BaseEnemyBehavior>().TakeDamage((int)MathUtils.GaussianRandom(min_pellet_damage, max_pellet_damage), direction.normalized);
                hit.collider.GetComponent<BaseEnemyBehavior>().TargetingThreshold = 100;
            }

            if (stats.EShells)
                FXUtils.InstanceFXObject(2, hit.point - direction * 0.001f, Quaternion.identity, null, true, 10, 0.1f, 2f, .1f);

            MathUtils.DrawPoint(hit.point, 0.04f, Color.cyan, Mathf.Infinity);
        }
    }

    void Fire(bool big = false)
    {
        int mod_count = pellet_count;
        float mod_spread = spread_angle;
        if (big)
        {
            mod_count *= 2;
            mod_spread *= 2f;
            slide_vector = -transform.forward;
        }
        //generate a square shot pattern based off from the player's view vector
        int side_length = (int)Mathf.Sqrt(mod_count);
        for (int i = 0; i < mod_count; i++)
        {
            //set the shotgun vector to it's starting position
            Vector3 raycast_dir = cam_transform.forward;
            raycast_dir = Quaternion.AngleAxis(mod_spread / 2.0f, cam_transform.up) * raycast_dir;
            raycast_dir = Quaternion.AngleAxis(mod_spread / 2.0f, cam_transform.right) * raycast_dir;

            //calculate the amount of times it should rotate down
            int y_row = i / side_length;

            //calculate the amount of times it should rotate to the right
            int x_col = i;
            if (i >= side_length)
                x_col = i % side_length;

            //rotate the vector
            raycast_dir = Quaternion.AngleAxis(-(mod_spread / (float)(side_length - 1)) * (float)x_col, cam_transform.up) * raycast_dir;
            raycast_dir = Quaternion.AngleAxis(-(mod_spread / (float)(side_length - 1)) * (float)y_row, cam_transform.right) * raycast_dir;

            //raycast
            FirePellet(raycast_dir);
        }
    }

    public void InitialPunch()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam_transform.position, cam_transform.forward, out hit, punch_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy")))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                FXUtils.InstanceFXObject(0, hit.point, Quaternion.identity);

                AudioUtils.InstanceSound(3, transform.position, this, transform.root, false, 0.6f, .65f);
            }
            else
            {
                FXUtils.InstanceFXObject(1, hit.point, Quaternion.FromToRotation(Vector3.forward, -cam_transform.forward));
                hit.collider.gameObject.GetComponent<BaseEnemyBehavior>().TakeDamage(punch_damage, transform.forward.normalized);
                hit.collider.GetComponent<BaseEnemyBehavior>().TargetingThreshold = 100;
                AudioUtils.InstanceSound(3, transform.position, this, transform.root, false, 1.0f, .85f);
            }

            MathUtils.DrawPoint(hit.point, 0.04f, Color.cyan, Mathf.Infinity);
        }
    }

    public void HoldPunchLogic()
    {
        //check if the player is holding their punch
        if (!p_anim.GetCurrentAnimatorStateInfo(0).IsName("FistHold"))
        {
            hitbox.active = false;
            return;
        }

        //if they are, then measure their forward velocity, if it doesn't break the damage cap then don't do anything
        float forward_dot = Vector3.Dot(transform.forward, rb.velocity.normalized);

        if (forward_dot * rb.velocity.magnitude < hold_punch_min_speed)
            return;

        //interpolate between the min and max damage by the speed that the player is going
        float speed = (rb.velocity.magnitude - hold_punch_min_speed) / (hold_punch_max_speed - hold_punch_min_speed);
        int damage = (int)Mathf.Lerp(hold_punch_min, hold_punch_max, speed);

        Debug.Log(damage);

        //Update any enemies that are too far away to be punched by removing them from the remembered punched enemies
        bool valid = false;
        while (!valid)
        {
            valid = true;
            for (int i = 0; i < punched_enemies.Count; i++)
            {
                float distance = (punched_enemies[i].transform.position - transform.position).magnitude;
                if (distance <= punch_distance * 2f) continue;
                punched_enemies.RemoveAt(i);
                valid = false;
                break;
            }
        }

        //finally punch whatever enemies are in front by modifying the punch hitbox
        hitbox.damage = damage;
        hitbox.knockback_dir = cam_transform.forward;
        hitbox.active = true;
    }

    void PunchLogic()
    {
        //handle the setting of the animation
        punching = Input.GetKey(KeyCode.F);
        p_anim.SetBool("Punching", punching);

        HoldPunchLogic();
    }

    void StompLogic()
    {
        Vector3 crouch_offset = Vector3.zero;

        if (aircrouching)
            crouch_offset = new Vector3(0.0f, 1.0f, 0.0f);

        bool modified_grounded = Physics.BoxCast(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f),
            -Vector3.up, Quaternion.identity, ground_check_distance * Mathf.Clamp((rb.velocity.magnitude / 2.0f), 1.0f, Mathf.Infinity), LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy"));
        // if the player is grounded perform a stomp check
        if (modified_grounded)
        {
            //the player must be moving fast enough to activate the hitbox
            if (last_vertical_velocity < stompbox.min_speed)
            {
                stompbox.active = false;
                return;
            }

            float damage_lerp = (last_vertical_velocity - stompbox.min_speed) / (stompbox.max_speed - stompbox.min_speed);
            stompbox.damage_lerp = damage_lerp;
            stompbox.active = true;
            last_vertical_velocity = 0.0f;
        }
        else
            stompbox.active = false;
        //store the player's last vertical velocity
        last_vertical_velocity = -rb.velocity.y;
    }

    void Look()
    {
        //handle camera rotation
        rot_x += Input.GetAxis("Mouse X") * mouse_sens.x * (PlayerPrefs.GetFloat("Mouse Sensitivity", 0.5f) * 2.0f);
        rot_y += Input.GetAxis("Mouse Y") * mouse_sens.y * (PlayerPrefs.GetFloat("Mouse Sensitivity", 0.5f) * 2.0f);

        rot_y = Mathf.Clamp(rot_y, -90.0f, 90.0f);

        transform.rotation = Quaternion.Euler(0.0f, rot_x, 0.0f);

        //handle camera roll and bobbing
        cam_transform.localPosition = cam_pivot;
        float velocity_scalar = rb.velocity.magnitude / _movespeed;
        if (grounded && !sliding)
        {
            cam_transform.localPosition += Vector3.up * Mathf.Sin(animation_tick * viewbob_frequency) * viewbob_amplitude * velocity_scalar;
        }

        float vel_proj = Vector3.Dot(rb.velocity.normalized, transform.right) * Mathf.Clamp01(velocity_scalar);
        cam_transform.localRotation = Quaternion.Euler(-rot_y, 0.0f, Mathf.Lerp(cam_roll_amount, -cam_roll_amount, (vel_proj + 1.0f) / 2.0f));
    }

    void AnimateShotgun()
    {
        if (!stats.HasShotgun)
        {
            shotgun_position.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            return;
        }
        shotgun_position.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        //Handle basic viewbobbing
        float move_velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).magnitude;
        float x_lerp = 2.0f * Mathf.Abs((animation_tick - 0.5f) / shotgun_frequency - Mathf.Floor((animation_tick - 0.5f) / shotgun_frequency + 0.5f));
        float x_pos = Mathf.Lerp(-shotgun_sway_x, shotgun_sway_x, x_lerp);
        x_pos = Mathf.Lerp(0.0f, x_pos, move_velocity / _movespeed);

        float PI = Mathf.PI;
        float y_lerp = Mathf.Abs(Mathf.Sin((PI * animation_tick) / (0.5f * shotgun_frequency) - (PI / (shotgun_frequency))));
        float y_pos = Mathf.Lerp(shotgun_amplitude, 364.0f, y_lerp);
        y_pos = Mathf.Lerp(364.0f, y_pos, move_velocity / _movespeed);
        shotgun_position.anchoredPosition = new Vector2(x_pos, y_pos);

        //Handle shooting animations
        //normal behavior
        if (stats.Shells > 0 && !reloading)
        {
            if (Input.GetButton("Fire1") && !fired)
                StartCoroutine(FireSequence());

            if (Input.GetButton("Fire2") && !fired && stats.Shells > 1)
            {
                stats.face.OneTimeAnimationDriver(6);
                StartCoroutine(BigFireSequence());
            }

            if (Input.GetButton("Fire2") && !fired && stats.Shells < 2)
                StartCoroutine(FireSequence());

            if (Input.GetKeyDown(KeyCode.R) && !reloading && stats.Shells < 2 && !fired)
                StartCoroutine(ReloadSequence());
        }
        //auto reload
        else if (stats.Shells <= 0)
        {
            if (!reloading)
                StartCoroutine(ReloadSequence());
        }
    }

    bool CheckGrounded()
    {
        Vector3 crouch_offset = Vector3.zero;

        if (aircrouching)
            crouch_offset = new Vector3(0.0f, 1.0f, 0.0f);

        MathUtils.DrawBoxCastBox(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), Quaternion.identity, -Vector3.up, ground_check_distance, Color.cyan);
        //Debug.Log(Physics.BoxCast(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), -Vector3.up, Quaternion.identity, ground_check_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy")));
        return Physics.BoxCast(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), -Vector3.up, Quaternion.identity, ground_check_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy"));
        //return Physics.Raycast(transform.position - Vector3.up * 0.9f + crouch_offset, -Vector3.up, ground_check_distance, LayerMask.GetMask("Ground"));
    }

    bool CheckGrounded(ref RaycastHit hit)
    {
        Vector3 crouch_offset = Vector3.zero;

        if (aircrouching)
            crouch_offset = new Vector3(0.0f, 1.0f, 0.0f);

        MathUtils.DrawBoxCastBox(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), Quaternion.identity, -Vector3.up, ground_check_distance, Color.cyan);
        //Debug.Log(Physics.BoxCast(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), -Vector3.up, Quaternion.identity, ground_check_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy")));
        return Physics.BoxCast(transform.position + crouch_offset, Vector3.one * (col.radius - 0.0001f), -Vector3.up, out hit, Quaternion.identity, ground_check_distance, LayerMask.GetMask("Ground") | LayerMask.GetMask("Enemy"));
        //return Physics.Raycast(transform.position - Vector3.up * 0.9f + crouch_offset, -Vector3.up, ground_check_distance, LayerMask.GetMask("Ground"));
    }

    void ApplyFriction()
    {
        Vector2 input_vector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (sliding) input_vector = slide_vector;
        float control = 1.0f;
        if (input_vector.magnitude == 0.0f) control = c_friction;
        float new_speed = _movespeed - Time.deltaTime * friction * control;
        if (new_speed < 0.0f) new_speed = 0.0f;
        new_speed /= _movespeed;

        rb.velocity *= new_speed;
    }

    void GroundAccel()
    {
        float addspeed, accelspeed, currentspeed, wishspeed;

        Vector3 wishdir = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") * horizontal_gravity;
        wishdir.Normalize();

        wishspeed = wishdir.magnitude * _movespeed;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            wishspeed = wishdir.magnitude * _walkspeed;

        currentspeed = Vector3.Dot(rb.velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if (addspeed <= 0)
            return;

        accelspeed = _accelspeed * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        rb.velocity += new Vector3(accelspeed * wishdir.x, accelspeed * wishdir.y, accelspeed * wishdir.z);
    }

    void AirAccel()
    {
        float addspeed, accelspeed, currentspeed, wishspeed;

        Vector3 wishdir = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal") * horizontal_gravity;
        wishdir.Normalize();

        wishspeed = wishdir.magnitude * _movespeed;

        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            wishspeed = wishdir.magnitude * _walkspeed;

        if (wishspeed > airmult_cap)
            wishspeed = airmult_cap;

        currentspeed = Vector3.Dot(rb.velocity, wishdir);
        addspeed = wishspeed - currentspeed;

        if (addspeed <= 0)
            return;

        accelspeed = _accelspeed * Time.deltaTime * wishspeed;
        if (accelspeed > addspeed)
            accelspeed = addspeed;

        rb.velocity += new Vector3(accelspeed * wishdir.x, accelspeed * wishdir.y, accelspeed * wishdir.z);
    }

    //set sliding flag, capsule collider height, and camera positions
    void CheckSliding()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (sliding == false)
            {
                col.height = 1.0f;
                if (grounded)
                {
                    if (aircrouching)
                    {
                        transform.position += Vector3.up;
                        aircrouching = false;
                    }
                    cam_pivot = new Vector3(0.0f, -.25f, 0.0f);
                    col.center = new Vector3(0.0f, -0.5f, 0.0f);
                    sliding = true;
                    current_airspeed = 0.0f;
                }
                else
                {
                    aircrouching = true;
                    cam_pivot = new Vector3(0.0f, .75f, 0.0f);
                    col.center = new Vector3(0.0f, 0.5f, 0.0f);
                }
                set_slide_vector = false;
                set_slide_speed = false;
                terminal_speed_hit = false;
                slide_timer = max_slide_timer;
            }
        }
        else
        {
            if (sliding == true || aircrouching)
            {
                if (aircrouching)
                {
                    transform.position += Vector3.up * 0.5f;
                    aircrouching = false;
                }
                cam_pivot = new Vector3(0.0f, 0.5f, 0.0f);
                col.height = 2.0f;
                col.center = new Vector3(0.0f, 0.0f, 0.0f);
            }
            sliding = false;
            set_slide_vector = false;
            set_slide_speed = false;
            terminal_speed_hit = false;
            slide_timer = max_slide_timer;
        }
    }

    //all slide physics
    void Slide()
    {
        //cap airspeed
        if (!grounded)
        {
            if (rb.velocity.magnitude > max_sj_airspeed)
            {
                Vector3 air_vel = rb.velocity.normalized;
                rb.velocity = air_vel * max_sj_airspeed;
            }
            slide_timer = max_slide_timer;
            current_airspeed = rb.velocity.magnitude;
        }
        //simply slide in a direction
        else
        {
            //sliding normally
            //perform the grounded check to see if the player is on a slope
            RaycastHit hit = new RaycastHit();
            Vector3 normal = Vector3.up;
            if (CheckGrounded(ref hit))
                normal = hit.normal;
            if (normal == Vector3.up || rb.velocity.y > -0.1f && !terminal_speed_hit)
            {
                if (slide_timer > 0.0f)
                    slide_timer -= Time.deltaTime;
                else
                    return;
                if (!set_slide_vector)
                {
                    slide_vector = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
                    slide_vector.Normalize();
                    if (slide_vector.magnitude == 0)
                        slide_vector = transform.forward;
                    set_slide_vector = true;
                }
                rb.velocity += slide_vector.normalized * slidespeed * Time.deltaTime;
            }
            else
            {
                //get the orthogonal
                slide_vector = Vector3.ProjectOnPlane(rb.velocity.normalized, normal).normalized;
                rb.velocity += slide_vector.normalized * slope_accel * Time.deltaTime;
                if (rb.velocity.magnitude >= terminal_speed_threshold)
                    terminal_speed_hit = true;
            }
        }
    }

    void CheckInteractable()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if (!Physics.Raycast(cam_transform.position, cam_transform.forward, out hit, punch_distance, ~(LayerMask.GetMask("Player") | LayerMask.GetMask("Ignore Raycast"))))
                return;

            if (hit.collider.tag == "Interactable")
                hit.collider.GetComponent<Interactable>().Interact();
        }
    }

    void DeathCam()
    {
        if (!dead)
        {
            animation_tick = 0.0f;
            dead = true;
        }
        else
        {
            cam_transform.localPosition = Vector3.Lerp(new Vector3(0.0f, 0.5f, 0.0f), death_cam_height, Mathf.Clamp01(animation_tick * death_cam_interp));
            if (stats.LastDamageDealer != null)
                cam_transform.rotation = Quaternion.LookRotation((stats.LastDamageDealer.transform.position - transform.position).normalized, transform.up);
            shotgun_position.anchoredPosition = Vector3.Lerp(new Vector3(0.0f, 364.1079f, 0.0f), new Vector3(0.0f, 37.0f, 0.0f), Mathf.Clamp01(animation_tick * death_cam_interp));
        }
    }

    void DeathLogic()
    {
        if (Input.anyKey)
        {
            if (started_respawn)
                return;
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            StartCoroutine(RespawnSequence());
            started_respawn = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        stats = GetComponent<PlayerStats>();

        rot_y = 0.0f;
        max_slide_timer = slide_timer;

        dead = false;

        shotgun_root = (RectTransform)transform.GetChild(1).GetChild(1);
        shotgun_position = (RectTransform)shotgun_root.GetChild(0);

        anim = transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Animator>();
        r_anim = transform.GetChild(1).GetChild(1).GetComponent<Animator>();
        p_anim = transform.GetChild(1).GetChild(9).GetComponent<Animator>();
        cam_transform = Camera.main.transform;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        cam_pivot = cam_transform.localPosition;

        hitbox = transform.GetChild(4).GetComponent<PunchHitboxBehavior>();
        stompbox = transform.GetChild(5).GetComponent<StompDamagingFieldBehavior>();
        punched_enemies = new List<GameObject>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        p_menu = GetComponent<PauseMenuBehavior>();

        col.material.dynamicFriction = 500.0f;
        col.material.dynamicFriction = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //don't do anything if you die
        if (stats.HP <= 0)
        {
            stats.AnnounceText = "Press 'ESC' to close the game, press any other key to respawn at last checkpoint";
            DeathCam();
            DeathLogic();
            return;
        }

        if (p_menu.paused) return;

        grounded = CheckGrounded();
        PunchLogic();
        Look();
        CheckInteractable();
        AnimateShotgun();
        CheckSliding();
        StompLogic();
        if (grounded)
        {
            ApplyFriction();
            if (!sliding)
                GroundAccel();
            else
                Slide();

            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector3(rb.velocity.x, jump_speed + rb.velocity.y, rb.velocity.z);
                StartCoroutine(JumpSequence());
                if (sliding)
                {
                    float current_vel = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
                    if (current_vel < 1.0f) current_vel = 1.0f;
                    rb.velocity = transform.forward * Mathf.Clamp(sj_speed * current_vel, 0.0f, max_sj_speed) + transform.up * (jump_speed + rb.velocity.y) / 1.5f;
                    set_slide_vector = false;
                }
            }
            if (!landed) StartCoroutine(LandSequence());
            landed = true;
        }
        else
        {
            if (!sliding)
                AirAccel();
            else
                Slide();
            set_slide_speed = false;
            landed = false;
        }
    }

    private void FixedUpdate()
    {
        if (!dead)
            animation_tick += Time.deltaTime * rb.velocity.magnitude / _movespeed;
        else
            animation_tick += Time.deltaTime;
    }
}
