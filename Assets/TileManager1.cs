using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class TileManager : MonoBehaviour
{
    public GameObject Parent { get; private set; }
    private byte[] exits = new byte[6];
    [SerializeField] private GameObject centre;
    [SerializeField] private GameObject[] edges;
    [SerializeField] private GameObject edge;

    private void Start()
    {
        edges = new GameObject[6];
    }

    public void SetExits(byte[] _exits)
    {
        exits = _exits;
        for (int i = 0; i < exits.Length; i++)
        {
            // WTF
            try 
            {
                if (edges[i].name == "") 
                { }
            }
            catch(NullReferenceException) 
            {
                if (exits[i] == 1)
                {
                    Vector3 Target = new Vector3(0.0f, 0.0f, 0.0f);
                    switch (i)
                    {
                        case 0:
                            Target.x += 1;
                            break;
                        case 1:
                            Target.y += 1;
                            break;
                        case 2:
                            Target.z += 1;
                            break;
                        case 3:
                            Target.x -= 1;
                            break;
                        case 4:
                            Target.y -= 1;
                            break;
                        case 5:
                            Target.z -= 1;
                            break;
                    }
                    GameObject edgeInstance = Instantiate(edge, this.transform.position + Target, Quaternion.identity, this.transform);
                    edges[i] = edgeInstance;
                    centre.SetActive(true);
                }
            }
        }
    }

    public void ResetExits()
    {
        Destroy(this.transform);
    }
}
