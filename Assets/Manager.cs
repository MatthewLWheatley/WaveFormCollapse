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
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int maxX = 5, maxY = 5, maxZ = 5;

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

    int count = 0;

    public void Reset(bool fuck)
    {
        if ((Time.realtimeSinceStartup - startTime) > 5.0f)
        {
            //Debug.Log($"{Time.realtimeSinceStartup - startTime}");
            startTime = Time.realtimeSinceStartup;

            if (fuck)
            {
                Debug.Log("respawn");
                count++;
            }
            else 
            {
                Debug.Log("fuck");
            }

            if (count < 50)
            {
                foreach (var tile in map)
                {
                    tile.Value.Delete();
                }
                map = new Dictionary<(int, int, int), Tile>();
                entropy = new List<byte[]>();
                CreateRules();
                SpawnTiles();
            }
        }
    }

    private void Update()
    {
        

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
                    tile.Value.UpdateExits();
                }
                //Debug.Log($"{}");

                Reset(true);
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

    public class Tile
    {
        public Manager Parent { get; private set; }
        public GameObject Child { get; private set; }
        public TileManager ChildTile { get; private set; }

        private int maxX, maxY, maxZ;
        private bool collapsed = false;
        private (int x, int y, int z) pos;
        private List<byte[]> entropy = new List<byte[]>();

        private byte[] exits = new byte[6];

        public void Initialize((int x, int y, int z) position, int mx, int my, int mz, List<byte[]> ent, Manager parent, GameObject child)
        {
            pos = position;
            maxX = mx; maxY = my; maxZ = mz;
            Parent = parent;
            entropy = new List<byte[]>(ent); // Deep copy if necessary
            Child = child;
            ChildTile = Child.GetComponent<TileManager>();
        }

        public bool GetCollapsed()
        {
            return entropy.Count() == 1;
        }

        public List<byte[]> GetEntropy()
        {
            return entropy;
        }

        public int GetEntropyCount()
        {
            return entropy.Count;
        }

        public void UpdateEntropy()
        {
            var _targetPos = pos;

            // Loop through all directions and check entropy
            for (int _dir = 0; _dir < 6; _dir++)
            {
                UpdateEntropyDir(_dir);
            }
        }

        public void UpdateEntropyDir(int _dir)
        {
            // Calculate the target position based on the direction
            var _targetPos = pos;
            switch (_dir)
            {
                case 0: // +x
                    _targetPos.x = (_targetPos.x + 1) % maxX;
                    break;
                case 3: // -x
                    _targetPos.x = (_targetPos.x - 1 + maxX) % maxX;
                    break;
                case 1: // +y
                    _targetPos.y = (_targetPos.y + 1) % maxY;
                    break;
                case 4: // -y
                    _targetPos.y = (_targetPos.y - 1 + maxY) % maxY;
                    break;
                case 2: // +z
                    _targetPos.z = (_targetPos.z + 1) % maxZ;
                    break;
                case 5: // -z
                    _targetPos.z = (_targetPos.z - 1 + maxZ) % maxZ;
                    break;
            }

            // Get the target tile and its entropy
            var _targetTile = Parent.GetTile(_targetPos);
            var _targetEntropy = _targetTile.GetEntropy();
            List<byte[]> _toRemove = new List<byte[]>();
            var _correspondingExit = (_dir + 3) % 6;
            List<byte> _possibleExits = new List<byte>();

            //find all the possible exits
            foreach (var _exit in _targetEntropy)
            {
                if (!_possibleExits.Contains(_exit[_correspondingExit]))
                {
                    _possibleExits.Add(_exit[_correspondingExit]);
                }
            }

            //exit if no removals will happen
            if (_possibleExits.Count > 1)
            {
                return;
            }

            //filter throgh entropy adding to the a remove list 
            for (int i = 0; i < entropy.Count; i++)
            {
                if (!_possibleExits.Contains(entropy[i][_dir]))
                {
                    _toRemove.Add(entropy[i]);
                }
            }

            //remove everything in the remove list
            for (int i = 0; i < _toRemove.Count; i++)
            {
                entropy.Remove(_toRemove[i]);
            }
        }

        public void CollapseEntropy()
        {
            int _randNum = Random.Range(0, entropy.Count);
            if (entropy.Count == 0)
            {
                Parent.GetComponent<Manager>().Reset(false);
                return;
            }
            //Debug.Log($"{entropy.Count} {_randNum}");
            exits = entropy[_randNum];
            entropy = new List<byte[]>();
            entropy.Add(exits);
            collapsed = true;
        }

        public void UpdateExits() 
        {
            ChildTile.SetExits(exits);
            //Debug.Log($"{exits.Count()}");
            ChildTile.UpdateExits();
        }

        public void Delete()
        {
            Destroy(Child);
        }
    }
}
