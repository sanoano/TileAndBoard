using System.Xml;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextDialogue : MonoBehaviour
{// This dialogue box accepts int arguments that correspond with status messages for the player. Disappear after a couple of secs

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dialogueTMP;
    [SerializeField] private TextMeshProUGUI headerTMP;
    [SerializeField] private GameObject background;

    [Header("UI Variables")]
    [SerializeField] private float fadeDelay = 4.0f;
    [SerializeField] private float fadeDuration = 1.0f;
    private float opacityInactive = 0.0f;
    private float opacityActive = 1.0f;

    private float lastUpdateTime;
    private string textLast = "";

    private int boardMax, handMax;

    void Start()
    {
        SetOpacity(opacityInactive);
        background.SetActive(false);

        boardMax = CardManager.instance.maxCards;
        handMax = BoardManager.Instance.maxCardsPerPlayer;
    }

    void Update()
    {
        if (dialogueTMP.text != textLast)
        {
            textLast = dialogueTMP.text;
            lastUpdateTime = Time.time;
            SetOpacity(opacityActive);
        }

        if (Time.time - lastUpdateTime > fadeDelay)
        {
            SetOpacity(opacityInactive);
            dialogueTMP.text = "";
            dialogueTMP.text = "";
        }
    }

    public void DialogueRecieveStatus(int code)
    {
        if (code == 1)
        {
            headerTMP.text = "Insufficient TP";
            dialogueTMP.text = "You do not have enough Tactics Points for this action!";
        }
        else if (code == 2)
        {
            headerTMP.text = "Insufficient Card Actions";
            dialogueTMP.text = "You cannot perform any more Card Actions this turn!";
        }
        else if (code == 3)
        {
            headerTMP.text = "Reached Card Limit";
            dialogueTMP.text = "You cannot place more than " + boardMax + " cards on your board!";
        }
        else if (code == 4)
        {
            headerTMP.text = "Reached Hand Limit";
            dialogueTMP.text = "The maximum amount of cards you can hold is " + handMax + "!";
        }
        else if (code == 5)
        {
            headerTMP.text = "Not Your Turn";
            dialogueTMP.text = "You can't take any actions until your opponent ends their turn!";
        }
        else if (code == 6)
        {
            headerTMP.text = "Invalid Space";
            dialogueTMP.text = "This space is already occupied by a card!";
        }
        else if (code == 7)
        {
            headerTMP.text = "Invalid Space";
            dialogueTMP.text = "You can't place a card on your opponent's board!";
        }
        else
        {
            headerTMP.text = "Unknown Error!";
            dialogueTMP.text = "idk what's going on!";
        }
    }

    private void SetOpacity(float alpha)
    {
        Color color = dialogueTMP.color;
        color.a = Mathf.Clamp01(alpha);
        dialogueTMP.color = color;
        headerTMP.color = color;
        if (alpha  > 0)
            background.SetActive(true);
        else
            background.SetActive(false);
    }
}