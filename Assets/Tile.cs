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
    public HashSet<byte[]> entropy;

    public void Initialize((int x, int y, int z) _position, (int x, int y, int z) _max, HashSet<byte[]> _ent)
    {
        pos = _position;
        max = _max;

        entropy = new HashSet<byte[]>();
        foreach (var en in _ent)
        {
            entropy.Add(en);
        }
    }

    public void SetExits(byte[] Exits)
    {
        entropy = new HashSet<byte[]>();
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

    public HashSet<byte[]> GetEntropy()
    {
        return new HashSet<byte[]>(entropy);
    }

    public void SetEntropy(HashSet<byte[]> ent)
    {
        entropy = new HashSet<byte[]>();
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