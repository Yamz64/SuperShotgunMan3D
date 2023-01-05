using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    public Shader affine_shader;
    private List<ShadowCaster> casters;
    private List<Material> affine_materials;
    class ShadowCaster
    {
        public Vector3 position;
        public Vector3 shadow_position;
    }

    Vector3 GetShadowPosition(ShadowCaster caster)
    {
        //first check beneath the sprite to see if there is a valid sector
        RaycastHit hit;
        if (Physics.Raycast(caster.position, -Vector3.up, out hit, float.PositiveInfinity, LayerMask.GetMask("Ground")))
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

            if (material.shader == affine_shader)
            {
                //return this object's shadow position
                return hit.point;
            }
        }
        return Vector3.positiveInfinity;
    }

    void UpdateShadowPositions()
    {
        //first get a list of all shadowcasting objects
        casters = new List<ShadowCaster>();

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] props = GameObject.FindGameObjectsWithTag("Prop");
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");
        GameObject[] pickups = GameObject.FindGameObjectsWithTag("Pickup");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        for (int i = 0; i < enemies.Length; i++)
        {
            ShadowCaster caster = new ShadowCaster();
            caster.position = enemies[i].transform.position;
            caster.shadow_position = Vector3.positiveInfinity;
            casters.Add(caster);
        }
        for (int i = 0; i < props.Length; i++)
        {
            ShadowCaster caster = new ShadowCaster();
            caster.position = props[i].transform.position;
            caster.shadow_position = Vector3.positiveInfinity;
            casters.Add(caster);
        }
        for (int i = 0; i < projectiles.Length; i++)
        {
            ShadowCaster caster = new ShadowCaster();
            caster.position = projectiles[i].transform.position;
            caster.shadow_position = Vector3.positiveInfinity;
            casters.Add(caster);
        }
        for (int i = 0; i < pickups.Length; i++)
        {
            ShadowCaster caster = new ShadowCaster();
            caster.position = pickups[i].transform.position;
            caster.shadow_position = Vector3.positiveInfinity;
            casters.Add(caster);
        }
        {
            ShadowCaster caster = new ShadowCaster();
            caster.position = player.transform.position;
            caster.shadow_position = Vector3.positiveInfinity;
            casters.Add(caster);
        }

        //next generate a shadow position for each caster
        for (int i = 0; i < casters.Count; i++)
        {
            Vector3 shadow_position = GetShadowPosition(casters[i]);
            casters[i].shadow_position = shadow_position;
        }

        Vector4[] shadow_positions = new Vector4[256];
        for (int i = 0; i < 256; i++)
        {
            if (i == casters.Count)
                break;
            shadow_positions[i] = new Vector4(casters[i].shadow_position.x, casters[i].shadow_position.y, casters[i].shadow_position.z, .5f);
        }
        for (int i = 0; i < affine_materials.Count; i++)
        {
            affine_materials[i].SetVectorArray("_CastingObjects", shadow_positions);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //now get every material that uses the affine texture warp shader
        Renderer[] renderers = (Renderer[])Object.FindObjectsOfType(typeof(Renderer));
        affine_materials = new List<Material>();

        for(int i=0; i<renderers.Length; i++)
        {
            for(int j=0; j<renderers[i].materials.Length; j++)
            {
                if(renderers[i].materials[j].shader == affine_shader)
                {
                    affine_materials.Add(renderers[i].materials[j]);
                }
            }
        }

        //finally set every affine material's shadow buffers
        UpdateShadowPositions();
    }

    private void Update()
    {
        UpdateShadowPositions();
    }
}
