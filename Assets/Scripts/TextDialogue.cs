using System;
using System.Xml;
using System.Collections;
using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.UI;

public class TextDialogue : MonoBehaviour
{// This dialogue box accepts int arguments that correspond with status messages for the player. Disappear after a couple of secs

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI dialogueTMP;
    [SerializeField] private TextMeshProUGUI headerTMP;
    [SerializeField] private Image background;

    [Header("UI Variables")]
    [SerializeField] private float fadeDelay = 4.0f;
    [SerializeField] private float fadeDuration = 1.0f;
    private float opacityInactive = 0.0f;
    private float opacityActive = 1.0f;

    private float lastUpdateTime;
    private string textLast = "";

    private int boardMax, handMax;

    public static TextDialogue instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        Color color = new Color(255, 255, 255, 0);
        background.color = color;
        dialogueTMP.color = color;
        headerTMP.color = color;

        
        boardMax = BoardManager.Instance.maxCardsPerPlayer;
        handMax = CardManager.instance.maxCards;
    }

   

    public void DialogueRecieveStatus(int code)
    {
        StopAllCoroutines();
        Color invisible = new Color(255, 255, 255, 0);
        background.color = invisible;
        dialogueTMP.color = invisible;
        headerTMP.color = invisible;

        AudioManager.singleton.PlaySound("uiError", false);
        switch (code)
        {
            case 1:
                headerTMP.text = "Insufficient TP";
                dialogueTMP.text = "You do not have enough Tactics Points for this action!";
                break;
            case 2:
                headerTMP.text = "Insufficient Card Actions";
                dialogueTMP.text = "You cannot perform any more Card Actions this turn!";
                break;
            case 3:
                headerTMP.text = "Reached Card Limit";
                dialogueTMP.text = "You cannot place more than " + boardMax + " cards on your board!";
                break;
            case 4:
                headerTMP.text = "Reached Hand Limit";
                dialogueTMP.text = "The maximum amount of cards you can hold is " + handMax + "!";
                break;
            case 5:
                headerTMP.text = "Not Your Turn";
                dialogueTMP.text = "You can't take any actions until your opponent ends their turn!";
                break;
            case 6:
                headerTMP.text = "Invalid Space";
                dialogueTMP.text = "This space is already occupied by a card!";
                break;
            case 7:
                headerTMP.text = "Invalid Space";
                dialogueTMP.text = "You can't place a card on your opponent's board!";
                break;
            default:
                headerTMP.text = "Unknown Error!";
                dialogueTMP.text = "idk what's going on!";
                break;
        }

        Color opaque = new Color(255, 255, 255, 255);
        //print("meep");

       
        var backgroundTween = new ColorTween {
            from = background.color,
            to = opaque,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => background.color = value,
        };
        var dialogueTween = new ColorTween {
            from = dialogueTMP.color,
            to = opaque,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => dialogueTMP.color = value,
        };
        var headerTween = new ColorTween {
            from = headerTMP.color,
            to = opaque,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => headerTMP.color = value,
        };

        dialogueTMP.gameObject.AddTween(dialogueTween);
        headerTMP.gameObject.AddTween(headerTween);
        background.gameObject.AddTween(backgroundTween);

        StartCoroutine(FadeAway());

    }

    IEnumerator FadeAway()
    {
        yield return new WaitForSeconds(fadeDelay);
        
        Color invisible = new Color(255, 255, 255, 0);
        
        var backgroundTween = new ColorTween {
            from = background.color,
            to = invisible,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => background.color = value,
        };
        var dialogueTween = new ColorTween {
            from = dialogueTMP.color,
            to = invisible,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => dialogueTMP.color = value,
        };
        var headerTween = new ColorTween {
            from = headerTMP.color,
            to = invisible,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => headerTMP.color = value,
        };

        dialogueTMP.gameObject.AddTween(dialogueTween);
        headerTMP.gameObject.AddTween(headerTween);
        background.gameObject.AddTween(backgroundTween);

        yield return null;

    }
    
    


}