using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpeedAudioHandler : MonoBehaviour
{
    private Rigidbody rb;
    private AudioSource source;

    // Start is called before the first frame update
    void Start()
    {
        rb = transform.root.GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //based off from how fast the player is going, adjust the volume of this audiosource
        source.volume = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp(rb.velocity.magnitude - 20.0f, 0.0f, Mathf.Infinity) / 30.0f) * PlayerPrefs.GetFloat("SFX Volume", 1.0f);
    }
}
