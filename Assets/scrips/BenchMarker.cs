using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
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



    public TextMeshProUGUI t_TR100;
    public TextMeshProUGUI t_TR80;
    public TextMeshProUGUI t_TotalTime;
    public TextMeshProUGUI t_RunCount;
    public TextMeshProUGUI t_RegionsCollapsed;
    public TextMeshProUGUI t_RegionsRendered;

    public bool running = false;
    public bool rendering = false;
    public bool realTimeRendering = false;

    public int RunCount;
    public float CollapseTime = 0.0f;
    List<Manager> managerScripts = new List<Manager>();
    public List<float> runTimes = new List<float>();

    void Start()
    {
        Application.targetFrameRate = 100000;

        if (!randomSeed) 
        {
            Random.InitState(seed);
        }

        t_RegionsCollapsed.text = "0";
        t_RegionsRendered.text = "0";
        t_RunCount.text = "0";
        t_TR80.text = "0";
        t_TotalTime.text = "0";
        t_TR100.text = "0";
    }

    float startTime = 0.0f;

    private void Update()
    {
        if (!running) return;
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
            t_RegionsCollapsed.text = string.Format(intst.renderedCount.ToString());
            t_RegionsRendered.text = string.Format(intst.collapseCount.ToString());
            
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

            t_RunCount.text = string.Format(RunCount.ToString());
            t_TotalTime.text = string.Format(CollapseTime.ToString());

            t_TR80.text = string.Format(CalculateAverageExcludingOutliers().ToString());
            t_TR100.text = string.Format((CollapseTime/(float)RunCount).ToString());
            DeleteManagers();
            SpawnManagers();
            startTime = Time.time;
        }
    }

    public void PauseManagers()
    {
        foreach (Manager intst in managerScripts)
        {
            intst.Running = !intst.Running;
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
        managerScripts = new List<Manager>();
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
        startTime = Time.time;
        foreach (Manager intst in managerScripts)
        {
            intst.renderOnFinish = rendering;
        }
        foreach (Manager intst in managerScripts)
        {
            intst.realTimeRender = realTimeRendering;
        }
        //PauseManagers();
    }

    public void Run() 
    { 
        DeleteManagers();
        SpawnManagers();
        running = true;
    }

    public void Pause()
    {
       running = !running;
        PauseManagers();
    }

    public void Cancel()
    {
        running = false;
        t_RegionsCollapsed.text = "0";
        t_RegionsRendered.text = "0";
        t_RunCount.text = "0";
        t_TR80.text = "0";
        t_TotalTime.text = "0";
        t_TR100.text = "0";
        DeleteManagers();
        RunCount = 0;
        CollapseTime = 0;
        runTimes = new List<float>();
    }

    public void RenderRealTime() 
    {
        realTimeRendering = !realTimeRendering;
        foreach (Manager intst in managerScripts)
        {
            intst.renderOnFinish = rendering;
        }
        foreach (Manager intst in managerScripts)
        {
            intst.realTimeRender = realTimeRendering;
        }
    }

    public void RenderAtFinish()
    {
        rendering = !rendering;
        foreach (Manager intst in managerScripts)
        {
            intst.renderOnFinish = rendering;
        }
        foreach (Manager intst in managerScripts)
        {
            intst.realTimeRender = realTimeRendering;
        }
    }
}
