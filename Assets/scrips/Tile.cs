using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Tile
{
    //HashSet is 0.089 5*5 5*5
    //hashset is 0.083 5*5 5*5

    private (int x, int y, int z) max;
    public (int x, int y, int z) pos;
    public HashSet<int> entropy;

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, HashSet<int> _ent)
    {
        pos = _position;
        max = _max;


        entropy = new HashSet<int>();
        foreach (var en in _ent)
        {
            entropy.Add(en);
        }
    }

    public void SetExits(int Exits)
    {
        entropy = new HashSet<int>();
        entropy.Add(Exits);
    }

    public int GetExits()
    {
        if (entropy.Count == 1)
        {
            return entropy.First();
        }
        return -1;
    }

    public bool GetCollapsed()
    {
        return entropy.Count == 1;
    }

    public HashSet<int> GetEntropy()
    {
        return new HashSet<int>(entropy);
    }

    public void SetEntropy(HashSet<int> ent)
    {
        entropy = new HashSet<int>();
        foreach (var en in ent) 
        { 
            entropy.Add(en);
        }
    }

    public int GetEntropyCount()
    {
        return entropy.Count;
    }
}