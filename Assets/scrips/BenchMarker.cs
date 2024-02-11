using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class BenchMarker : MonoBehaviour
{
    public int seed;
    public bool randomSeed = false;

    public GameObject managerPrefab;
    public int maxX;
    public int maxY;
    public int maxZ;
    public int RegionX;
    public int RegionY;
    public int RegionZ;
    public int width;
    public int depth;

    public int RunCount;
    public float CollapseTime = 0.0f;
    public TextMeshProUGUI CollapsedTime;
    public TextMeshProUGUI RunText;
    public TextMeshProUGUI RunningTotal;
    public TextMeshProUGUI colapseTotal;
    public TextMeshProUGUI renderTotal;

    List<Manager> managerScripts = new List<Manager>();

    System.Random rnd;

    void Start()
    {
        Application.targetFrameRate = 100000;
        System.Random rnd = new System.Random(seed);
        if (randomSeed) 
        { 
            seed = rnd.Next();
        }
    }

    float startTime = 0.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DeleteManagers(); 
            rnd = new System.Random(seed);
            seed = randomSeed ? seed = rnd.Next() : seed;
            
            SpawnManagers();

        }
        List<Manager> list = new List<Manager>();
        foreach (Manager intst in managerScripts)
        {
            colapseTotal.text = string.Format(intst.renderedCount.ToString());
            renderTotal.text = string.Format(intst.collapseCount.ToString());
            
            if (intst.collapsed && intst.rendered)
            {

                RunCount++;
                
                list.Add(intst);
                //Debug.Log(RunCount);
            }
        }
        foreach (Manager intst in list)
        {
            managerScripts.Remove(intst);
        }
        if (managerScripts.Count == 0)
        {
            
            CollapseTime += (Time.time - startTime);
            startTime = Time.time;

            RunText.text = string.Format(RunCount.ToString());
            RunningTotal.text = string.Format(CollapseTime.ToString());
            CollapsedTime.text = string.Format($"{CollapseTime / RunCount}");
            DeleteManagers();
            SpawnManagers();
        }
    }

    void DeleteManagers()
    {
        foreach (Transform child in transform)
        {
            DestroyChildAndMesh(child);
        }
    }

    void DestroyChildAndMesh(Transform parent)
    {
        foreach (Transform child in parent)
        {
            DestroyChildAndMesh(child); // Recursively destroy children
        }

        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            Destroy(meshFilter.mesh); // Destroy the mesh
        }

        Destroy(parent.gameObject); // Destroy the child GameObject
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
                GameObject managerInstance = Instantiate(managerPrefab, position, Quaternion.identity, this.transform);

                // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
                Manager managerScript = managerInstance.GetComponent<Manager>();
                managerScripts.Add(managerScript);
                if (managerScript != null)
                {
                    managerScript.max = (maxX, maxY, maxZ);
                    managerScript.regionSize = (RegionX, RegionY, RegionZ);
                    managerScript.seed = seed;
                }
            }
        }
    }
}
