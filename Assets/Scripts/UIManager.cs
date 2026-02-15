using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.Audio;


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    
    [Header("Info Panel")]
    [SerializeField] private GameObject uiInfoPrefab;
    [SerializeField] private GameObject uiInfoPrefabInstance;
    [SerializeField] private GameObject Canvas;

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
        
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            settingsMenu.SetActive(!settingsMenu.activeSelf);
            DestroyCurrentInfoInstance();
        }
    }

    public void CreateInfoPanel(Vector2Int position)
    {
        if (uiInfoPrefabInstance)
        {
            DestroyImmediate(uiInfoPrefabInstance);
        }

        Vector3 spawnPosition = new Vector3(Input.mousePosition.x + infoXOffset, Input.mousePosition.y + infoYOffset);
        
        uiInfoPrefabInstance = Instantiate(uiInfoPrefab, spawnPosition, Quaternion.identity, Canvas.transform);

        Transform[] children = uiInfoPrefabInstance.GetComponentsInChildren<Transform>();

        TextMeshProUGUI damageText = children[2].gameObject.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI defenseText = children[4].gameObject.GetComponent<TextMeshProUGUI>();;
        TextMeshProUGUI totalText = children[6].gameObject.GetComponent<TextMeshProUGUI>();;

        int damageTotal = 0;
        int defenseTotal = 0;

        if (BoardManager.Instance.damageInstances.Count != 0)
        {
            foreach (BoardManager.DamageInstance thing in BoardManager.Instance.damageInstances)
            {
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
        else
        {
            damageText.text = "None";
        }
        
        if (BoardManager.Instance.defenseInstances.Count != 0)
        {
            foreach (BoardManager.DefenseInstance thing in BoardManager.Instance.defenseInstances)
            {
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
        else
        {
            defenseText.text = "None";
        }

        int total = damageTotal - defenseTotal;
        if (total < 0) total = 0;

        totalText.text = total.ToString();


    }

    public void DestroyCurrentInfoInstance()
    {
        if (uiInfoPrefabInstance)
        {
            DestroyImmediate(uiInfoPrefabInstance);
        }
        
    }
    
}
