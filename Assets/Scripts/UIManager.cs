using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static CardDeck;
using Tweens;


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;

    public enum InteractionState : byte
    {
        None,
        Attacking,
        Defending,
        Moving
    }

    [Header("Interaction State")] 
    public InteractionState interactionState;
    
    [Header("Tile Info Panel")]
    [SerializeField] private GameObject uiInfoPrefab;
    private GameObject uiInfoPrefabInstance;
    [SerializeField] private GameObject InfoPanelPos;
    
    [Header("Canvas")]
    public GameObject Canvas;

    [Header("Draw Card Button")]
    [SerializeField] private GameObject DrawCardButton;
    private UIDialogueSlide drawCardSlide;
    private bool slideOnce;

    [Header("Card Info Panel")] 
    [SerializeField] private GameObject cardInfoPrefab;
    private GameObject cardInfoPrefabInstance;
    [SerializeField] private GameObject CardInfoPanelPos;

    [Header("Actions Info Panel")]
    [SerializeField] private GameObject actionsInfoPrefab;
    private GameObject actionsInfoPrefabInstance;
    [SerializeField] private GameObject actionsInfoPanelPos;

    [Header("Tactics Display")] 
    [SerializeField] private TextMeshProUGUI tacticsText;
    [SerializeField] private TextMeshProUGUI actionsText;

    [Header("Health Display")] [SerializeField]
    private TextMeshProUGUI player1HealthDisplay;
    [SerializeField] private TextMeshProUGUI player2HealthDisplay;

    [Header("Controls Display")]
    [SerializeField] private TextMeshProUGUI controlsText;

    [Header("Card Amount Display")]
    [SerializeField] private TextMeshProUGUI player1CardAmount;
    [SerializeField] private TextMeshProUGUI player2CardAmount;

    [Header("Hand Amount Display")] 
    [SerializeField] private TextMeshProUGUI handAmount;

    [Header("Turn Display")] 
    [SerializeField] private TextMeshProUGUI turnCountText;
    [SerializeField] private TextMeshProUGUI turnTimer;

    [Header("Usernames")] 
    public TextMeshProUGUI player1Name;
    public TextMeshProUGUI player2Name;
    
    [Header("Settings Menu")] 
    public GameObject settingsMenu;

    [Header("Victory Text")] 
    [SerializeField] private TextMeshProUGUI victoryText;

    [Header("Lens Cap")] 
    [SerializeField] private Image lensCap;
    [SerializeField] private float fadeDuration;

    public AudioMixer mixer;
    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        interactionState = InteractionState.None;
        EnableControlsText();
        
    }

    public void Start()
    {
        BoardManager.Instance.cardPlaced.AddListener(UpdateCardAmountDisplay);
        BoardManager.Instance.damageTaken.AddListener(UpdateHealthDisplay);
        BoardManager.Instance.cardDied.AddListener(UpdateCardAmountDisplay);
        UpdateHealthDisplay();
        UpdateCardAmountDisplay();
        
        Color invisible = new Color(0, 0, 0, 0);

        lensCap.gameObject.SetActive(true);
        
        var backgroundTween = new ColorTween {
            from = lensCap.color,
            to = invisible,
            duration = fadeDuration,
            easeType = EaseType.SineOut,
            onUpdate = (_, value) => lensCap.color = value,
            onEnd = (instance) => {
                lensCap.gameObject.SetActive(false);
            },
        };

        var instance = lensCap.gameObject.AddTween(backgroundTween);
        drawCardSlide = DrawCardButton.GetComponent<UIDialogueSlide>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && interactionState == InteractionState.None && BoardManager.Instance.currentSelectedTileGameObject == null)
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
            DestroyCurrentInfoInstance();
        }

        tacticsText.text = $"Manna: {ManaManager.instance.currentManaPoints}";

        handAmount.text = $"{CardManager.instance.playerHand.Count} / {CardManager.instance.maxCards}";

        turnCountText.text = $"Turn {TurnManager.instance.turnCount}";

        /*if (TurnManager.instance.isYourTurn)
        {
            turnTimer.text = String.Format("{0:0}:{1:00}", Mathf.Floor(((int)TurnManager.instance.currentTime) / 60), ((int)TurnManager.instance.currentTime) % 60);
        }
        else
        {
            turnTimer.text = String.Empty;
        }*/

        turnTimer.text = String.Format("{0:0}:{1:00}", Mathf.Floor(((int)TurnManager.instance.currentTime) / 60), ((int)TurnManager.instance.currentTime) % 60);

        if (TurnManager.instance.isYourTurn && !slideOnce)
        {
            drawCardSlide.SlideIn();
            slideOnce = true;
        }
        else if (!TurnManager.instance.isYourTurn && slideOnce)
        {
            drawCardSlide.SlideOut();
            slideOnce = false;
        }
    }

    void UpdateHealthDisplay()
    {
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            player1HealthDisplay.text =
                $"Life Points: {BoardManager.Instance.player1Health} / {BoardManager.Instance.startingPlayerHealth}";
        
            player2HealthDisplay.text =
                $"Life Points: {BoardManager.Instance.player2Health} / {BoardManager.Instance.startingPlayerHealth}";
        }
        else
        {
            player1HealthDisplay.text =
                $"Life Points: {BoardManager.Instance.player2Health} / {BoardManager.Instance.startingPlayerHealth}";
        
            player2HealthDisplay.text =
                $"Life Points: {BoardManager.Instance.player1Health} / {BoardManager.Instance.startingPlayerHealth}";
        }
       
    }

    void UpdateCardAmountDisplay()
    {
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            player1CardAmount.text =
                $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player1)} / {BoardManager.Instance.maxCardsPerPlayer}"
                ;
        
            player2CardAmount.text =
                $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player2)} / {BoardManager.Instance.maxCardsPerPlayer}"
                ;

        }
        else
        {
            player1CardAmount.text =
                $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player2)} / {BoardManager.Instance.maxCardsPerPlayer}"
                ;
        
            player2CardAmount.text =
                $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player1)} / {BoardManager.Instance.maxCardsPerPlayer}"
                ;
        }
        
    }

    public void CreateInfoPanel(Vector2Int position, Player.PlayerId playerId)
    {
        if (uiInfoPrefabInstance)
        {
            Destroy(uiInfoPrefabInstance);
        }
        
        uiInfoPrefabInstance = Instantiate(uiInfoPrefab, InfoPanelPos.transform.position, Quaternion.identity, InfoPanelPos.transform);

        Transform[] children = uiInfoPrefabInstance.GetComponentsInChildren<Transform>();

        TextMeshProUGUI damageText = children[2].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defenseText = children[4].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI totalText = children[6].gameObject.GetComponent<TextMeshProUGUI>();

        int damageTotal = 0;
        int defenseTotal = 0;

        if (BoardManager.Instance.damageInstances.Count != 0)
        {
            foreach (BoardManager.DamageInstance thing in BoardManager.Instance.damageInstances)
            {
                if(thing.ID == playerId) continue;
                foreach (Vector2Int damagePosition in thing.Positions)
                {
                    if (Equals(damagePosition, position))
                    {
                        damageText.text += thing.Name + ": " + thing.Damage + "\n";
                        damageTotal += thing.Damage;
                    }
                }
            }
        }
       
        
        if (BoardManager.Instance.defenseInstances.Count != 0)
        {
            foreach (BoardManager.DefenseInstance thing in BoardManager.Instance.defenseInstances)
            {
                if(thing.ID != playerId) continue;
                foreach (Vector2Int defensePosition in thing.Positions)
                {
                    if (Equals(defensePosition, position))
                    {
                        defenseText.text += thing.Name + ": " + thing.Defense + "\n";
                        defenseTotal += thing.Defense;
                    }
                }
            }
        }
        

        int total = damageTotal - defenseTotal;
        if (total < 0) total = 0;

        totalText.text = total.ToString();



    }

    public void CreateCardInfoPanel(Vector2Int position, Player.PlayerId playerId)
    {

        BoardManager.Unit unitToDisplay = default;
        GameObject cardVisual = null;
        
        // if (cardInfoPrefabInstance)
        // {
        //     Destroy(cardInfoPrefabInstance);
        // }

        bool unitFound = false;
        foreach (BoardManager.Unit unit in BoardManager.Instance.unitsList)
        {
            if (unit.ID == playerId && unit.Position == position)
            {
                unitToDisplay = unit;
                cardVisual = BoardManager.Instance.localBoard.Visuals[unit.Position.x, unit.Position.y];
                unitFound = true;
                break;
            }
        }

        if (!unitFound) return;

        cardInfoPrefabInstance = Instantiate(cardInfoPrefab, CardInfoPanelPos.transform.position, Quaternion.identity, CardInfoPanelPos.transform);

        UIDialogueSlide slideScript = CardInfoPanelPos.GetComponent<UIDialogueSlide>();
        Transform[] cardChildren = cardInfoPrefabInstance.GetComponentsInChildren<Transform>(true);

        /*TextMeshProUGUI cardInfoText = cardChildren[2].gameObject.GetComponent<TextMeshProUGUI>();

        cardInfoText.text = "";
        cardInfoText.text += $"Name: {unitToDisplay.Name}" + "\n";
        cardInfoText.text += $"Health: {unitToDisplay.Health}" + "\n";
        cardInfoText.text += $"Movement: {unitToDisplay.Movement}" + "\n";
        cardInfoText.text += $"Defense: {unitToDisplay.Defense}" + "\n";
        cardInfoText.text += $"Damage: {unitToDisplay.Damage}" + "\n";*/

        //Value fields (ie. not headers or icons)
        TextMeshProUGUI cardInfoHeader = cardChildren[1].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoHealth = cardChildren[10].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoSpeed = cardChildren[11].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoDamage = cardChildren[12].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoDefence = cardChildren[13].gameObject.GetComponent<TextMeshProUGUI>();

        //Cosmetic headers
        Image cardIconSpeed = cardChildren[7].gameObject.GetComponent<Image>();
        Image cardIconDamage = cardChildren[8].gameObject.GetComponent<Image>();
        Image cardIconDefence = cardChildren[9].gameObject.GetComponent<Image>();

        TextMeshProUGUI cardInfoSpeedHeader = cardChildren[3].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoDamageHeader = cardChildren[4].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI cardInfoDefenceHeader = cardChildren[5].gameObject.GetComponent<TextMeshProUGUI>();

        cardInfoHeader.text = "Card Info: " + unitToDisplay.Name;
        cardInfoHealth.text = unitToDisplay.Health.ToString();

        //These if statements just hide icons+numbers for stats at 0
        if (unitToDisplay.Movement > 0)
        {
            cardInfoSpeed.text = unitToDisplay.Movement.ToString();
            cardInfoSpeedHeader.text = "Speed:";
            cardIconSpeed.enabled = true;
            
        }
        else
        {
            cardInfoSpeed.text = "";
            cardInfoSpeedHeader.text = "";
            cardIconSpeed.enabled = false;
        }

        if (unitToDisplay.Damage > 0)
        {
            cardInfoDamage.text = unitToDisplay.Damage.ToString();
            cardInfoDamageHeader.text = "Damage:";
            cardIconDamage.enabled = true;
        }
        else
        {
            cardInfoDamage.text = "";
            cardInfoDamageHeader.text = "";
            cardIconDamage.enabled = false;
        }

        if (unitToDisplay.Defense > 0)
        {
            cardInfoDefence.text = unitToDisplay.Defense.ToString();
            cardInfoDefenceHeader.text = "Defence:";
            cardIconDefence.enabled = true;
        }
        else
        {
            cardInfoDefence.text = "";
            cardInfoDefenceHeader.text = "";
            cardIconDefence.enabled = false;
        }

        StopAllCoroutines();

        //nasty nasty way to do this...but idc
        foreach (Vector2Int coord in unitToDisplay.AttackPositions)
        {// Simple logic tree to find out which squares should show up...inelegant but robust enough...
            int x = coord.x;
            int y = coord.y;

            if (y == 0) 
            {
                if (x == 0)
                    cardChildren[15].gameObject.SetActive(true);
                else if (x == 1)
                    cardChildren[16].gameObject.SetActive(true);
                else if (x == 2)
                    cardChildren[17].gameObject.SetActive(true);
            }
            else if (y == 1)
            {
                if (x == 0)
                    cardChildren[18].gameObject.SetActive(true);
                else if (x == 1)
                    cardChildren[19].gameObject.SetActive(true);
                else if (x == 2)
                    cardChildren[20].gameObject.SetActive(true);
            }
            else if (y == 2)
            {
                if (x == 0)
                    cardChildren[21].gameObject.SetActive(true);
                else if (x == 1)
                    cardChildren[22].gameObject.SetActive(true);
                else if (x == 2)
                    cardChildren[23].gameObject.SetActive(true);
            }
        }

        StartCoroutine(RotateGridShape(cardChildren, new List<Vector2Int>(unitToDisplay.AttackPositions)));


        if (unitToDisplay.ID == GameManager.instance.playerId)
        {
            /*actionsInfoPrefabInstance = Instantiate(actionsInfoPrefab, actionsInfoPanelPos.transform.position,
                Quaternion.identity,
                actionsInfoPanelPos.transform);*/

            //Transform[] actionChildren = actionsInfoPrefabInstance.GetComponentsInChildren<Transform>();

            //var panel = actionChildren[2].gameObject;

            var buttons = cardInfoPrefabInstance.GetComponentsInChildren<Button>();
            
            buttons[3].GetComponentInChildren<TextMeshProUGUI>().text = $"Recall (-{unitToDisplay.AttackPositions.Count} Mna)";

            print(buttons[0].gameObject.name);
            if (unitToDisplay.Damage > 0)
            {
                buttons[0].onClick.AddListener(BoardManager.Instance.PrepareAttack);
                if (!ManaManager.instance.CanAfford(unitToDisplay.AttackPositions.Count) || !TurnManager.instance.isYourTurn ||
                    unitToDisplay.HasActed)
                {
                    buttons[0].interactable = false;
                }
            }
            else
            {
                buttons[0].gameObject.SetActive(false);
            }


            if (unitToDisplay.Defense > 0)
            {
                buttons[1].onClick.AddListener(BoardManager.Instance.PrepareDefense);
                if (!ManaManager.instance.CanAfford(unitToDisplay.AttackPositions.Count) || !TurnManager.instance.isYourTurn ||
                    unitToDisplay.HasActed)
                {
                    buttons[1].interactable = false;
                }
            }
            else
            {
                buttons[1].gameObject.SetActive(false);
            }

            if (unitToDisplay.Movement > 0)
            {
                buttons[2].onClick.AddListener(BoardManager.Instance.PrepareMovement);
                if (!ManaManager.instance.CanAfford(unitToDisplay.AttackPositions.Count) || !TurnManager.instance.isYourTurn ||
                    unitToDisplay.HasActed)
                {
                    buttons[2].interactable = false;
                }
            }
            else
            {
                buttons[2].gameObject.SetActive(false);
            }

            buttons[3].onClick.AddListener(delegate { CardManager.instance.RecallCard(cardVisual, unitToDisplay); });
            if (!ManaManager.instance.CanAfford(unitToDisplay.AttackPositions.Count) || !TurnManager.instance.isYourTurn
                || CardManager.instance.playerHand.Count >= CardManager.instance.maxCards)
            {
                buttons[3].interactable = false;
            }

        }

        AudioManager.singleton.PlaySound("scrollOpen", true);
        slideScript.SlideIn();

    }

    public void DestroyCurrentInfoInstance()
    {
        if (uiInfoPrefabInstance)
        {
            DestroyImmediate(uiInfoPrefabInstance);
        }
        
        if (cardInfoPrefabInstance)
        {
            DestroyImmediate(cardInfoPrefabInstance);
        }

        StopAllCoroutines();

        /*if (actionsInfoPrefabInstance)
        {
            DestroyImmediate(actionsInfoPrefabInstance);
        }*/
        
    }

    public void EnableControlsText()
    {
        switch (interactionState)
        {
            case InteractionState.Attacking:
                
                controlsText.gameObject.SetActive(true);
                controlsText.text = "Right Click to Rotate\nLeft Click to Confirm\nEsc to Cancel";


                break;
            
            case InteractionState.Defending:

                controlsText.gameObject.SetActive(true);
                controlsText.text = "Right Click to Rotate\nLeft Click to Confirm\nEsc to Cancel";

                break;
            
            
            case InteractionState.Moving:

                controlsText.gameObject.SetActive(true);
                controlsText.text = "Click on Tile to move\nEsc to Cancel";

                break;
            
            case InteractionState.None:

                controlsText.gameObject.SetActive(false);

                break;
        }
    }

    public IEnumerator DisplayEndGameScreen(Player.PlayerId id)
    {
        victoryText.gameObject.SetActive(true);
        
        if (id == Player.PlayerId.Player1)
        {
            victoryText.text = "Player 1 wins!";
        }
        else
        {
            victoryText.text = "Player 2 wins!";
        }

        yield return new WaitForSeconds(5.0f);
        
        GameManager.instance.DisconnectUser();

        yield return null;
    }
    
    private IEnumerator RotateGridShape(Transform[] children, List<Vector2Int> positions)
    {


        float angle = 90.0f;

        while (true)
        {
            
            yield return new WaitForSeconds(3.0f);

            for (int i = 0; i < positions.Count; i++)
            {

                float xpos = positions[i].x;
                float ypos = positions[i].y;

                float angleRad = angle * Mathf.Deg2Rad;

                float rotY;
                float rotX;
                rotX = (xpos - 1) * Mathf.Cos(angleRad) - (ypos - 1) * Mathf.Sin(angleRad) + 1;
                rotY = (xpos - 1) * Mathf.Sin(angleRad) + (ypos - 1) * Mathf.Cos(angleRad) + 1;


                Vector2Int newCoords = new Vector2Int(Mathf.RoundToInt(rotX), Mathf.RoundToInt(rotY));
                print(newCoords);
                positions[i] = newCoords;
            }

            for (int j = 15; j < 24; j++)
            {
                children[j].gameObject.SetActive(false);
            }

            foreach (Vector2Int coord in positions)
            {// Simple logic tree to find out which squares should show up...inelegant but robust enough...
                int x = coord.x;
                int y = coord.y;

                if (y == 0)
                {
                    if (x == 0)
                        children[15].gameObject.SetActive(true);
                    else if (x == 1)
                        children[16].gameObject.SetActive(true);
                    else if (x == 2)
                        children[17].gameObject.SetActive(true);
                }
                else if (y == 1)
                {
                    if (x == 0)
                        children[18].gameObject.SetActive(true);
                    else if (x == 1)
                        children[19].gameObject.SetActive(true);
                    else if (x == 2)
                        children[20].gameObject.SetActive(true);
                }
                else if (y == 2)
                {
                    if (x == 0)
                        children[21].gameObject.SetActive(true);
                    else if (x == 1)
                        children[22].gameObject.SetActive(true);
                    else if (x == 2)
                        children[23].gameObject.SetActive(true);
                }
            }
        }

        yield return null;
    }
}
          
