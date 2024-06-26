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

    /// <summary>
    /// 0 = normal/region
    /// 1 = stiched
    /// 3 = Repeated WFC
    /// </summary>
    public int managerVersion = 0;
    public GameObject[] managerPrefabs;

    public int maxX;
    public int maxY;
    public int maxZ;
    public int RegionX;
    public int RegionY;
    public int RegionZ;
    public int StitchSize;
    public int width;
    public int depth;

    public int MaxRunCount = 100;

    public int complexity = 0;

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

    public Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();

    //void InitRules()
    //{
    //    byte[] _r = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
    //    List<byte[]> ent = new List<byte[]>();
    //    //entropy.Add(-1, new byte[] { _r[0], _r[0], _r[0], _r[0], _r[0], _r[0] });

    //    bool threeD = false;
    //    if (maxX > 1 && maxY > 1 && maxZ > 1)
    //    {
    //        threeD = true;
    //    }
    //    if (complexity >= 2)
    //    {
    //        ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[1] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[1] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1] });
    //        if (threeD)
    //        {
    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[0], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[1], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[0], _r[0], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[0], _r[0], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[0], _r[1], _r[0], _r[1] });

    //            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[1], _r[1] });

    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[0], _r[1], _r[1] });
    //            ent.Add(new byte[] { _r[0], _r[1], _r[0], _r[1], _r[1], _r[1] });
    //        }
    //    }
    //    if (complexity >= 3)
    //    {
    //        ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[2] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[2] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[2] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[2], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[0], _r[0], _r[2] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[0], _r[0], _r[2] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[2], _r[0], _r[2] });
    //        if (threeD)
    //        {
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[0], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[0], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[0], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[2], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[0], _r[0], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[0], _r[0], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[0], _r[2], _r[0], _r[2] });

    //            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[2], _r[2], _r[2] });

    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[2], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[0], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[2], _r[2] });
    //            ent.Add(new byte[] { _r[0], _r[2], _r[0], _r[2], _r[2], _r[2] });
    //        }
    //    }
    //    if (complexity >= 4)
    //    {
    //        ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[0], _r[3] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[0], _r[3] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[0], _r[3] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[3], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[0], _r[0], _r[3] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[0], _r[0], _r[3] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[3], _r[0], _r[3] });

    //        if (threeD)
    //        {
    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[0], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[0], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[0], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[3], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[0], _r[0], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[0], _r[0], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[0], _r[3], _r[0], _r[3] });

    //            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[3], _r[3], _r[3] });

    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[3], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[0], _r[3], _r[3] });
    //            ent.Add(new byte[] { _r[0], _r[3], _r[0], _r[3], _r[3], _r[3] });
    //        }
    //    }
    //    if (complexity >= 5)
    //    {
    //        ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[0], _r[4] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[0], _r[4] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[0], _r[4] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[4], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[0], _r[0], _r[4] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[0], _r[0] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[0], _r[0], _r[4] });
    //        ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[4], _r[0], _r[4] });

    //        if (threeD)
    //        {
    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[0], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[0], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[0], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[4], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[0], _r[0], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[0], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[0], _r[0], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[0], _r[4], _r[0], _r[4] });

    //            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[4], _r[4], _r[4] });

    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[4], _r[0] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[0], _r[4], _r[4] });
    //            ent.Add(new byte[] { _r[0], _r[4], _r[0], _r[4], _r[4], _r[4] });
    //        }
    //    }
    //    for (int i = 0; i < ent.Count; i++)
    //    {
    //        entropy.Add(i, ent[i]);
    //    }
    //}

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

    public void ChangeWFCVerison(string type) 
    {
        switch (type) 
        {
            case "WFC":
                managerVersion = 0;
                break;
            case "N-WFC":
                managerVersion = 1;
                break;
            case "S-WFC":
                managerVersion = 2;
                break;
            case "R-WFC":
                managerVersion = 4;
                break;
        }
    }

    private void Update()
    {

        if (!running) return;
        if (RunCount >= MaxRunCount) return;
        List<Manager> list = new List<Manager>();
        foreach (Manager intst in managerScripts)
        {
            t_RegionsCollapsed.text = string.Format(intst.renderedCount.ToString());
            t_RegionsRendered.text = string.Format(intst.collapseCount.ToString());
            
            //if (intst.collapsed && intst.rendered)
            //{

            //    RunCount++;
                
            //    list.Add(intst);
            //    //Debug.Log(RunCount);
            //}
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
        //Debug.Log($"{countToRemove}, {total}, {sortedTimes.Count}");
        return total / sortedTimes.Count; // Return the average of the adjusted list
    }

    void DeleteManagers()
    {
        foreach (Transform child in transform)
        {
            DestroyChildAndMesh(child);
        }
        managerScripts = new List<Manager>();
            //System.GC.Collect();
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
                GameObject managerInstance = Instantiate(managerPrefabs[managerVersion], position, Quaternion.identity, this.transform);

                // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
                Manager managerScript = managerInstance.GetComponent<Manager>();
                managerScripts.Add(managerScript);
                if (managerScript != null)
                {
                    managerScript.max = (maxX, maxY, maxZ);
                    managerScript.regionSize = (RegionX, RegionY, RegionZ);
                    managerScript.seed = seed;
                    managerScript.complexity = complexity;
                    //managerScript.entropy = entropy;
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
