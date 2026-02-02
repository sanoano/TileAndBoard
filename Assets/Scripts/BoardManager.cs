using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;

public class BoardManager : NetworkBehaviour
{

    public static BoardManager Instance;
    
    struct PlayerBoard
    {

        public GameObject[,] TileTransforms;
        public GameObject[,] Visuals;
        
        public PlayerBoard(GameObject[,] tileTransforms, GameObject[,] visuals)
        {
            TileTransforms = tileTransforms;
            Visuals = visuals;
        }
        
    }
        
    struct DamageInstance
    {
        public String Names;
        public Player.PlayerId ID;
        public int Damage;
        public List<Tuple<int, int>> Positions;

        public DamageInstance(string names, Player.PlayerId id, int damage, List<Tuple<int, int>> positions)
        {
            Names = names;
            ID = id;
            Damage = damage;
            Positions = positions;
        }
    }

    struct DefenseInstance
    {
        public String Names;
        public Player.PlayerId ID;
        public int Defense;
        public List<Tuple<int, int>> Positions;

        public DefenseInstance(string names, Player.PlayerId id, int defense, List<Tuple<int, int>> positions)
        {
            Names = names;
            ID = id;
            Defense = defense;
            Positions = positions;
        }
    }

    struct Unit
    {
        public String Names;
        public Player.PlayerId ID;
        public int Health;
        public int Damage;
        public int Movement;
        public List<Tuple<int, int>> AttackPositions;
        public Tuple<int, int> Position;

        public Unit(string names, Player.PlayerId id, int health, int damage, int movement, List<Tuple<int, int>> attackPositions, Tuple<int, int> position)
        {
            Names = names;
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

    private List<Unit> unitsList;
    private List<DamageInstance> damageInstances;
    private List<DefenseInstance> defenseInstances;

    [Header("Board References")] 
    [SerializeField]
    private GameObject player1BoardGameObject;
    [SerializeField] 
    private GameObject player2BoardGameObject;

    [SerializeField] private LayerMask tileLayer;
    private Camera cam;

    public Tuple<int, int> CurrentSelectedTile;
    public GameObject currentSelectedTileGameObject;

    private InputAction select;

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
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            tileLayer= LayerMask.GetMask("Player1Tile");
        }
        else
        {
            tileLayer= LayerMask.GetMask("Player2Tile");
        }
        
        print(GameManager.instance.playerId);
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayer))
            {
                GameObject[,] arrayToCheck;
                if (GameManager.instance.playerId == Player.PlayerId.Player1)
                {
                    arrayToCheck = player1Board.TileTransforms;
                }
                else
                {
                    arrayToCheck = player2Board.TileTransforms;
                }
                
                if(currentSelectedTileGameObject) 
                {
                    DestroyImmediate(currentSelectedTileGameObject.GetComponent<Outline>());
                }

                if (currentSelectedTileGameObject == hit.transform.gameObject)
                {
                    currentSelectedTileGameObject = null;
                    CurrentSelectedTile = null;
                }
                else
                {
                    currentSelectedTileGameObject = hit.transform.gameObject;
                
                    CurrentSelectedTile = CoordinatesOf<GameObject>(arrayToCheck, hit.transform.gameObject);

                    if (!hit.transform.gameObject.GetComponent<Outline>())
                    {

                        Outline outline = hit.transform.gameObject.AddComponent<Outline>();
                        outline.OutlineColor = Color.limeGreen;
                        outline.OutlineWidth = 6;

                    }
                }

                
            }
            
        }
    }
    
    // Source - https://stackoverflow.com/a
    // Posted by Dan Tao
    // Retrieved 2026-01-29, License - CC BY-SA 2.5

    public static Tuple<int, int> CoordinatesOf<T>(T[,] matrix, T value)
    {
        int w = matrix.GetLength(0); // width
        int h = matrix.GetLength(1); // height

        for (int x = 0; x < w; ++x)
        {
            for (int y = 0; y < h; ++y)
            {
                if (matrix[x, y].Equals(value))
                    return Tuple.Create(x, y);
            }
        }

        return Tuple.Create(-1, -1);
    }

}
