//using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UIManagerMainMenu : MonoBehaviour
{//Mmmm buttons
    private const string GamesWonKey = "GamesWon";

    [SerializeField] private CameraMainMenu cameraScript;
    private Lobby lobby;

    [Header("Components")]
    [SerializeField] private GameObject title;//Title graphic
    [SerializeField] private GameObject status;//Connection status messages
    [SerializeField] private GameObject[] playerNameElements;
    //[SerializeField] private GameObject versionText;
    private TextMeshProUGUI statusTMP;

    [Header("Tooltips")]
    [SerializeField] private GameObject ttipLP;//(0)
    [SerializeField] private GameObject ttipPriv;//(1)
    [SerializeField] private GameObject ttipTime;//(2)

    [Header("Screens")]
    [SerializeField] private GameObject[] presstostart; //(0)

    [SerializeField] private GameObject[] buttons1;//(1) buttons2, options, credits, Quit
    [SerializeField] private GameObject[] buttons2;//(2) Create Game, Join Private Game, Find Game, Back 

    [SerializeField] private GameObject[] createGame;//(3)
    [SerializeField] private GameObject[] joinGame;//(4) obsolete, direct join is now in findGame
    [SerializeField] private GameObject[] findGame;//(5)
    [SerializeField] private GameObject[] options;//(6)
    [SerializeField] private GameObject[] tutorial;//(7)
    [SerializeField] private GameObject[] loading;//(8)
    [SerializeField] private GameObject[] credits;//(9)
    [SerializeField] private GameObject[] sureQuit;//(10) 

    GameObject[][] UIlist;
    private int currentState = 0;

    [Header("Credits")]
    [SerializeField] private RectTransform creditsListTrans;
    [SerializeField] private float crawlSpeed;
    private float crawlRate;
    private Vector2 startPos;

    [HideInInspector] public bool dontPlayPageSound = false;

    void Start()
    {
        UIlist = new GameObject[][] {presstostart, buttons1, buttons2, createGame, joinGame, findGame, options, tutorial, loading, credits, sureQuit};

        statusTMP = status.GetComponent<TextMeshProUGUI>();
        statusTMP.text = "";

        SetMenuLevel(0);

        startPos = creditsListTrans.anchoredPosition;

        lobby = NetworkManager.Singleton.GetComponent<Lobby>();
    }

    public void SetMenuScreen(int newState)
    {//Each screen has an ID. When setting up buttons, you just need to know the code for what screen you want a button to bring up.

        if (currentState > 0 && newState != currentState)
        {
            if (newState < currentState || newState == 10)
                AudioManager.singleton.PlaySound("stonePush", false, 0.4f);
            else
                if (!dontPlayPageSound)
                    AudioManager.singleton.PlaySound("scrollOpen", false, 0.6f);
                else
                    dontPlayPageSound = false;
        }

        if (newState == 3)
        {
            lobby?.LoadPreferredMatchSettings();
        }

        if (newState != 5)
        {
            lobby?.StopLanDiscovery();
        }

        // try
        // {
        foreach (GameObject[] array in UIlist)
        {
            foreach (GameObject element in array)
            {
                element.SetActive(false);
            }
        }
        //}
        // catch (Exception e)
        // {
        //     
        // }


        if (newState != 9)
        { 
            foreach (GameObject element in UIlist[newState])
            {
                element.SetActive(true);

                if (newState == 1)
                {
                    foreach (GameObject button in buttons1)
                    {
                        UIDialogueSlide buttons1SlideScript = button.GetComponent<UIDialogueSlide>();
                        if (buttons1SlideScript != null)
                            StartCoroutine(PlaySlideNextFrame(buttons1SlideScript, true));

                    }
                }
                else if (newState == 2)
                {
                    foreach (GameObject button in buttons2)
                    {
                        UIDialogueSlide buttons2SlideScript = button.GetComponent<UIDialogueSlide>();
                        if (buttons2SlideScript != null)
                            StartCoroutine(PlaySlideNextFrame(buttons2SlideScript, true));
                    }

                    foreach (GameObject button in buttons1)
                    {// This won't work because buttons1 is disabled at this point. oh well.
                        UIDialogueSlide buttons1SlideScript = button.GetComponent<UIDialogueSlide>();
                        if (buttons1SlideScript != null)
                            StartCoroutine(PlaySlideNextFrame(buttons1SlideScript, false));
                    }
                }
                else if (newState == 3)
                {
                    foreach (GameObject menu in createGame)
                    {
                        UIDialogueSlide menuSlideScript = menu.GetComponent<UIDialogueSlide>();
                        if (menuSlideScript != null)
                            StartCoroutine(PlaySlideNextFrame(menuSlideScript, true));
                    }
                }
            }
        }
        else
        {
            StartCredits();
        }

        //Makes sure the status messages don't clog up the nice views of irrelevant menus
        if (newState >= 3 && newState < 6)
            status.SetActive(true);
        else
        {
            status.SetActive(false);
            statusTMP.text = "";
        }

        //Player name input stuff yknow
        if (newState == 3 || newState == 5 || newState == 2)
            foreach (GameObject element in playerNameElements)
                element.SetActive(true);
        else
            foreach (GameObject element in playerNameElements)
                element.SetActive(false);


        //camera stuff
        if (newState == 3 || newState == 4 || newState == 9)
            cameraScript.SetCameraState(2);
        else if (newState == 5 || newState == 8)
            cameraScript.SetCameraState(1);
        else
            cameraScript.SetCameraState(0); 

        currentState = newState;

    }

    public void SetMenuLevel(int menuLevel)
    {//0 is the press to start screen, 1 is buttons1, 2 is buttons2, 3 is anything beyond that. Helps determine visiblity of title logo
        if (menuLevel < 3)
            title.SetActive(true);
        else
            title.SetActive(false);
    }

    IEnumerator PlaySlideNextFrame(UIDialogueSlide script, bool slidingIn)
    {
        yield return null;
        if (slidingIn)
            script.SlideIn();
        else
            script.SlideOut();
    }

    private void Update()
    {
        if (Input.anyKeyDown && currentState < 1)
        {
            SetMenuScreen(1);
            SetMenuLevel(1);
        }

        if (currentState == 9)
        {
            crawlRate = crawlSpeed * Time.deltaTime;
        }

        creditsListTrans.anchoredPosition += Vector2.up * crawlRate;

        //probably a smarter way to do this but oh well!!
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == 2 || currentState == 6 || currentState == 9)
            {
                SetMenuScreen(1);
                SetMenuLevel(1);
            }
            else if (currentState == 3 || currentState == 5 || currentState == 7)
            {
                SetMenuScreen(2);
                SetMenuLevel(2);
            }
        }

    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void StartCredits()
    {
        creditsListTrans.anchoredPosition = startPos;

        foreach (GameObject element in UIlist[9])
        {
            element.SetActive(true);
        }
    }

    public void OpenWebsite()
    {
        Application.OpenURL("https://splitchance.com/games.html");
    }

    public int GetGamesWon()
    {
        return PlayerPrefs.GetInt(GamesWonKey, 0);
    }

}
