using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickupBehavior : MonoBehaviour
{
    public float explosive_shells_duration, invincibility_duration;
    public int small_hp_bonus, big_hp_bonus, armor_bonus;
    private enum PowerupType { SMALL_HP, BIG_HP, ARMOR, E_ROUNDS, INVINCIBILITY, SHOTGUN};
    [SerializeField]
    private PowerupType type;
    [SerializeField]
    private Sprite eshells_sprite, invincibility_sprite;

    //only on level one is the normal sound used, otherwise play the alternate version
    void PlayPickupShotgunSound()
    {
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            AudioUtils.InstanceVoice(0, transform.position, this);
            string option = PlayerPrefs.GetString("Subtitles", "Off");
            if (option.Equals("On"))
            { 
                //GameObject subtitles = GameObject.FindGameObjectWithTag("Subtitle");
                //StartCoroutine(subtitles.GetComponent<Subtitles_Behavior>().displaySubtitles(3.0f, "[Shotgun Cocks]" + System.Environment.NewLine + "Uhhh...I'll call you Margherit."));
            }
        }
        else
            AudioUtils.InstanceVoice(1, transform.position, this);
    }

    //various burps whenever the player picks up a small item (indices 2-4)
    void PlaySmallPickupSound()
    {
        int index = Random.Range(2, 5);
        AudioUtils.InstanceVoice(index, transform.position, this, null, false, 1f, Random.Range(0.5f, 1.0f));
    }

    //big burp whenever the player picks up a big item (indices 5-6), has a rare chance to play the stock burp sound (index 7)
    void PlayBigPickupSound()
    {
        int index = Random.Range(5, 7);
        if (Random.Range(0, 100) == 0)
        {
            index = 7;
            AudioUtils.InstanceVoice(index, transform.position, this);
        }
        else
            AudioUtils.InstanceVoice(index, transform.position, this, null, false, 1f, Random.Range(0.5f, 1.0f));
    }

    //plays whenever the player picks up a big health at low hp
    void PlayHealthPickupCritical()
    {
        AudioUtils.InstanceVoice(8, transform.position, this);
    }

    // Start is called before the first frame update
    void Start()
    {
        //based on the powerup type update the material index
        Material mat = GetComponent<Renderer>().material;

        mat.SetFloat("_SpriteIndex", (float)type);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if the player is detected, do something to the player based on the powerup
        if (other.tag != "Player")
            return;

        bool used = false;
        PlayerStats stats = other.GetComponent<PlayerStats>();

        switch (type){
            case PowerupType.SMALL_HP:
                stats.HP += small_hp_bonus;
                stats.AnnounceText = "Picked up pizza slice!";
                used = true;
                PlaySmallPickupSound();
                break;
            case PowerupType.BIG_HP:
                if (stats.HP + big_hp_bonus < 100)
                {
                    if (stats.HP > 20)
                        PlayBigPickupSound();
                    else
                        PlayHealthPickupCritical();
                    stats.HP += big_hp_bonus;
                    stats.AnnounceText = "Picked up pizza pie!";
                    used = true;
                }
                else if (stats.HP < 100)
                {
                    stats.HP = 100;
                    stats.AnnounceText = "Picked up pizza pie!";
                    used = true;
                    PlayBigPickupSound();
                }
                break;
            case PowerupType.ARMOR:
                if (stats.AP < 100)
                {
                    stats.AP += armor_bonus;
                    stats.AnnounceText = "Picked up a cold one!";
                    used = true;
                    PlaySmallPickupSound();
                }
                break;
            case PowerupType.E_ROUNDS:
                if(stats.PowerupTime <= 0)
                {
                    stats.PowerupTime = explosive_shells_duration;
                    stats.EShells = true;
                    stats.PowerUpSprite = eshells_sprite;
                    stats.AnnounceText = "Picked up explosive rounds!";
                    stats.face.OneTimeAnimationDriver(4);
                    used = true;
                    PlayBigPickupSound();
                }
                break;
            case PowerupType.INVINCIBILITY:
                if(stats.PowerupTime <= 0)
                {
                    stats.PowerupTime = invincibility_duration;
                    stats.Invincible = true;
                    stats.PowerUpSprite = invincibility_sprite;
                    stats.AnnounceText = "Picked up some sort of doo-hickey?";
                    stats.face.OneTimeAnimationDriver(4);
                    used = true;
                    PlayBigPickupSound();
                }
                break;
            case PowerupType.SHOTGUN:
                stats.HasShotgun = true;
                used = true;
                PlayPickupShotgunSound();
                break;
        }
        if(used)
            Destroy(gameObject);
    }
}
