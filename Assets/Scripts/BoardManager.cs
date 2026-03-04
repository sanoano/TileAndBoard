using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Tweens;
using Unity.Mathematics;
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

    [Header("Board References")] [SerializeField]
    private GameObject player1BoardGameObject;

    [SerializeField] private GameObject player2BoardGameObject;

    [Header("Layers")] public LayerMask playerSpecificLayer;
    [SerializeField] private LayerMask interactionLayers;
    private Camera cam;

    [Header("Selected Tile")] public Vector2Int CurrentSelectedTile;
    public GameObject currentSelectedTileGameObject;

    [Header("Parameters")] 
    [SerializeField] private float placeAnimationTime;
    [SerializeField] private float cardMoveAnimationTime;

    [SerializeField] private float angle = 90.0f;

    private InputAction select;

    private OrbitCamera cameraInfo;

    private List<Vector2Int> workingPositions = null;
    private Unit currentlySelectedUnit;
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
    }

    private void Start()
    {
        player1Board = new PlayerBoard(new GameObject[3, 3],
            new GameObject[3, 3]
        );

        player2Board = new PlayerBoard(new GameObject[3, 3],
            new GameObject[3, 3]
        );


        int childIndex = 1;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                player1Board.TileTransforms[i, j] =
                    player1BoardGameObject.GetComponentsInChildren<Transform>()[childIndex].gameObject;

                player2Board.TileTransforms[i, j] =
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
            enemyBoard = player2Board;
        }
        else
        {
            localBoard = player2Board;
            enemyBoard = player1Board;
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

    public void LateUpdate()
    {


        switch (UIManager.Instance.interactionState)
        {
            case UIManager.InteractionState.None:

                if (Input.GetMouseButtonDown(0))
                {
                    if (UIManager.Instance.settingsMenu.activeSelf) return;
                    if (cameraInfo.cameraState == OrbitCamera.CameraState.Free) return;

                    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    //If we hit something.
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, interactionLayers))
                    {
                        if (currentSelectedTileGameObject != null)
                        {
                            if (hit.transform.gameObject == currentSelectedTileGameObject)
                            {
                                currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;
                                currentSelectedTileGameObject = null;
                                CurrentSelectedTile = new Vector2Int(-1, -1);
                                UIManager.Instance.DestroyCurrentInfoInstance();
                                
                                foreach (var tile in player1Board.TileTransforms)
                                {
                                    tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                }
                                
                                foreach (var tile in player2Board.TileTransforms)
                                {
                                    tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                }
                                
                                UpdateTileVisuals();
                                
                                return;
                            }
                            else
                            {
                                foreach (var tile in player1Board.TileTransforms)
                                {
                                    tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                }

                                foreach (var tile in player2Board.TileTransforms)
                                {
                                    tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                }

                                currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;
                                UIManager.Instance.DestroyCurrentInfoInstance();

                                currentSelectedTileGameObject = hit.transform.gameObject;

                                CurrentSelectedTile =
                                    CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);

                                if (Equals(CurrentSelectedTile, new Vector2Int(-1, -1)))
                                {
                                    CurrentSelectedTile =
                                        CoordinatesOf<GameObject>(player2Board.TileTransforms,
                                            hit.transform.gameObject);
                                    UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player2);
                                    UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile,
                                        Player.PlayerId.Player2);

                                    foreach (Unit unit in unitsList)
                                    {
                                        if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player2)
                                        {
                                            foreach (var tile in player1Board.TileTransforms)
                                            {
                                                tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                            }

                                            foreach (var position in unit.AttackPositions)
                                            {
                                                player1Board.TileTransforms[position.x, position.y]
                                                    .GetComponent<tileColour>().TileRecieveSignal(1);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);
                                    UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile,
                                        Player.PlayerId.Player1);

                                    foreach (Unit unit in unitsList)
                                    {
                                        if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player1)
                                        {
                                            foreach (var tile in player2Board.TileTransforms)
                                            {
                                                tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                            }

                                            foreach (var position in unit.AttackPositions)
                                            {
                                                player2Board.TileTransforms[position.x, position.y]
                                                    .GetComponent<tileColour>().TileRecieveSignal(1);
                                            }
                                        }
                                    }
                                }

                                return;
                            }
                        }

                        currentSelectedTileGameObject = hit.transform.gameObject;

                        CurrentSelectedTile =
                            CoordinatesOf<GameObject>(player1Board.TileTransforms, hit.transform.gameObject);

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
                                    foreach (var tile in player1Board.TileTransforms)
                                    {
                                        tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                    }

                                    foreach (var position in unit.AttackPositions)
                                    {
                                        player1Board.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                                            .TileRecieveSignal(1);
                                    }
                                }
                            }
                        }
                        else
                        {
                            UIManager.Instance.CreateInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);
                            UIManager.Instance.CreateCardInfoPanel(CurrentSelectedTile, Player.PlayerId.Player1);

                            foreach (Unit unit in unitsList)
                            {
                                if (unit.Position == CurrentSelectedTile && unit.ID == Player.PlayerId.Player1)
                                {
                                    foreach (var tile in player2Board.TileTransforms)
                                    {
                                        tile.GetComponent<tileColour>().TileRecieveSignal(0);
                                    }

                                    foreach (var position in unit.AttackPositions)
                                    {
                                        player2Board.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                                            .TileRecieveSignal(1);
                                    }
                                }
                            }
                        }

                        currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.green;
                    }
                }

                break;
            
            
            case UIManager.InteractionState.Attacking:

                if (Input.GetKeyDown(KeyCode.R))
                {
                    foreach (Vector2Int position in workingPositions)
                    {
                        enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
                    }

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
                            .TileRecieveSignal(1);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    foreach (Vector2Int position in workingPositions)
                    {
                        enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
                    }

                    workingPositions = null;
                    currentlySelectedUnit = default;
                    
                    currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

                    currentSelectedTileGameObject = null;
                    CurrentSelectedTile = new Vector2Int(-1, 1);
                    
                    UpdateTileVisuals();
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    
                    TacticsManager.instance.RemoveTacticsPoints(1);
                    
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    foreach (Vector2Int position in workingPositions)
                    {
                        enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
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

                    currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

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
                    
                }

                break;
            
            case UIManager.InteractionState.Defending:
                
                if (Input.GetKeyDown(KeyCode.R))
                {
                    foreach (Vector2Int position in workingPositions)
                    {
                        localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
                    }
                    
                    

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
                        localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(2);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    foreach (Vector2Int position in workingPositions)
                    {
                        localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
                    }

                    workingPositions = null;
                    currentlySelectedUnit = default;
                    
                    currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

                    currentSelectedTileGameObject = null;
                    CurrentSelectedTile = new Vector2Int(-1, 1);
                    
                    UpdateTileVisuals();
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    
                    TacticsManager.instance.RemoveTacticsPoints(1);
                    
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    foreach (Vector2Int position in workingPositions)
                    {
                        localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                            .TileRecieveSignal(0);
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

                    currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

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
                    
                }
                
                
                break;
            
            case UIManager.InteractionState.Moving:

                if (currentlySelectedUnit.Movement == 0)
                {
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    
                    foreach (var tile in localBoard.TileTransforms)
                    {
                        tile.GetComponent<tileColour>().TileRecieveSignal(0);
                    }
                    
                    UpdateTileVisuals();
                }

                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    MoveCard(3);
                }
                
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (GameManager.instance.playerId == Player.PlayerId.Player1)
                    {
                        MoveCard(0);
                    }
                    else
                    {
                        MoveCard(1);
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                   MoveCard(2);
                }

                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (GameManager.instance.playerId == Player.PlayerId.Player1)
                    {
                        MoveCard(1);
                    }
                    else
                    {
                        MoveCard(0);
                    }
                }

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    UIManager.Instance.interactionState = UIManager.InteractionState.None;
                    
                    foreach (var tile in localBoard.TileTransforms)
                    {
                        tile.GetComponent<tileColour>().TileRecieveSignal(0);
                    }
                    
                    UpdateTileVisuals();
                }
                
                break;
        }
            
    }

    public void MoveCard(int direction)
    {

        if (currentlySelectedUnit.Movement > 0 &&
            !currentAdjacentPositions[direction].Equals(new Vector2Int(-1, -1)))
        {
            Vector3 position = new Vector3(
                localBoard.TileTransforms[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y]
                    .transform.position.x,
                localBoard.TileTransforms[currentAdjacentPositions[direction].x, currentAdjacentPositions[direction].y]
                    .transform.position.y + 0.5f,
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
                tile.GetComponent<tileColour>().TileRecieveSignal(0);
            }

            currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile);

            foreach (var pos in currentAdjacentPositions)
            {
                if (pos.Equals(new Vector2Int(-1, -1))) continue;
                localBoard.TileTransforms[pos.x, pos.y].GetComponent<tileColour>().TileRecieveSignal(3);
            }

           
        }
    }
    

    [Rpc(SendTo.SpecifiedInParams)]
    public void MoveCardRpc(int index, Vector2Int newPos, RpcParams rpcParams = default)
    {
        
        print(newPos);        
        
        Unit unitToMove = unitsList[index];

        Vector3 position = new Vector3(enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.x,
            enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.y + 0.5f,
            enemyBoard.TileTransforms[newPos.x, newPos.y].transform.position.z
            );

        enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y].transform.position = position;
        
        GameObject visual = enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y];
        enemyBoard.Visuals[unitToMove.Position.x, unitToMove.Position.y] = null;
        enemyBoard.Visuals[newPos.x, newPos.y] = visual;

        unitToMove.Position = newPos;

    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void AddDefenseInstanceRpc(string name, Player.PlayerId pID, int defense, Vector2Int[] positions, RpcParams rpcParams = default)
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
    public void AddDamageInstanceRpc(string name, Player.PlayerId pID, int damage, Vector2Int[] positions, RpcParams rpcParams = default)
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
        
        //horrible, needs improvement
        foreach (var instance in damageInstances)
        {
            foreach (var position in instance.Positions)
            {
                if (instance.ID == Player.PlayerId.Player1)
                {
                    player2Board.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(1);
                }
                else
                {
                    player1Board.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(1);
                }
            }
        }
        
        foreach (var instance in defenseInstances)
        {
            foreach (var position in instance.Positions)
            {
                if (instance.ID == Player.PlayerId.Player2)
                {
                    player2Board.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(2);
                }
                else
                {
                    player1Board.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(2);
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
            hasActed: false

        );

        unitsList.Add(unit);

        CardManager.instance.RemoveCard(cardVisual);
        CardManager.instance.playerHandVisuals.Remove(cardVisual);
        CardManager.instance.playerHand.Remove(cardData);

        localBoard.Visuals[coordinates.x, coordinates.y] = cardVisual;

        // cardVisual.transform.parent = tile.transform;

        Vector3 position;

        Quaternion rotation;

        if (unit.ID == Player.PlayerId.Player1)
        {
            position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z);
            rotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }
        else
        {
            position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z);
            rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        }

        Vector3 scale = new Vector3(5, 6, 4);

        var positionTween = new PositionTween()
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

        Vector3 position;

        Quaternion rotation;

        if (unit.ID == Player.PlayerId.Player1)
        {
            player1Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
            rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z);
        }
        else
        {
            player2Board.Visuals[unit.Position.x, unit.Position.y] = cardVisual;
            rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            position = new Vector3(tile.transform.position.x, tile.transform.position.y + 0.5f, tile.transform.position.z);
        }

        Vector3 scale = new Vector3(5, 6, 4);

        cardVisual.transform.position = position;
        cardVisual.transform.rotation = rotation;
        cardVisual.transform.localScale = scale;
    }

    public void PrepareAttack()
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

        if (workingPositions == null) return;

        foreach (var tile in enemyBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0);
        }
        

        foreach (Vector2Int position in workingPositions)
        {
            enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(1);
        }
        
        UIManager.Instance.interactionState = UIManager.InteractionState.Attacking;
        
    }

    public void PrepareDefense()
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

        if (workingPositions == null) return;

        foreach (var tile in enemyBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0);
        }
        
        foreach (var tile in localBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0);
        }

        foreach (Vector2Int position in workingPositions)
        {
            localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>().TileRecieveSignal(2);
        }
        
        UIManager.Instance.interactionState = UIManager.InteractionState.Defending;
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

        if (workingPositions == null) return;

        foreach (var tile in enemyBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0);
        }
        
        foreach (var tile in localBoard.TileTransforms)
        {
            tile.GetComponent<tileColour>().TileRecieveSignal(0);
        }

        currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile);

        foreach (var pos in currentAdjacentPositions)
        {
            if (pos.Equals(new Vector2Int(-1, -1))) continue;
            localBoard.TileTransforms[pos.x, pos.y].GetComponent<tileColour>().TileRecieveSignal(3);
        }

        UIManager.Instance.interactionState = UIManager.InteractionState.Moving;

        currentlySelectedUnit.HasActed = true;


    }

    Vector2Int[] GetAdjacentTiles(Vector2Int position)
    {
        Vector2Int[] adjacent = new Vector2Int[4];


        if (position.x + 1 <= 2)
        {
            adjacent[0] = new Vector2Int(position.x + 1, position.y); 
        }
        else
        {
            adjacent[0] = new Vector2Int(-1, -1);
        }

        if (position.x - 1 >= 0)
        {
            adjacent[1] = new Vector2Int(position.x - 1, position.y);
        }
        else
        {
            adjacent[1] = new Vector2Int(-1, -1);
        }
        
        if (position.y + 1 <= 2)
        {
            adjacent[2] = new Vector2Int(position.x, position.y + 1);
        }
        else
        {
            adjacent[2] = new Vector2Int(-1, -1);
        }

        if (position.y - 1 >= 0)
        {
            adjacent[3] = new Vector2Int(position.x, position.y - 1);
        }
        else
        {
            adjacent[3] = new Vector2Int(-1, -1);
        }
        
    
        return adjacent;
    }

    public void EvaluateDamage(Player.PlayerId playerId)
    {
        
        if (playerId == Player.PlayerId.Player1)
        {

            foreach (var unit in unitsList)
            {
                
                if (unit.ID == Player.PlayerId.Player2) continue;
                
                int workingDamage = 0;
                
                foreach (var damageInstance in damageInstances)
                {
                    if (damageInstance.ID == Player.PlayerId.Player1) continue;
                    
                    if (damageInstance.Positions.Contains(unit.Position))
                    {
                        workingDamage += damageInstance.Damage;
                    } 
                }

                foreach (var defenseInstance in defenseInstances)
                {
                    if (defenseInstance.Positions.Contains(unit.Position))
                    {
                        workingDamage -= defenseInstance.Defense;
                    }
                }

                if (workingDamage > 0)
                {
                    unit.Health -= workingDamage;
                }
            }
        }
        else
        {
            foreach (var unit in unitsList)
            {
                int workingDamage = 0;
                
                if (unit.ID == Player.PlayerId.Player1) continue;
                
                foreach (var damageInstance in damageInstances)
                {
                    if (damageInstance.ID == Player.PlayerId.Player2) continue;
                    
                    if (damageInstance.Positions.Contains(unit.Position))
                    {
                        workingDamage += damageInstance.Damage;
                    } 
                }

                foreach (var defenseInstance in defenseInstances)
                {
                    if (defenseInstance.Positions.Contains(unit.Position))
                    {
                        workingDamage -= defenseInstance.Defense;
                    }
                }

                if (workingDamage > 0)
                {
                    unit.Health -= workingDamage;
                }
            }
        }
        
        PruneUnitList();
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
