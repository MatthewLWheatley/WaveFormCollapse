using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

//[System.Serializable]
//public class Tile
//{
//    public Manager Parent { get; private set; }
//    public GameObject Child { get; private set; }
//    public TileManager ChildTile { get; private set; }

//    private int maxX, maxY, maxZ;
//    private bool collapsed = false;
//    private (int x, int y, int z) pos;
//    private HashSet<byte[]> entropy;

//    private byte[] exits = new byte[6];


//    (int x, int y, int z)[] Dirs;

//    public void Initialize((int x, int y, int z) position, int mx, int my, int mz, List<byte[]> ent, Manager parent, GameObject child)
//    {
//        pos = position;
//        maxX = mx; maxY = my; maxZ = mz;
//        Parent = parent;
//        entropy = new HashSet<byte[]>(new ByteArrayComparer());
//        foreach (var en in ent)
//        {
//            entropy.Add(en);
//        }
//        Child = child;
//        ChildTile = Child.GetComponent<TileManager>();
//        Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };
//    }


//    List<Tile> tiles;
//    HashSet<byte[]> toRemove = new HashSet<byte[]>(new ByteArrayComparer());
//    List<byte> possbileExits = new List<byte>();

//    public void InitSurroundings()
//    {
//        tiles = new List<Tile>();
//        toRemove = new HashSet<byte[]>(new ByteArrayComparer());
//        for (int _dir = 0; _dir < 6; _dir++)
//        {
//            var _targetPos = pos;
//            _targetPos.x = (_targetPos.x + Dirs[_dir].x + maxX) % maxX;
//            _targetPos.y = (_targetPos.y + Dirs[_dir].y + maxY) % maxY;
//            _targetPos.z = (_targetPos.z + Dirs[_dir].z + maxZ) % maxZ;
//            tiles.Add(Parent.GetTile(_targetPos));
//        }
//    }

//    public void UpdateEntropy()
//    {
//        // Loop through all directions and check entropy
//        for (int _dir = 0; _dir < 6; _dir++)
//        {
//            var _targetEntropy = tiles[_dir].GetEntropy();
//            toRemove = new HashSet<byte[]>(new ByteArrayComparer());
//            var _correspondingExit = (_dir + 3) % 6;
//            possbileExits = new List<byte>();

//            //find all the possible exits
//            foreach (var _exit in _targetEntropy) if (!possbileExits.Contains(_exit[_correspondingExit])) possbileExits.Add(_exit[_correspondingExit]);

//            //filter through entropy adding to the remove HashSet
//            foreach (var ent in entropy) if (!possbileExits.Contains(ent[_dir])) toRemove.Add(ent);

//            //remove everything in the remove list
//            foreach (var item in toRemove) entropy.Remove(item);
//        }
//    }

//    public void CollapseEntropy()
//    {
//        int entropyCount = entropy.Count;
//        if (entropyCount == 0)
//        {
//            // Handle the case when there's no entropy left
//            Parent.GetComponent<Manager>().StartAgain(false);
//            return;
//        }

//        int _randNum = Random.Range(0, entropyCount);
//        byte[] randomEntropyElement = entropy.ElementAt(_randNum);
//        exits = randomEntropyElement;

//        entropy.Clear();
//        entropy.Add(exits);

//        collapsed = true;
//    }

//    public void SetExits()
//    {
//        ChildTile.SetExits(exits);
//    }

//    public bool GetCollapsed()
//    {
//        return entropy.Count == 1;
//    }

//    public List<byte[]> GetEntropy()
//    {
//        return new List<byte[]>(entropy);
//    }

//    public int GetEntropyCount()
//    {
//        return entropy.Count;
//    }
//}



public class TileManagers : MonoBehaviour
{
    public (int x, int y, int z) max;
    Dictionary<(int x, int y, int z), Tile> mTile;
    Dictionary<(int x, int y, int z), GameObject> mGameObject;

    public List<byte[]> entropy = new List<byte[]>();


    private void Start()
    {
        mTile = new Dictionary<(int x, int y, int z), Tile>();
        mGameObject = new Dictionary<(int x, int y, int z), GameObject>();

        InitRules();
        InitTiles();
    }

    private void InitRules()
    {
        byte[] _r = { 0x00, 0x01 };

        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[1] });
        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0] });
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0] });
        entropy.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1] });
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1] });
        entropy.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1] });
        entropy.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0] });
        entropy.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1] });
    }

    private void InitTiles()
    {
        for (int x = 0; x < max.x; x++)
        {
            for (int y = 0; y < max.x; y++)
            {
                for (int z = 0; z < max.x; z++)
                {
                    Tile TempTile = new Tile();
                    TempTile.Initialize((x, y, z), max, entropy);
                }
            }
        }
    }

    (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };

    public void UpdateEntropy((int x, int y, int z) pos)
    {
        // Loop through all directions and check entropy
        for (int _dir = 0; _dir < 6; _dir++)
        {
            (int x, int y, int z) _targetPosition = ((pos.x + Dirs[_dir].x), (pos.y + Dirs[_dir].y), (pos.z + Dirs[_dir].z));
            List<byte[]> _targetEntropy = mTile[_targetPosition].GetEntropy();
            List<byte[]> toRemove = new List<byte[]>();
            var _correspondingExit = (_dir + 3) % 6;
            List<byte> possbileExits = new List<byte>();

            //find all the possible exits
            foreach (var _exit in _targetEntropy) if (!possbileExits.Contains(_exit[_correspondingExit])) possbileExits.Add(_exit[_correspondingExit]);

            //filter through entropy adding to the remove HashSet
            foreach (var ent in entropy) if (!possbileExits.Contains(ent[_dir])) toRemove.Add(ent);

            //remove everything in the remove list
            foreach (var item in toRemove) entropy.Remove(item);
        }
    }

    public void CollapseEntropy((int x, int y, int z) pos)
    {
        int entropyCount = mTile[pos].GetEntropyCount();
        if (entropyCount == 0) return;

        int _randNum = Random.Range(0, entropyCount);
        byte[] randomEntropyElement = mTile[pos].GetEntropy()[_randNum];
        mTile[pos].entropy = new List<byte[]>();
        mTile[pos].entropy.Add(randomEntropyElement);
    }
}

[System.Serializable]
public class Tile
{
    private (int x, int y, int z) max;
    private (int x, int y, int z) pos;
    public List<byte[]> entropy;

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, List<byte[]> _ent)
    {
        pos = _position;
        max = _max;

        entropy = new List<byte[]>();
        foreach (var en in _ent)
        {
            entropy.Add(en);
        }
    }

    public void SetExits(byte[] Exits)
    {
        entropy = new List<byte[]>();
        entropy.Add(Exits);
    }

    public byte[] GetExits()
    {
        if (entropy.Count == 1)
        {
            return entropy.First();
        }
        return new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
    }

    public bool GetCollapsed()
    {
        return entropy.Count == 1;
    }

    public List<byte[]> GetEntropy()
    {
        return new List<byte[]>(entropy);
    }

    public int GetEntropyCount()
    {
        return entropy.Count;
    }
}