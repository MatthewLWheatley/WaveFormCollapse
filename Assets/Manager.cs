

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public (int x, int y, int z) max;
    private Dictionary<(int x, int y, int z), Tile> mTile;
    private Dictionary<(int x, int y, int z), GameObject> mGameObject;
    private HashSet<(int x, int y, int z)> mNotCollapsesed;
    [SerializeField] private GameObject tilePrefab;

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
        mTile = new Dictionary<(int x, int y, int z), Tile>();
        mGameObject = new Dictionary<(int x, int y, int z), GameObject>();
        mNotCollapsesed = new HashSet<(int x, int y, int z)>();
        mStack = new Stack<(int x, int y, int z)>();

        StartTime = Time.time;
        
        InitRules();
        InitTiles();
    }

    private void Update()
    {
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
                else 
                { 
                    //failCount = 0;
                }
            }
            foreach (var pos in mNotCollapsesed)
            {
                UpdateEntropy(pos);
            }
            var list = GetLowestEntropyList();
            //Debug.Log($"{mNotCollapsesed.Count}");

            CollapseEntropy(list.ElementAt(Random.Range(0, list.Count)));
        }
        else if (!rendered)
        {
            FinishCollapseTime = Time.time;
            //Debug.Log($"rendering");
            foreach (var tile in mTile)
            {
                Vector3 _targetPos = new Vector3((float)tile.Key.x*3 + transform.position.x, (float)tile.Key.y * 3 + transform.position.y, (float)tile.Key.z * 3 + transform.position.z);
                GameObject TempTile = Instantiate(tilePrefab,_targetPos,Quaternion.identity,this.transform);

                TileProps temp = TempTile.GetComponent<TileProps>();
                temp.SetExits(entropy[tile.Value.GetExits()]);
            }
            CombineMeshes();

            rendered = true;
        }
    }

    private void InitRules()
    {
        byte[] _r = { 0x00, 0x01 };

        
        entropy.Add(0, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1] });
        entropy.Add(1, new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[1] });
        entropy.Add(2, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[1] });
        entropy.Add(3, new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(4, new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0] });
        entropy.Add(5, new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(6, new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1] });
        entropy.Add(7, new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1] });
    }

    private void InitTiles()
    {
        for (int x = 0; x < max.x; x++)
        {
            for (int y = 0; y < max.y; y++)
            {
                for (int z = 0; z < max.z; z++)
                {
                    Tile TempTile = new Tile();
                    TempTile.Initialize((x, y, z), max, entropy.Keys.ToHashSet());
                    mTile.Add((x, y, z), TempTile);
                    mNotCollapsesed.Add((x, y, z));
                }
            }
        }
    }

    private void UpdateEntropy((int x, int y, int z) pos)
    {
        // Loop through all directions and check entropy
        for (int _dir = 0; _dir < 6; _dir++)
        {
            (int x, int y, int z) _targetPosition = pos;
            _targetPosition.x = (_targetPosition.x + Dirs[_dir].x + max.x) % max.x;
            _targetPosition.y = (_targetPosition.y + Dirs[_dir].y + max.y) % max.y;
            _targetPosition.z = (_targetPosition.z + Dirs[_dir].z + max.z) % max.z;
            List<int> _targetEntropy = mTile[_targetPosition].GetEntropy();
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

        int _randNum = Random.Range(0, entropyCount);
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
}
