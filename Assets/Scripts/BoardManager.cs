using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Tweens;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using UnityEngine.SocialPlatforms;

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
    public class Unit
    {
        public String Name;
        public Player.PlayerId ID;
        public int Health;
        public int Damage;
        public int Defense;
        public int Movement;
        public List<Vector2Int> AttackPositions;
        public Vector2Int Position;
        public bool HasActed;

        public Unit(string name, Player.PlayerId id, int health, int damage, int movement,
            List<Vector2Int> attackPositions, Vector2Int position, int defense, bool hasActed)
        {
            Name = name;
            ID = id;
            Health = health;
            Damage = damage;
            Movement = movement;
            AttackPositions = attackPositions;
            Position = position;
            Defense = defense;
            HasActed = hasActed;
        }
    }

    private PlayerBoard player1Board;
    private PlayerBoard player2Board;

    public PlayerBoard localBoard;
    public PlayerBoard enemyBoard;

    [Header("Lists")] public List<Unit> unitsList;
    public List<DamageInstance> damageInstances;
    public List<DefenseInstance> defenseInstances;

    [Header("Board References")] 
    [SerializeField] private GameObject player1BoardGameObject;
    [SerializeField] private GameObject player2BoardGameObject;
    [SerializeField] private GameObject islandBottom1;
    [SerializeField] private GameObject islandBottom2;

    public bool board1DoOnce = false;
    public bool board2DoOnce = false;
    private int criticalHealthThreshold;

    private float speed1 = 0;
    private float speed2 = 0;
    private const float maxSpeed = 50.0f;
    private const float acceleration = 5.0f;

    [HideInInspector] public bool attackInProgress;
    

    [Header("Layers")] public LayerMask playerSpecificLayer;
    [SerializeField] private LayerMask interactionLayers;
    private Camera cam;

    [Header("Selected Tile")] public Vector2Int CurrentSelectedTile;
    public GameObject currentSelectedTileGameObject;
    private Unit currentlySelectedUnit;

    [Header("Parameters")] 
    [SerializeField] private float placeAnimationTime;
    [SerializeField] private float cardMoveAnimationTime;
    [SerializeField] private float angle = 90.0f;
    public int maxCardsPerPlayer;
    public int startingPlayerHealth;
    [SerializeField] private bool boardTakesFullDamage;
    [Tooltip("This is in milliseconds!")]
    [SerializeField] private int attackDelay;
    [SerializeField] private float cardVerticalOffset;

    [Header("Player Health")] 
    [HideInInspector] public int player1Health;
    [HideInInspector] public int player2Health;

    [Header("Events")]
    public UnityEvent cardPlaced;
    public UnityEvent<Player.PlayerId> damageTaken;
    public UnityEvent cardDied;
    
    private InputAction select;

    private OrbitCamera cameraInfo;

    private List<Vector2Int> workingPositions = null;
    
    private Vector2Int[] currentAdjacentPositions;
    
    

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

        Transform[] island1Components = player1BoardGameObject.GetComponentsInChildren<Transform>(true);
        Transform[] island2Components = player2BoardGameObject.GetComponentsInChildren<Transform>(true);

    }

    private void Start()
    {
        player1Board = new PlayerBoard(new GameObject[3, 3],
            new GameObject[3, 3]
        );

        player2Board = new PlayerBoard(new GameObject[3, 3],
            new GameObject[3, 3]
        );


        int childIndex = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                player1Board.TileTransforms[i, j] =
                    player1BoardGameObject.GetComponentsInChildren<BoxCollider>()[childIndex].gameObject;

                player2Board.TileTransforms[i, j] =
                    player2BoardGameObject.GetComponentsInChildren<BoxCollider>()[childIndex].gameObject;

                childIndex++;
            }


        }

        unitsList = new List<Unit>();
        damageInstances = new List<DamageInstance>();
        defenseInstances = new List<DefenseInstance>();

        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            localBoard = player1Board;
            enemyBoard = player2Board;
        }
        else
        {
            localBoard = player2Board;
            enemyBoard = player1Board;
        }

        player1Health = startingPlayerHealth;
        player2Health = startingPlayerHealth;

        criticalHealthThreshold = startingPlayerHealth/2;

        currentSelectedTileGameObject = null;
        
        damageTaken.Invoke(Player.PlayerId.Player1);
        damageTaken.Invoke(Player.PlayerId.Player2);
    }

    private void Update()
    {
        if (speed1 < maxSpeed && board1DoOnce)
        {
            speed1 += acceleration * Time.deltaTime;
            islandBottom1.transform.position = new Vector3(islandBottom1.transform.position.x, islandBottom1.transform.position.y - speed1 * Time.deltaTime, islandBottom1.transform.position.z);
        }

        if (speed1 >= maxSpeed)
        {
            Destroy(islandBottom1);
            board1DoOnce = false;
            speed1 = 0.0f;
        }

        if (speed2 < maxSpeed && board2DoOnce)
        {
            speed2 += acceleration * Time.deltaTime;
            islandBottom2.transform.position = new Vector3(islandBottom2.transform.position.x, islandBottom2.transform.position.y - speed2 * Time.deltaTime, islandBottom2.transform.position.z);
        }

        if (speed2 >= maxSpeed)
        {
            Destroy(islandBottom2);
            board2DoOnce = false;
            speed2 = 0.0f;
        }
            
    }

    public override void OnNetworkSpawn()
    {
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            playerSpecificLayer = LayerMask.GetMask("Player1Tile");
        }
        else
        {
            playerSpecificLayer = LayerMask.GetMask("Player2Tile");
        }

        print(GameManager.instance.playerId);
    }

    private void TileSelect()
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
                //If we already have something selected
                if (currentSelectedTileGameObject != null)
                {

                    //If we are selecting the same tile again, we deselect
                    if (hit.transform.gameObject == currentSelectedTileGameObject)
                    {
                        NullSelection();

                        return;
                    }

                    //If it's a different tile
                    // ClearTiles();

                    //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;
                    UIManager.Instance.DestroyCurrentInfoInstance();

                    currentSelectedTileGameObject = hit.transform.gameObject;
                    
                    //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.green;

                    CurrentSelectedTile =
                        CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);
                    //If the tile is not on the players board
                    if (Equals(CurrentSelectedTile, new Vector2Int(-1, -1)))
                    {
                        CurrentSelectedTile =
                            CoordinatesOf<GameObject>(player2Board.TileTransforms,
                                hit.transform.gameObject);
                        UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player2);
                        UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile,
                            Player.PlayerId.Player2);

                        bool cardFound = false;
                        foreach (Unit unit in unitsList)
                        {
                            if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player2)
                            {
                                // ClearTiles();
                                //
                                //
                                // foreach (var position in unit.AttackPositions)
                                // {
                                //     player1Board.TileTransforms[position.x, position.y]
                                //         .GetComponent<tileColour>().TileRecieveSignal(1, true);
                                // }
                                //
                                // cardFound = true;
                                // break;
                            }
                        }
                        //If the new tile selected has no card on it, show the live board status again.
                        if (cardFound == false)
                        {
                            // ClearTiles();
                            //
                            // UpdateTileVisuals();
                        }
                    }
                    //If the tile is on the player board
                    else
                    {
                        UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);
                        UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile,
                            Player.PlayerId.Player1);

                        bool cardFound = false;
                        foreach (Unit unit in unitsList)
                        {
                            if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player1)
                            {
                                // ClearTiles();
                                //
                                //
                                // foreach (var position in unit.AttackPositions)
                                // {
                                //     player2Board.TileTransforms[position.x, position.y]
                                //         .GetComponent<tileColour>().TileRecieveSignal(1, true);
                                // }
                                //
                                // cardFound = true;
                                // break;
                            }
                        }
                        //If the new tile selected has no card on it, show the live board status again.
                        if (cardFound == false)
                        {
                            // ClearTiles();
                            //
                            // UpdateTileVisuals();
                        }
                    }

                    return;
                }
                else
                {
                    //If we don't already have something selected.
                    currentSelectedTileGameObject = hit.transform.gameObject;

                    CurrentSelectedTile =
                        CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);

                    //If tile is not on our board
                    if (Equals(CurrentSelectedTile, new Vector2Int(-1, -1)))
                    {
                        CurrentSelectedTile =
                            CoordinatesOf<GameObject>(player2Board.TileTransforms, hit.transform.gameObject);
                        UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player2);
                        UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile, Player.PlayerId.Player2);

                        foreach (Unit unit in unitsList)
                        {
                            if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player2)
                            {
                                // ClearTiles();
                                //
                                // foreach (var position in unit.AttackPositions)
                                // {
                                //     player1Board.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                                //         .TileRecieveSignal(1, true);
                                // }
                            }
                        }
                    }
                    //If tile is on our board
                    else
                    {
                        UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);
                        UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);

                        foreach (Unit unit in unitsList)
                        {
                            if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player1)
                            {
                                // ClearTiles();
                                //
                                // foreach (var position in unit.AttackPositions)
                                // {
                                //     player2Board.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                                //         .TileRecieveSignal(1, true);
                                // }
                            }
                        }
                    }

                    //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.green;
                }
            }
            

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentSelectedTileGameObject != null)
            {
                NullSelection();
            }   
        }
        
    }

    public void NullSelection()
    {
        if (currentSelectedTileGameObject != null)
        {
            // currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;
        }
        currentSelectedTileGameObject = null;
        CurrentSelectedTile = new Vector2Int(-1, -1);
        UIManager.Instance.DestroyCurrentInfoInstance();

        // ClearTiles();
        //
        // UpdateTileVisuals();

    }

    void Attacking()
    {
        if (Input.GetMouseButtonDown(1))
        {
            
            foreach (Vector2Int position in workingPositions)
            {
                enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }
            
            UpdateTileVisuals();

            for (int i = 0; i < workingPositions.Count; i++)
            {
                print(workingPositions[i]);
                float xpos = workingPositions[i].x;
                float ypos = workingPositions[i].y;

                float angleRad = angle * Mathf.Deg2Rad;

                float rotY;
                float rotX;
                rotX = (xpos - 1) * Mathf.Cos(angleRad) - (ypos - 1) * Mathf.Sin(angleRad) + 1;
                rotY = (xpos - 1) * Mathf.Sin(angleRad) + (ypos - 1) * Mathf.Cos(angleRad) + 1;

                print(rotX);
                print(rotY);

                // rotX = Mathf.Clamp(rotX, 0f, 2.0f);
                // rotY = Mathf.Clamp(rotY, 0f, 2.0f);

                Vector2Int newCoords = new Vector2Int(Mathf.RoundToInt(rotX), Mathf.RoundToInt(rotY));
                print(newCoords);
                workingPositions[i] = newCoords;
            }

            foreach (Vector2Int position in workingPositions)
            {
                enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(1, true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            workingPositions = null;
            currentlySelectedUnit = default;

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            UpdateTileVisuals();
        }

        if (Input.GetMouseButtonDown(0))
        {
           
            
            ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.AttackPositions.Count);


            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            string name = currentlySelectedUnit.Name;
            int damage = currentlySelectedUnit.Damage;
            Vector2Int[] positions = new Vector2Int[workingPositions.Count];

            currentlySelectedUnit.HasActed = true;

            for (int i = 0; i < workingPositions.Count; i++)
            {
                positions[i] = workingPositions[i];
                print(positions[i]);
            }

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            if (NetworkManager.Singleton)
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    AddDamageInstanceRpc(name, GameManager.instance.playerId, damage, positions,
                        RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                AddDamageInstanceLocal(name, GameManager.instance.playerId, damage, positions);
            }
        }
    }

    public void Defending()
    {
        if (Input.GetMouseButtonDown(1))
        {
            foreach (Vector2Int position in workingPositions)
            {
                localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }
            
            UpdateTileVisuals();


            for (int i = 0; i < workingPositions.Count; i++)
            {
                
                float xpos = workingPositions[i].x;
                float ypos = workingPositions[i].y;

                float angleRad = angle * Mathf.Deg2Rad;

                float rotY;
                float rotX;
                rotX = (xpos - 1) * Mathf.Cos(angleRad) - (ypos - 1) * Mathf.Sin(angleRad) + 1;
                rotY = (xpos - 1) * Mathf.Sin(angleRad) + (ypos - 1) * Mathf.Cos(angleRad) + 1;


                Vector2Int newCoords = new Vector2Int(Mathf.RoundToInt(rotX), Mathf.RoundToInt(rotY));
                print(newCoords);
                workingPositions[i] = newCoords;
            }

            foreach (Vector2Int position in workingPositions)
            {
                localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(2, true);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            workingPositions = null;
            currentlySelectedUnit = default;

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            UpdateTileVisuals();
        }

        if (Input.GetMouseButtonDown(0))
        {
            
            
            ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.AttackPositions.Count);

            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            string name = currentlySelectedUnit.Name;
            int defense = currentlySelectedUnit.Defense;
            Vector2Int[] positions = new Vector2Int[workingPositions.Count];

            currentlySelectedUnit.HasActed = true;

            for (int i = 0; i < workingPositions.Count; i++)
            {
                positions[i] = workingPositions[i];
                print(positions[i]);
            }

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            if (NetworkManager.Singleton)
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    AddDefenseInstanceRpc(name, GameManager.instance.playerId, defense, positions,
                        RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                AddDefenseInstanceLocal(name, GameManager.instance.playerId, defense, positions);
            }
        }
    }

    void Moving()
    {
        if (currentlySelectedUnit.Movement == 0)
        {
            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();

            foreach (var tile in localBoard.TileTransforms)
            {
                tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            }

            UpdateTileVisuals();
        }

        // if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     MoveCard(3);
        // }

        // if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        // {
        //     if (GameManager.instance.playerId == Player.PlayerId.Player1)
        //     {
        //         MoveCard(0);
        //     }
        //     else
        //     {
        //         MoveCard(1);
        //     }
        // }

        // if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        // {
        //     MoveCard(2);
        // }

        // if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        // {
        //     if (GameManager.instance.playerId == Player.PlayerId.Player1)
        //     {
        //         MoveCard(1);
        //     }
        //     else
        //     {
        //         MoveCard(0);
        //     }
        // }

         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
         RaycastHit hit;

        if (Input.GetMouseButtonDown(0)) 
        {
          if (Physics.Raycast(ray, out hit, Mathf.Infinity, playerSpecificLayer))
            {
                for(int i = 0; i < currentAdjacentPositions.Length; i++)
                {
                    if (currentAdjacentPositions[i].Equals(new Vector2Int(-1, -1))) continue;
                    if (localBoard.TileTransforms[currentAdjacentPositions[i].x, currentAdjacentPositions[i].y] == hit.transform.gameObject)
                    {
                        MoveCard(i);
                    }
                }
            }  
        }
        

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();

            foreach (var tile in localBoard.TileTransforms)
            {
                tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            }

            UpdateTileVisuals();
        }
    }

    public void LateUpdate()
    {
        switch (UIManager.Instance.interactionState)
        {
            case UIManager.InteractionState.None:

                TileSelect();

                break;


            case UIManager.InteractionState.Attacking:

                Attacking();

                break;

            case UIManager.InteractionState.Defending:

                Defending();

                break;

            case UIManager.InteractionState.Moving:

                Moving();

                break;
        }
    }

    public void MoveCard(int direction)
    {
        if (currentlySelectedUnit.Movement > 0 &&
            !currentAdjacentPositions[direction].Equals(new Vector2Int(-1, -1)))
        {
            currentlySelectedUnit.HasActed = true;


            Vector3 position = new Vector3(
                localBoard.TileTransforms[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y]
                    .transform.position.x,
                localBoard.TileTransforms[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y]
                    .transform.position.y + cardVerticalOffset,
                localBoard.TileTransforms[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y]
                    .transform.position.z);

            var tween = new PositionTween()
            {
                to = position,
                duration = cardMoveAnimationTime,
                easeType = EaseType.ElasticOut
            };

            localBoard.Visuals[CurrentSelectedTile.x, CurrentSelectedTile.y].AddTween(tween);

            GameObject visual = localBoard.Visuals[CurrentSelectedTile.x, CurrentSelectedTile.y];
            localBoard.Visuals[CurrentSelectedTile.x, CurrentSelectedTile.y] = null;
            localBoard.Visuals[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y] = visual;

            currentlySelectedUnit.Position = currentAdjacentPositions[direction];

            currentlySelectedUnit.Movement -= 1;
            CurrentSelectedTile = currentAdjacentPositions[direction];
            
            var randInt = Random.Range(0, 1);
            if (randInt == 0)
            {
                AudioManager.singleton.PlaySound("cardMove1", true); 
            }
            else if(randInt == 1)
            {
                AudioManager.singleton.PlaySound("cardMove2", true); 
            }

            if (NetworkManager.Singleton)
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (clientIds == NetworkManager.LocalClientId) continue;
                    MoveCardRpc(unitsList.IndexOf(currentlySelectedUnit), currentAdjacentPositions[direction],
                        RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }


            foreach (var tile in localBoard.TileTransforms)
            {
                tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            }
            
            ClearTiles();
            UpdateTileVisuals();

            currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile);

            foreach (var pos in currentAdjacentPositions)
            {
                if (pos.Equals(new Vector2Int(-1, -1))) continue;
                localBoard.TileTransforms[pos.x, pos.y].GetComponent<tileColour>().TileRecieveSignal(3, false);
            }
        }
    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void MoveCardRpc(int index, Vector2Int newPos, RpcParams rpcParams = default)
    {
        print(newPos);

        Unit unitToMove = unitsList[index];

        Vector3 position = new Vector3(enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.x,
            enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.y + cardVerticalOffset,
            enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.z
        );

        enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y].transform.position = position;

        GameObject visual = enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y];
        enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y] = null;
        enemyBoard.Visuals[newPos.x, newPos.y] = visual;

        unitToMove.Position = newPos;
    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void AddDefenseInstanceRpc(string name, Player.PlayerId pID, int defense, Vector2Int[] positions,
        RpcParams rpcParams = default)
    {
        List<Vector2Int> positionList = new List<Vector2Int>();

        foreach (var position in positions)
        {
            positionList.Add(position);
        }

        DefenseInstance instance = new DefenseInstance(
            name: name,
            id: pID,
            defense: defense,
            positions: new List<Vector2Int>(positionList)
        );

        defenseInstances.Add(instance);

        UpdateTileVisuals();
    }
    
    public void AddDefenseInstanceLocal(string name, Player.PlayerId pID, int defense, Vector2Int[] positions)
    {
        List<Vector2Int> positionList = new List<Vector2Int>();

        foreach (var position in positions)
        {
            positionList.Add(position);
        }

        DefenseInstance instance = new DefenseInstance(
            name: name,
            id: pID,
            defense: defense,
            positions: new List<Vector2Int>(positionList)
        );

        defenseInstances.Add(instance);

        UpdateTileVisuals();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void AddDamageInstanceRpc(string name, Player.PlayerId pID, int damage, Vector2Int[] positions,
        RpcParams rpcParams = default)
    {
        List<Vector2Int> positionList = new List<Vector2Int>();

        foreach (var position in positions)
        {
            positionList.Add(position);
        }

        DamageInstance instance = new DamageInstance(
            name: name,
            id: pID,
            damage: damage,
            positions: new List<Vector2Int>(positionList)
        );

        damageInstances.Add(instance);

        UpdateTileVisuals();
    }
    
    public void AddDamageInstanceLocal(string name, Player.PlayerId pID, int damage, Vector2Int[] positions)
    {
        List<Vector2Int> positionList = new List<Vector2Int>();

        foreach (var position in positions)
        {
            positionList.Add(position);
        }

        DamageInstance instance = new DamageInstance(
            name: name,
            id: pID,
            damage: damage,
            positions: new List<Vector2Int>(positionList)
        );

        damageInstances.Add(instance);

        UpdateTileVisuals();
    }

    public void UpdateTileVisuals()
    {
        //for player 1 board
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int workingDamage = 0;
                int workingDefense = 0;
                bool somethingHere = false;

                foreach (var damageInstance in damageInstances)
                {
                    if (damageInstance.Positions.Contains(new Vector2Int(i, j)) &&
                        damageInstance.ID == Player.PlayerId.Player2)
                    {
                        workingDamage += damageInstance.Damage;
                        somethingHere = true;
                    }
                }

                foreach (var defenseInstance in defenseInstances)
                {
                    if (defenseInstance.Positions.Contains(new Vector2Int(i, j)) &&
                        defenseInstance.ID == Player.PlayerId.Player1)
                    {
                        workingDefense += defenseInstance.Defense;
                        somethingHere = true;
                    }
                }

                player1Board.TileTransforms[i, j].GetComponent<tileColour>()
                    .TileRecieveDamage(workingDamage, workingDefense);

                workingDamage -= workingDefense;

                if (somethingHere)
                {
                    if (workingDamage <= 0)
                    {
                        player1Board.TileTransforms[i, j].GetComponent<tileColour>().TileRecieveSignal(2, false);
                    }
                    else
                    {
                        player1Board.TileTransforms[i, j].GetComponent<tileColour>().TileRecieveSignal(1, false);
                    }
                }
            }
        }

        //for player 2 board
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int workingDamage = 0;
                int workingDefense = 0;
                bool somethingHere = false;

                foreach (var damageInstance in damageInstances)
                {
                    if (damageInstance.Positions.Contains(new Vector2Int(i, j)) &&
                        damageInstance.ID == Player.PlayerId.Player1)
                    {
                        workingDamage += damageInstance.Damage;
                        somethingHere = true;
                    }
                }

                foreach (var defenseInstance in defenseInstances)
                {
                    if (defenseInstance.Positions.Contains(new Vector2Int(i, j)) &&
                        defenseInstance.ID == Player.PlayerId.Player2)
                    {
                        workingDefense += defenseInstance.Defense;
                        somethingHere = true;
                    }
                }
                
                player2Board.TileTransforms[i, j].GetComponent<tileColour>()
                    .TileRecieveDamage(workingDamage, workingDefense);

                workingDamage -= workingDefense;

                if (somethingHere)
                {
                    if (workingDamage <= 0)
                    {
                        player2Board.TileTransforms[i, j].GetComponent<tileColour>().TileRecieveSignal(2, false);
                    }
                    else
                    {
                        player2Board.TileTransforms[i, j].GetComponent<tileColour>().TileRecieveSignal(1, false);
                    }
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
            defense: cardData.Defence,
            movement: cardData.Speed,
            attackPositions: new List<Vector2Int>(cardData.Range),
            position: coordinates,
            hasActed: true
        );

        unitsList.Add(unit);

        CardManager.instance.RemoveCard(cardVisual);
        CardManager.instance.playerHandVisuals.Remove(cardVisual);
        CardManager.instance.playerHand.Remove(cardData);

        localBoard.Visuals[coordinates.x, coordinates.y] = cardVisual;
        
        Vector3 position;

        Quaternion rotation;
        
        position = new Vector3(tile.transform.position.x, tile.transform.position.y + cardVerticalOffset,
                tile.transform.position.z);
        rotation = Quaternion.Euler(new Vector3(45, 0, -90));
        
        

        Vector3 scale = new Vector3(7, 8, 6);
    
        var positionTween = new PositionTween()
        {
            to = position,
            duration = placeAnimationTime,
            easeType = EaseType.ExpoOut
        };

        var rotationTween = new LocalRotationTween()
        {
            to = rotation,
            duration = placeAnimationTime,
            easeType = EaseType.ExpoOut
        };

        var scaleTween = new LocalScaleTween()
        {
            to = scale,
            duration = placeAnimationTime,
            easeType = EaseType.ExpoOut,
            onEnd = (Instance) =>
            {
                cardVisual.gameObject.transform.parent = tile.transform;
            }
        };

        var instances = cardVisual.AddTween(positionTween, rotationTween, scaleTween);

        cardVisual.GetComponent<CardDrag>().isPlaced = true;
        
        AudioManager.singleton.PlaySound("cardPlace1", true, 1.0f);

        cardPlaced.Invoke();

        if (NetworkManager.Singleton)
        {
            foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientIds == NetworkManager.Singleton.LocalClientId) continue;
                PlaceCardRpc(cardData.ID, unit.ID, unit.Position, cardData.Health, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
            }
        }
        
       

        
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void PlaceCardRpc(int ID, Player.PlayerId playerId, Vector2Int coordinates, int unitHealth, RpcParams rpcParams = default)
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
            health: unitHealth,
            damage: cardData.Damage,
            defense: cardData.Defence,
            movement: cardData.Speed,
            attackPositions: new List<Vector2Int>(cardData.Range),
            position: coordinates,
            hasActed: true
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

        var cardVisual = CardManager.instance.BuildCard(cardData);

        cardVisual.GetComponent<CardDrag>().isPlaced = true;

        
        if (unit.ID == Player.PlayerId.Player1)
        {
            player1Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
           
        }
        else
        {
            player2Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
            
        }
        
        Quaternion rotation = Quaternion.Euler(new Vector3(45, 0, 90));
        Vector3 position = new Vector3(tile.transform.position.x, tile.transform.position.y + cardVerticalOffset,
            tile.transform.position.z);

        Vector3 scale = new Vector3(7, 8, 6);

        cardVisual.transform.position = position;
        cardVisual.transform.localRotation = rotation;
        cardVisual.transform.localScale = scale;

        cardVisual.transform.parent = tile.transform;

        cardPlaced.Invoke();
    }

    public void PrepareAttack()
    {
        
        UIManager.Instance.DestroyCurrentInfoInstance();

        foreach (Unit unit in unitsList)
        {
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositions);
                currentlySelectedUnit = unit;
            }
        }
        
        
        if (workingPositions == null) return;
        
        // foreach (var tile in enemyBoard.TileTransforms)
        // {
        //     tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
        //     tile.GetComponent<tileColour>().TileRecieveDamage(0, 0);
        // }
        
        ClearTiles();
        UpdateTileVisuals();


        foreach (Vector2Int position in workingPositions)
        {
            enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(1, true);
        }

        UIManager.Instance.interactionState = UIManager.InteractionState.Attacking;

        UIManager.Instance.EnableControlsText();
    }

    public void PrepareDefense()
    {

        UIManager.Instance.DestroyCurrentInfoInstance();

        foreach (Unit unit in unitsList)
        {
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositions);
                currentlySelectedUnit = unit;
            }
        }
        
        

        if (workingPositions == null) return;

        ClearTiles();
        UpdateTileVisuals();

        foreach (Vector2Int position in workingPositions)
        {
            localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(2, true);
        }

        UIManager.Instance.interactionState = UIManager.InteractionState.Defending;

        UIManager.Instance.EnableControlsText();
    }

    public void PrepareMovement()
    {
        // if (!TacticsManager.instance.CanAfford(1)) return;

        
        UIManager.Instance.DestroyCurrentInfoInstance();

        foreach (Unit unit in unitsList)
        {
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositions);
                currentlySelectedUnit = unit;
            }
        }
        
        ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.AttackPositions.Count);
        if (workingPositions == null) return;

        ClearTiles();
        UpdateTileVisuals();

        currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile);

        foreach (var pos in currentAdjacentPositions)
        {
            if (pos.Equals(new Vector2Int(-1, -1))) continue;
            localBoard.TileTransforms[pos.x, pos.y].GetComponent<tileColour>().TileRecieveSignal(3, false);
        }

        UIManager.Instance.interactionState = UIManager.InteractionState.Moving;

        UIManager.Instance.EnableControlsText();
    }

    public void ClearTiles()
    {
        foreach (var tile in enemyBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            tile.GetComponent<tileColour>().TileRecieveDamage(0, 0);
        }

        foreach (var tile in localBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            tile.GetComponent<tileColour>().TileRecieveDamage(0, 0);
        }
    }

    Vector2Int[] GetAdjacentTiles(Vector2Int position)
    {
        Vector2Int[] adjacent = new Vector2Int[4];


        if (position.x + 1 <= 2)
        {
            bool cardPresent = false;
            foreach (var unit in unitsList)
            {
                if (unit.ID != GameManager.instance.playerId) continue;
                if (Equals(unit.Position, new Vector2Int(position.x + 1, position.y)))
                {
                    cardPresent = true;
                }
            }

            if (cardPresent == false)
            {
                adjacent[0] = new Vector2Int(position.x + 1, position.y);
            }
            else
            {
                adjacent[0] = new Vector2Int(-1, -1);
            }
        }
        else
        {
            adjacent[0] = new Vector2Int(-1, -1);
        }

        if (position.x - 1 >= 0)
        {
            bool cardPresent = false;
            foreach (var unit in unitsList)
            {
                if (unit.ID != GameManager.instance.playerId) continue;
                if (Equals(unit.Position, new Vector2Int(position.x - 1, position.y)))
                {
                    cardPresent = true;
                }
            }

            if (cardPresent == false)
            {
                adjacent[1] = new Vector2Int(position.x - 1, position.y);
            }
            else
            {
                adjacent[1] = new Vector2Int(-1, -1);
            }
        }
        else
        {
            adjacent[1] = new Vector2Int(-1, -1);
        }

        if (position.y + 1 <= 2)
        {
            bool cardPresent = false;
            foreach (var unit in unitsList)
            {
                if (unit.ID != GameManager.instance.playerId) continue;
                if (Equals(unit.Position, new Vector2Int(position.x, position.y + 1)))
                {
                    cardPresent = true;
                }
            }

            if (cardPresent == false)
            {
                adjacent[2] = new Vector2Int(position.x, position.y + 1);
            }
            else
            {
                adjacent[2] = new Vector2Int(-1, -1);
            }
        }
        else
        {
            adjacent[2] = new Vector2Int(-1, -1);
        }

        if (position.y - 1 >= 0)
        {
            bool cardPresent = false;
            foreach (var unit in unitsList)
            {
                if (unit.ID != GameManager.instance.playerId) continue;
                if (Equals(unit.Position, new Vector2Int(position.x, position.y - 1)))
                {
                    cardPresent = true;
                }
            }

            if (cardPresent == false)
            {
                adjacent[3] = new Vector2Int(position.x, position.y - 1);
            }
            else
            {
                adjacent[3] = new Vector2Int(-1, -1);
            }
        }
        else
        {
            adjacent[3] = new Vector2Int(-1, -1);
        }


        return adjacent;
    }

    public async Task EvaluateDamage(Player.PlayerId playerId)
    {

        attackInProgress = true;
        
        //Evaluate PLayer 1 Damage
        if (playerId == Player.PlayerId.Player1)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int workingDamage = 0;

                    bool damageInstancePresent = false;
                    bool defenseInstancePresent = false;

                    foreach (var damageInstance in damageInstances)
                    {
                        if (damageInstance.Positions.Contains(new Vector2Int(i, j)) &&
                            damageInstance.ID == Player.PlayerId.Player2)
                        {
                            damageInstancePresent = true;
                            workingDamage += damageInstance.Damage;
                        }
                    }

                    foreach (var defenseInstance in defenseInstances)
                    {
                        if (defenseInstance.Positions.Contains(new Vector2Int(i, j)) &&
                            defenseInstance.ID == Player.PlayerId.Player1)
                        {
                            defenseInstancePresent = true;
                            workingDamage -= defenseInstance.Defense;
                        }
                    }

                    if (!damageInstancePresent && !defenseInstancePresent) continue;
                    

                    if (workingDamage < 0)
                    {
                        workingDamage = 0;
                    }

                    bool attackBlocked = false;
                    foreach (var unit in unitsList)
                    {
                        if (unit.ID == Player.PlayerId.Player2) continue;

                        if (Equals(unit.Position, new Vector2Int(i, j)))
                        {
                            unit.Health -= workingDamage;
                            player1Board.Visuals[i, j].GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                                unit.Health.ToString();
                            attackBlocked = true;
                        }
                    }

                    if (!attackBlocked && workingDamage > 0)
                    {
                        if (boardTakesFullDamage)
                        {
                            BoardTakeDamage(workingDamage, Player.PlayerId.Player1);
                        }
                        else
                        {
                            BoardTakeDamage(1, Player.PlayerId.Player1);
                        }
                        
                    }
                    
                    tileColour tc = player1Board.TileTransforms[i, j].GetComponentInChildren<tileColour>();
                    tc.TileRecieveSignal(0, false);
                    tc.TileRecieveDamage(0, 0);


                    if (attackBlocked)
                    {
                        //Play card attack sounds
                        int randInt = Random.Range(0, 2);
                            if (randInt == 0)
                            {
                                AudioManager.singleton.PlaySound("combatSword0", true);
                            }
                            else
                            {
                                AudioManager.singleton.PlaySound("combatSword1", true);
                            }
                    }
                    else
                    {
                        if (workingDamage > 0)
                        {
                            //Play board attack sounds
                            AudioManager.singleton.PlaySound("boardAttack", true);
                            
                        }
                        else
                        {
                            //Play card defend sounds
                            AudioManager.singleton.PlaySound("shieldBlock", true);
                        }
                    }

                    if (attackBlocked)
                    {
                        tc.TileRecievePopup(workingDamage, 0);
                    }
                    else
                    {
                        tc.TileRecievePopup(workingDamage, 2);
                    }

                    await Task.Delay(attackDelay);
                }
            }


            List<DamageInstance> toRemove = new List<DamageInstance>();
            foreach (var damageInstance in damageInstances)
            {
                if (damageInstance.ID == Player.PlayerId.Player2)
                {
                    toRemove.Add(damageInstance);
                }
            }

            foreach (var damageInstance in toRemove)
            {
                damageInstances.Remove(damageInstance);
            }

            List<DefenseInstance> toRemoveDefense = new List<DefenseInstance>();
            foreach (var defenseInstance in defenseInstances)
            {
                if (defenseInstance.ID == Player.PlayerId.Player1)
                {
                    toRemoveDefense.Add(defenseInstance);
                }
            }

            foreach (var defenseInstance in toRemoveDefense)
            {
                defenseInstances.Remove(defenseInstance);
            }
        }
        //Evaluate Player 2 Damage
        else
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int workingDamage = 0;

                    bool damageInstancePresent = false;
                    bool defenseInstancePresent = false;

                    foreach (var damageInstance in damageInstances)
                    {
                        if (damageInstance.Positions.Contains(new Vector2Int(i, j)) &&
                            damageInstance.ID == Player.PlayerId.Player1)
                        {
                            damageInstancePresent = true;
                            workingDamage += damageInstance.Damage;
                        }
                    }

                    foreach (var defenseInstance in defenseInstances)
                    {
                        if (defenseInstance.Positions.Contains(new Vector2Int(i, j)) &&
                            defenseInstance.ID == Player.PlayerId.Player2)
                        {
                            defenseInstancePresent = true;
                            workingDamage -= defenseInstance.Defense;
                        }
                    }

                    if (!damageInstancePresent && !defenseInstancePresent) continue;

                    if (workingDamage < 0)
                    {
                        workingDamage = 0;
                    }

                    bool attackBlocked = false;
                    foreach (var unit in unitsList)
                    {
                        if (unit.ID == Player.PlayerId.Player1) continue;

                        if (Equals(unit.Position, new Vector2Int(i, j)))
                        {
                            unit.Health -= workingDamage;
                            player2Board.Visuals[i, j].GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                                unit.Health.ToString();
                            attackBlocked = true;
                        }
                    }

                    if (!attackBlocked && workingDamage > 0)
                    {
                        if (boardTakesFullDamage)
                        {
                            BoardTakeDamage(workingDamage, Player.PlayerId.Player2);
                        }
                        else
                        {
                            BoardTakeDamage(1, Player.PlayerId.Player2);
                        }
                        
                    }

                    tileColour tc = player2Board.TileTransforms[i, j].GetComponentInChildren<tileColour>();
                    tc.TileRecieveSignal(0, false);
                    tc.TileRecieveDamage(0, 0);


                    if (attackBlocked)
                    {
                        //Play card attack sounds
                        int randInt = Random.Range(0, 2);
                            if (randInt == 0)
                            {
                                AudioManager.singleton.PlaySound("combatSword0", true);
                            }
                            else
                            {
                                AudioManager.singleton.PlaySound("combatSword1", true);
                            }
                    }
                    else
                    {
                        if (workingDamage > 0)
                        {
                            //Play board attack sounds
                            AudioManager.singleton.PlaySound("boardAttack", true);
                            
                        }
                        else
                        {
                            //Play card defend sounds
                            AudioManager.singleton.PlaySound("shieldBlock", true);
                        }
                    }

                    if (attackBlocked)
                    {
                        tc.TileRecievePopup(workingDamage, 0);
                    }
                    else
                    {
                        tc.TileRecievePopup(workingDamage, 2);
                    }
                    
                    
                    PruneUnitList();
                    await Task.Delay(attackDelay);
                }
                
            }


            List<DamageInstance> toRemove = new List<DamageInstance>();
            foreach (var damageInstance in damageInstances)
            {
                if (damageInstance.ID == Player.PlayerId.Player1)
                {
                    toRemove.Add(damageInstance);
                }
            }

            foreach (var damageInstance in toRemove)
            {
                damageInstances.Remove(damageInstance);
            }

            List<DefenseInstance> toRemoveDefense = new List<DefenseInstance>();
            foreach (var defenseInstance in defenseInstances)
            {
                if (defenseInstance.ID == Player.PlayerId.Player2)
                {
                    toRemoveDefense.Add(defenseInstance);
                }
            }

            foreach (var defenseInstance in toRemoveDefense)
            {
                defenseInstances.Remove(defenseInstance);
            }
        }

        foreach (var tile in player1Board.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
        }

        foreach (var tile in player2Board.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
        }

        UpdateTileVisuals();
        PruneUnitList();
        attackInProgress = false;
    }

    public void PruneUnitList()
    {
        List<Unit> unitsToDelete = new List<Unit>();

        foreach (var unit in unitsList)
        {
            if (unit.Health <= 0)
            {
                if (unit.ID == Player.PlayerId.Player1)
                {
                    Destroy(player1Board.Visuals[unit.Position.x, unit.Position.y]);
                }
                else
                {
                    Destroy(player2Board.Visuals[unit.Position.x, unit.Position.y]);
                }

                unitsToDelete.Add(unit);
            }
        }


        foreach (var unit in unitsToDelete)
        {
            unitsList.Remove(unit);
        }

        cardDied.Invoke();
    }

    void BoardTakeDamage(int damage, Player.PlayerId id)
    {
        if (id == Player.PlayerId.Player1)
        {
            player1Health -= damage;

            if (player1Health <= 0)
            {   
                StartCoroutine(UIManager.Instance.DisplayEndGameScreen(Player.PlayerId.Player2));
            }

            if (player1Health <= criticalHealthThreshold && !board1DoOnce)
            {
                board1DoOnce = true;
                
            }
            damageTaken.Invoke(Player.PlayerId.Player1);
                
        }
        else
        {
            player2Health -= damage;

            if (player2Health <= 0)
            {
                StartCoroutine(UIManager.Instance.DisplayEndGameScreen(Player.PlayerId.Player1));
            }

            if (player2Health <= criticalHealthThreshold && !board2DoOnce)
            {
                board2DoOnce = true;
        
            }
            damageTaken.Invoke(Player.PlayerId.Player2);
        }

        
    }

    public int GetCardAmount(Player.PlayerId id)
    {
        int totalCards = 0;

        foreach (var unit in unitsList)
        {
            if (unit.ID == id)
            {
                totalCards++;
            }
        }

        return totalCards;
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
