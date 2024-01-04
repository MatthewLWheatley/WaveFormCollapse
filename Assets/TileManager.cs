using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private int[] tiles = {0,0,0,0,0,0};

    public TileManager(int[] Exits) 
    { 
        
        tiles = Exits;
    }
}
