using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Region
{
    private Dictionary<(int x, int y, int z), Tile> Tiles;
    private HashSet<(int x, int y, int z)> NotCollapsesed;

    (int x, int y, int z) max;
    (int x, int y, int z) min;
    (int x, int y, int z) pos;


    public void Initialize((int x, int y, int z) _pos, (int x, int y, int z) _max, (int x, int y, int z) _min, HashSet<int> _ent)
    {
        pos = _pos;
        max = _max;
        min = _min;

        Tiles = new Dictionary<(int x, int y, int z), Tile> ();
        NotCollapsesed = new HashSet<(int x, int y, int z)> ();

        SpawnTiles(_ent);
    }

    public void SpawnTiles(HashSet<int> _ent)
    {
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                for (int z = min.z; z < max.z; z++)
                {
                    Tile TempTile = new Tile();
                    TempTile.Initialize((x, y, z), _ent);
                    Tiles.Add((x, y, z), TempTile);
                    NotCollapsesed.Add((x, y, z));
                }
            }
        }
    }

    public Dictionary<(int x, int y, int z), Tile> GetTiles() 
    {
        return Tiles;
    }
}