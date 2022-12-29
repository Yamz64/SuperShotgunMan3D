using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretTrigger : MonoBehaviour
{
    private bool triggered;

    private void Start()
    {
        triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if(other.tag == "Player")
        {
            AudioUtils.InstanceSound(5, transform.position, this);
            other.GetComponent<PlayerStats>().AnnounceText = "You found a secret!";
            triggered = true;
        }
    }
}
