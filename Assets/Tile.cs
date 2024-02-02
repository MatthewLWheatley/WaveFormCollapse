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
    private (int x, int y, int z) max;
    private (int x, int y, int z) pos;
    public List<int> entropy;

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, List<int> _ent)
    {
        pos = _position;
        max = _max;

        entropy = new List<int>();
        foreach (var en in _ent)
        {
            entropy.Add(en);
        }
    }

    public void SetExits(int Exits)
    {
        entropy = new List<int>();
        entropy.Add(Exits);
    }

    public int GetExits()
    {
        if (entropy.Count == 1)
        {
            return entropy.First();
        }
        return 0;
    }

    public bool GetCollapsed()
    {
        return entropy.Count == 1;
    }

    public List<int> GetEntropy()
    {
        return new List<int>(entropy);
    }

    public void SetEntropy(List<int> ent)
    {
        entropy = new List<int>();
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