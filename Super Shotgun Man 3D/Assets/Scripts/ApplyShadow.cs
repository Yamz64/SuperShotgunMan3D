using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyShadow : MonoBehaviour
{
    private void Awake()
    {
        for(int i=0; i<transform.childCount; i++)
        {
            if(transform.GetChild(i).gameObject.tag == "Shadow")
            {
                transform.GetChild(i).GetComponent<ShadowBehavior>().casting_object = gameObject;
                break;
            }
        }
    }
}
