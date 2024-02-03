using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
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

    public int RunCount;
    public float CollapseTime = 0.0f;
    public TextMeshProUGUI CollapsedTime;
    public TextMeshProUGUI RunText;
    public TextMeshProUGUI RunningTotal;

    List<Manager> managerScripts = new List<Manager>();

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            DeleteManagers();
            SpawnManagers();
        }
        List<Manager> list = new List<Manager>();
        foreach (Manager intst in managerScripts)
        {
            if (intst.collapsed && intst.rendered)
            {
                CollapseTime += intst.FinishCollapseTime - intst.StartTime;
                CollapsedTime.text = string.Format($"{CollapseTime / RunCount}");

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
            DeleteManagers();
            SpawnManagers();
        }

        RunText.text = string.Format(RunCount.ToString());
        RunningTotal.text = string.Format(CollapseTime.ToString());
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
                GameObject managerInstance = Instantiate(managerPrefab, position, Quaternion.identity, this.transform);

                // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
                Manager managerScript = managerInstance.GetComponent<Manager>();
                managerScripts.Add(managerScript);
                if (managerScript != null)
                {
                    managerScript.max = (maxX, maxY, maxZ);
                }
            }
        }
    }
}
