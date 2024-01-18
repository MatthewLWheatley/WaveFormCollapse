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
    public List<byte[]> entropy;

    private void Update()
    {
        //Debug.Log($"{exits.Length}");
        //turn off when debugging but make the centre not active when tile is empty
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
        //Debug.Log($"{_targetPos.x},{_targetPos.y},{_targetPos.z}");

        // Loop through all directions and check entropy
        for (int dir = 0; dir < 6; dir++)
        {
            UpdateEntropyDir(dir);
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
        //Debug.Log($"pos: {pos.x},{pos.y},{pos.z}");
        //Debug.Log($"target pos: {_targetPos.x},{_targetPos.y},{_targetPos.z}");

        // Get the target tile and its entropy
        var _targetTile = Parent.GetComponent<Manager>().GetTile(_targetPos).GetComponent<TileManager>();
        var _targetEntropy = _targetTile.GetEntropy();

        Debug.Log($"shit {_targetEntropy.Count}");

        // Check if the target tile's entropy configuration is compatible with this tile's entropy
        foreach (var entry in _targetEntropy)
        {
            List<byte[]> _toRemove = new List<byte[]>();
            var _correspondingExit = (_dir + 3) % 6;
            for(int i = 0; i < entropy.Count; i++) 
            {
                if (entropy[i][_dir] != entry[_correspondingExit]) 
                {
                    _toRemove.Add(entropy[i]);
                }             
            }

            for (int i = 0; i < _toRemove.Count; i++) 
            {
                entropy.Remove(_toRemove[i]);
            }
        }

        // Optional: Log the updated entropy count
        //Debug.Log($"Updated Entropy Count: {this.GetEntropyCount()}");
    }

    public void CollapseEntropy()
    {
        int _randNum = Random.Range(0, entropy.Count);
        Debug.Log($"CollapseEntropy rand number: {_randNum}  entropy count: {entropy.Count}");
        exits = entropy[_randNum];
        entropy = new List<byte[]>();
        entropy.Add(exits);
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
            // Assuming the value 1 in exits array means the exit is open
            // and the corresponding cube should be active.
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
