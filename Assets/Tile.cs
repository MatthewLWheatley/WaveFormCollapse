using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public Manager Parent { get; private set; }
    public GameObject Child { get; private set; }
    public TileManager ChildTile { get; private set; }

    private int maxX, maxY, maxZ;
    private bool collapsed = false;
    private (int x, int y, int z) pos;
    private HashSet<byte[]> entropy;

    private byte[] exits = new byte[6];


    (int x, int y, int z)[] Dirs;

    public void Initialize((int x, int y, int z) position, int mx, int my, int mz, List<byte[]> ent, Manager parent, GameObject child)
    {
        pos = position;
        maxX = mx; maxY = my; maxZ = mz;
        Parent = parent;
        entropy = new HashSet<byte[]>(new ByteArrayComparer());
        foreach (var en in ent)
        {
            entropy.Add(en);
        }
        Child = child;
        ChildTile = Child.GetComponent<TileManager>();
        Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };
    }


    List<Tile> tiles;
    HashSet<byte[]> toRemove = new HashSet<byte[]>(new ByteArrayComparer());
    List<byte> possbileExits = new List<byte>();

    public void InitSurroundings()
    { 
        tiles = new List<Tile>();
        toRemove = new HashSet<byte[]>(new ByteArrayComparer());
        for (int _dir = 0; _dir< 6; _dir++)
        {
            // Calculate the target position based on the direction
            var _targetPos = pos;
            _targetPos.x = (_targetPos.x + Dirs[_dir].x + maxX) % maxX;
            _targetPos.y = (_targetPos.y + Dirs[_dir].y + maxY) % maxY;
            _targetPos.z = (_targetPos.z + Dirs[_dir].z + maxZ) % maxZ;
            tiles.Add(Parent.GetTile(_targetPos));
            //Debug.Log($"dir:{_dir}  pos:{pos.x},{pos.y},{pos.z} dir");
            //Debug.Log($"pos: {tiles[_dir].pos.x},{tiles[_dir].pos.y},{tiles[_dir].pos.z}");
            //Debug.Log($"{Parent.GetTile(_targetPos).pos.x}");
        }
    }

    public void UpdateEntropy()
    {
        // Loop through all directions and check entropy
        for (int _dir = 0; _dir < 6; _dir++)
        {
            var _targetEntropy = tiles[_dir].GetEntropy();
            toRemove = new HashSet<byte[]>(new ByteArrayComparer());
            var _correspondingExit = (_dir + 3) % 6;
            possbileExits = new List<byte>();

            //find all the possible exits
            foreach (var _exit in _targetEntropy) if (!possbileExits.Contains(_exit[_correspondingExit])) possbileExits.Add(_exit[_correspondingExit]);

            //filter through entropy adding to the remove HashSet
            foreach (var ent in entropy) if (!possbileExits.Contains(ent[_dir])) toRemove.Add(ent);

            //remove everything in the remove list
            foreach (var item in toRemove) entropy.Remove(item);
        }
    }

    public void CollapseEntropy()
    {
        int entropyCount = entropy.Count;
        if (entropyCount == 0)
        {
            // Handle the case when there's no entropy left
            Parent.GetComponent<Manager>().StartAgain(false);
            return;
        }

        int _randNum = Random.Range(0, entropyCount);
        byte[] randomEntropyElement = entropy.ElementAt(_randNum);
        exits = randomEntropyElement;

        entropy.Clear();
        entropy.Add(exits);

        collapsed = true;
    }

    public void SetExits()
    {
        ChildTile.SetExits(exits);
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


//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

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
//    public (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1) };

//    private byte[] exits = new byte[6];

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
//    }


//    public void UpdateEntropy()
//    {
//        // Loop through all directions and check entropy
//        for (int i = 0; i < Dirs.Length; i++)
//        {
//            var _targetPos = (((pos.x + Dirs[i].x) + maxX) % maxX, ((pos.y + Dirs[i].y) + maxY) % maxY, ((pos.z + Dirs[i].z) + maxZ) % maxZ);

//            // Get the target tile and its entropy
//            var _targetEntropy = Parent.GetTile(_targetPos).GetEntropy();
//            HashSet<byte[]> _toRemove = new HashSet<byte[]>(new ByteArrayComparer());
//            List<byte> _possibleExits = new List<byte>();

//            //creating a list of all possbile exits
//            foreach (var _exit in _targetEntropy) if (!_possibleExits.Contains(_exit[(i + 3) % 6])) _possibleExits.Add(_exit[(i + 3) % 6]);

//            //exit if no removals will happen
//            if (_possibleExits.Count > 1) return;

//            //filter through entropy adding to the remove HashSet
//            foreach (var ent in entropy) if (!_possibleExits.Contains(ent[i])) _toRemove.Add(ent);

//            //remove everything in the remove list
//            foreach (var item in _toRemove) entropy.Remove(item);
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

//        exits = entropy.ElementAt(Random.Range(0, entropyCount));

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
