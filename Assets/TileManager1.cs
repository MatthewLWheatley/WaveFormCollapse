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
    public Dictionary<byte[], bool> entropy;

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
    public Dictionary<byte, bool> GetEntropy()
    {
        return new Dictionary<byte, bool>(); // entropy.Where(rule => rule.Value == true).ToDictionary(rule => rule.Key, rule => rule.Value);
    }

    public int GetEntropyCount()
    {
        return entropy.Where(rule => rule.Value == true).ToDictionary(rule => rule.Key, rule => rule.Value).Count;
    }

    public void UpdateEntropy(byte[] _exits)
    {
        exits = _exits;
    }

    public void CollapseEntropy()
    {
        var _targetPos = pos;
        Debug.Log($"{_targetPos.x},{_targetPos.y},{_targetPos.z}");

        // Loop through all directions and check entropy
        for (int dir = 0; dir < 6; dir++)
        {
            CheckEntropy(dir);
        }
    }

    public void CheckEntropy(int _dir) 
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

        // Check if the target tile's entropy configuration is compatible with this tile's entropy
        foreach (var entry in _targetEntropy)
        {
            byte targetExit = entry.Key;
            bool isPossible = entry.Value;

            // Check the corresponding exit in the current tile
            // Assuming the exits in the current tile and the target tile are aligned (e.g., +x in current is -x in target)
            int correspondingExit = (_dir + 3) % 6; // Opposite direction

            // If the target tile's exit is possible, the corresponding exit in this tile must be possible too
            if (isPossible && exits[correspondingExit] != targetExit)
            {
                // Mark this tile's entropy as false for the corresponding exit configuration
                byte[] currentEntropyKey = exits.Clone() as byte[];
                currentEntropyKey[correspondingExit] = targetExit;
                if (entropy.ContainsKey(currentEntropyKey))
                {
                    entropy[currentEntropyKey] = false;
                }
            }
        }

        // Optional: Log the updated entropy count
        Debug.Log($"Updated Entropy Count: {this.GetEntropyCount()}");
    }

    public void UpdateCubesBasedOnExits()
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
