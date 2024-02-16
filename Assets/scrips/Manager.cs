using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    public int seed;
    public (int x, int y, int z) max;
    public (int x, int y, int z) regionSize;


    private Dictionary<(int x, int y, int z), Region> mRegion;
    private Dictionary<(int x, int y, int z), GameObject> mGameObject;
    public List<(int x, int y, int z)> mNotCollapsesed;
    [SerializeField] private GameObject RegionPrefab;

    public TextMeshProUGUI RunningTotal;

    private Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();
    public bool collapsed = false;
    public bool rendered = false;

    private (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1)};

    private Stack<(int x, int y, int z)> mStack;

    private (int x, int y, int z) maxRegion;

    public float StartTime; 
    public float FinishCollapseTime;

    public int mCollapseCount = 0;

    private void Start()
    {
        mGameObject = new Dictionary<(int x, int y, int z), GameObject>();
        mNotCollapsesed = new List<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();
        mRegion = new Dictionary<(int x, int y, int z), Region> ();


        StartTime = Time.time;
        time = Time.time;


        InitRules();
        InitRegions();
    }

    public int collapseCount = 0;
    public int renderedCount = 0;
    int collapseNum = 0;
    (int x, int y, int z) targetRegion = (0, 0, 0);


    float time;

    private void Update()
    {
        mCollapseCount = mNotCollapsesed.Count;

        time = Time.time;
        
        if (renderedCount >= mRegion.Count) rendered = true;

        if (!collapsed)
        {
            var r = mRegion[targetRegion];
            r.running = true;
            r.RunUpdate();
            r.RunRenderer();
            if (r.resetCount > 0) 
            {
                if (mStack.Count > 0)
                {
                    foreach (var tile in mNotCollapsesed)
                    {
                        ResetRegion(tile, 0);
                        mRegion[tile].ResetRegionState();
                    }
                    //Debug.Log("pop");
                    var temp5 = mStack.Pop();
                    mNotCollapsesed.Add(temp5);
                    targetRegion = temp5;
                    ResetRegion(targetRegion, 0);
                    //r.RunRenderer();
                    foreach (var tile in mNotCollapsesed)
                    {
                        mRegion[tile].UpdateAllEntropy();
                    }
                }
                else 
                {
                    Debug.Log("stack empty");
                    Debug.Log($"{mNotCollapsesed.Count}");
                    foreach (var tile in mNotCollapsesed)
                    {
                        Debug.Log($"{tile.x},{tile.y},{tile.z}");
                        ResetRegion(tile, 0);
                    }
                }
            }
            if (r.collapsed)
            {
                //r.RunRenderer();
                //r.CombineMeshes();
                mStack.Push(targetRegion);
                r.running = false;
                //r.RunRenderer();
                collapseCount++;
                mNotCollapsesed.Remove(targetRegion);
                UpdateRegionEntropyList(true);
                
            }
            RunningTotal.text = string.Format(collapseCount.ToString());
        }
        else if (!rendered)
        {
            var r = mRegion[mRegion.ElementAt(renderedCount).Key];
            r.RunRenderer();
            if (r.rendered)
            {
                renderedCount++;
            }
            r.rendered = true;
            RunningTotal.text = string.Format(renderedCount.ToString());
        }
    }

    private void InitRules()
    {
        byte[] _r = { 0x00, 0x01, 0x02};

        //entropy.Add(-1, new byte[] { _r[0], _r[0], _r[0], _r[0], _r[0], _r[0] });

        entropy.Add(0, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1] });
        entropy.Add(1, new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[1] });
        entropy.Add(2, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[1] });
        entropy.Add(3, new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(4, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0] });
        entropy.Add(5, new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0] });
        entropy.Add(6, new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1] });
        entropy.Add(7, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(8, new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1] });
        entropy.Add(9, new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1] });

        entropy.Add(10, new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[2] });
        entropy.Add(11, new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[2] });
        entropy.Add(12, new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[2] });
        entropy.Add(13, new byte[] { _r[2], _r[0], _r[2], _r[2], _r[0], _r[0] });
        entropy.Add(14, new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[0] });
        entropy.Add(15, new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[0] });
        entropy.Add(16, new byte[] { _r[2], _r[0], _r[0], _r[0], _r[0], _r[2] });
        entropy.Add(17, new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[0] });
        entropy.Add(18, new byte[] { _r[0], _r[0], _r[2], _r[0], _r[0], _r[2] });
        entropy.Add(19, new byte[] { _r[0], _r[0], _r[0], _r[2], _r[0], _r[2] });

        //entropy.Add(8, new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[1] });
        //entropy.Add(9, new byte[] { _r[1], _r[1], _r[0], _r[1], _r[0], _r[1] });
        //entropy.Add(10, new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[1] });
        //entropy.Add(11, new byte[] { _r[1], _r[1], _r[1], _r[1], _r[0], _r[0] });
        //entropy.Add(12, new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[0] });
        //entropy.Add(13, new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[0] });
        //entropy.Add(14, new byte[] { _r[0], _r[1], _r[0], _r[1], _r[0], _r[1] });
        //entropy.Add(15, new byte[] { _r[1], _r[1], _r[0], _r[0], _r[0], _r[1] });

        //entropy.Add(16, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[1] });
        //entropy.Add(17, new byte[] { _r[1], _r[0], _r[0], _r[1], _r[1], _r[1] });
        //entropy.Add(18, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[1] });
        //entropy.Add(19, new byte[] { _r[1], _r[0], _r[1], _r[1], _r[1], _r[0] });
        //entropy.Add(20, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[0] });
        //entropy.Add(21, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[0] });
        //entropy.Add(22, new byte[] { _r[0], _r[0], _r[0], _r[1], _r[1], _r[1] });
        //entropy.Add(23, new byte[] { _r[1], _r[0], _r[0], _r[0], _r[1], _r[1] });

        //entropy.Add(24, new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[1] });
        //entropy.Add(25, new byte[] { _r[1], _r[1], _r[0], _r[1], _r[1], _r[1] });
        //entropy.Add(26, new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[1] });
        //entropy.Add(27, new byte[] { _r[1], _r[1], _r[1], _r[1], _r[1], _r[0] });
        //entropy.Add(28, new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[0] });
        //entropy.Add(29, new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[0] });
        //entropy.Add(30, new byte[] { _r[0], _r[1], _r[0], _r[1], _r[1], _r[1] });
        //entropy.Add(31, new byte[] { _r[1], _r[1], _r[0], _r[0], _r[1], _r[1] });
    }

    private void InitRegions() 
    {
        var regions = new Dictionary<(int x, int y, int z), (int minX, int minY, int minZ, int maxX, int maxY, int maxZ)>();

        int posX = 0, posY = 0, posZ = 0; // Trackers for the chunk's position

        for (int x = 0; x < max.x; x += regionSize.x, posX++)
        {
            posY = 0; // Reset Y position for each new X level
            for (int y = 0; y < max.y; y += regionSize.y, posY++)
            {
                posZ = 0; // Reset Z position for each new Y level in the curresnt X
                for (int z = 0; z < max.z; z += regionSize.z, posZ++)
                {
                    int endX = Math.Min(x + regionSize.x, max.x);
                    int endY = Math.Min(y + regionSize.y, max.y);
                    int endZ = Math.Min(z + regionSize.z, max.z);

                    // Use the position as the key and the chunk boundaries as the value
                    //Debug.Log($"({posX}, {posY}, {posZ}), ({x}, {y}, {z}, {endX}, {endY}, {endZ})");

                    regions.Add((posX, posY, posZ), (x, y, z, endX, endY, endZ));
                }
            }
        }
        maxRegion = (posX, posY, posZ);

        foreach (var region in regions)
        {
            //Debug.Log($"{region.Value.minX},{region.Value.minY},{region.Value.minZ},{region.Value.maxX},{region.Value.maxY},{region.Value.maxZ}");

            Vector3 position = new Vector3(0, 0, 0);//new Vector3(region.Value.minX * 3, region.Value.minY * 3, region.Value.minZ * 3);

            // Instantiate the manager prefab
            GameObject RegionInstance = Instantiate(RegionPrefab, position, Quaternion.identity, this.transform);
            RegionInstance.GetComponent<Region>().seed = seed;
            // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
            Region RegionScript = RegionInstance.GetComponent<Region>();
            mRegion.Add(region.Key, RegionScript);
            var min = (region.Value.minX, region.Value.minY, region.Value.minZ);
            var max = (region.Value.maxX, region.Value.maxY, region.Value.maxZ);
            RegionScript.Initialize(region.Key, max, min, entropy, this);
            mNotCollapsesed.Add(region.Key);
        }
    }

    public List<int> GetTileEntropy((int x, int y, int z) _tP)
    {
        // Normalize the tile position within the bounds of the world.
        _tP.x = (_tP.x + max.x) % max.x;
        _tP.y = (_tP.y + max.y) % max.y;
        _tP.z = (_tP.z + max.z) % max.z;

        // Calculate the target region coordinates based on the tile's position.
        // This assumes that regionSize.{x,y,z} are greater than 0 to avoid division by zero.
        (int x, int y, int z) _tR = (_tP.x / regionSize.x, _tP.y / regionSize.y, _tP.z / regionSize.z);


        //if (mRegion[_tR].GetTile(_tP).Count == 0) Debug.Log($"{_tP.x},{_tP.y},{_tP.z} {mRegion[_tR].GetTile(_tP).Count}");

        //mRegion[_tR]
        // Fetch and return the entropy for the tile within the target region.
        return mRegion[_tR].GetTile(_tP);
    }

    public void ResetRegion((int x, int y, int z) _startRegion, int counter)
    {
        ((int x, int y, int z) pos, (int x, int y, int z) min, (int x, int y, int z) max) region = (mRegion[targetRegion].pos, mRegion[targetRegion].min, mRegion[targetRegion].max);
        MeshFilter meshFilter;
        if ((mRegion[targetRegion].transform.TryGetComponent<MeshFilter>(out meshFilter)))
        {
            Destroy(meshFilter.mesh); // Destroy the mesh
        }
        Destroy(mRegion[targetRegion].gameObject);
        mRegion.Remove(targetRegion);

        Vector3 position = new Vector3(0, 0, 0);
        GameObject RegionInstance = Instantiate(RegionPrefab, position, Quaternion.identity, this.transform);
        Region RegionScript = RegionInstance.GetComponent<Region>();
        mRegion.Add(targetRegion, RegionScript);
        RegionScript.Initialize(region.pos, region.max, region.min, entropy, this);
        collapseCount = (maxRegion.x * maxRegion.y * maxRegion.z) - mNotCollapsesed.Count;
    }

    public void ResetAllRegion()
    {
        foreach (var GO in mGameObject) 
        {
            Destroy(GO.Value);
        }
        mGameObject = new Dictionary<(int x, int y, int z), GameObject>();
        MeshFilter meshFilter;
        foreach (var R in mRegion)
        {
            if ((mRegion[targetRegion].transform.TryGetComponent<MeshFilter>(out meshFilter)))
            {
                Destroy(meshFilter.mesh); // Destroy the mesh
            }
            Destroy(R.Value);
        }

        mNotCollapsesed = new List<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();
        mRegion = new Dictionary<(int x, int y, int z), Region>();
        entropy = new Dictionary<int, byte[]>();

        collapseCount = 0;

        Start();
    }

    public void UpdateRegionEntropyList(bool change) 
    {
        if (mNotCollapsesed.Count > 0)
        {
            // Update all entropy values first.
            foreach (var tile in mNotCollapsesed)
            {
                mRegion[tile].ResetRegionState();
                mRegion[tile].UpdateAllEntropy();
                //mRegion[tile].UpdateAllEntropy();
            }

            // Create a dictionary of regions and their entropy values.
            var mRegionEntropy = mNotCollapsesed.ToDictionary(key => key, key => mRegion[key].GetEntropy());

            // Find the minimum entropy value.
            int minEntropy = mRegionEntropy.Values.Min();

            // Select all regions that have this minimum entropy value.
            var lowEntropyRegions = mRegionEntropy.Where(pair => pair.Value == minEntropy).Select(pair => pair.Key).ToList();

            // Safely update targetRegion with the first region of the lowest entropy, if any.
            if (lowEntropyRegions.Any())
            {
                if (change) targetRegion = lowEntropyRegions[0];
            }
        }
        else 
        {
            collapsed = true;
        }
    }

    public void CheckDone() 
    {
        var count = 0;
        foreach (var region in mRegion) 
        {
            if (region.Value.collapsed) 
            {
                count++;
            }
        }
        if (count > max.x * max.y * max.z) collapsed = true;
    }

    void CombineMeshes()
    {
        //// Ensure this GameObject has a MeshFilter component
        //MeshFilter meshFilter = GetComponent<MeshFilter>();
        //if (meshFilter == null)
        //{
        //    meshFilter = gameObject.AddComponent<MeshFilter>();
        //}

        //// Ensure this GameObject has a MeshRenderer component
        //MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        //if (meshRenderer == null)
        //{
        //    meshRenderer = gameObject.AddComponent<MeshRenderer>();
        //}

        //// Get all MeshFilter components from child objects
        //MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(false);

        //// Group meshes by material
        //Dictionary<Material, List<MeshFilter>> materialGroups = new Dictionary<Material, List<MeshFilter>>();

        //foreach (MeshFilter mf in meshFilters)
        //{
        //    if (mf.sharedMesh == null || mf == meshFilter) continue; // Skip if the sharedMesh is null or it's the parent MeshFilter

        //    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
        //    if (mr == null || mr.sharedMaterial == null) continue; // Skip if there's no MeshRenderer or sharedMaterial

        //    if (!materialGroups.ContainsKey(mr.sharedMaterial))
        //    {
        //        materialGroups[mr.sharedMaterial] = new List<MeshFilter>();
        //    }
        //    materialGroups[mr.sharedMaterial].Add(mf);
        //}

        //// Combine meshes for each material group
        //CombineInstance[] combine = new CombineInstance[materialGroups.Count];
        //Material[] materials = new Material[materialGroups.Count];
        //int materialIndex = 0;
        //List<GameObject> objectsToDestroy = new List<GameObject>();
        //foreach (KeyValuePair<Material, List<MeshFilter>> kvp in materialGroups)
        //{
        //    CombineInstance[] materialCombines = new CombineInstance[kvp.Value.Count];
        //    for (int i = 0; i < kvp.Value.Count; i++)
        //    {
        //        materialCombines[i].mesh = kvp.Value[i].sharedMesh;
        //        materialCombines[i].transform = kvp.Value[i].transform.localToWorldMatrix;
        //        objectsToDestroy.Add(kvp.Value[i].gameObject); // Mark the child object for deletion
        //    }

        //    // Combine meshes for the current material
        //    Mesh combinedMesh = new Mesh();
        //    combinedMesh.CombineMeshes(materialCombines, true, true);

        //    combine[materialIndex].mesh = combinedMesh;
        //    combine[materialIndex].transform = Matrix4x4.identity;
        //    materials[materialIndex] = kvp.Key;
        //    materialIndex++;
        //}

        //// Create a new mesh and combine all the grouped meshes into it
        //Mesh finalMesh = new Mesh();
        //finalMesh.CombineMeshes(combine, false, false);
        //meshFilter.mesh = finalMesh;
        //meshRenderer.materials = materials; // Set the materials array

        //// Optionally, reset the position if needed
        //meshRenderer.transform.position = Vector3.zero;

        //// Destroy the child GameObjects
        //foreach (GameObject go in objectsToDestroy)
        //{
        //    Destroy(go);
        //}
    }
}
