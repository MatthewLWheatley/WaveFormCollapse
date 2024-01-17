using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    [SerializeField] GameObject tile;

    [SerializeField] Dictionary<(int, int, int), GameObject> map = new Dictionary<(int, int, int), GameObject>();
    [SerializeField] int maxX = 5, maxY = 5, maxZ = 5;

    [SerializeField] Dictionary<byte[], bool> entropy = new Dictionary<byte[], bool>();

    private float lastUpdateTime = 0f; // Keep track of the last update time

    private void Start()
    {
        CreateRules();
        SpawnTiles();
    }

    private void CreateRules()
    {
        byte[] _r = { 0x00, 0x01 };

        for (int i = 0; i < 64; i++)
        {
            byte[] _rule = new byte[6];
            for (int j = 0; j < 6; j++)
            {
                // Set the j-th bit of i and assign it to the j-th position in _rule
                _rule[j] = (byte)((i >> j) & 1);
            }
            entropy.Add(_rule, true);
        }

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
                    GameObject newTile = Instantiate(tile, tilePosition, Quaternion.identity);
                    map[(x, y, z)] = newTile;
                    var _tileManager = newTile.GetComponent<TileManager>();
                    //Debug.Log($"{x}, {y}, {z}");
                    _tileManager.exits = new byte[] {0x00,0x00,0x00,0x00,0x00,0x00};
                    _tileManager.pos = (x, y, z);
                    _tileManager.entropy = entropy;
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

    int count = 0;

    private void Update()
    {
        if (Time.time - lastUpdateTime >= 5f)
        {
            /*
            //debug code
            foreach (var tileEntry in map)
            {
                GameObject tileObject = tileEntry.Value;
                TileManager tileManager = tileObject.GetComponent<TileManager>();
                tileManager.UpdateEntopy((byte)count);
            }
            count++;
            if (count >= 64) count = 0;
            */

            //get all tiles entropy
            //find all with the lowest entropy
            var _list = GetLowestEntropyList();
            //Debug.Log($"{_list}");

            //randomly pick one
            var _tile = map[_list[Random.Range(0, _list.Count)]];

            //collapse the entropy for that one
            _tile.GetComponent<TileManager>().CollapseEntropy();

            //by picking the one of the possible entropy

            //propergate the collapse to the sorunding

            lastUpdateTime = Time.time;
        }
    }


    public GameObject GetTile((int x,int y,int z) _pos) 
    {
        return map[_pos];
    }
}
