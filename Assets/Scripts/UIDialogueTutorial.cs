using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UIDialogueTutorial : MonoBehaviour
{//Similar to the main menu UI manager, loads the contents of different arrays full of graphics (text, images), and puts those arrays on another array. I love arrays!!

    [Header("Components")]
    [SerializeField] private GameObject header;
    private TextMeshProUGUI headerTMP;

    [Header("Tutorial Content")]
    [SerializeField] private string[] headers;
    [SerializeField] private GameObject[] page0;//Title graphic, high concept of gameplay
    [SerializeField] private GameObject[] page1;
    [SerializeField] private GameObject[] page2;
    [SerializeField] private GameObject[] page3;
    [SerializeField] private GameObject[] page4;
    [SerializeField] private GameObject[] page5;
    [SerializeField] private GameObject[] page6;
    //Right now we have to manually add pages/content.

    GameObject[][] pages;
    private int pageIndex = 0;

    void Start()
    {
        pages = new GameObject[][] { page0, page1, page2, page3, page4, page5, page6 };

        foreach (GameObject[] array in pages)
        {
            foreach (GameObject element in array)
            {
                element.SetActive(false);
            }
        }

        foreach (GameObject element in pages[pageIndex])
        {
            element.SetActive(true);
        }

        headerTMP = header.GetComponent<TextMeshProUGUI>();
        headerTMP.text = headers[pageIndex];
    }

    public void FlipPage(bool forwards)
    {
        if (forwards)
        {
            if (pageIndex == pages.Length - 1)
                pageIndex = 0;
            else
                pageIndex++;
        }
        else
        {
            if (pageIndex == 0)
                pageIndex = pages.Length - 1;
            else
                pageIndex--;
        }

        foreach (GameObject[] array in pages)
        {
            foreach (GameObject element in array)
            {
                element.SetActive(false);
            }
        }

        foreach (GameObject element in pages[pageIndex])
        {
            element.SetActive(true);
        }

        headerTMP.text = headers[pageIndex];
        //Debug.Log(pageIndex);
    }
}
