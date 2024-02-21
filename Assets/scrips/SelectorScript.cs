using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SelectorScript : MonoBehaviour
{
    public GameObject benchMarker;
    public BenchMarker bench;
    public string type;
    public GameObject menu;
    public GameObject[] menus;

    public GameObject[] Buttons;


    private void Start()
    {
        bench = benchMarker.GetComponent<BenchMarker>();
    }


    private void Update()
    {
        if (bench.running)
        {
            this.GetComponent<Button>().interactable = false;
            foreach (var tempMenu in menus) 
            { 
                tempMenu.GetComponent<TMP_InputField>().interactable = false;
            }
        }
        else 
        {
            this.GetComponent<Button>().interactable = true;
            foreach (var tempMenu in menus)
            {
                tempMenu.GetComponent<TMP_InputField>().interactable = true;
            }
        }

    }

    public void GridChangeSize(string text)
    {
        string temp;
        if (type == "WFC")
        {
            if (text == "x")
            {
                temp = menus[0].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxX = newTemp;
                    bench.RegionX = newTemp;
                }
            }
            if (text == "y")
            {
                temp = menus[1].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxY = newTemp;
                    bench.RegionY = newTemp;
                }
            }
            if (text == "z")
            {
                temp = menus[2].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxZ = newTemp;
                    bench.RegionZ = newTemp;
                }
            }
        }
        else 
        {
            if (text == "x")
            {
                temp = menus[0].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxX = newTemp;
                }
            }
            if (text == "y")
            {
                temp = menus[1].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxY = newTemp;
                }
            }
            if (text == "z")
            {
                temp = menus[2].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.maxZ = newTemp;
                }
            }

            if (text == "Rx") 
            {
                temp = menus[3].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.RegionX = newTemp;
                }
            }
            if (text == "Ry")
            {
                temp = menus[4].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.RegionY = newTemp;
                }
            }
            if (text == "Rz")
            {
                temp = menus[5].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.RegionZ = newTemp;
                }
            }

            if (text == "Sx")
            {
                temp = menus[6].GetComponent<TMP_InputField>().text;
                int newTemp;
                bool success = int.TryParse(temp, out newTemp);
                if (success)
                {
                    bench.StitchSize = newTemp;
                }
            } 
        }
    }

    public void EnableMenu()
    {
        foreach (var tempButton in Buttons) 
        { 
            tempButton.SetActive(false);
        }
        menu.SetActive(true);
    }

}
