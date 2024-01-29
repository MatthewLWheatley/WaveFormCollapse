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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DeleteManagers();
            SpawnManagers();
        }
    }

    void DeleteManagers()
    {
        // Assuming this function is part of a MonoBehaviour and you want to destroy all children of this GameObject
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
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
                GameObject managerInstance = Instantiate(managerPrefab, position, Quaternion.identity,this.transform);

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
