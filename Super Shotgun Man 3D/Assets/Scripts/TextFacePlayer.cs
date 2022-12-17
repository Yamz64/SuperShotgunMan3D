using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

public class TextFacePlayer : MonoBehaviour
{
    public float fade_distance_min, fade_distance_max;

    private TextMeshPro text;

    void LerpColor()
    {
        Vector3 player_pos = GameObject.FindGameObjectWithTag("Player").transform.position;
        float distance = (player_pos - transform.position).magnitude;

        float lerp = (distance - fade_distance_min) / (fade_distance_max - fade_distance_min);

        text.color = Color.Lerp(Color.white, new Color(1.0f, 1.0f, 1.0f, 0.0f), lerp);
    }

    private void Start()
    {
        text = GetComponent<TextMeshPro>();
    }
    
    // Update is called once per frame
    void Update()
    {
        LerpColor();
        transform.forward = -(GameObject.FindGameObjectWithTag("Player").transform.position - transform.position).normalized;
    }
}
