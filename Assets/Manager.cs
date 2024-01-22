using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] public int maxX = 5, maxY = 5, maxZ = 5;

    private Dictionary<(int, int, int), Tile> map = new Dictionary<(int, int, int), Tile>();
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

        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1]});
        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0]});
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1]});
    }

    private void SpawnTiles()
    {
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    Vector3 position = new Vector3(x * 3 + this.transform.position.x, y * 3 + this.transform.position.y, z * 3 + this.transform.position.z);
                    GameObject Child = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                    Tile tile = new Tile();
                    tile.Initialize((x, y, z), maxX, maxY, maxZ, entropy, this.gameObject.GetComponent<Manager>(),Child);
                    map[(x, y, z)] = tile;
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
            var temp = tile.Value.GetEntropyCount();
            if (tile.Value.GetCollapsed())
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

    public void StartAgain(bool fuck)
    {

        

        if ((Time.realtimeSinceStartup - startTime) > 0.0f)
        {
            //Debug.Log($"{Time.realtimeSinceStartup - startTime}");
            startTime = Time.realtimeSinceStartup;

            if (fuck)
            {
                Debug.Log("respawn");
            }
            else
            {
                Debug.Log("fuck");
            }

            foreach (var tile in map)
            {
                Destroy(tile.Value.Child);
            }

            map = new Dictionary<(int, int, int), Tile>();
            entropy = new List<byte[]>();
            CreateRules();
            SpawnTiles();

        }
    }

    bool start = false;
    bool end = true;

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        { 
            if (start) 
            {
                StartAgain(true);
                end = true;
            }
            start = true;
            startTime = Time.realtimeSinceStartup;
            Debug.Log("started");
        }
        

        if (!start) return;
        if (Time.time - lastUpdateTime >= 0.0000000001f)
        {
            //get all tiles entropy
            //find all with the lowest entropy
            var _list = GetLowestEntropyList();

            //randomly pick one
            if (_list.Count == 0)
            {
                foreach (var tile in map)
                {
                    tile.Value.CollapseEntropy();
                    tile.Value.SetExits();
                }
                if (end) 
                {
                    end = false;
                    Debug.Log($"{(Time.realtimeSinceStartup - startTime)}");
                }
                //Reset(true);
                return;
            }

            int _randNum = Random.Range(0, _list.Count);
            (int x, int y, int z) _tilePos = _list[_randNum];
            var _tile = map[_tilePos];

            //collapse the entropy for that one
            //by picking the one of the possible entropy
            _tile.CollapseEntropy();

            for (int x = 0; x < maxX; x++) 
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        (int x, int y, int z) _targetPos = (x,y,z);

                        _tile = map[_targetPos];
                        _tile.UpdateEntropy();
                    }
                }
            }

            for (int x = 0; x < maxX; x++)
            {
                for (int y = 0; y < maxY; y++)
                {
                    for (int z = 0; z < maxZ; z++)
                    {
                        (int x, int y, int z) _targetPos = (x, y, z);

                        _tile = map[_targetPos];
                        _tile.UpdateEntropy();
                    }
                }
            }

            lastUpdateTime = Time.time;
        }
    }

    public Tile GetTile((int x, int y, int z) _pos) => map[_pos];
}

public class ByteArrayComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[] x, byte[] y)
    {
        if (x == y) return true;
        if (x == null || y == null) return false;
        if (x.Length != y.Length) return false;
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i]) return false;
        }
        return true;
    }

    public int GetHashCode(byte[] obj)
    {
        if (obj == null || obj.Length == 0) return 0;
        int hash = 17;
        foreach (byte element in obj)
        {
            hash = hash * 31 + element.GetHashCode();
        }
        return hash;
    }
}