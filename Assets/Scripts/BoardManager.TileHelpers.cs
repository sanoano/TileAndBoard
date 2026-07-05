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
        IReadOnlyList<Unit> units,
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
        IReadOnlyList<Unit> units,
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
        IReadOnlyList<DamageInstance> damageInstances,
        int damageInstanceCount,
        IReadOnlyList<DefenseInstance> defenseInstances,
        int defenseInstanceCount)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                Vector2Int position = new Vector2Int(i, j);
                TilePressure pressure = CalculateTilePressure(position, damageOwner, defenseOwner,
                    damageInstances, damageInstanceCount, defenseInstances, defenseInstanceCount);

                tileColour tile = board.TileTransforms[i, j].GetComponent<tileColour>();
                tile.TileRecieveDamage(pressure.Damage, pressure.Defense);

                if (!pressure.HasPressure) continue;

                tile.TileRecieveSignal(pressure.NetDamage <= 0 ? 2 : 1, false);
            }
        }
    }

    private static TilePressure CalculateTilePressure(
        Vector2Int position,
        Player.PlayerId damageOwner,
        Player.PlayerId defenseOwner,
        IReadOnlyList<DamageInstance> damageInstances,
        int damageInstanceCount,
        IReadOnlyList<DefenseInstance> defenseInstances,
        int defenseInstanceCount)
    {
        int damage = 0;
        int defense = 0;
        bool hasPressure = false;

        for (int i = 0; i < damageInstanceCount; i++)
        {
            DamageInstance damageInstance = damageInstances[i];
            if (damageInstance.ID != damageOwner || !damageInstance.ContainsPosition(position)) continue;

            damage += damageInstance.Damage;
            hasPressure = true;
        }

        for (int i = 0; i < defenseInstanceCount; i++)
        {
            DefenseInstance defenseInstance = defenseInstances[i];
            if (defenseInstance.ID != defenseOwner || !defenseInstance.ContainsPosition(position)) continue;

            defense += defenseInstance.Defense;
            hasPressure = true;
        }

        return new TilePressure(damage, defense, hasPressure);
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

    private readonly struct TilePressure
    {
        public readonly int Damage;
        public readonly int Defense;
        public readonly bool HasPressure;
        public int NetDamage => Damage - Defense;

        public TilePressure(int damage, int defense, bool hasPressure)
        {
            Damage = damage;
            Defense = defense;
            HasPressure = hasPressure;
        }
    }
}
