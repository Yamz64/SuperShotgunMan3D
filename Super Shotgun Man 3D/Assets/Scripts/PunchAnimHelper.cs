using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchAnimHelper : MonoBehaviour
{
    private PlayerMovement movement;

    private void Start()
    {
        movement = transform.root.GetComponent<PlayerMovement>();
    }

    public void InitialPunch() { movement.InitialPunch(); }
    public void PlaySoundSwing() { AudioUtils.InstanceSound(2, transform.position, this, transform.root, false, 1f, .85f); }
}
