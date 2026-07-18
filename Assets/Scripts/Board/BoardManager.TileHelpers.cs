using System.Collections.Generic;
using UnityEngine;

public partial class BoardManager
{
    private static readonly Vector2Int InvalidTile = new Vector2Int(-1, -1);
    private static readonly Vector2Int[] AdjacentOffsets =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private static Vector2Int[] GetAdjacentTiles(
        Vector2Int position,
        Unit[] units,
        int unitCount,
        Player.PlayerId playerId)
    {
        Vector2Int[] adjacent = new Vector2Int[AdjacentOffsets.Length];

        for (int i = 0; i < AdjacentOffsets.Length; i++)
        {
            Vector2Int candidate = position + AdjacentOffsets[i];
            adjacent[i] = IsTileInBounds(candidate) && !HasUnitAt(candidate, units, unitCount, playerId)
                ? candidate
                : InvalidTile;
        }

        return adjacent;
    }

    private static bool IsTileInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x <= 2 && position.y >= 0 && position.y <= 2;
    }

    private static bool HasUnitAt(
        Vector2Int position,
        Unit[] units,
        int unitCount,
        Player.PlayerId playerId)
    {
        for (int i = 0; i < unitCount; i++)
        {
            Unit unit = units[i];
            if (unit.ID == playerId && unit.Position == position)
            {
                return true;
            }
        }

        return false;
    }

    private static void UpdateBoardTileVisuals(
        PlayerBoard board,
        Player.PlayerId damageOwner,
        Player.PlayerId defenseOwner,
        DamageInstance[] damageInstances,
        int damageInstanceCount,
        DefenseInstance[] defenseInstances,
        int defenseInstanceCount)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int position = new Vector2Int(i, j);
                TotalDamage totalDamage = CalculateTotalDamage(position, damageOwner, defenseOwner,
                    damageInstances, damageInstanceCount, defenseInstances, defenseInstanceCount);

                tileColour tile = board.TileTransforms[i, j].GetComponent<tileColour>();
                tile.TileRecieveDamage(totalDamage.IncomingDamage, totalDamage.Defense);

                if (!totalDamage.HasAttackOrDefense) continue;

                tile.TileRecieveSignal(totalDamage.NetDamage <= 0 ? 2 : 1, false);
            }
        }
    }

    private static TotalDamage CalculateTotalDamage(
        Vector2Int position,
        Player.PlayerId damageOwner,
        Player.PlayerId defenseOwner,
        DamageInstance[] damageInstances,
        int damageInstanceCount,
        DefenseInstance[] defenseInstances,
        int defenseInstanceCount)
    {
        int damage = 0;
        int defense = 0;
        bool hasAttackOrDefense = false;

        for (int i = 0; i < damageInstanceCount; i++)
        {
            DamageInstance damageInstance = damageInstances[i];
            if (damageInstance.ID != damageOwner || !damageInstance.ContainsPosition(position)) continue;

            damage += damageInstance.Damage;
            hasAttackOrDefense = true;
        }

        for (int i = 0; i < defenseInstanceCount; i++)
        {
            DefenseInstance defenseInstance = defenseInstances[i];
            if (defenseInstance.ID != defenseOwner || !defenseInstance.ContainsPosition(position)) continue;

            defense += defenseInstance.Defense;
            hasAttackOrDefense = true;
        }

        return new TotalDamage(damage, defense, hasAttackOrDefense);
    }

    public Vector2Int CoordinatesOf<T>(T[,] matrix, T value)
    {
        return FindCoordinates(matrix, value);
    }

    private static Vector2Int FindCoordinates<T>(T[,] matrix, T value)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (comparer.Equals(matrix[x, y], value)) return new Vector2Int(x, y);
            }
        }

        return InvalidTile;
    }

    private readonly struct TotalDamage
    {
        public readonly int IncomingDamage;
        public readonly int Defense;
        public readonly bool HasAttackOrDefense;
        public int NetDamage => IncomingDamage - Defense;
        public int ClampedNetDamage => Mathf.Max(0, NetDamage);

        public TotalDamage(int incomingDamage, int defense, bool hasAttackOrDefense)
        {
            IncomingDamage = incomingDamage;
            Defense = defense;
            HasAttackOrDefense = hasAttackOrDefense;
        }
    }
}
