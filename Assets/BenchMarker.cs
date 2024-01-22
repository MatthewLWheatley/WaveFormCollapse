using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BenchMarker : MonoBehaviour
{
    public GameObject managerPrefab;
    public int maxX;
    public int maxY;
    public int maxZ;

    public int width;
    public int depth;

    void Start()
    {
        SpawnManagers();
    }

    void SpawnManagers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < depth; z++)
            {
                // Calculate the position for each manager
                Vector3 position = new Vector3(x * maxX * 3, 0, z * maxZ * 3);

                // Instantiate the manager prefab
                GameObject managerInstance = Instantiate(managerPrefab, position, Quaternion.identity);

                // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
                Manager managerScript = managerInstance.GetComponent<Manager>();
                if (managerScript != null)
                {
                    managerScript.maxX = maxX;
                    managerScript.maxY = maxY;
                    managerScript.maxZ = maxZ;
                }
            }
        }
    }
}
