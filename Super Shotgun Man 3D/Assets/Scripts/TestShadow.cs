using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TestShadow : MonoBehaviour
{
    private Material last_mat;
    void DrawShadow()
    {
        //first check beneath the sprite to see if there is a valid sector
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, float.PositiveInfinity, LayerMask.GetMask("Ground")))
        {

            //now search that subsector for the submesh that contains the material struck
            MeshCollider col = hit.collider as MeshCollider;

            Mesh mesh = col.sharedMesh;

            //optimization check to avoid the for loop

            int limit = hit.triangleIndex * 3;
            int submesh;

            for (submesh = 0; submesh < mesh.GetTriangles(submesh).Length; submesh++)
            {
                int num_indices = mesh.GetTriangles(submesh).Length;
                if (num_indices > limit) break;

                limit -= num_indices;
            }

            Material material = col.GetComponent<MeshRenderer>().sharedMaterials[submesh];

            //clear this object's shadow if it leaves its last known submesh
            if (last_mat != null)
            {
                if (material != last_mat)
                {
                    last_mat.SetVectorArray("_CastingObjects", new Vector4[1] { new Vector4(69.0f, 69.0f, 69.0f, 69.0f) });
                }
            }

            //set the first index of the other materials shadow position and width
            Vector4[] temp = new Vector4[1] { new Vector4(hit.point.x, hit.point.y, hit.point.z, 1.0f) };
            Debug.Log(temp[0]);
            material.SetVectorArray("_CastingObjects", temp);
            last_mat = material;
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawShadow();
    }
}
