using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Slide : MonoBehaviour
{
    public float speed_factor;
    public List<Transform> points;

    bool IsBetweenAB(Vector3 A, Vector3 B, Vector3 C)
    {
        return Vector3.Dot((B - A).normalized, (C - B).normalized) < 0f && Vector3.Dot((A - B).normalized, (C - A).normalized) < 0f;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            for (int x = 0; x < points.Count-1; x++)
            {
                if (IsBetweenAB(points[x].position, points[x + 1].position, collision.transform.position))
                {
                    collision.gameObject.GetComponent<Rigidbody>().AddForce(points[x + 1].position * speed_factor);
                    break;
                }
            }
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
