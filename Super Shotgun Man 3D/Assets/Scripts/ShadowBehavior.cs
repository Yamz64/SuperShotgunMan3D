using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowBehavior : MonoBehaviour
{
    public GameObject casting_object;
    [SerializeField]
    private float shadow_radius = 0.5f;
    Vector3 parent_pos;
    private int last_triangleindex;
    void UpdateShadowPosition()
    {
        RaycastHit hit;
        if(Physics.Raycast(parent_pos, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Ground"))){
            transform.position = hit.point;
            transform.forward = -hit.normal;
            transform.position += -transform.forward * 0.01f;
        }
    }

    void UpdateColor()
    {
        GetComponent<MeshRenderer>().material.SetFloat("_ShadowIntensity", 0.0f);
        // first check beneath the sprite to see if there is a valid sector
        RaycastHit hit;
        if (Physics.Raycast(parent_pos, -Vector3.up, out hit, float.PositiveInfinity, LayerMask.GetMask("Ground")))
        {
            GetComponent<MeshRenderer>().material.SetFloat("_ShadowIntensity", 0.75f);

            //now search that subsector for the submesh that contains the material struck
            MeshCollider col = hit.collider as MeshCollider;

            Mesh mesh = col.sharedMesh;

            //optimization check to avoid the for loop
            if (last_triangleindex == hit.triangleIndex) return;
            else last_triangleindex = hit.triangleIndex;

            int limit = hit.triangleIndex * 3;
            int submesh;

            for (submesh = 0; submesh < mesh.GetTriangles(submesh).Length; submesh++)
            {
                int num_indices = mesh.GetTriangles(submesh).Length;
                if (num_indices > limit) break;

                limit -= num_indices;
            }

            Material material = col.GetComponent<MeshRenderer>().sharedMaterials[submesh];

            //set this material's light level to the lightlevel of the material
            if (GetComponent<MeshRenderer>().material.HasProperty("_ShadowColor") && material.HasProperty("_UnlitColor"))
                GetComponent<MeshRenderer>().material.SetColor("_ShadowColor", material.GetColor("_UnlitColor"));
            return;
        }
        last_triangleindex = -1;
    }

    private void Start()
    {
        parent_pos = casting_object.transform.position;
        GetComponent<MeshRenderer>().material.SetFloat("_ShadowRadius", shadow_radius);
    }

    // Update is called once per frame
    void Update()
    {
        parent_pos = casting_object.transform.position;
        UpdateShadowPosition();
        UpdateColor();
    }
}
