using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public int MaxRunCount = 100;

    public int RunCount;
    public float CollapseTime = 0.0f;
    public TextMeshProUGUI CollapsedTime;
    public TextMeshProUGUI RunText;
    public TextMeshProUGUI RunningTotal;
    public TextMeshProUGUI colapseTotal;
    public TextMeshProUGUI renderTotal;

    List<Manager> managerScripts = new List<Manager>();

    public List<float> runTimes = new List<float>();

    void Start()
    {
        Application.targetFrameRate = 100000;

        if (!randomSeed) 
        {
            Random.InitState(seed);
        }
    }

    float startTime = 0.0f;

    private void Update()
    {
        if (RunCount >= MaxRunCount) return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DeleteManagers();
            if (randomSeed) seed++;
            Random.InitState(seed);

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

            runTimes.Add((Time.time - startTime));

            RunText.text = string.Format(RunCount.ToString());
            RunningTotal.text = string.Format(CollapseTime.ToString());

            CollapsedTime.text = string.Format(CalculateAverageExcludingOutliers().ToString());
            DeleteManagers();
            SpawnManagers();
            startTime = Time.time;
        }
    }

    float CalculateAverageExcludingOutliers()
    {
        if (runTimes.Count == 0) return 0f;
        // Directly return the average if the list is too small for outlier removal to make sense
        if (runTimes.Count <= 10) return CollapseTime / (float)RunCount;

        // Calculate number of elements to remove from each end
        int countToRemove = (int)(runTimes.Count * 0.1f); // 5% from each end, total 10%

        // Ensure we have a sorted copy of the run times
        List<float> sortedTimes = new List<float>(runTimes);
        sortedTimes.Sort();

        // Adjust for edge cases where the list size might be too small after removal
        if (2 * countToRemove > sortedTimes.Count) return CollapseTime / (float)RunCount;

        // Remove outliers

        sortedTimes.RemoveRange(sortedTimes.Count - countToRemove, countToRemove);
        sortedTimes.RemoveRange(0, countToRemove); 
                                                   

        // Calculate the average of the remaining times
        float total = 0;
        foreach (float time in sortedTimes)
        {
            total += time;
        }
        Debug.Log($"{countToRemove}, {total}, {sortedTimes.Count}");
        return total / sortedTimes.Count; // Return the average of the adjusted list
    }

    void DeleteManagers()
    {
        foreach (Transform child in transform)
        {
            DestroyChildAndMesh(child);
        }
        System.GC.Collect();
    }

    void DestroyChildAndMesh(Transform parent)
    {
        foreach (Transform child in parent)
        {
            DestroyChildAndMesh(child); // Recursively destroy children
        }

        MeshFilter meshFilter = parent.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.mesh != null)
        {
            DestroyImmediate(meshFilter.mesh); // Destroy the mesh immediately
            meshFilter.mesh = null; // Clear the reference
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
