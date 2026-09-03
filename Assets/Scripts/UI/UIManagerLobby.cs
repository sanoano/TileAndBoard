using System;
using System.Collections;
using System.Linq;
using TMPro;
using Tweens;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UIManager;
using static UnityEngine.GraphicsBuffer;

public class UIManagerLobby : MonoBehaviour
{// This controls all manner of 2D and 3D interface and decorations for the lobby

    public static UIManagerLobby Instance;

    [Header("Session Info")]
    [SerializeField] private TextMeshProUGUI sessionNameTMP;
    [SerializeField] private TextMeshProUGUI joinCodeTMP;
    [SerializeField] private TextMeshProUGUI timerLengthTMP;
    [SerializeField] private TextMeshProUGUI lifePointsTMP;

    [SerializeField] private TextMeshProUGUI player1NameTMP, player2NameTMP, player1StatusTMP, player2StatusTMP;

    [SerializeField] private TextMeshProUGUI countdownTMP;

    [SerializeField] private GameObject waitingStatus;
    private TextMeshProUGUI waitingStatusTMP;

    [SerializeField] private Button readyGameButton;
    private TextMeshProUGUI readyGameButtonTMP;

    [SerializeField] private Button leaveGameButton;//Opens up leave confirmation menu. Can't access when a game is starting
    [SerializeField] private GameObject leaveMenu;

    [SerializeField] private Image lensCap;

    [SerializeField] private GameObject nameBanner;
    [SerializeField] private GameObject infoBanner;
    private RectTransform infoBannerRect;

    

    [Header("3D Objects")]
    [SerializeField] private GameObject otherIsland;
    private bool islandDoOnce = false;

    [Header("Parameters")]
    [SerializeField] private float fadeDuration;
    private int gameTime, gameLP;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        readyGameButtonTMP = readyGameButton.GetComponentInChildren<TextMeshProUGUI>();
        waitingStatusTMP = waitingStatus.GetComponentInChildren<TextMeshProUGUI>();

        infoBannerRect = infoBanner.GetComponent<RectTransform>();
        infoBannerRect.sizeDelta = new Vector2(300, 290);
        timerLengthTMP.text = "";
        lifePointsTMP.text = "";
    }

    void Start()
    {
        countdownTMP.text = "";
        //playerListTMP.text = "";

        player1NameTMP.text = "";
        player2NameTMP.text = "";
        player1StatusTMP.text = "";
        player2StatusTMP.text = "";

        otherIsland.SetActive(false);

        UIDialogueSlide slideScript = nameBanner.GetComponent<UIDialogueSlide>();
        slideScript.SlideIn();

        StartCoroutine(UnfurlSessionInfo());

    }

    private IEnumerator UnfurlSessionInfo()
    {
        Vector2 start = infoBannerRect.sizeDelta;
        Vector2 end = new Vector2(620, 290);

        float timer = 0.0f;
        float duration = 0.25f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            infoBannerRect.sizeDelta = Vector2.Lerp(start, end, t);

            yield return null;
        }

        timerLengthTMP.text = "Timer Duration: " + gameTime;
        lifePointsTMP.text = "Life Points: " + gameLP;

        yield return null;
    }

    public void SetParameterValues(int time, int lp)
    {
        gameTime = time;
        gameLP = lp;
    }

    public void CountdownUI(string number)
    {
        countdownTMP.text = number.ToString();
    }

    /*public void UpdatePlayerList(string name)//REDUNDANT
    {
        playerListTMP.text += name;
    }*/

    public void UpdatePlayerName(bool host, string name)
    {
        if (host)
            player1NameTMP.text = name;
        else
            player2NameTMP.text = name;
    }

    public void UpdatePlayerStatus(bool host, string status)
    {
        if (host)
            player1StatusTMP.text = status;
        else 
            player2StatusTMP.text = status;
    }

    public void UpdateSessionInfo(string name, string code)
    {
        sessionNameTMP.text = name;
        joinCodeTMP.text = code;
    }

    public void UpdateWaitingText(int state)//0 waiting for p2 to join, 1 waiting for ready up, 2 for both ready
    {
        if (state == 0)
            waitingStatusTMP.text = "Waiting for other player...";
        else if (state == 1)
            waitingStatusTMP.text = "Waiting for both players to ready up...";
        else
            waitingStatusTMP.text = "";
    }

    public void UpdateReadyButton(int state)//0 for available to click, 1 able to back out, 2 is game starting
    {
        if (state == 0)
        {
            readyGameButton.interactable = true;
            readyGameButtonTMP.text = "Ready Up";
        }
        else if (state == 1)
        {
            readyGameButton.interactable = true;
            readyGameButtonTMP.text = "Cancel";
        }
        else if (state == 2)
        {
            readyGameButton.interactable = false;
            readyGameButtonTMP.text = "Cancel";
            leaveGameButton.interactable = false;
        }
    }

    public void EnableIsland(bool enable)
    {
        if (islandDoOnce == false)
        {
            otherIsland.SetActive(enable);
            islandDoOnce = true;
        }
    }

    public IEnumerator StartGameRoutine()
    {

        lensCap.gameObject.SetActive(true);

        Color opaque = new Color(0, 0, 0, 255);

        var backgroundTween = new ColorTween
        {
            from = lensCap.color,
            to = opaque,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => lensCap.color = value,
        };

        var instance = lensCap.gameObject.AddTween(backgroundTween);

        yield return instance.AwaitDecommission();

        if (NetworkManager.Singleton.LocalClient.IsSessionOwner || NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("BattleArena", LoadSceneMode.Single);
        }


        yield return null;

    }

    public void LeaveConfirmationMenu(bool hide)
    {//When you go to quit to the main menu, a confirmation is thrown onto the screen
        if (hide)
            leaveMenu.SetActive(false);
        else
            leaveMenu.SetActive(true);
    }
}
