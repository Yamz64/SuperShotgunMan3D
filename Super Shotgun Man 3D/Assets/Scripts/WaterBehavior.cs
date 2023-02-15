using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBehavior : MonoBehaviour
{
    public bool lethal = true;
    public float minimum_horizontal_projection = 0.5f, minimum_skim_speed, skip_velocity;
    private Vector3 last_player_position;
    private bool kill_player;
    private MeshCollider col;

    // Start is called before the first frame update
    void Start()
    {
        kill_player = false;
        col = GetComponent<MeshCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        last_player_position = player.transform.position;
        last_player_position += player.GetComponent<CapsuleCollider>().center;

        if (kill_player)
        {
            if (player.transform.GetChild(0).transform.position.y < col.bounds.max.y && lethal)
            {
                if (player.GetComponent<PlayerStats>().HP > 0)
                    player.GetComponent<PlayerStats>().TakeDamage(int.MaxValue);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //check if it's the player entering the trigger, if it is then first see if the player's last position was above the surface of the water
        if (other.tag != "Player")
            return;

        if (last_player_position.y < col.bounds.max.y)
            kill_player = true;

        //now check if the player's overall velocity vector is pointed horizontal enough to avoid drowning
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb.velocity.x == 0 && rb.velocity.z == 0)
            kill_player = true;
        else
        {
            Vector3 velocity_no_y = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).normalized;
            if (Vector3.Dot(rb.velocity.normalized, velocity_no_y) < minimum_horizontal_projection)
                kill_player = true;
        }

        //see if the player is moving fast enough to move across the water
        if (rb.velocity.magnitude < minimum_skim_speed)
            kill_player = true;

        //if the player shouldn't be killed then see how the player should skim across the surface
        if(!kill_player)
        {
            //first put the player at the surface of the water and zero their y velocity
            CapsuleCollider player_col = other.GetComponent<CapsuleCollider>();
            other.transform.position = new Vector3(other.transform.position.x, col.bounds.max.y + player_col.height / 2.0f, other.transform.position.z);
            rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);

            bool special_case = false;
            //determine if the player should skip
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                rb.velocity += Vector3.up * skip_velocity;
                special_case = true;
            }

            //allow the player to jump out
            if (Input.GetKey(KeyCode.Space))
            {
                rb.velocity += Vector3.up * other.GetComponent<PlayerMovement>().jump_speed;
                special_case = true;
            }

            //allow the player to skim
            if (!special_case)
                other.transform.position += Vector3.up * 0.01f;
        }
    }
}
