using System;
using UnityEngine;

public class ManaManager : MonoBehaviour
{
    public static ManaManager instance;

    public int currentManaPoints;

    [Header("Parameters")] 
    [SerializeField] private int maxMana;
    [SerializeField] private int startingMana;
    
    
    
    
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

        currentManaPoints = startingMana;

    }

    public void AddManaPoints(int amount)
    {

        var temp = currentManaPoints + amount;

        if (temp > maxMana)
        {
            currentManaPoints = maxMana;
        }
        else
        {
            currentManaPoints = temp;
        }


    }

    public void RemoveManaPoints(int amount)
    {

        var temp = currentManaPoints - amount;

        currentManaPoints = temp;

    }

    public bool CanAfford(int amount)
    {
        if (currentManaPoints - amount < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
}
