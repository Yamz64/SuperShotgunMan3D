using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FXUtils
{
    private static FXScriptableObject FX_Prefabs = Resources.Load<FXScriptableObject>("FX Container");

    public static void InstanceFXObject(int o, Vector3 position, Quaternion rotation, Transform parent = null, bool alt_mode = false, int damage = 75, float duration = 0.5f, float max_radius = 1.0f)
    {
        GameObject instance = (GameObject)Object.Instantiate(FX_Prefabs.FX_ARRAY[o], position, rotation, parent);

        //behaviors for alternate modes of effects
        if (alt_mode)
        {
            switch (o){
                case 2:
                    instance.GetComponent<ExplosionBehavior>().damaging = true;
                    break;
            }
        }

        //set values for certain effects
        switch (o)
        {
            case 2:
                instance.GetComponent<ExplosionBehavior>().damage = damage;
                instance.GetComponent<ExplosionBehavior>().effect_duration = duration;
                instance.GetComponent<ExplosionBehavior>().max_radius = max_radius;
                break;
        }
    }
}
