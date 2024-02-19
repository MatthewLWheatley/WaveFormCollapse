using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TMPro;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class StitchedManager : Manager
{
    List<int> ParallelisedNumbers = new List<int>();

    public override void CustomUpdate()
    {
        //Debug.Log("running");
        mCollapseCount = mNotCollapsesed.Count;

        time = Time.time;

        //if (renderedCount >= mRegion.Count) rendered = true;

        if (!collapsed)
        {
            //Stitch();
            ParallelGrid();
        }
        else if (!rendered)
        {
            //var r = mRegion[mRegion.ElementAt(renderedCount).Key];
            //if (renderOnFinish) r.RunRenderer();
            //if (r.rendered)
            //{
            //    renderedCount++;
            //}
            //r.rendered = true;
            ////RunningTotal.text = string.Format(renderedCount.ToString());
        }
    }

    private void Stitch((int x, int y, int z) targetR) 
    {
        var r = mRegion[targetR];
        r.running = true;
        r.RunUpdate();
        
        //if (r.resetCount > 0)
        //{
        //    //if (mStack.Count > 0)
        //    //{
        //    //    foreach (var tile in mNotCollapsesed)
        //    //    {
        //    //        ResetRegion(tile, 0);
        //    //        mRegion[tile].ResetRegionState();
        //    //    }
        //    //    //Debug.Log("pop");
        //    //    var temp5 = mStack.Pop();
        //    //    mNotCollapsesed.Add(temp5);
        //    //    targetRegion = temp5;
        //    //    ResetRegion(targetRegion, 0);
        //    //    if (realTimeRender) r.RunRenderer();
        //    //    foreach (var tile in mNotCollapsesed)
        //    //    {
        //    //        mRegion[tile].UpdateAllEntropy();
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    //Debug.Log("stack empty");
        //    //    //Debug.Log($"{mNotCollapsesed.Count}");
        //    //    foreach (var tile in mNotCollapsesed)
        //    //    {
        //    //        //Debug.Log($"{tile.x},{tile.y},{tile.z}");
        //    //        ResetRegion(tile, 0);
        //    //    }
        //    //}
        //}
        if (r.collapsed)
        {
            //r.RunRenderer();
            //r.CombineMeshes();
            mStack.Push(targetRegion);
            r.running = false;
            //if(renderOnFinish)r.RunRenderer();
            collapseCount++;
            mNotCollapsesed.Remove(targetRegion);
            //UpdateRegionEntropyList(true);

        }
        //RunningTotal.text = string.Format(collapseCount.ToString());
    }

    public bool cunt = false;
    public bool shit = false;
    public bool wank = false;
    private void ParallelGrid() 
    {
        
        bool fuck = true;

        if (fuck && !cunt)
        {
            Parallel.For(0, maxRegion.x, x =>
            {
                for (int y = 0; y < maxRegion.y; y++)
                {
                    for (int z = 0; z < maxRegion.z; z++)
                    {
                        if ((x + z + y) % 2 == 0)
                        {

                            int tempint = (int)x;
                            Stitch((tempint, y, z));
                            if (!mRegion[(tempint, y, z)].collapsed)
                            {
                                fuck = false;
                            }
                        }
                    }
                }
            });
            UpdateRegionEntropyList(true);
            
            for (int x = 0; x < maxRegion.x; x++)
            {
                for (int y = 0; y < maxRegion.y; y++)
                {
                    for (int z = 0; z < maxRegion.z; z++)
                    {
                        if ((x + z + y) % 2 == 0)
                        {
                            if (realTimeRender) mRegion[(x, y, z)].RunRenderer();
                        }
                    }
                }
            }

            if (!fuck) return;
        }
        cunt = true;
        shit = true;

        UpdateRegionEntropyList(true);

        UpdateRegionEntropyList(true);

        if (shit && !wank)
        {

            //Parallel.For(0, maxRegion.x, x =>
            //{
            for(int x = 0; x < maxRegion.x; x++) { 
                for (int y = 0; y < maxRegion.y; y++)
                {
                    for (int z = 0; z < maxRegion.z; z++)
                    {
                        if ((x + z + y) % 2 != 0)
                        {
                            int tempint = (int)x;
                            Stitch((tempint, y, z));
                            //if (!mRegion[(tempint, y, z)].collapsed)
                            //{
                            //    shit = false;
                            //}
                            shit = false;
                        }
                    }
                }
            }//);

            UpdateRegionEntropyList(true);

            for (int x = 0; x < maxRegion.x; x++)
            {
                for (int y = 0; y < maxRegion.y; y++)
                {
                    for (int z = 0; z < maxRegion.z; z++)
                    {
                        if (realTimeRender) mRegion[(x, y, z)].RunRenderer();
                    }
                }
            }
            if (!shit) return;
        }
        wank = true;
        //collapsed = true;
    }
}

//public struct stitchJob : IJobParallelFor 
//{ 


//}
