using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Slide : MonoBehaviour
{
    public float speed_factor;          //How fast to push the player
    public List<Transform> points;      //The list of points to push the player along
    
    //Check if C is between A and B
    bool IsBetweenAB(Vector3 A, Vector3 B, Vector3 C)
    {
        return Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f;
    }

    private void OnCollisionStay(Collision collision)
    {
        //If player, push them to next point
        if (collision.gameObject.tag.Equals("Player"))
        {
            for (int x = 0; x < points.Count-2; x++)
            {
                //If between 2 points, move towards second point
                if (IsBetweenAB(points[x].position, points[x + 1].position, collision.transform.position))
                {
                    Debug.Log("Between points " + x + " and " + (x + 1));
                    collision.gameObject.GetComponent<Rigidbody>().AddForce((points[x + 1].position - collision.gameObject.transform.position).normalized * (speed_factor * 0.5f));
                    break;
                }
            }
            //Otherwise, add force towards end point
            collision.gameObject.GetComponent<Rigidbody>().AddForce((points[points.Count - 1].position - collision.gameObject.transform.position).normalized * speed_factor);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
