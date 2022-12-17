using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomBoxInteractable : Interactable
{
    public int active_song;

    public override void Interact()
    {
        active_song++;
        if (active_song == 3)
            active_song = 0;

        AudioUtils.StopeMusic(this);
        AudioUtils.PlayMusic(active_song, this, 1f);
    }

    private void Start()
    {
        active_song = 0;
        AudioUtils.PlayMusic(0, this, 1f);
    }
}
