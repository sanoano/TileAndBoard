using System;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;


public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    
    [SerializeField] private GameObject uiInfoPrefab;
    [SerializeField] private GameObject uiInfoPrefabInstance;
    [SerializeField] private GameObject Canvas;

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

    public void CreateInfoPanel(Tuple<int, int> position)
    {
        if (uiInfoPrefabInstance)
        {
            DestroyImmediate(uiInfoPrefabInstance);
        }
        
        uiInfoPrefabInstance = Instantiate(uiInfoPrefab, Input.mousePosition, Quaternion.identity, Canvas.transform);

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
                foreach (Tuple<int, int> damagePosition in thing.Positions)
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
                foreach (Tuple<int, int> defensePosition in thing.Positions)
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
