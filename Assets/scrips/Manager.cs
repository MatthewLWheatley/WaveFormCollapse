using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    public Dictionary<(int x, int y, int z), GameObject> GamobjectMap = new Dictionary<(int x, int y, int z), GameObject>();

    [SerializeField] private GameObject tilePrefab; // Prefab for individual tiles, used in rendering.

    public int seed;
    public (int x, int y, int z) max;
    public (int x, int y, int z) regionSize;


    public Dictionary<(int x, int y, int z), NestedRegion> mRegion;
    public Dictionary<(int x, int y, int z), TileProps> mGameObject;
    public List<(int x, int y, int z)> mNotCollapsesed;
    [SerializeField] protected GameObject RegionPrefab;

    public TextMeshProUGUI RunningTotal;

    public Dictionary<int, byte[]> entropy = new Dictionary<int, byte[]>();
    public bool collapsed = false;
    public bool rendered = false;

    public int complexity = 0;

    public (int x, int y, int z)[] Dirs = new (int x, int y, int z)[] { (1, 0, 0), (0, 1, 0), (0, 0, 1), (-1, 0, 0), (0, -1, 0), (0, 0, -1)};

    public List<(int x, int y, int z)> mList;

    protected (int x, int y, int z) maxRegion;

    protected float StartTime; 
    public float FinishCollapseTime;

    public int mCollapseCount = 0;

    public bool realTimeRender = false;
    public bool renderOnFinish = false;

    public bool Running = true;

    private void Start()
    {
        mGameObject = new Dictionary<(int x, int y, int z), TileProps>();
        mNotCollapsesed = new List<(int x, int y, int z)>();
        mList = new List<(int x, int y, int z)>();
        mRegion = new Dictionary<(int x, int y, int z), NestedRegion> ();

        mStack = new Stack<(int x, int y, int z)>();

        StartTime = Time.time;
        time = Time.time;


        InitRules();
        InitRegions();
    }

    public int collapseCount = 0;
    public int renderedCount = 0;
    protected (int x, int y, int z) targetRegion = (0, 0, 0);

    public float time;

    public int failCount = 0;

    Stack<(int x, int y, int z)> mStack;

    private void Update()
    {
        //Debug.Log("running");
        mCollapseCount = mNotCollapsesed.Count;

        time = Time.time;

        if (renderedCount >= mRegion.Count) rendered = true;

        if (!collapsed)
        {
            var r = mRegion[targetRegion];
            r.running = true;
            r.RunUpdate();
            if (realTimeRender) r.RunRenderer();
            if (r.resetCount > 10)
            {
                r.resetCount = 0;
                failCount++;
                for (int i = 0; i < failCount; i++)
                {
                    if (mStack.Count > 0)
                    {
                        foreach (var tile in mNotCollapsesed)
                        {
                            ResetRegion(tile, 0);
                            mRegion[tile].ResetRegionState();
                        }
                        //Debug.Log("pop");
                        var temp5 = mStack.Pop();
                        mNotCollapsesed.Add(temp5);
                        targetRegion = temp5;
                        ResetRegion(targetRegion, 0);
                        if (realTimeRender) r.RunRenderer();
                        foreach (var tile in mNotCollapsesed)
                        {
                            mRegion[tile].UpdateAllEntropy();
                        }
                    }
                    else
                    {
                        Debug.Log("stack empty");
                        Debug.Log($"{mNotCollapsesed.Count}");
                        foreach (var tile in mNotCollapsesed)
                        {
                            Debug.Log($"{tile.x},{tile.y},{tile.z}");
                            ResetRegion(tile, 0);
                        }
                        failCount = 0;
                    }
                }
            }
            if (r.collapsed)
            {
                //r.RunRenderer();
                //r.CombineMeshes();
                mStack.Push(targetRegion);
                r.running = false;
                //if(renderOnFinish)r.RunRenderer();
                collapseCount++;
                mNotCollapsesed.Remove(targetRegion);
                foreach (var tile in mNotCollapsesed)
                {
                    //ResetRegion(tile, 0);
                    mRegion[tile].ResetRegionState();
                }
                foreach (var tile in mNotCollapsesed)
                {
                    mRegion[tile].UpdateAllEntropy();
                }
                    UpdateRegionEntropyList(true);

            }
            RunningTotal.text = string.Format(collapseCount.ToString());
        }
        else if (!rendered)
        {
            var r = mRegion[mRegion.ElementAt(renderedCount).Key];
            if (renderOnFinish) r.RunRenderer();
            if (r.rendered)
            {
                renderedCount++;
            }
            r.rendered = true;
            RunningTotal.text = string.Format(renderedCount.ToString());
        }
       
    }

    public void ResetRegions((int x, int y, int z) failedRegion)
    {
        var surroundingOffsets = new (int x, int y, int z)[] {
        (1, 0, 0), (-1, 0, 0), // X-axis neighbors
        (0, 1, 0), (0, -1, 0), // Y-axis neighbors
        (0, 0, 1), (0, 0, -1)  // Z-axis neighbors
            };

        List<(int x, int y, int z)> regionsToReset = new List<(int x, int y, int z)>();

        // Add the failed region itself to the reset list
        regionsToReset.Add(failedRegion);

        // Identify and add surrounding regions to the reset list
        foreach (var offset in surroundingOffsets)
        {
            var neighbor = (failedRegion.x + offset.x, failedRegion.y + offset.y, failedRegion.z + offset.z);
            if (mRegion.ContainsKey(neighbor))
            {
                regionsToReset.Add(neighbor);
            }
        }

        // Reset identified regions
        foreach (var region in regionsToReset)
        {
            if (mRegion.ContainsKey(region))
            { 
                mRegion[region].ResetRegionState();
                mNotCollapsesed.Add(region); 
            }
        }
    }

    protected void InitRules()
    {
        byte[] _r = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
        List<byte[]> ent = new List<byte[]>();
        //entropy.Add(-1, new byte[] { _r[0], _r[0], _r[0], _r[0], _r[0], _r[0] });

        bool threeD = false;
        if (max.x > 1 && max.y > 1 && max.z > 1)
        {
            threeD = true;
        }
        if (complexity >= 2)
        {
            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[1] });
            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[1] });
            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[1] });
            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[0], _r[0] });
            ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[0], _r[0] });
            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[0], _r[0] });
            ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[0], _r[1] });
            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[0], _r[0] });
            ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[0], _r[1] });
            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[0], _r[1] });
            if (threeD)
            {
                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[0], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[1], _r[0], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[0], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[0], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[0], _r[0], _r[1] });
                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[0], _r[0] });
                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[0], _r[0], _r[1] });
                ent.Add(new byte[] { _r[0], _r[1], _r[0], _r[1], _r[0], _r[1] });

                ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[0], _r[1], _r[0], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[0], _r[0], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[0], _r[0], _r[1], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[1], _r[1], _r[1] });

                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[1], _r[0], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[1], _r[1], _r[0], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[1], _r[1], _r[0] });
                ent.Add(new byte[] { _r[0], _r[1], _r[1], _r[0], _r[1], _r[1] });
                ent.Add(new byte[] { _r[0], _r[1], _r[0], _r[1], _r[1], _r[1] });
            }
        }
        if (complexity >= 3)
        {
            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[2] });
            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[2] });
            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[2] });
            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[2], _r[0], _r[0] });
            ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[0], _r[0] });
            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[0], _r[0] });
            ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[0], _r[0], _r[2] });
            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[0], _r[0] });
            ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[0], _r[0], _r[2] });
            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[2], _r[0], _r[2] });
            if (threeD)
            {
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[0], _r[2] });
                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[0], _r[2] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[0], _r[2] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[2], _r[0], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[0], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[0], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[0], _r[0], _r[2] });
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[0], _r[0] });
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[0], _r[0], _r[2] });
                ent.Add(new byte[] { _r[0], _r[2], _r[0], _r[2], _r[0], _r[2] });

                ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[2], _r[2] });
                ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[2], _r[2] });
                ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[0], _r[2], _r[0], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[0], _r[0], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[0], _r[0], _r[2], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[2], _r[2], _r[2] });

                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[2], _r[2] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[2], _r[0], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[2], _r[2], _r[0], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[2], _r[0] });
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[0], _r[2], _r[2] });
                ent.Add(new byte[] { _r[0], _r[2], _r[2], _r[2], _r[2], _r[2] });
                ent.Add(new byte[] { _r[0], _r[2], _r[0], _r[2], _r[2], _r[2] });
            }
        }
        if (complexity >= 4)
        {
            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[0], _r[3] });
            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[0], _r[3] });
            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[0], _r[3] });
            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[3], _r[0], _r[0] });
            ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[0], _r[0] });
            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[0], _r[0] });
            ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[0], _r[0], _r[3] });
            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[0], _r[0] });
            ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[0], _r[0], _r[3] });
            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[3], _r[0], _r[3] });

            if (threeD)
            {
                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[0], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[0], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[0], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[3], _r[0], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[0], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[0], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[0], _r[0], _r[3] });
                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[0], _r[0] });
                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[0], _r[0], _r[3] });
                ent.Add(new byte[] { _r[0], _r[3], _r[0], _r[3], _r[0], _r[3] });

                ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[0], _r[3], _r[0], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[0], _r[0], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[0], _r[0], _r[3], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[3], _r[3], _r[3] });

                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[3], _r[0], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[3], _r[3], _r[0], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[3], _r[3], _r[0] });
                ent.Add(new byte[] { _r[0], _r[3], _r[3], _r[0], _r[3], _r[3] });
                ent.Add(new byte[] { _r[0], _r[3], _r[0], _r[3], _r[3], _r[3] });
            }
        }
        if (complexity >= 5)
        {
            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[0], _r[4] });
            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[0], _r[4] });
            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[0], _r[4] });
            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[4], _r[0], _r[0] });
            ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[0], _r[0] });
            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[0], _r[0] });
            ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[0], _r[0], _r[4] });
            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[0], _r[0] });
            ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[0], _r[0], _r[4] });
            ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[4], _r[0], _r[4] });

            if (threeD)
            {
                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[0], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[0], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[0], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[4], _r[0], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[0], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[0], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[0], _r[0], _r[4] });
                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[0], _r[0] });
                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[0], _r[0], _r[4] });
                ent.Add(new byte[] { _r[0], _r[4], _r[0], _r[4], _r[0], _r[4] });

                ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[0], _r[4], _r[0], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[0], _r[0], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[0], _r[0], _r[4], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[0], _r[0], _r[0], _r[4], _r[4], _r[4] });

                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[4], _r[0], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[4], _r[4], _r[0], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[4], _r[4], _r[0] });
                ent.Add(new byte[] { _r[0], _r[4], _r[4], _r[0], _r[4], _r[4] });
                ent.Add(new byte[] { _r[0], _r[4], _r[0], _r[4], _r[4], _r[4] });
            }
        }
        for (int i = 0; i < ent.Count; i++)
        {
            entropy.Add(i, ent[i]);
        }
    }

    public void SpawnTiles(Dictionary<(int x, int y, int z), Tile> mTile) 
    {
        foreach (var tile in mTile)
        {
            var TempTile = tile.Value;
            TileProps temp = mGameObject[tile.Key];
            var e = mTile[tile.Key].GetExits();
            byte[] ent;
            if (e == -1)
            {
                ent = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            }
            else
            {
                ent = entropy[e];
            }
            temp.SetExits(ent, tile.Key);
            //CombineMeshes();
        }
    }

    protected virtual void InitRegions() 
    {
        var regions = new Dictionary<(int x, int y, int z), (int minX, int minY, int minZ, int maxX, int maxY, int maxZ)>();

        int posX = 0, posY = 0, posZ = 0; // Trackers for the chunk's position

        for (int x = 0; x < max.x; x += regionSize.x, posX++)
        {
            posY = 0; // Reset Y position for each new X level
            for (int y = 0; y < max.y; y += regionSize.y, posY++)
            {
                posZ = 0; // Reset Z position for each new Y level in the curresnt X
                for (int z = 0; z < max.z; z += regionSize.z, posZ++)
                {
                    int endX = Math.Min(x + regionSize.x, max.x);
                    int endY = Math.Min(y + regionSize.y, max.y);
                    int endZ = Math.Min(z + regionSize.z, max.z);

                    // Use the position as the key and the chunk boundaries as the value
                    //Debug.Log($"({posX}, {posY}, {posZ}), ({x}, {y}, {z}, {endX}, {endY}, {endZ})");

                    regions.Add((posX, posY, posZ), (x, y, z, endX, endY, endZ));
                    
                }
            }
        }
        maxRegion = (posX, posY, posZ);

        for (int x = 0; x < max.x; x++)
        {
            for (int y = 0; y < max.y; y++)
            {
                for (int z = 0; z < max.z; z++)
                {
                    Vector3 _targetPos = new Vector3((float)x * 3, (float)y * 3, (float)z * 3);
                    GameObject TempTile = Instantiate(tilePrefab, _targetPos, Quaternion.identity, transform);
                    //TileProps tileProps = new TileProps();
                    mGameObject.Add((x, y, z), TempTile.GetComponent<TileProps>());
                }
            }
        }

        foreach (var region in regions)
        {
            //Debug.Log($"{region.Value.minX},{region.Value.minY},{region.Value.minZ},{region.Value.maxX},{region.Value.maxY},{region.Value.maxZ}");

            Vector3 position = new Vector3(0, 0, 0);//new Vector3(region.Value.minX * 3, region.Value.minY * 3, region.Value.minZ * 3);

            // Instantiate the manager prefab
            //GameObject RegionInstance = Instantiate(RegionPrefab, position, Quaternion.identity, this.transform);
            //RegionInstance.GetComponent<Region>().seed = seed;
            // Assuming the Manager script is attached to the prefab and has public maxX, maxY, maxZ
            NestedRegion RegionScript = new NestedRegion();
            mRegion.Add(region.Key, RegionScript);
            var min = (region.Value.minX, region.Value.minY, region.Value.minZ);
            var max = (region.Value.maxX, region.Value.maxY, region.Value.maxZ);
            RegionScript.Initialize(region.Key, max, min, entropy, this);
            mNotCollapsesed.Add(region.Key);
        }

        GamobjectMap = new Dictionary<(int x, int y, int z), GameObject>();
    }

    public HashSet<int> GetTileEntropy((int x, int y, int z) _tP)
    {
        // Normalize the tile position within the bounds of the world.
        _tP.x = (_tP.x + max.x) % max.x;
        _tP.y = (_tP.y + max.y) % max.y;
        _tP.z = (_tP.z + max.z) % max.z;

        // Calculate the target region coordinates based on the tile's position.
        // This assumes that regionSize.{x,y,z} are greater than 0 to avoid division by zero.
        (int x, int y, int z) _tR = (_tP.x / regionSize.x, _tP.y / regionSize.y, _tP.z / regionSize.z);


        //if (mRegion[_tR].GetTile(_tP).Count == 0) Debug.Log($"{_tP.x},{_tP.y},{_tP.z} {mRegion[_tR].GetTile(_tP).Count}");

        //mRegion[_tR]
        // Fetch and return the entropy for the tile within the target region.
        return mRegion[_tR].GetTile(_tP);
    }

    public void ResetRegion((int x, int y, int z) _startRegion, int counter)
    {
        ((int x, int y, int z) pos, (int x, int y, int z) min, (int x, int y, int z) max) region = (mRegion[targetRegion].pos, mRegion[targetRegion].min, mRegion[targetRegion].max);

        mRegion.Remove(targetRegion);

        Vector3 position = new Vector3(0, 0, 0);
        NestedRegion RegionScript = new NestedRegion();
        mRegion.Add(targetRegion, RegionScript);
        RegionScript.Initialize(region.pos, region.max, region.min, entropy, this);
        collapseCount = (maxRegion.x * maxRegion.y * maxRegion.z) - mNotCollapsesed.Count;
    }

    public void ResetAllRegion()
    {
        mNotCollapsesed = new List<(int x, int y, int z)>();
        mList = new List<(int x, int y, int z)>();
        mRegion = new Dictionary<(int x, int y, int z), NestedRegion>();
        entropy = new Dictionary<int, byte[]>();

        collapseCount = 0;

        Start();
    }

    public void UpdateRegionEntropyList(bool change) 
    {
        if (mNotCollapsesed.Count > 0)
        {
            // Update all entropy values first.
            for(int i = 0; i < mNotCollapsesed.Count; i++)
            {
                mRegion[mNotCollapsesed[i]].ResetRegionState();
                //mRegion[tile].UpdateAllEntropy();
            }

            for (int i = 0; i < mNotCollapsesed.Count; i++)
            {
                mRegion[mNotCollapsesed[i]].UpdateAllEntropy();
            }
            ConcurrentDictionary<(int x, int y, int z), int> mRegionEntropy = new ConcurrentDictionary<(int x, int y, int z), int>();

            for (int i = 0; i < mNotCollapsesed.Count; i++) 
            {
                mRegionEntropy.TryAdd(mNotCollapsesed[i], mRegion[mNotCollapsesed[i]].GetEntropy());
            }

            //var tempDictionary = mNotCollapsesed.ToDictionary(key => key, key => mRegion[key].GetEntropy());


            // Find the minimum entropy value.
            int minEntropy = mRegionEntropy.Values.Min();

            // Select all regions that have this minimum entropy value.
            var lowEntropyRegions = mRegionEntropy.Where(pair => pair.Value == minEntropy).Select(pair => pair.Key).ToList();

            // Safely update targetRegion with the first region of the lowest entropy, if any.
            if (lowEntropyRegions.Any())
            {
                System.Random rnd = new System.Random();
                if (change) targetRegion = lowEntropyRegions[rnd.Next(0,lowEntropyRegions.Count())];
            }
        }
        else 
        {
            collapsed = true;
        }
    }

    public void CheckDone() 
    {
        var count = 0;
        foreach (var region in mRegion) 
        {
            if (region.Value.collapsed) 
            {
                count++;
            }
        }
        if (count > max.x * max.y * max.z) collapsed = true;
    }
}
