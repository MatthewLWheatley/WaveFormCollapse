

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public int seed;
    public (int x, int y, int z) max;
    public (int x, int y, int z) regionSize;


    private Dictionary<(int x, int y, int z), Region> mRegion;
    private Dictionary<(int x, int y, int z), GameObject> mGameObject;
    private HashSet<(int x, int y, int z)> mNotCollapsesed;
    [SerializeField] private GameObject RegionPrefab;

    public TextMeshProUGUI RunningTotal;

    private Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();
    public bool collapsed = false;
    public bool rendered = false;

    private (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };

    private Stack<(int x, int y, int z)> mStack;

    public float StartTime; 
    public float FinishCollapseTime;

    private int failCount = 0;

    private void Start()
    {
        mGameObject = new Dictionary<(int x, int y, int z), GameObject>();
        mNotCollapsesed = new HashSet<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();
        mRegion = new Dictionary<(int x, int y, int z), Region> ();

        StartTime = Time.time;
        time = Time.time;

        rnd = new System.Random(seed);


        InitRules();
        InitRegions();
    }

    public int collapseCount = 0;
    public int renderedCount = 0;
    int collapseNum = 0;
    (int x, int y, int z) targetRegion = (0, 0, 0);


    float time;

    System.Random rnd;

    private void Update()
    {
        time = Time.time;
        //Parallel.ForEach(mRegion, region =>
        //{
        //    mRegion[region.Key].RunUpdate();
        //}); 
        //foreach (var region in mRegion)
        //{
        //    mRegion[region.Key].RunUpdate();
        //}
        //Debug.Log($"{count}");
        if (collapseCount >= mRegion.Count) collapsed = true;
        if (renderedCount >= mRegion.Count) rendered = true;



        if (!collapsed)
        {
            var r = mRegion[targetRegion];
            r.running = true;
            //Debug.Log($"{collapseCount}");
            if (r.collapsed)
            {
                r.running = false;
                r.RunRenderer();
                //Debug.Log($"{StartTime - Time.time}");
                collapseCount++;
                mNotCollapsesed.Remove(targetRegion);
                //Debug.Log($"{StartTime - Time.time}");
                UpdateRegionEntropyList();
                //Debug.Log($"{StartTime - Time.time}");
                //foreach (var region in mRegion) 
                //{
                //    region.Value.UpdateAllEntropy();
                //}
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
            RegionScript.Initialize(region.Key, max, min, entropy, this,transform.position);
            mNotCollapsesed.Add(region.Key);
        }
    }

    public List<int> GetTileEntopry((int x, int y, int z) _tP) 
    {
        _tP.x = (_tP.x + max.x) % max.x;
        _tP.y = (_tP.y + max.y) % max.y;
        _tP.z = (_tP.z + max.z) % max.z;
        (int x, int y, int z) _targetRegion = (_tP.x & regionSize.x, _tP.y & regionSize.y, _tP.z & regionSize.z);
        //Debug.Log($"& {_targetRegion.x},{_targetRegion.y},{_targetRegion.z},  {_tP.x},{_tP.y},{_tP.z}");
        _targetRegion = (_tP.x % regionSize.x, _tP.y % regionSize.y, _tP.z % regionSize.z);
        //Debug.Log($"% {_targetRegion.x},{_targetRegion.y},{_targetRegion.z},  {_tP.x},{_tP.y},{_tP.z}");
        _targetRegion = (_tP.x / regionSize.x, _tP.y / regionSize.y, _tP.z / regionSize.z);
        //Debug.Log($"/ {_targetRegion.x},{_targetRegion.y},{_targetRegion.z},  {_tP.x},{_tP.y},{_tP.z}");
        return mRegion[_targetRegion].GetTile(_tP);
    }

    //TODO:
    public void ResetRegion((int x, int y, int z) _targetRegion)
    {
        ((int x, int y, int z) pos, (int x, int y, int z) min, (int x, int y, int z) max) region = (mRegion[_targetRegion].pos, mRegion[_targetRegion].min, mRegion[_targetRegion].max);
        Destroy(mRegion[_targetRegion]);
        mRegion.Remove(_targetRegion);
        Destroy(mGameObject[_targetRegion]);
        mGameObject.Remove(_targetRegion);

        //Debug.Log($"{region.Value.minX},{region.Value.minY},{region.Value.minZ},{region.Value.maxX},{region.Value.maxY},{region.Value.maxZ}");

        Vector3 position = new Vector3(0, 0, 0);//new Vector3(region.Value.minX * 3, region.Value.minY * 3, region.Value.minZ * 3);


        // Instantiate the manager prefab
        GameObject RegionInstance = Instantiate(RegionPrefab, position, Quaternion.identity, this.transform);
        RegionInstance.GetComponent<Region>().seed = seed;
        // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
        Region RegionScript = RegionInstance.GetComponent<Region>();
        mRegion.Add(_targetRegion, RegionScript);
        RegionScript.Initialize(region.pos, region.max, region.max, entropy, this, transform.position);
        mNotCollapsesed.Add(region.pos);
    }

    public void UpdateRegionEntropyList() 
    {

        ConcurrentDictionary<(int x, int y, int z), int> mRegionEntropy = new ConcurrentDictionary<(int x, int y, int z), int>();

        foreach (var tile in mRegion)
        {
            mRegion[tile.Key].UpdateAllEntropy();
        }

        Parallel.ForEach(mNotCollapsesed, key =>
        {
            var _ent = mRegion[key].GetEntropy();
            mRegionEntropy[key] = _ent; // ConcurrentDictionary handles the thread safety
        });
        //foreach (var key in mNotCollapsesed) 
        //{
        //    var _ent = mRegion[key].GetEntropy();
        //    mRegionEntropy[key] = _ent; // ConcurrentDictionary handles the thread safety
        //}

        HashSet<(int, int, int)> lowEntropyList = new HashSet<(int, int, int)>();
        int tempLowNum = int.MaxValue;
        foreach (var pos in mRegionEntropy)
        {
            var temp = pos.Value;
            if (temp < tempLowNum)
            {
                lowEntropyList = new HashSet<(int, int, int)>();
                lowEntropyList.Add(pos.Key);
                tempLowNum = temp;
            }
            else if (temp == tempLowNum)
            {
                lowEntropyList.Add(pos.Key);
            }
        }

        
        if (lowEntropyList.Count > 0)targetRegion = lowEntropyList.ElementAt(0);


        //Debug.Log($"{StartTime - Time.time}");
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
