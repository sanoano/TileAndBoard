using UnityEngine;

public partial class BoardManager
{
    public bool AddUnit(Unit unit)
    {
        if (unitsCount >= unitsList.Length)
        {
            Debug.LogWarning($"Cannot add unit {unit.Name}; max unit capacity reached.");
            return false;
        }

        unitsList[unitsCount] = unit;
        unitsCount++;
        return true;
    }

    public int IndexOfUnit(Unit unit)
    {
        for (int i = 0; i < unitsCount; i++)
        {
            if (unitsList[i].Equals(unit)) return i;
        }

        return -1;
    }

    public void RemoveUnit(Unit unit)
    {
        RemoveUnitAt(IndexOfUnit(unit));
    }

    public void RemoveUnitAt(int index)
    {
        if (index < 0 || index >= unitsCount) return;

        for (int i = index; i < unitsCount - 1; i++)
        {
            unitsList[i] = unitsList[i + 1];
        }

        unitsCount--;
        unitsList[unitsCount] = default;

        if (currentlySelectedUnitIndex == index)
        {
            currentlySelectedUnit = default;
            currentlySelectedUnitIndex = -1;
        }
        else if (currentlySelectedUnitIndex > index)
        {
            currentlySelectedUnitIndex--;
        }
    }

    private bool AddDamageInstance(DamageInstance instance)
    {
        if (damageInstanceCount >= damageInstances.Length)
        {
            Debug.LogWarning($"Cannot add damage instance {instance.Name}; max damage instance capacity reached.");
            return false;
        }

        damageInstances[damageInstanceCount] = instance;
        damageInstanceCount++;
        return true;
    }

    private bool AddDefenseInstance(DefenseInstance instance)
    {
        if (defenseInstanceCount >= defenseInstances.Length)
        {
            Debug.LogWarning($"Cannot add defense instance {instance.Name}; max defense instance capacity reached.");
            return false;
        }

        defenseInstances[defenseInstanceCount] = instance;
        defenseInstanceCount++;
        return true;
    }

    private void RemoveDamageInstanceAt(int index)
    {
        if (index < 0 || index >= damageInstanceCount) return;

        for (int i = index; i < damageInstanceCount - 1; i++)
        {
            damageInstances[i] = damageInstances[i + 1];
        }

        damageInstanceCount--;
        damageInstances[damageInstanceCount] = default;
    }

    private void RemoveDefenseInstanceAt(int index)
    {
        if (index < 0 || index >= defenseInstanceCount) return;

        for (int i = index; i < defenseInstanceCount - 1; i++)
        {
            defenseInstances[i] = defenseInstances[i + 1];
        }

        defenseInstanceCount--;
        defenseInstances[defenseInstanceCount] = default;
    }

    private void RemoveDamageInstancesForPlayer(Player.PlayerId id)
    {
        for (int i = damageInstanceCount - 1; i >= 0; i--)
        {
            if (damageInstances[i].ID == id)
            {
                RemoveDamageInstanceAt(i);
            }
        }
    }

    private void RemoveDefenseInstancesForPlayer(Player.PlayerId id)
    {
        for (int i = defenseInstanceCount - 1; i >= 0; i--)
        {
            if (defenseInstances[i].ID == id)
            {
                RemoveDefenseInstanceAt(i);
            }
        }
    }
}
