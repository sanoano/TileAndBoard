using System;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Audio;
using UnityEngine.UI;


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

    [Header("Usernames")] 
    public TextMeshProUGUI player1Name;
    public TextMeshProUGUI player2Name;
    
    [Header("Settings Menu")] 
    public GameObject settingsMenu;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && interactionState == InteractionState.None && BoardManager.Instance.currentSelectedTileGameObject == null)
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
            DestroyCurrentInfoInstance();
        }

        tacticsText.text = $"Tactics Points: {TacticsManager.instance.currentTacticsPoints}";
        actionsText.text = $"Card Actions: {TacticsManager.instance.currentActions}";

        handAmount.text = $"{CardManager.instance.playerHand.Count} / {CardManager.instance.maxCards}";
    }

    void UpdateHealthDisplay()
    {
        player1HealthDisplay.text =
            $"Life Points: {BoardManager.Instance.player1Health} / {BoardManager.Instance.startingPlayerHealth}";
        
        player2HealthDisplay.text =
            $"Life Points: {BoardManager.Instance.player2Health} / {BoardManager.Instance.startingPlayerHealth}";
    }

    void UpdateCardAmountDisplay()
    {
        player1CardAmount.text =
            $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player1)} / {BoardManager.Instance.maxCardsPerPlayer}"
            ;
        
        player2CardAmount.text =
            $"Cards: {BoardManager.Instance.GetCardAmount(Player.PlayerId.Player2)} / {BoardManager.Instance.maxCardsPerPlayer}"
            ;
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
        
        Transform[] cardChildren = cardInfoPrefabInstance.GetComponentsInChildren<Transform>();

        TextMeshProUGUI cardInfoText = cardChildren[2].gameObject.GetComponent<TextMeshProUGUI>();

        cardInfoText.text = "";
        cardInfoText.text += $"Name: {unitToDisplay.Name}" + "\n";
        cardInfoText.text += $"Health: {unitToDisplay.Health}" + "\n";
        cardInfoText.text += $"Movement: {unitToDisplay.Movement}" + "\n";
        cardInfoText.text += $"Defense: {unitToDisplay.Defense}" + "\n";
        cardInfoText.text += $"Damage: {unitToDisplay.Damage}" + "\n";


        if (unitToDisplay.ID == GameManager.instance.playerId)
        {
            actionsInfoPrefabInstance = Instantiate(actionsInfoPrefab, actionsInfoPanelPos.transform.position,
                Quaternion.identity,
                actionsInfoPanelPos.transform);

            Transform[] actionChildren = actionsInfoPrefabInstance.GetComponentsInChildren<Transform>();

            var panel = actionChildren[2].gameObject;

            var buttons = panel.GetComponentsInChildren<Button>();
            print(buttons[0].gameObject.name);
            if (unitToDisplay.Damage > 0)
            {
                buttons[0].onClick.AddListener(BoardManager.Instance.PrepareAttack);
                if (TacticsManager.instance.currentActions <= 0 || !TurnManager.instance.isYourTurn ||
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
                if (TacticsManager.instance.currentActions <= 0 || !TurnManager.instance.isYourTurn ||
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
                if (TacticsManager.instance.currentActions <= 0 || !TurnManager.instance.isYourTurn ||
                    unitToDisplay.HasActed)
                {
                    buttons[2].interactable = false;
                }
            }
            else
            {
                buttons[2].gameObject.SetActive(false);
            }
            
            buttons[3].onClick.AddListener(delegate{CardManager.instance.RecallCard(cardVisual, unitToDisplay);});
            if (TacticsManager.instance.currentTacticsPoints <= 0 || !TurnManager.instance.isYourTurn)
            {
                buttons[3].interactable = false;
            }

        }

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

        if (actionsInfoPrefabInstance)
        {
            DestroyImmediate(actionsInfoPrefabInstance);
        }
        
    }

    public void EnableControlsText()
    {
        switch (interactionState)
        {
            case InteractionState.Attacking:
                
                controlsText.gameObject.SetActive(true);
                controlsText.text = "R to Rotate\nEnter to Confirm\nEsc to Cancel";


                break;
            
            case InteractionState.Defending:

                controlsText.gameObject.SetActive(true);
                controlsText.text = "R to Rotate\nEnter to Confirm\nEsc to Cancel";

                break;
            
            
            case InteractionState.Moving:

                controlsText.gameObject.SetActive(true);
                controlsText.text = "WASD/Arrows to Move\nEsc to Cancel";

                break;
            
            case InteractionState.None:

                controlsText.gameObject.SetActive(false);

                break;
        }
    }
    
}
