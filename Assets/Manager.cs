using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    [SerializeField] GameObject tile;

    [SerializeField] Dictionary<(int, int, int), GameObject> map = new Dictionary<(int, int, int), GameObject>();
    [SerializeField] int maxX = 5, maxY = 5, maxZ = 5;

    List<byte[]> entropy = new List<byte[]> ();

    private float lastUpdateTime = 0f; // Keep track of the last update time

    int rndSeed = 1;

    private void Start()
    {
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

        //for (int i = 0; i < 64; i++)
        //{
        //    byte[] _rule = new byte[6];
        //    for (int j = 0; j < 6; j++)
        //    {
        //        // Set the j-th bit of i and assign it to the j-th position in _rule
        //        _rule[j] = (byte)((i >> j) & 1);
        //    }
        //    byte[] dont = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        //    if (_rule == dont) 
        //    {
        //        continue;
        //    }
        //    entropy.Add(_rule);
        //}

        foreach (var tile in map)
        { 
            tile.Value.GetComponent<TileManager>().entropy = entropy;
        }
    }

    private void SpawnTiles()
    {
        for (int x = 0; x < maxX; x++)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int z = 0; z < maxZ; z++)
                {
                    map.Add((x, y, z), null);
                    // Create a new tile only if the position in the map is marked as true
                    Vector3 tilePosition = new Vector3(x*3, y * 3, z * 3);
                    GameObject newTile = Instantiate(tile, tilePosition, Quaternion.identity,this.gameObject.transform);
                    map[(x, y, z)] = newTile;
                    var _tileManager = newTile.GetComponent<TileManager>();
                    _tileManager.exits = new byte[] {0x00,0x00,0x00,0x00,0x00,0x00};
                    _tileManager.pos = (x, y, z);
                    for (int i = 0; i < entropy.Count; i++)
                    {
                        _tileManager.entropy.Add(entropy[i]);
                    }
                    _tileManager.maxX = maxX;
                    _tileManager.maxY = maxY;
                    _tileManager.maxZ = maxZ;
                    _tileManager.Parent = this.gameObject;
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

    private void Update()
    {
        //if (Time.time - lastUpdateTime >= 0.0000001f)
        {
            //get all tiles entropy
            //find all with the lowest entropy
            var _list = GetLowestEntropyList();

            //randomly pick one
            //Random.seed = rndSeed;
            if (_list.Count == 0) return;
            int _randNum = Random.Range(0, _list.Count);
            //Debug.Log(_list.Count);
            (int x, int y, int z) _tilePos = _list[_randNum];
            var _tile = map[_tilePos].GetComponent<TileManager>();

            //collapse the entropy for that one
            //by picking the one of the possible entropy
            _tile.CollapseEntropy();

            //propergate the collapse to the sorunding
            List<(int x, int y, int z)> dirs = new List<(int x, int y, int z)> {
                (1,0,0),(-1,0,0),(0,1,0),(0,-1,0),(0,0,1),(0,0,-1),
                (2,0,0),(-2,0,0),(0,2,0),(0,-2,0),(0,0,2),(0,0,-2),
                (1,0,1),(-1,0,1),(1,0,-1),(-1,0,-1),
                (1,1,0),(-1,1,0),(1,-1,0),(-1,-1,0),
                (0,1,1),(0,-1,1),(0,1,-1),(0,-1,-1)};
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
}
