using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using static System.Net.WebRequestMethods;

public class Region : MonoBehaviour
{
    public Manager manager;
    private Dictionary<(int x, int y, int z), Tile> mTile;
    public (int x, int y, int z) max;
    public (int x, int y, int z) min;
    public (int x, int y, int z) pos;
    public Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();
    public bool collapsed = false;
    public bool rendered = false;
    private HashSet<(int x, int y, int z)> mNotCollapsesed;
    private int failCount = 0;

    private Vector3 transPosisiton = Vector3.zero;

    [SerializeField] private GameObject tilePrefab;

    private Stack<(int x, int y, int z)> mStack;
    private (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };

    public float StartTime;
    public float FinishCollapseTime;

    private System.Random rnd;

    public bool running = false;

    private void Update()
    {
        if(running)
        if (!collapsed)
        {
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
                    if (mTile[tile].GetEntropyCount() == 0)
                    {
                        failed = true;
                    }
                }
                if (failed)
                {
                    failCount++;
                    HashSet<(int x, int y, int z)> tempList = new HashSet<(int x, int y, int z)>();
                    if (failCount >= mStack.Count)
                    {
                        failCount = mStack.Count - 1;
                    }
                    if (mStack.Count == 0)
                    {
                        failCount = 0;
                    }
                    for (int i = 0; i < failCount; i++)
                    {
                        //Debug.Log($"{mStack.Count} {failCount}");
                        var temp = mStack.Pop();
                        mTile[temp].SetEntropy(entropy.Keys.ToHashSet());
                        tempList.Add(temp);
                        mNotCollapsesed.Add(temp);
                    }
                    Parallel.ForEach(mNotCollapsesed, temp =>
                    {
                        mTile[temp].SetEntropy(entropy.Keys.ToHashSet());
                    });
                    return;
                }
            }
            var temp2 = GetLowestEntropyFullList();
            foreach (var pos in temp2)
            {
                UpdateEntropy(pos,true);
            }
            rnd = new System.Random();
            var list = GetLowestEntropyList();
            var num = rnd.Next(0, list.Count);
            var num2 = list.ElementAt(rnd.Next(0, list.Count));
            //Debug.Log($"{list.Count} {num} {num2}"); 
            CollapseEntropy(num2);
        }
    }

    public void RunUpdate()
    {
        
    }

    public void RunRenderer() 
    {
        if (!rendered)
        {
            Debug.Log($"rendering");
            foreach (var tile in mTile)
            {
                Vector3 _targetPos = new Vector3((float)tile.Key.x * 3 + transPosisiton.x, (float)tile.Key.y * 3 + transPosisiton.y, (float)tile.Key.z * 3 + transPosisiton.z);
                GameObject TempTile = Instantiate(tilePrefab, _targetPos, Quaternion.identity, transform);

                TileProps temp = TempTile.GetComponent<TileProps>();
                temp.SetExits(entropy[tile.Value.GetExits()]);
            }

            CombineMeshes();

            rendered = true;
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

        System.Random rnd = new System.Random();
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
            Destroy(meshFilters[i].gameObject); // Disable the child object

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

        meshRenderer.transform.position = new Vector3(0, 0, 0);
    }

    public List<int> GetTile((int x, int y, int z) _targetTile)
    {
        return mTile[_targetTile].GetEntropy();
    } 
}