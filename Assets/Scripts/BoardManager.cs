using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

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
        
    public struct DamageInstance
    {
        public String Name;
        public Player.PlayerId ID;
        public int Damage;
        public List<Tuple<int, int>> Positions;

        public DamageInstance(string name, Player.PlayerId id, int damage, List<Tuple<int, int>> positions)
        {
            Name = name;
            ID = id;
            Damage = damage;
            Positions = positions;
        }
    }

    public struct DefenseInstance
    {
        public String Name;
        public Player.PlayerId ID;
        public int Defense;
        public List<Tuple<int, int>> Positions;

        public DefenseInstance(string name, Player.PlayerId id, int defense, List<Tuple<int, int>> positions)
        {
            Name = name;
            ID = id;
            Defense = defense;
            Positions = positions;
        }
    }

    public struct Unit
    {
        public String Name;
        public Player.PlayerId ID;
        public int Health;
        public int Damage;
        public int Movement;
        public List<Tuple<int, int>> AttackPositions;
        public Tuple<int, int> Position;

        public Unit(string name, Player.PlayerId id, int health, int damage, int movement, List<Tuple<int, int>> attackPositions, Tuple<int, int> position)
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

    public List<Unit> unitsList;
    public List<DamageInstance> damageInstances;
    public List<DefenseInstance> defenseInstances;

    [Header("Board References")] 
    [SerializeField]
    private GameObject player1BoardGameObject;
    [SerializeField] 
    private GameObject player2BoardGameObject;

    [SerializeField] private LayerMask playerSpecificLayer;
    [SerializeField] private LayerMask interactionLayers;
    private Camera cam;

    public Tuple<int, int> CurrentSelectedTile;
    public GameObject currentSelectedTileGameObject;

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
                    CurrentSelectedTile = null;
                    UIManager.Instance.DestroyCurrentInfoInstance();
                }
                else
                {
                    //If the tile is different, do this.
                    currentSelectedTileGameObject = hit.transform.gameObject;
                
                    CurrentSelectedTile = CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);
                    if (Equals(CurrentSelectedTile, new Tuple<int, int>(-1, -1)))
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
                    CurrentSelectedTile = null;
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
