using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSkyboxBehavior : MonoBehaviour
{
    public float x_increment, y_increment;
    public float x_offset, y_offset;
    private PlayerMovement movement;
    private Material mat;

    void UpdateSkyBox()
    {
        mat.SetVector("_SkyboxOffset", new Vector2((movement._RotX) * x_increment + x_offset, (movement._RotY + y_offset) * y_increment + y_offset));
    }

    // Start is called before the first frame update
    void Start()
    {
        movement = transform.root.GetComponent<PlayerMovement>();
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSkyBox();
    }
}
