using System;
using UnityEngine;

public class TacticsManager : MonoBehaviour
{
    public static TacticsManager instance;

    public int currentTacticsPoints;

    [Header("Parameters")] 
    [SerializeField] private int maxTacticPoints;
    [SerializeField] private int startingTacticPoints;
    public int tacticsPointsPerTurn;
    
    
    
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

        currentTacticsPoints = startingTacticPoints;
        
    }

    public void AddTacticsPoints(int amount)
    {

        var temp = currentTacticsPoints + amount;

        if (temp > maxTacticPoints)
        {
            currentTacticsPoints = maxTacticPoints;
        }
        else
        {
            currentTacticsPoints = temp;
        }


    }

    public void RemoveTacticsPoints(int amount)
    {

        var temp = currentTacticsPoints - amount;

        currentTacticsPoints = temp;

    }

    public bool CanAfford(int amount)
    {
        if (currentTacticsPoints - amount < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
}
