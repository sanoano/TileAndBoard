using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class BoardManager
{
    [Serializable]
    public struct PlayerBoard
    {
        public GameObject[,] TileTransforms;
        public GameObject[,] Visuals;

        public PlayerBoard(GameObject[,] tileTransforms, GameObject[,] visuals)
        {
            TileTransforms = tileTransforms;
            Visuals = visuals;
        }
    }

    [Serializable]
    public struct DamageInstance : INetworkSerializable
    {
        public string Name;
        public Player.PlayerId ID;
        public int Damage;
        public Vector2Int[] Positions;
        public int PositionCount;

        public DamageInstance(string name, Player.PlayerId id, int damage, IList<Vector2Int> positions)
        {
            Name = name;
            ID = id;
            Damage = damage;
            Positions = new Vector2Int[MaxAttackPositions];
            PositionCount = Mathf.Min(positions.Count, MaxAttackPositions);

            for (int i = 0; i < PositionCount; i++)
            {
                Positions[i] = positions[i];
            }
        }

        public bool ContainsPosition(Vector2Int position)
        {
            for (int i = 0; i < PositionCount; i++)
            {
                if (Positions[i] == position) return true;
            }

            return false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref Positions);
            serializer.SerializeValue(ref PositionCount);
        }
    }

    [Serializable]
    public struct DefenseInstance : INetworkSerializable
    {
        public string Name;
        public Player.PlayerId ID;
        public int Defense;
        public Vector2Int[] Positions;
        public int PositionCount;

        public DefenseInstance(string name, Player.PlayerId id, int defense, IList<Vector2Int> positions)
        {
            Name = name;
            ID = id;
            Defense = defense;
            Positions = new Vector2Int[MaxAttackPositions];
            PositionCount = Mathf.Min(positions.Count, MaxAttackPositions);

            for (int i = 0; i < PositionCount; i++)
            {
                Positions[i] = positions[i];
            }
        }

        public bool ContainsPosition(Vector2Int position)
        {
            for (int i = 0; i < PositionCount; i++)
            {
                if (Positions[i] == position) return true;
            }

            return false;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Defense);
            serializer.SerializeValue(ref Positions);
            serializer.SerializeValue(ref PositionCount);
        }
    }

    [Serializable]
    public struct Unit : INetworkSerializable
    {
        public string Name;
        public int CardID;
        public int Cost;
        public Player.PlayerId ID;
        public int Health;
        public int Damage;
        public int Defense;
        public int Movement;
        public Vector2Int[] AttackPositions;
        public int AttackPositionCount;
        public Vector2Int Position;
        public bool HasActed;

        public Unit(string name, int cardID, int cost, Player.PlayerId id, int health, int damage, int movement,
            IList<Vector2Int> attackPositions, Vector2Int position, int defense, bool hasActed)
        {
            Name = name;
            CardID = cardID;
            ID = id;
            Health = health;
            Damage = damage;
            Movement = movement;
            AttackPositions = new Vector2Int[MaxAttackPositions];
            AttackPositionCount = Mathf.Min(attackPositions.Count, MaxAttackPositions);

            for (int i = 0; i < AttackPositionCount; i++)
            {
                AttackPositions[i] = attackPositions[i];
            }

            Position = position;
            Defense = defense;
            HasActed = hasActed;
            Cost = cost;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref CardID);
            serializer.SerializeValue(ref Cost);
            serializer.SerializeValue(ref ID);
            serializer.SerializeValue(ref Health);
            serializer.SerializeValue(ref Damage);
            serializer.SerializeValue(ref Defense);
            serializer.SerializeValue(ref Movement);
            serializer.SerializeValue(ref AttackPositions);
            serializer.SerializeValue(ref AttackPositionCount);
            serializer.SerializeValue(ref Position);
            serializer.SerializeValue(ref HasActed);
        }
    }
}
