using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public GameObject Parent = null;

    public int maxX, maxY, maxZ;

    private bool collapsed = false;

    [SerializeField] GameObject centre;

    [SerializeField] GameObject[] cubes = new GameObject[6];
    /// <summary>
    /// 0 : +x
    /// 1 : +y
    /// 2 : +z
    /// 3 : -x
    /// 4 :  -y
    /// 5 : -z
    /// </summary>
    public byte[] exits = new byte[6];

    public (int x, int y, int z) pos;
    public List<byte[]> entropy = new List<byte[]>();

    private void Update()
    {
        if (exits[0] + exits[1] + exits[2] + exits[3] + exits[4] + exits[5] == 0)
        {
            centre.SetActive(false);
            //Debug.Log("fuck");
        }
        else
        {
            centre.SetActive(true);
            //Debug.Log("shit");
        }
    }

    public bool GetCollapsed()
    {
        return (collapsed);
    }

    //TODO: Fix
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
        var _targetTile = Parent.GetComponent<Manager>().GetTile(_targetPos).GetComponent<TileManager>();
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
        exits = entropy[_randNum];
        entropy = new List<byte[]>();
        entropy.Add(exits);
        UpdateExits();
        collapsed = true;
    }


    public void UpdateExits()
    {
        if (cubes.Length != exits.Length)
        {
            Debug.LogError("Cubes array and exits array do not match in length.");
            return;
        }

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
    }
}
