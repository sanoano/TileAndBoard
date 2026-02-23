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
    
    [Header("Canvas")]
    [SerializeField] private GameObject Canvas;

    [Header("Card Info Panel")] 
    [SerializeField] private GameObject cardInfoPrefab;
    private GameObject cardInfoPrefabInstance;
    

    [Header("Settings Menu")] 
    public GameObject settingsMenu;

    public AudioMixer mixer;
    
    [Header("UI Element Offsets")]
    [SerializeField] private int infoXOffset;
    [SerializeField] private int infoYOffset;

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

        Vector3 spawnPosition = new Vector3(Input.mousePosition.x + infoXOffset, Input.mousePosition.y + infoYOffset);
        
        uiInfoPrefabInstance = Instantiate(uiInfoPrefab, spawnPosition, Quaternion.identity, Canvas.transform);

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

        Vector3 spawnPosition = new Vector3(Input.mousePosition.x + infoXOffset + 300 * 2, Input.mousePosition.y + infoYOffset);

        cardInfoPrefabInstance = Instantiate(cardInfoPrefab, spawnPosition, Quaternion.identity, Canvas.transform);
        
        Transform[] children = cardInfoPrefabInstance.GetComponentsInChildren<Transform>();

        TextMeshProUGUI cardInfoText = children[2].gameObject.GetComponent<TextMeshProUGUI>();

        cardInfoText.text = "";
        cardInfoText.text += $"Name: {unitToDisplay.Name}" + "\n";
        cardInfoText.text += $"Health: {unitToDisplay.Health}" + "\n";
        cardInfoText.text += $"Speed: {unitToDisplay.Movement}" + "\n";
        cardInfoText.text += $"Defense: {unitToDisplay.Defense}" + "\n";
        cardInfoText.text += $"Damage: {unitToDisplay.Damage}" + "\n";

        var panel = children[4].gameObject;

        if (unitToDisplay.ID == GameManager.instance.playerId)
        {
            var buttons = panel.GetComponentsInChildren<Button>();
            print(buttons[0].gameObject.name);
            if (unitToDisplay.Damage > 0)
            {
                buttons[0].onClick.AddListener(BoardManager.Instance.PrepareAttack);
            }
            else
            {
                buttons[0].gameObject.SetActive(false);
            }

            if (unitToDisplay.Defense > 0)
            {
                //placeholder
            }
            else
            {
                buttons[1].gameObject.SetActive(false);
            }

            if (unitToDisplay.Movement > 0)
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
        
    }
    
}
