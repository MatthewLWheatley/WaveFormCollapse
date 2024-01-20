using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject Parent { get; private set; }
    [SerializeField] private GameObject[] cubes = new GameObject[6];
    private byte[] exits = new byte[6];
    [SerializeField] private GameObject centre;

    public void SetExits(byte[] _exits) 
    {
        exits = _exits;
    }

    public void UpdateExits()
    {
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] == 1)
            {
                cubes[i].SetActive(true);
            }
            else
            {
                cubes[i].SetActive(false);
            }
        }
        for (int i = 0; i < exits.Length; i++)
        {
            if (exits[i] > 0) centre.SetActive(true);
        }
    }

    public void ResetExits()
    {
        Destroy(this);
    }
}
