using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static System.Net.WebRequestMethods;

public class Region : MonoBehaviour
{
    public int seed;
    public Manager manager;
    private Dictionary<(int x, int y, int z), Tile> mTile;
    public (int x, int y, int z) max;
    public (int x, int y, int z) min;
    public (int x, int y, int z) pos;
    public Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();
    public bool collapsed = false;
    public bool rendered = false;
    private HashSet<(int x, int y, int z)> mNotCollapsesed;
    public int failCount = 0;

    private Vector3 transPosisiton = Vector3.zero;

    [SerializeField] private GameObject tilePrefab;

    private Stack<(int x, int y, int z)> mStack;
    private (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };

    public float StartTime;
    public float FinishCollapseTime;

    private System.Random rnd;

    public bool running = false;


    private void Start()
    {
        rnd = new System.Random(seed);
    }

    private void Update()
    {
        //rnd = new System.Random(seed);
        FinishCollapseTime += Time.deltaTime;
        if (FinishCollapseTime <= 0.01)
        {
            return;
        }    

        if (running)
        {
            FinishCollapseTime = 0;
            if (!collapsed)
            {
                var temp4 = GetLowestEntropyFullList();

                foreach ((int x, int y, int z) pos in temp4)
                {
                    UpdateEntropy(pos, true);
                }
                if (mNotCollapsesed.Count == 0)
                {
                    collapsed = true;
                    return;
                }
                else
                {
                    bool failed = false;

                    foreach (var tile in mNotCollapsesed)
                    {
                        //Debug.Log($"a.{mTile[tile].GetEntropyCount()}");
                        if (mTile[tile].GetEntropyCount() == 0)
                        {
                            failed = true;
                            //Debug.Log($"{tile.x},{tile.y},{tile.z} failed");
                            //mTile[tile].SetEntropy(entropy.Keys.ToHashSet());
                            //Debug.Log($"b.{mTile[tile].GetEntropyCount()}");
                        }
                    }
                    if (failed)
                    {
                        //Debug.Log($" {failCount} {mStack.Count} {mNotCollapsesed.Count}");
                        failCount++;
                        HashSet<(int x, int y, int z)> tempList = new HashSet<(int x, int y, int z)>();
                        if (failCount > mStack.Count)
                        {
                            failCount = mStack.Count;
                            //Debug.Log("fuck");
                        }
                        if (mStack.Count == 0)
                        {
                            failCount = 0;
                        }
                        for (int i = 0; i < failCount; i++)
                        {
                            Debug.Log($"{mStack.Count} {failCount}");
                            var temp = mStack.Pop();
                            mTile[temp].SetEntropy(entropy.Keys.ToHashSet());
                            tempList.Add(temp);
                            mNotCollapsesed.Add(temp);
                        }
                        Parallel.ForEach(mNotCollapsesed, temp =>
                        {
                            mTile[temp].SetEntropy(entropy.Keys.ToHashSet());
                        });
                        //foreach (var tile in mNotCollapsesed)
                        //{
                        //    mTile[tile].SetEntropy(entropy.Keys.ToHashSet());
                        //}
                        //return;
                    }
                }
                var temp2 = GetLowestEntropyFullList();

                foreach ((int x, int y, int z) pos in temp2)
                {
                    UpdateEntropy(pos, true);
                }
                //rnd = new System.Random(seed);

                var list = GetLowestEntropyList();
                var num = rnd.Next(0, list.Count);
                var num2 = list.ElementAt(rnd.Next(0, list.Count));
                //Debug.Log($"{list.Count} {num} {num2}");
                CollapseEntropy(num2);

                //Debug.Log($"{mNotCollapsesed.Count}");

                //foreach (var tile in mNotCollapsesed)
                //{
                //    //Debug.Log($"a.{mTile[tile].GetEntropyCount()}");
                //    if (mTile[tile].GetEntropyCount() == 0)
                //    {
                //        //mTile[tile].SetEntropy(entropy.Keys.ToHashSet());
                //        //Debug.Log($"b.{tile.x},{tile.y},{tile.z}");
                //    }
                //}
            }
            RunRenderer();
        }
    }

    public void RunRenderer() 
    {
        if (!rendered)
        {
            // Iterate backwards through the child list
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                // Destroy the child GameObject
                Destroy(transform.GetChild(i).gameObject);
            }

            //Debug.Log($"rendering");
            foreach (var tile in mTile)
            {
                Vector3 _targetPos = new Vector3((float)tile.Key.x * 3 + transPosisiton.x, (float)tile.Key.y * 3 + transPosisiton.y, (float)tile.Key.z * 3 + transPosisiton.z);
                GameObject TempTile = Instantiate(tilePrefab, _targetPos, Quaternion.identity, transform);

                TileProps temp = TempTile.GetComponent<TileProps>();
                var e = tile.Value.GetExits();
                byte[] ent;
                if (e == -1)
                { 
                    ent = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
                }
                else
                {
                    ent = entropy[e];
                }
                temp.SetExits(ent, tile.Key);
            }

            CombineMeshes();

            //rendered = true;
        }
    }

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, (int x, int y, int z) _min, Dictionary<int, byte[]> _ent, Manager _man, Vector3 _transPos)
    {
        pos = _position;
        min = _min;
        max = _max;
        manager = _man;



        mTile = new Dictionary<(int x, int y, int z), Tile>();
        mNotCollapsesed = new HashSet<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();

        entropy = _ent;
        foreach (var en in _ent)
        {
            if(!entropy.ContainsKey(en.Key))
            entropy.Add(en.Key,en.Value);
        }
        InitTiles();

        StartTime = Time.time;

        rnd = new System.Random(seed);
    }

    private void InitTiles()
    {
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    Tile TempTile = new Tile();
                    TempTile.Initialize((x, y, z), max, entropy.Keys.ToHashSet());
                    mTile.Add((x, y, z), TempTile);
                    mNotCollapsesed.Add((x, y, z));
                }
            }
        }
    }

    public void UpdateAllEntropy() 
    {
        var temp = GetLowestEntropyFullList();

        foreach ((int x, int y, int z) pos in temp)
        {
            UpdateEntropy(pos, true);
        }
    }

    private void UpdateEntropy((int x, int y, int z) pos, bool full)
    {
        // Loop through all directions and check entropy
        for (int _dir = 0; _dir < 6; _dir++)
        {
            (int x, int y, int z) _tP = pos;
            _tP.x = (_tP.x + Dirs[_dir].x);
            _tP.y = (_tP.y + Dirs[_dir].y);
            _tP.z = (_tP.z + Dirs[_dir].z);
            List<int> _targetEntropy;
            if (full || !full)
            {
                if (_tP.x >= max.x || _tP.x <= min.x || _tP.y >= max.y || _tP.y <= min.y || _tP.z >= max.z || _tP.z <= min.z)
                {
                    _targetEntropy = manager.GetTileEntopry(_tP);
                }
                else
                {
                    _targetEntropy = mTile[_tP].GetEntropy();
                }
            }
            else 
            {
                _targetEntropy = entropy.Keys.ToList();
            }
            List<int> toRemove = new List<int>();
            var _correspondingExit = (_dir + 3) % 6;
            HashSet<byte> possbileExits = new HashSet<byte>();

            //find all the possible exits
            foreach (var _exit in _targetEntropy) if (!possbileExits.Contains(entropy[_exit][_correspondingExit])) possbileExits.Add(entropy[_exit][_correspondingExit]);


            foreach (var ent in mTile[pos].entropy) if (!possbileExits.Contains(entropy[ent][_dir])) toRemove.Add(ent);

            //remove everything in the remove list
            foreach (var item in toRemove) mTile[pos].entropy.Remove(item);
        }
    }

    private void CollapseEntropy((int x, int y, int z) pos)
    {
        int entropyCount = mTile[pos].GetEntropyCount();
        if (entropyCount == 0) return;

        int _randNum = rnd.Next(0, entropyCount);
        int randomEntropyElement = mTile[pos].GetEntropy().ElementAt(_randNum);
        mTile[pos].entropy = new HashSet<int>();
        mTile[pos].entropy.Add(randomEntropyElement);
        mNotCollapsesed.Remove(pos);
        mStack.Push(pos);
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

    public List<int> GetTile((int x, int y, int z) _targetTile)
    {
        return mTile[_targetTile].GetEntropy();
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
        foreach (KeyValuePair<Material, List<MeshFilter>> kvp in materialGroups)
        {
            CombineInstance[] materialCombines = new CombineInstance[kvp.Value.Count];
            for (int i = 0; i < kvp.Value.Count; i++)
            {
                materialCombines[i].mesh = kvp.Value[i].sharedMesh;
                materialCombines[i].transform = kvp.Value[i].transform.localToWorldMatrix;
                kvp.Value[i].gameObject.SetActive(false); // Disable the child object
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
    }
}