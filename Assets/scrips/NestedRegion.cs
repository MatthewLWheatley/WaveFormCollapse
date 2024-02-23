using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static System.Net.WebRequestMethods;

public class NestedRegion
{
    // Variables for managing procedural generation and rendering.
    public int seed; // Seed for the random number generator to ensure reproducibility.
    public Manager manager; // Reference to a manager class that probably handles global game state or level management.
    public Dictionary<(int x, int y, int z), Tile> mTile; // Stores tiles with their coordinates as keys.
    public (int x, int y, int z) max; // Maximum bounds for the region.
    public (int x, int y, int z) min; // Minimum bounds for the region.
    public (int x, int y, int z) pos; // Current position of the region.
    public Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>(); // Stores entropy values for procedural generation.
    public bool collapsed = false; // Flag to indicate if the wave function collapse has completed.
    public bool rendered = false; // Flag to indicate if the region has been rendered.
    public List<(int x, int y, int z)> mNotCollapsesed; // Tracks tiles that have not yet collapsed.
    public int failCount = 0; // Counter for the number of failures during generation, used for error handling or retries.
    public int resetCount = 0;
    public int maxFailCount = 10;

    private Vector3 transPosisiton = Vector3.zero; // Translation position for rendering adjustments.

    [SerializeField] private GameObject tilePrefab; // Prefab for individual tiles, used in rendering.

    private Stack<(int x, int y, int z)> mStack; // Stack to manage the order of tile collapses.
    private (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) }; // Directions for neighbor checking.

    public float StartTime; // Tracks the start time of the generation for performance metrics.
    public float FinishCollapseTime; // Tracks the time it takes to finish collapsing for performance metrics.

    public bool running = false; // Flag to control the update loop.


    public int mCollapseCount = 0;

    private void Update()
    {
        mCollapseCount = mNotCollapsesed.Count;
    }

    public bool updateingEntropy = false;
    public int tileCounter = 0;

    System.Random rnd = new System.Random();

    public void RunUpdate()
    {
        if (CheckAndHandleCollapse()) return;
        AttemptCollapseRandomTile();
    }

    public void UpdateAllEntropy()
    {
        var temp = GetLowestEntropyFullList();
        
        foreach ((int x, int y, int z) pos in temp)
        {
            UpdateEntropy(pos, true);
        }
    }

    private bool CheckAndHandleCollapse()
    {
        if (mNotCollapsesed.Count == 0)
        {
            collapsed = true;
            return true; // All tiles have collapsed.
        }

        if (CheckForFailState()) HandleFailState();
        return collapsed; // Return true if handling fail state resulted in a collapse.
    }

    private bool CheckCollpase() 
    {
        foreach (var tile in mTile) 
        {
            if (!tile.Value.GetCollapsed()) collapsed = false;
            
        }
        if(collapsed == false) return false;
        else return true;
    }

    private bool CheckForFailState()
    {

        return mNotCollapsesed.Any(tile => mTile[tile].GetEntropyCount() == 0);
    }

    private void HandleFailState()
    {
        for (int i = 0; i < mNotCollapsesed.Count; i++)
        {
            var temp = mNotCollapsesed[i];
            mTile[temp].SetEntropy(entropy.Keys.ToHashSet());
        }

        failCount++;
        for (int i = 0; i < failCount; i++) 
        { 
            if (mStack.Count > 0)
            {
                var lastPos = mStack.Pop();
                mNotCollapsesed.Add(lastPos);

                mTile[lastPos].SetEntropy(entropy.Keys.ToHashSet());

                UpdateEntropy(lastPos, true);

                ResetRegionState();
            } 
        }

        if (failCount > mStack.Count) 
        { 
            failCount = 0;
            resetCount++;
            ResetRegionState();
            UpdateAllEntropy();
        }

        
        if (!CheckForFailState())
        {
            return;
        }
    }

    private void AttemptCollapseRandomTile()
    {
        var lowEntropyList = GetLowestEntropyList().ToList(); // Assumes this could change from earlier if failures were handled.
        if (lowEntropyList.Count == 0) return;

        var randomIndex = rnd.Next(0, lowEntropyList.Count);
        var selectedPos = lowEntropyList[randomIndex];
        if (!CollapseEntropy(selectedPos)) 
        { 
            HandleFailState();
        }
    }

    public void RunRenderer()
    {
        if (!rendered) // Check if the region has not been rendered yet.
        {
            manager.SpawnTiles(mTile);
        }
    }

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, (int x, int y, int z) _min, Dictionary<int, byte[]> _ent, Manager _man)
    {
        // Initialize region with specified parameters, preparing it for the procedural generation process.
        pos = _position;
        min = _min;
        max = _max;
        manager = _man;
        mTile = new Dictionary<(int x, int y, int z), Tile>();
        mNotCollapsesed = new List<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();
        
        entropy = _ent;
        foreach (var en in _ent)
        {
            if (!entropy.ContainsKey(en.Key))
                entropy.Add(en.Key, en.Value);
        }
        InitTiles(); // Initialize tiles based on the specified bounds and entropy.

        //StartTime = Time.time; // Record start time for performance tracking.
    }

    private void InitTiles()
    {
        //Debug.Log($"max:{max.x},{max.y},{max.z} min:{min.x},{min.y},{min.z}fuck you");
        // Initialize all tiles within the specified bounds with maximum entropy.
        for (int x = min.x; x < max.x; x++)
        {
            //Debug.Log(x);
            for (int y = min.y; y < max.y; y++)
            {
                //Debug.Log(y);
                for (int z = min.z; z < max.z; z++)
                {
                    //Debug.Log(z);
                    if (!mTile.ContainsKey((x, y, z)))
                    {
                        //Debug.Log($"{x}, {y}, {z}");
                        //Debug.Log("ficlklclckckngdusujjnksl;ujnikhsejnklrfhujnlikesgvujnhkmred");
                        //Vector3 _targetPos = new Vector3((float)x * 3 + transPosisiton.x, (float)y * 3 + transPosisiton.y, (float)z * 3 + transPosisiton.z);
                        //GameObject TempTile = Instantiate(tilePrefab, _targetPos, Quaternion.identity, transform);
                        ////TempTile.AddComponent<Tile>();
                        //Tile temp = TempTile.GetComponent<Tile>();
                        Tile temp = new Tile();
                        temp.Initialize((x, y, z), max, entropy.Keys.ToHashSet());

                        mTile.Add((x, y, z), temp);
                        mNotCollapsesed.Add((x, y, z));
                        //GamobjectMap.Add((x, y, z), TempTile);
                    }

                }
            }
        }
    }

    private void UpdateEntropy((int x, int y, int z) pos, bool full)
    {

        List<int> toRemove = new List<int>();
        // Loop through all directions and check entropy
        for (int _dir = 0; _dir < 6; _dir++)
        {
            (int x, int y, int z) _tP = (pos.x + Dirs[_dir].x, pos.y + Dirs[_dir].y, pos.z + Dirs[_dir].z);
            if (pos.x == _tP.x && pos.y == _tP.y && pos.z == _tP.z) continue;
            List<int> _tE;
            bool isOutside = _tP.x < min.x || _tP.x >= max.x || _tP.y < min.y || _tP.y >= max.y || _tP.z < min.z || _tP.z >= max.z;
            
            if(isOutside)_tE = manager.GetTileEntropy(_tP).ToList();
            else _tE = mTile[_tP].GetEntropy().ToList();

            int _correspondingExit = (_dir + 3) % 6;
            List<byte> possibleExits = new List<byte>();

            foreach (var _exit in _tE)
            {
                byte exitValue = entropy[_exit][_correspondingExit];
                possibleExits.Add(exitValue);
            }

            foreach (var ent in mTile[pos].entropy.ToList())
            {
                if (!possibleExits.Contains(entropy[ent][_dir]))
                {
                    toRemove.Add(ent);
                }
            }
        }
        foreach (var remove in toRemove)
        {
            if (mTile[pos].entropy.Contains(remove)) mTile[pos].entropy.Remove(remove);
        }
    }

    private bool CollapseEntropy((int x, int y, int z) pos)
    {
        UpdateAllEntropy();
        int entropyCount = mTile[pos].GetEntropyCount();
        if (entropyCount == 0)
        {
            ResetRegionState();
            return false;
        }

        int _randNum = rnd.Next(0, entropyCount);
        int randomEntropyElement = mTile[pos].GetEntropy().ElementAt(_randNum);
        mTile[pos].entropy = new HashSet<int>();
        mTile[pos].entropy.Add(randomEntropyElement);
        //manager.mGameObject[pos].collapsed = true;
        mNotCollapsesed.Remove(pos);
        mStack.Push(pos);
        return true;

    }

    private HashSet<(int, int, int)> GetLowestEntropyList()
    {
        HashSet<(int, int, int)> lowEntopyList = new HashSet<(int, int, int)>();
        int tempLowNum = entropy.Count + 2;
        foreach (var pos in mNotCollapsesed)
        {
            var temp = mTile[pos].GetEntropyCount();
            if (temp < tempLowNum)
            {
                lowEntopyList = new HashSet<(int, int, int)>();
                lowEntopyList.Add(pos);
                tempLowNum = temp;
            }
            else if (temp == tempLowNum)
            {
                lowEntopyList.Add(pos);
            }
        }
        return lowEntopyList;
    }

    private List<(int, int, int)> GetLowestEntropyFullList()
    {
        List<(int, int, int)> orderedList = new List<(int, int, int)>();

        // Calculate entropy for each position and store in a dictionary
        Dictionary<(int, int, int), int> entropyMap = new Dictionary<(int, int, int), int>();
        foreach (var pos in mNotCollapsesed)
        {
            var temp = mTile[pos].GetEntropyCount();
            entropyMap[pos] = temp;
        }

        // Sort positions by entropy (ascending order)
        var sortedPositions = entropyMap.OrderBy(pair => pair.Value);

        // Add sorted positions to the ordered list
        foreach (var entry in sortedPositions)
        {
            orderedList.Add(entry.Key);
        }

        return orderedList;
    }

    public int GetEntropy() 
    {
        int _ent = 0;
        // Calculate entropy for each position and store in a dictionary
        foreach (var pos in mNotCollapsesed)
        {
            if (pos.x == min.x || pos.x == max.x)
            {   
                if (pos.y == min.y || pos.y == max.y)
                {
                    if (pos.z == min.z || pos.z == max.z)
                    {
                        UpdateEntropy(pos, false);
                        var temp = mTile[pos].GetEntropyCount();
                        _ent += temp;
                    }
                }
            }
        }
        return _ent;
    }

    public HashSet<int> GetTile((int x, int y, int z) _targetTile)
    {
        //HashSet<int> result = new HashSet<int>();
        return mTile[_targetTile].entropy;
    }

    public void ResetRegionState()
    {
        foreach (var pos in mNotCollapsesed)
        {
            mTile[pos].SetEntropy(entropy.Keys.ToHashSet());
        }
        collapsed = false;
        rendered = false;
    }

    private void ResetVisuals()
    {
        //foreach (Transform child in transform)
        //{
        //    Destroy(child.gameObject);
        //}
    }

    public void CombineMeshes()
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
        //foreach (KeyValuePair<Material, List<MeshFilter>> kvp in materialGroups)
        //{
        //    CombineInstance[] materialCombines = new CombineInstance[kvp.Value.Count];
        //    for (int i = 0; i < kvp.Value.Count; i++)
        //    {
        //        materialCombines[i].mesh = kvp.Value[i].sharedMesh;
        //        materialCombines[i].transform = kvp.Value[i].transform.localToWorldMatrix;
        //        kvp.Value[i].gameObject.SetActive(false); // Disable the child object
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
    }
}