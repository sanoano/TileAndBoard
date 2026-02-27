using System;
using UnityEngine;
using System.Collections.Generic;
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
    [SerializeField] private GameObject Canvas;
    

    [Header("Card Info Panel")] 
    [SerializeField] private GameObject cardInfoPrefab;
    private GameObject cardInfoPrefabInstance;
    [SerializeField] private GameObject CardInfoPanelPos;

    [Header("Actions Info Panel")]
    [SerializeField] private GameObject actionsInfoPrefab;
    private GameObject actionsInfoPrefabInstance;
    [SerializeField] private GameObject actionsInfoPanelPos;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && interactionState == InteractionState.None)
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
            DestroyCurrentInfoInstance();
        }
    }

    public void CreateInfoPanel(Vector2Int position, Player.PlayerId playerId)
    {
        if (uiInfoPrefabInstance)
        {
            Destroy(uiInfoPrefabInstance);
        }
        
        uiInfoPrefabInstance = Instantiate(uiInfoPrefab, Vector3.zero, Quaternion.identity, InfoPanelPos.transform);

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
                unitFound = true;
                break;
            }
        }

        if (!unitFound) return;

        cardInfoPrefabInstance = Instantiate(cardInfoPrefab, Vector3.zero, Quaternion.identity, CardInfoPanelPos.transform);

        actionsInfoPrefabInstance = Instantiate(actionsInfoPrefab, Vector3.zero, Quaternion.identity,
            actionsInfoPanelPos.transform);
        
        Transform[] actionChildren = actionsInfoPrefabInstance.GetComponentsInChildren<Transform>();
        Transform[] cardChildren = cardInfoPrefabInstance.GetComponentsInChildren<Transform>();

        TextMeshProUGUI cardInfoText = cardChildren[2].gameObject.GetComponent<TextMeshProUGUI>();

        cardInfoText.text = "";
        cardInfoText.text += $"Name: {unitToDisplay.Name}" + "\n";
        cardInfoText.text += $"Health: {unitToDisplay.Health}" + "\n";
        cardInfoText.text += $"Speed: {unitToDisplay.Movement}" + "\n";
        cardInfoText.text += $"Defense: {unitToDisplay.Defense}" + "\n";
        cardInfoText.text += $"Damage: {unitToDisplay.Damage}" + "\n";
        
        var panel = actionChildren[2].gameObject;

        if (unitToDisplay.ID == GameManager.instance.playerId && unitToDisplay.HasActed == false)
        {
            var buttons = panel.GetComponentsInChildren<Button>();
            print(buttons[0].gameObject.name);
            if (unitToDisplay.Damage > 0 && TurnManager.instance.isYourTurn)
            {
                buttons[0].onClick.AddListener(BoardManager.Instance.PrepareAttack);
            }
            else
            {
                buttons[0].gameObject.SetActive(false);
            }

            if (unitToDisplay.Defense > 0 && TurnManager.instance.isYourTurn)
            {
                buttons[1].onClick.AddListener(BoardManager.Instance.PrepareDefense);
            }
            else
            {
                buttons[1].gameObject.SetActive(false);
            }

            if (unitToDisplay.Movement > 0 && TurnManager.instance.isYourTurn)
            { 
                //placeholder
            }
            else
            {
                buttons[2].gameObject.SetActive(false);
            }
        }
        else
        {
            panel.SetActive(false);
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
    
}
