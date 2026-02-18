using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Tweens;
using UnityEditor.Experimental.GraphView;

public class BoardManager : NetworkBehaviour
{

    public static BoardManager Instance;
    
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
    public struct DamageInstance
    {
        public String Name;
        public Player.PlayerId ID;
        public int Damage;
        public List<Vector2Int> Positions;

        public DamageInstance(string name, Player.PlayerId id, int damage, List<Vector2Int> positions)
        {
            Name = name;
            ID = id;
            Damage = damage;
            Positions = positions;
        }
    }
    
    [Serializable]
    public struct DefenseInstance
    {
        public String Name;
        public Player.PlayerId ID;
        public int Defense;
        public List<Vector2Int> Positions;

        public DefenseInstance(string name, Player.PlayerId id, int defense, List<Vector2Int> positions)
        {
            Name = name;
            ID = id;
            Defense = defense;
            Positions = positions;
        }
    }

    [Serializable]
    public struct Unit
    {
        public String Name;
        public Player.PlayerId ID;
        public int Health;
        public int Damage;
        public int Movement;
        public List<Vector2Int> AttackPositions;
        public Vector2Int Position;

        public Unit(string name, Player.PlayerId id, int health, int damage, int movement, List<Vector2Int> attackPositions, Vector2Int position)
        {
            Name = name;
            ID = id;
            Health = health;
            Damage = damage;
            Movement = movement;
            AttackPositions = attackPositions;
            Position = position;
        }
    }

    private PlayerBoard player1Board;
    private PlayerBoard player2Board;

    public PlayerBoard localBoard;

    [Header("Lists")]
    public List<Unit> unitsList;
    public List<DamageInstance> damageInstances;
    public List<DefenseInstance> defenseInstances;

    [Header("Board References")] 
    [SerializeField]
    private GameObject player1BoardGameObject;
    [SerializeField] 
    private GameObject player2BoardGameObject;

    [Header("Layers")]
    public LayerMask playerSpecificLayer;
    [SerializeField] private LayerMask interactionLayers;
    private Camera cam;

    [Header("Selected Tile")]
    public Vector2Int CurrentSelectedTile;
    public GameObject currentSelectedTileGameObject;

    [Header("Parameters")] 
    [SerializeField] private float placeAnimationTime;

    private InputAction select;

    private OrbitCamera cameraInfo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        select = InputSystem.actions.FindAction("Click");

        cam = Camera.main;

        cameraInfo = cam.GetComponent<OrbitCamera>();

        interactionLayers = LayerMask.GetMask("Player1Tile", "Player2Tile");
    }

    private void Start()
    {
        player1Board = new PlayerBoard(new GameObject[3,3],
            new GameObject[3,3]
        );
        
        player2Board = new PlayerBoard(new GameObject[3,3],
            new GameObject[3,3]
        );
        

        int childIndex = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                player1Board.TileTransforms[i,j] =
                    player1BoardGameObject.GetComponentsInChildren<Transform>()[childIndex].gameObject;
                
                player2Board.TileTransforms[i,j] =
                    player2BoardGameObject.GetComponentsInChildren<Transform>()[childIndex].gameObject;

                Debug.Log(player1BoardGameObject.GetComponentsInChildren<Transform>()[childIndex]);

                childIndex++;
            }

            
        }

        unitsList = new List<Unit>();
        damageInstances = new List<DamageInstance>();
        defenseInstances = new List<DefenseInstance>();

        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            localBoard = player1Board;
        }
        else
        {
            localBoard = player2Board;
        }
        

    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            playerSpecificLayer= LayerMask.GetMask("Player1Tile");
        }
        else
        {
            playerSpecificLayer= LayerMask.GetMask("Player2Tile");
        }
        
        print(GameManager.instance.playerId);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (UIManager.Instance.settingsMenu.activeSelf) return;
            if (cameraInfo.cameraState == OrbitCamera.CameraState.Free) return;
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            //If we hit something.
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionLayers))
            {
                
                //If there was a previously selected tile with an outline, remove it.
                if(currentSelectedTileGameObject) 
                {
                    DestroyImmediate(currentSelectedTileGameObject.GetComponent<Outline>());
                }
                
                //If the same tile was clicked again, deselect it.
                if (currentSelectedTileGameObject == hit.transform.gameObject)
                {
                    currentSelectedTileGameObject = null;
                    CurrentSelectedTile = new Vector2Int(-1, -1);
                    UIManager.Instance.DestroyCurrentInfoInstance();
                }
                else
                {
                    //If the tile is different, do this.
                    currentSelectedTileGameObject = hit.transform.gameObject;
                
                    CurrentSelectedTile = CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);
                    if (Equals(CurrentSelectedTile, new Vector2Int(-1, -1)))
                    {
                        CurrentSelectedTile = CoordinatesOf<GameObject>(player2Board.TileTransforms, hit.transform.gameObject);
                    }
                    
                    UIManager.Instance.CreateInfoPanel(CurrentSelectedTile);

                    if (!hit.transform.gameObject.GetComponent<Outline>())
                    {

                        Outline outline = hit.transform.gameObject.AddComponent<Outline>();
                        outline.OutlineColor = Color.limeGreen;
                        outline.OutlineWidth = 6;

                    }
                }

                
            }
            else
            {
                //If the raycast hit nothing at all.
                UIManager.Instance.DestroyCurrentInfoInstance();
                if (currentSelectedTileGameObject)
                {
                    DestroyImmediate(currentSelectedTileGameObject.GetComponent<Outline>());
                    currentSelectedTileGameObject = null;
                    CurrentSelectedTile = new Vector2Int(-1, -1);
                }
                
            }
            
        }
    }

    public void PlaceCard(GameObject cardVisual, CardDeck.CardData cardData, GameObject tile)
    {

        Vector2Int coordinates = CoordinatesOf<GameObject>(localBoard.TileTransforms, tile);
        
        Unit unit = new Unit(
            name: cardData.Name,
            id: GameManager.instance.playerId,
            health: cardData.Health,
            damage: cardData.Damage,
            movement: cardData.Speed,
            attackPositions: cardData.Range,
            position: coordinates
            );
        
        unitsList.Add(unit);
        
        CardManager.instance.RemoveCard(cardVisual);
        CardManager.instance.playerHandVisuals.Remove(cardVisual);
        CardManager.instance.playerHand.Remove(cardData);
        
        localBoard.Visuals[coordinates.x, coordinates.y] = cardVisual;
        
        cardVisual.transform.parent = tile.transform;

        Vector3 position; 

        Quaternion rotation;

        if (unit.ID == Player.PlayerId.Player1)
        {
            position = new Vector3(0, tile.transform.localPosition.y + 1, 0);
            rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
        else
        {
            position = new Vector3(0, tile.transform.localPosition.y - 1, 0);
            rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        }

        Vector3 scale = new Vector3(1, 0.8f, 0);

        var positionTween = new LocalPositionTween()
        {
            to = position,
            duration = placeAnimationTime,
            easeType = EaseType.CubicInOut
        };

        var rotationTween = new RotationTween()
        {
            to = rotation,
            duration = placeAnimationTime,
            easeType = EaseType.CubicInOut
        };

        var scaleTween = new LocalScaleTween()
        {
            to = scale,
            duration = placeAnimationTime,
            easeType = EaseType.CubicInOut
        };

        cardVisual.AddTween(positionTween, rotationTween, scaleTween);

        cardVisual.GetComponent<CardDrag>().isPlaced = true;
        
        foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientIds == NetworkManager.LocalClientId) continue;
            PlaceCardRpc(cardData.ID, unit.ID, unit.Position, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
        }
        
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void PlaceCardRpc(int ID, Player.PlayerId playerId, Vector2Int coordinates, RpcParams rpcParams = default)
    {

        CardDeck cardList = Resources.Load<CardDeck>("Data/MasterList");

        CardDeck.CardData cardData = new CardDeck.CardData();
        
        foreach (CardDeck.CardData card in cardList.Cards)
        {
            if (card.ID == ID)
            {
                cardData = card;
            }
        }
        
        Unit unit = new Unit(
            name: cardData.Name,
            id: playerId,
            health: cardData.Health,
            damage: cardData.Damage,
            movement: cardData.Speed,
            attackPositions: cardData.Range,
            position: coordinates
        );
        
        unitsList.Add(unit);
        
        GameObject tile;
        
        if (unit.ID == Player.PlayerId.Player1)
        {
            tile = player1Board.TileTransforms[unit.Position.x, unit.Position.y];
        }
        else
        {
            tile = player2Board.TileTransforms[unit.Position.x, unit.Position.y];
        }
        
        var cardVisual = Instantiate(CardManager.instance.cardVisualPrefab,
            Vector3.zero,
            Quaternion.identity,
            tile.transform);

        cardVisual.GetComponent<CardDrag>().isPlaced = true;
        
        Vector3 position; 
        
        Quaternion rotation;
        
        if (unit.ID == Player.PlayerId.Player1)
        {
            player1Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
            rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            position = new Vector3(0, tile.transform.localPosition.y + 1, 0);
        }
        else
        {
            player2Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
            rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            position = new Vector3(0, tile.transform.localPosition.y - 1, 0);
        }
        
        Vector3 scale = new Vector3(1, 0.8f, 0);
        
        cardVisual.transform.localPosition = position;
        cardVisual.transform.rotation = rotation;
        cardVisual.transform.localScale = scale;
    }
    
    
    
    // Source - https://stackoverflow.com/a
    // Posted by Dan Tao
    // Retrieved 2026-01-29, License - CC BY-SA 2.5

    public Vector2Int CoordinatesOf<T>(T[,] matrix, T value)
    {
        int w = matrix.GetLength(0); // width
        int h = matrix.GetLength(1); // height

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                if (matrix[x, y].Equals(value))
                    return new Vector2Int(x, y);
            }
        }

        return new Vector2Int(-1, -1);
    }

    // public static List<Vector2Int> convertToTupleList(List<int[]> input)
    // {
    //
    //     List<Vector2Int> output = new List<Vector2Int>();
    //
    //     foreach (int[] array in input)
    //     {
    //         Vector2Int tuple = new Vector2Int(array[0], array[1]);
    //         Debug.Log(tuple);
    //         output.Add(tuple);
    //     }
    //
    //     return output;
    // }

}
