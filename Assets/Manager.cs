using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int maxX = 5, maxY = 5, maxZ = 5;

    private Dictionary<(int, int, int), GameObject> map = new Dictionary<(int, int, int), GameObject>();
    private List<byte[]> entropy = new List<byte[]>();

    private float lastUpdateTime = 0f;

    private float startTime = 0f;

    private void Start()
    {
        startTime = Time.realtimeSinceStartup;
        CreateRules();
        SpawnTiles();
    }

    private void CreateRules()
    {
        byte[] _r = { 0x00, 0x01 };

        entropy.Add(new byte[] { _r[1], _r[1], _r[0], _r[0], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[1], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[1], _r[1], _r[0], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[1], _r[0], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[1], _r[0], _r[0], _r[1], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[1], _r[0], _r[0], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[1], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[1], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[0], _r[0], _r[1], _r[1]});
    }

    private void SpawnTiles()
    {
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    Vector3 position = new Vector3(x * 3, y * 3, z * 3);
                    GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    map[(x, y, z)] = tile;

                    TileManager tileManager = tile.GetComponent<TileManager>();
                    if (tileManager != null)
                    {
                        tileManager.Initialize((x, y, z), maxX, maxY, maxZ, entropy, this.gameObject);
                    }
                    else
                    {
                        Debug.LogError("TileManager component not found on the tile prefab.");
                    }
                }
            }
        }
    }

    private List<(int,int,int)> GetLowestEntropyList() 
    {
        List<(int, int, int)> lowEntopyList = new List<(int, int, int)>();
        int tempLowNum = entropy.Count+2;
        foreach (var tile in map) 
        {
            var temp = tile.Value.GetComponent<TileManager>().GetEntropyCount();
            if (tile.Value.GetComponent<TileManager>().GetCollapsed())
            {
                continue;
            }
            if (temp < tempLowNum)
            {
                lowEntopyList = new List<(int, int, int)>();
                lowEntopyList.Add(tile.Key);
                tempLowNum = temp;
            }
            else if (temp == tempLowNum) 
            {
                lowEntopyList.Add(tile.Key);
            }
        }

        return lowEntopyList;
    }

    public void Reset()
    {
        startTime = Time.realtimeSinceStartup;
        foreach (var tile in map) 
        {
            Destroy(tile.Value);
        }
        map = new Dictionary<(int, int, int), GameObject>();
        entropy = new List<byte[]>();
        CreateRules();
        SpawnTiles();
    }

    private void Update()
    {
        if (Time.time - lastUpdateTime >= 0.0000001f)
        {
            //get all tiles entropy
            //find all with the lowest entropy
            var _list = GetLowestEntropyList();

            //randomly pick one
            if (_list.Count == 0)
            {
                foreach (var tile in map) 
                {
                    tile.Value.GetComponent<TileManager>().UpdateExits();
                }
                Debug.Log($"{Time.realtimeSinceStartup- startTime}");
                Reset();
                return;
            }

            int _randNum = Random.Range(0, _list.Count);
            //Debug.Log(_list.Count);
            (int x, int y, int z) _tilePos = _list[_randNum];
            var _tile = map[_tilePos].GetComponent<TileManager>();

            //collapse the entropy for that one
            //by picking the one of the possible entropy
            _tile.CollapseEntropy();

            //propergate the collapse to the sorunding
            List<(int x, int y, int z)> dirs = new List<(int x, int y, int z)> {
                (1,0,0),(-1,0,0),(0,1,0),(0,-1,0),(0,0,1),(0,0,-1)};
                //(2,0,0),(-2,0,0),(0,2,0),(0,-2,0),(0,0,2),(0,0,-2),
                //(1,0,1),(-1,0,1),(1,0,-1),(-1,0,-1),
                //(1,1,0),(-1,1,0),(1,-1,0),(-1,-1,0),
                //(0,1,1),(0,-1,1),(0,1,-1),(0,-1,-1)};
            for (int i = 0; i < dirs.Count; i++)
            {
                (int x, int y, int z) _targetPos = _tilePos;
                _targetPos.x = (_targetPos.x + dirs[i].x) % maxX;
                if (_targetPos.x < 0) _targetPos.x += maxX;
                _targetPos.y = (_targetPos.y + dirs[i].y) % maxY;
                if (_targetPos.y < 0) _targetPos.y += maxY;
                _targetPos.z = (_targetPos.z + dirs[i].z) % maxZ;
                if (_targetPos.z < 0) _targetPos.z += maxZ;

                _tile = map[_targetPos].GetComponent<TileManager>();
                _tile.UpdateEntropy();
            }


            lastUpdateTime = Time.time;
        }
    }

    public GameObject GetTile((int x,int y,int z) _pos) 
    {
        return map[_pos];
    }

    class Tile
    {
        public GameObject Parent { get; private set; }

        private int maxX, maxY, maxZ;
        private bool collapsed = false;
        private (int x, int y, int z) pos;
        private List<byte[]> entropy = new List<byte[]>();

        [SerializeField] private GameObject centre;
        [SerializeField] private GameObject[] cubes = new GameObject[6];
        private byte[] exits = new byte[6];

        public void Initialize((int x, int y, int z) position, int mx, int my, int mz, List<byte[]> ent, GameObject parent)
        {
            pos = position;
            maxX = mx; maxY = my; maxZ = mz;
            Parent = parent;
            entropy = new List<byte[]>(ent); // Deep copy if necessary
            centre.SetActive(false);
        }

    }
}
