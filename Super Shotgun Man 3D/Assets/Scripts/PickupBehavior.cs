using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehavior : MonoBehaviour
{
    public float explosive_shells_duration, invincibility_duration;
    public int small_hp_bonus, big_hp_bonus, armor_bonus;
    private enum PowerupType { SMALL_HP, BIG_HP, ARMOR, E_ROUNDS, INVINCIBILITY};
    [SerializeField]
    private PowerupType type;
    [SerializeField]
    private Sprite eshells_sprite, invincibility_sprite;

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
                break;
            case PowerupType.BIG_HP:
                if (stats.HP + big_hp_bonus < 100)
                {
                    stats.HP += big_hp_bonus;
                    stats.AnnounceText = "Picked up pizza pie!";
                    used = true;
                }
                else if (stats.HP < 100)
                {
                    stats.HP = 100;
                    stats.AnnounceText = "Picked up pizza pie!";
                    used = true;
                }
                break;
            case PowerupType.ARMOR:
                if (stats.AP < 100)
                {
                    stats.AP += armor_bonus;
                    stats.AnnounceText = "Picked up a cold one!";
                    used = true;
                }
                break;
            case PowerupType.E_ROUNDS:
                if(stats.PowerupTime <= 0)
                {
                    stats.PowerupTime = explosive_shells_duration;
                    stats.EShells = true;
                    stats.PowerUpSprite = eshells_sprite;
                    stats.AnnounceText = "Picked up explosive rounds!";
                    used = true;
                }
                break;
            case PowerupType.INVINCIBILITY:
                if(stats.PowerupTime <= 0)
                {
                    stats.PowerupTime = invincibility_duration;
                    stats.Invincible = true;
                    stats.PowerUpSprite = invincibility_sprite;
                    stats.AnnounceText = "Picked up some sort of doo-hickey?";
                    used = true;
                }
                break;
        }
        if(used)
            Destroy(gameObject);
    }
}
