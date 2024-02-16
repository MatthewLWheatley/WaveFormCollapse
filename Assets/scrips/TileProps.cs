using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TileProps : MonoBehaviour
{
    public GameObject Parent { get; private set; }
    private byte[] exits = new byte[6];
    [SerializeField] private GameObject centre;
    [SerializeField] private GameObject[] edges;
    [SerializeField] private GameObject edge;
    [SerializeField] private Material[] mats;
    public bool renderered = false;

    private Mesh[] meshes;

    public Vector3 poss;

    private void Start()
    {
        edges = new GameObject[6];
    }

    bool done = false;

    public void SetExits(byte[] _exits,(int x, int y, int z) pos)
    {
        if(renderered) { return; }
        poss.x = pos.x; poss.y = pos.y; poss.z = pos.z;
        edges = new GameObject[6];
        exits = _exits;
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] > 0)
            {
                Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);
                switch (i)
                {
                    case 0:
                        Target.x += 1;
                        break;
                    case 1:
                        Target.y += 1;
                        break;
                    case 2:
                        Target.z += 1;
                        break;
                    case 3:
                        Target.x -= 1;
                        break;
                    case 4:
                        Target.y -= 1;
                        break;
                    case 5:
                        Target.z -= 1;
                        break;
                }
                if (!(edges[i] != null)) 
                { 
                    edges[i] = Instantiate(edge, transform.position + Target, Quaternion.identity, this.transform);
                    // Debug.Log(i);

                    centre.SetActive(true);
                    done = true;

                    edges[i].GetComponent<MeshRenderer>().material = mats[exits[i] - 1];
                    centre.GetComponent<MeshRenderer>().material = mats[exits[i] - 1];

                    renderered = true;
                }
            }
            else 
            {
                //if (edges[i] != null && edges[i].gameObject != null)
                //{
                //    edges[i].gameObject.SetActive(false);
                //}
            }
        }
        if (done)
        {
            CombineMeshes();
            for (int i = 0; i < exits.Length; i++)
            {
                if (edges[i] != null)
                    edges[i].SetActive(false);
                
            }
            centre.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        ResetExits();
    }

    public void ResetExits()
    {
        MeshFilter meshFilter;
        // Destroy all child GameObjects
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Destroy associated meshes
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            Destroy(meshFilter.sharedMesh);
        }
    }

    void CombineMeshes()
    {
        // Ensure this GameObject has a MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Ensure this GameObject has a MeshRenderer component
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Get all MeshFilter components from child objects
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(false);

        // Group meshes by material
        Dictionary<Material, List<MeshFilter>> materialGroups = new Dictionary<Material, List<MeshFilter>>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.sharedMesh == null || mf == meshFilter) continue; // Skip if the sharedMesh is null or it's the parent MeshFilter

            MeshRenderer mr = mf.GetComponent<MeshRenderer>();
            if (mr == null || mr.sharedMaterial == null) continue; // Skip if there's no MeshRenderer or sharedMaterial

            if (!materialGroups.ContainsKey(mr.sharedMaterial))
            {
                materialGroups[mr.sharedMaterial] = new List<MeshFilter>();
            }
            materialGroups[mr.sharedMaterial].Add(mf);
        }

        // Combine meshes for each material group
        CombineInstance[] combine = new CombineInstance[materialGroups.Count];
        Material[] materials = new Material[materialGroups.Count];
        int materialIndex = 0;
        List<GameObject> objectsToDestroy = new List<GameObject>();
        foreach (KeyValuePair<Material, List<MeshFilter>> kvp in materialGroups)
        {
            CombineInstance[] materialCombines = new CombineInstance[kvp.Value.Count];
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                materialCombines[i].mesh = kvp.Value[i].sharedMesh;
                materialCombines[i].transform = kvp.Value[i].transform.localToWorldMatrix;
                objectsToDestroy.Add(kvp.Value[i].gameObject); // Mark the child object for deletion
            }

            // Combine meshes for the current material
            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(materialCombines, true, true);

            combine[materialIndex].mesh = combinedMesh;
            combine[materialIndex].transform = Matrix4x4.identity;
            materials[materialIndex] = kvp.Key;
            materialIndex++;
        }

        // Create a new mesh and combine all the grouped meshes into it
        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(combine, false, false);
        meshFilter.mesh = finalMesh;
        meshRenderer.materials = materials; // Set the materials array

        // Optionally, reset the position if needed
        meshRenderer.transform.position = Vector3.zero;

        // Destroy the child GameObjects
        foreach (GameObject go in objectsToDestroy)
        {
            Destroy(go);
        }
    }
}
