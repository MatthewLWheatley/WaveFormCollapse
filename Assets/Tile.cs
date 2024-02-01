using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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

    public void SetEntropy(List<byte[]> ent)
    {
        entropy = new List<byte[]>();
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