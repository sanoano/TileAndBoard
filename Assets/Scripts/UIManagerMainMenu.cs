using Mono.Cecil.Cil;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class UIManagerMainMenu : MonoBehaviour
{//Mmmm buttons

    [SerializeField] private CameraMainMenu cameraScript;

    [SerializeField] private GameObject title;//Title graphic
    [SerializeField] private GameObject status;//Connection status messages
    private TextMeshProUGUI statusTMP;

    [SerializeField] private GameObject[] presstostart; //(0)

    [SerializeField] private GameObject[] buttons1;//Play, Options, Quit (no options for now) (1)
    [SerializeField] private GameObject[] buttons2;//Create Game, Join Private Game, Find Game, Back (2)

    [SerializeField] private GameObject[] createGame;//(3)
    [SerializeField] private GameObject[] joinGame;//(4)
    [SerializeField] private GameObject[] findGame;//(5)
    [SerializeField] private GameObject[] options;//(6)

    GameObject[][] UIlist;
    void Start()
    {
        UIlist = new GameObject[][] {presstostart, buttons1, buttons2, createGame, joinGame, findGame, options};

        statusTMP = status.GetComponent<TextMeshProUGUI>();
        statusTMP.text = "";
    }

    public void SetMenuScreen(int menuState)
    {//Each screen has an ID. When setting up buttons, you just need to know the code for what screen you want a button to bring up.
        foreach (GameObject[] array in UIlist)
        {
            foreach (GameObject element in array)
            {
                element.SetActive(false);
            }
        }

        foreach (GameObject element in UIlist[menuState])
        {
            element.SetActive(true);
        }

        //Makes sure the status messages don't clog up the nice views of irrelevant menus
        if (menuState >= 3 && menuState < 6)
            status.SetActive(true);
        else
        {
            status.SetActive(false);
            statusTMP.text = "";
        }

        //camera stuff
        if (menuState == 3 || menuState == 4)
            cameraScript.SetCameraState(2);
        else if (menuState == 5)
            cameraScript.SetCameraState(1);
        else
            cameraScript.SetCameraState(0);
    }

    public void SetMenuLevel(int menuLevel)
    {//0 is the press to start screen, 1 is buttons1, 2 is buttons2, 3 is anything beyond that. Helps determine visiblity of title
        if (menuLevel < 3)
            title.SetActive(true);
        else
            title.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
