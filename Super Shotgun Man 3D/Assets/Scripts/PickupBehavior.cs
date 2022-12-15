using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehavior : MonoBehaviour
{
    private enum PowerupType { SMALL_HP, BIG_HP, ARMOR, E_ROUNDS, INVINCIBILITY};
    [SerializeField]
    private PowerupType type;

    // Start is called before the first frame update
    void Start()
    {
        //based on the powerup type update the material index
        Material mat = GetComponent<Renderer>().material;

        mat.SetFloat("_SpriteIndex", (float)type);
    }
}
