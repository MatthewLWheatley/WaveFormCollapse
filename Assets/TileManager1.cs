using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TileManager : MonoBehaviour
{
    public GameObject Parent { get; private set; }
    private byte[] exits = new byte[6];
    [SerializeField] private GameObject centre;
    [SerializeField] private GameObject[] edges;
    [SerializeField] private GameObject edge;

    private Mesh[] meshes;

    private void Start()
    {
        edges = new GameObject[6];
    }

    bool done = false;

    public void SetExits(byte[] _exits)
    {
        exits = _exits;
        for (int i = 0; i < exits.Length; i++)
        {
            // WTF
            try 
            {
                if (edges[i].name == "") 
                { }
            }
            catch(NullReferenceException) 
            {
                if (exits[i] == 1)
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
                    GameObject edgeInstance = Instantiate(edge, this.transform.position + Target, Quaternion.identity, this.transform);
                    edges[i] = edgeInstance;
                    centre.SetActive(true);
                    done = true;
                }
            }

        }
        if (done)
        {
            done = false;
            CombineMeshes();
        }
    }

    public void ResetExits()
    {
        Destroy(this.transform);
    }

    void CombineMeshes()
    {
        // Ensure this GameObject has a MeshFilter component
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        // Get all MeshFilter components from child objects
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1]; // Exclude the parent's MeshFilter

        int index = 0;
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].sharedMesh == null) continue; // Skip if the sharedMesh is null
            if (meshFilters[i] == meshFilter) continue; // Skip the parent's MeshFilter

            combine[index].mesh = meshFilters[i].sharedMesh;
            combine[index].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false); // Disable the child object
            index++;
        }

        // Create a new mesh and combine all the child meshes into it
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);

        // Ensure this GameObject has a MeshRenderer component
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        // Set the material (assuming all child objects use the same material)
        if (meshFilters.Length > 1 && meshFilters[1].GetComponent<MeshRenderer>())
        {
            meshRenderer.sharedMaterial = meshFilters[1].GetComponent<MeshRenderer>().sharedMaterial;
        }

        meshRenderer.transform.position = new Vector3(0,0,0);
    }
}
