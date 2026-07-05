using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Tweens;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public partial class BoardManager : NetworkBehaviour
{

    public static BoardManager Instance;
    public const int MaxUnits = 18;
    public const int MaxDamageInstances = 18;
    public const int MaxDefenseInstances = 18;
    public const int MaxAttackPositions = 4;

    private PlayerBoard player1Board;
    private PlayerBoard player2Board;

    public PlayerBoard localBoard;
    public PlayerBoard enemyBoard;

    [Header("Lists")] public Unit[] unitsList;
    public int unitsCount;
    public DamageInstance[] damageInstances;
    public int damageInstanceCount;
    public DefenseInstance[] defenseInstances;
    public int defenseInstanceCount;

    [Header("Board References")] 
    [SerializeField] private GameObject player1BoardGameObject;
    [SerializeField] private GameObject player2BoardGameObject;
    [SerializeField] private GameObject islandBottom1;
    [SerializeField] private GameObject islandBottom2;
    [SerializeField] private tileFleshy tileFleshScript1;
    [SerializeField] private tileFleshy tileFleshScript2;

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
    private int currentlySelectedUnitIndex = -1;

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

        unitsList = new Unit[MaxUnits];
        unitsCount = 0;
        damageInstances = new DamageInstance[MaxDamageInstances];
        damageInstanceCount = 0;
        defenseInstances = new DefenseInstance[MaxDefenseInstances];
        defenseInstanceCount = 0;

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
                        for (int i = 0; i < unitsCount; i++)
                        {
                            Unit unit = unitsList[i];
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
                        for (int i = 0; i < unitsCount; i++)
                        {
                            Unit unit = unitsList[i];
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

                        for (int i = 0; i < unitsCount; i++)
                        {
                            Unit unit = unitsList[i];
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

                        for (int i = 0; i < unitsCount; i++)
                        {
                            Unit unit = unitsList[i];
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
        currentlySelectedUnit = default;
        currentlySelectedUnitIndex = -1;
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
            currentlySelectedUnitIndex = -1;

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            UpdateTileVisuals();
        }

        if (Input.GetMouseButtonDown(0))
        {
           
            
            ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.Cost);


            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                enemyBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            Vector2Int[] positions = new Vector2Int[workingPositions.Count];

            currentlySelectedUnit.HasActed = true;
            if (currentlySelectedUnitIndex >= 0 && currentlySelectedUnitIndex < unitsCount)
            {
                unitsList[currentlySelectedUnitIndex] = currentlySelectedUnit;
            }

            for (int i = 0; i < workingPositions.Count; i++)
            {
                positions[i] = workingPositions[i];
                print(positions[i]);
            }

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);
            
            var randInt = Random.Range(0, 2);
            if (randInt == 0) 
            {
                AudioManager.singleton.PlaySound("attackPlace1", false, 0.5f);
            }
            else 
            {
                AudioManager.singleton.PlaySound("attackPlace2", false, 0.5f);
            }

            if (NetworkManager.Singleton)
            {
                DamageInstance instance = new DamageInstance(
                    name: currentlySelectedUnit.Name,
                    id: GameManager.instance.playerId,
                    damage: currentlySelectedUnit.Damage,
                    positions: positions
                );

                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    AddDamageInstanceRpc(instance, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                AddDamageInstanceLocal(new DamageInstance(
                    name: currentlySelectedUnit.Name,
                    id: GameManager.instance.playerId,
                    damage: currentlySelectedUnit.Damage,
                    positions: positions
                ));
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
            currentlySelectedUnitIndex = -1;

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);

            UpdateTileVisuals();
        }

        if (Input.GetMouseButtonDown(0))
        {
            
            
            ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.Cost);

            UIManager.Instance.interactionState = UIManager.InteractionState.None;
            UIManager.Instance.EnableControlsText();
            foreach (Vector2Int position in workingPositions)
            {
                localBoard.TileTransforms[position.x, position.y].GetComponent<tileColour>()
                    .TileRecieveSignal(0, false);
            }

            Vector2Int[] positions = new Vector2Int[workingPositions.Count];

            currentlySelectedUnit.HasActed = true;
            if (currentlySelectedUnitIndex >= 0 && currentlySelectedUnitIndex < unitsCount)
            {
                unitsList[currentlySelectedUnitIndex] = currentlySelectedUnit;
            }

            for (int i = 0; i < workingPositions.Count; i++)
            {
                positions[i] = workingPositions[i];
                print(positions[i]);
            }

            //currentSelectedTileGameObject.GetComponent<Outline>().OutlineColor = Color.black;

            currentSelectedTileGameObject = null;
            CurrentSelectedTile = new Vector2Int(-1, 1);
            
            var randInt = Random.Range(0, 2);
            
            if (randInt == 0) 
            {
                AudioManager.singleton.PlaySound("defensePlace1", false, 0.5f);
            }
            else 
            {
                AudioManager.singleton.PlaySound("defensePlace2", false, 0.5f);
            }

            if (NetworkManager.Singleton)
            {
                DefenseInstance instance = new DefenseInstance(
                    name: currentlySelectedUnit.Name,
                    id: GameManager.instance.playerId,
                    defense: currentlySelectedUnit.Defense,
                    positions: positions
                );

                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    AddDefenseInstanceRpc(instance, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                AddDefenseInstanceLocal(new DefenseInstance(
                    name: currentlySelectedUnit.Name,
                    id: GameManager.instance.playerId,
                    defense: currentlySelectedUnit.Defense,
                    positions: positions
                ));
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
        if (currentlySelectedUnitIndex < 0 || currentlySelectedUnitIndex >= unitsCount) return;

        if (currentlySelectedUnit.Movement > 0 &&
            !currentAdjacentPositions[direction].Equals(new Vector2Int(-1, -1)))
        {
            int unitIndex = currentlySelectedUnitIndex;
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
            unitsList[unitIndex] = currentlySelectedUnit;
            CurrentSelectedTile = currentAdjacentPositions[direction];
            
            var randInt = Random.Range(0, 2);
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
                    MoveCardRpc(unitIndex, currentAdjacentPositions[direction],
                        RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }


            foreach (var tile in localBoard.TileTransforms)
            {
                tile.GetComponent<tileColour>().TileRecieveSignal(0, false);
            }
            
            ClearTiles();
            UpdateTileVisuals();

            currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile, unitsList, unitsCount, GameManager.instance.playerId);

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
        unitsList[index] = unitToMove;
    }


    [Rpc(SendTo.SpecifiedInParams)]
    public void AddDefenseInstanceRpc(DefenseInstance instance, RpcParams rpcParams = default)
    {
        AddDefenseInstance(instance);

        UpdateTileVisuals();
    }
    
    public void AddDefenseInstanceLocal(DefenseInstance instance)
    {
        AddDefenseInstance(instance);

        UpdateTileVisuals();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void AddDamageInstanceRpc(DamageInstance instance, RpcParams rpcParams = default)
    {
        AddDamageInstance(instance);

        UpdateTileVisuals();
    }
    
    public void AddDamageInstanceLocal(DamageInstance instance)
    {
        AddDamageInstance(instance);

        UpdateTileVisuals();
    }

    public void UpdateTileVisuals()
    {
        UpdateBoardTileVisuals(player1Board, Player.PlayerId.Player2, Player.PlayerId.Player1,
            damageInstances, damageInstanceCount, defenseInstances, defenseInstanceCount);
        UpdateBoardTileVisuals(player2Board, Player.PlayerId.Player1, Player.PlayerId.Player2,
            damageInstances, damageInstanceCount, defenseInstances, defenseInstanceCount);
    }


    public void PlaceCard(GameObject cardVisual, CardDeck.CardData cardData, GameObject tile)
    {
        Vector2Int coordinates = CoordinatesOf<GameObject>(localBoard.TileTransforms, tile);

        Unit unit = new Unit(
            name: cardData.Name,
            cardID: cardData.ID,
            cost: cardData.Cost,
            id: GameManager.instance.playerId,
            health: cardData.Health,
            damage: cardData.Damage,
            defense: cardData.Defence,
            movement: cardData.Speed,
            attackPositions: cardData.Range,
            position: coordinates,
            hasActed: true
        );

        if (!AddUnit(unit)) return;

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
                PlaceCardRpc(unit, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
            }
        }
        
       

        
    }

    private CardDeck.CardData CardDataFromUnit(Unit unit)
    {
        CardDeck.CardData cardData = new CardDeck.CardData
        {
            ID = unit.CardID,
            Name = unit.Name,
            Health = unit.Health,
            Cost = unit.Cost,
            Speed = unit.Movement,
            Defence = unit.Defense,
            Damage = unit.Damage,
            Range = new List<Vector2Int>(unit.AttackPositionCount)
        };

        for (int i = 0; i < unit.AttackPositionCount; i++)
        {
            cardData.Range.Add(unit.AttackPositions[i]);
        }

        return cardData;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void PlaceCardRpc(Unit unit, RpcParams rpcParams = default)
    {
        CardDeck.CardData cardData = CardDataFromUnit(unit);

        if (!AddUnit(unit)) return;

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
        workingPositions = null;
        currentlySelectedUnit = default;
        currentlySelectedUnitIndex = -1;

        for (int i = 0; i < unitsCount; i++)
        {
            Unit unit = unitsList[i];
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositionCount);
                for (int attackIndex = 0; attackIndex < unit.AttackPositionCount; attackIndex++)
                {
                    workingPositions.Add(unit.AttackPositions[attackIndex]);
                }
                currentlySelectedUnit = unit;
                currentlySelectedUnitIndex = i;
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
        workingPositions = null;
        currentlySelectedUnit = default;
        currentlySelectedUnitIndex = -1;

        for (int i = 0; i < unitsCount; i++)
        {
            Unit unit = unitsList[i];
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositionCount);
                for (int attackIndex = 0; attackIndex < unit.AttackPositionCount; attackIndex++)
                {
                    workingPositions.Add(unit.AttackPositions[attackIndex]);
                }
                currentlySelectedUnit = unit;
                currentlySelectedUnitIndex = i;
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
        workingPositions = null;
        currentlySelectedUnit = default;
        currentlySelectedUnitIndex = -1;

        for (int i = 0; i < unitsCount; i++)
        {
            Unit unit = unitsList[i];
            if (unit.Position == CurrentSelectedTile && unit.ID == GameManager.instance.playerId)
            {
                workingPositions = new List<Vector2Int>(unit.AttackPositionCount);
                for (int attackIndex = 0; attackIndex < unit.AttackPositionCount; attackIndex++)
                {
                    workingPositions.Add(unit.AttackPositions[attackIndex]);
                }
                currentlySelectedUnit = unit;
                currentlySelectedUnitIndex = i;
            }
        }
        
        ManaManager.instance.RemoveManaPoints(currentlySelectedUnit.Cost);
        if (workingPositions == null) return;

        ClearTiles();
        UpdateTileVisuals();

        currentAdjacentPositions = GetAdjacentTiles(CurrentSelectedTile, unitsList, unitsCount, GameManager.instance.playerId);

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
        return GetAdjacentTiles(position, unitsList, unitsCount, GameManager.instance.playerId);
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

                    bool cardDied = false;
                    bool damageInstancePresent = false;
                    bool defenseInstancePresent = false;

                    for (int damageIndex = 0; damageIndex < damageInstanceCount; damageIndex++)
                    {
                        DamageInstance damageInstance = damageInstances[damageIndex];
                        if (damageInstance.ContainsPosition(new Vector2Int(i, j)) &&
                            damageInstance.ID == Player.PlayerId.Player2)
                        {
                            damageInstancePresent = true;
                            workingDamage += damageInstance.Damage;
                        }
                    }

                    for (int defenseIndex = 0; defenseIndex < defenseInstanceCount; defenseIndex++)
                    {
                        DefenseInstance defenseInstance = defenseInstances[defenseIndex];
                        if (defenseInstance.ContainsPosition(new Vector2Int(i, j)) &&
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
                    for (int unitIndex = 0; unitIndex < unitsCount; unitIndex++)
                    {
                        Unit unit = unitsList[unitIndex];
                        if (unit.ID == Player.PlayerId.Player2) continue;

                        if (Equals(unit.Position, new Vector2Int(i, j)))
                        {
                            unit.Health -= workingDamage;
                            unitsList[unitIndex] = unit;
                            player1Board.Visuals[i, j].GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                                unit.Health.ToString();
                            attackBlocked = true;
                            var thing = player1Board.Visuals[i, j].GetComponent<SpriteRenderer>();
                            var colorTween = new ColorTween
                            {
                                from = thing.color,
                                to = Color.red,
                                duration = 0.15f,
                                easeType = EaseType.SineInOut,
                                usePingPong = true,
                                onUpdate = (_, value) => thing.color = value
                            };
                            thing.gameObject.AddTween(colorTween);
                            if (unit.Health <= 0)
                            {
                                cardDied = true;
                                PruneUnitVisuals();
                                var randInt = Random.Range(0, 10);
                                if (randInt == 9) 
                                {
                                    AudioManager.singleton.PlaySound("cardDie", true, 0.3f);
                                }
                                else 
                                {
                                    AudioManager.singleton.PlaySound("cardDie2", true, 0.3f);
                                }
                            }
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
                        if (workingDamage > 0)
                        {
                            AudioManager.singleton.PlaySound("cardAttack", true, 0.8f);
                        }
                        else
                        {
                            AudioManager.singleton.PlaySound("attackBlocked", true, 0.8f);
                        }
                            
                    }
                    else
                    {
                        if (workingDamage > 0)
                        {
                            //Play board attack sounds
                            AudioManager.singleton.PlaySound("boardAttack", true, 0.8f);
                            
                            StartCoroutine(tileFleshScript1.Pulse(attackDelay / 1000));
                        }
                        else
                        {
                            //Play card defend sounds
                            AudioManager.singleton.PlaySound("attackBlocked", true, 0.8f);
                        }
                    }

                    if (attackBlocked)
                    {
                        if (cardDied)
                        {
                            tc.TileRecievePopup(0, 3);
                        }
                        else
                        {
                            tc.TileRecievePopup(workingDamage, 0);
                        }
                        
                    }
                    else
                    {
                        tc.TileRecievePopup(workingDamage, 2);
                    }

                    await Task.Delay(attackDelay);
                }
            }

            RemoveDamageInstancesForPlayer(Player.PlayerId.Player2);
            RemoveDefenseInstancesForPlayer(Player.PlayerId.Player1);
        }
        //Evaluate Player 2 Damage
        else
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int workingDamage = 0;

                    bool cardDied = false;
                    bool damageInstancePresent = false;
                    bool defenseInstancePresent = false;

                    for (int damageIndex = 0; damageIndex < damageInstanceCount; damageIndex++)
                    {
                        DamageInstance damageInstance = damageInstances[damageIndex];
                        if (damageInstance.ContainsPosition(new Vector2Int(i, j)) &&
                            damageInstance.ID == Player.PlayerId.Player1)
                        {
                            damageInstancePresent = true;
                            workingDamage += damageInstance.Damage;
                        }
                    }

                    for (int defenseIndex = 0; defenseIndex < defenseInstanceCount; defenseIndex++)
                    {
                        DefenseInstance defenseInstance = defenseInstances[defenseIndex];
                        if (defenseInstance.ContainsPosition(new Vector2Int(i, j)) &&
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
                    for (int unitIndex = 0; unitIndex < unitsCount; unitIndex++)
                    {
                        Unit unit = unitsList[unitIndex];
                        if (unit.ID == Player.PlayerId.Player1) continue;

                        if (Equals(unit.Position, new Vector2Int(i, j)))
                        {
                            unit.Health -= workingDamage;
                            unitsList[unitIndex] = unit;
                            player2Board.Visuals[i, j].GetComponentsInChildren<TextMeshProUGUI>()[1].text =
                                unit.Health.ToString();
                            attackBlocked = true;
                            var thing = player2Board.Visuals[i, j].GetComponent<SpriteRenderer>();
                            var colorTween = new ColorTween
                            {
                                from = thing.color,
                                to = Color.red,
                                duration = 0.15f,
                                easeType = EaseType.SineInOut,
                                usePingPong = true,
                                onUpdate = (_, value) => thing.color = value
                            };
                            thing.gameObject.AddTween(colorTween);
                            if (unit.Health <= 0)
                            {
                                cardDied = true;
                                PruneUnitVisuals();
                                var randInt = Random.Range(0, 10);
                                if (randInt == 9) 
                                {
                                    AudioManager.singleton.PlaySound("cardDie", true, 0.3f);
                                }
                                else 
                                {
                                    AudioManager.singleton.PlaySound("cardDie2", true, 0.3f);
                                }
                                
                            }
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
                        if (workingDamage > 0)
                        {
                            AudioManager.singleton.PlaySound("cardAttack", true, 0.8f);
                        }
                        else
                        {
                            AudioManager.singleton.PlaySound("attackBlocked", true, 0.8f);
                        }
                            
                    }
                    else
                    {
                        if (workingDamage > 0)
                        {
                            //Play board attack sounds
                            AudioManager.singleton.PlaySound("boardAttack", true, 0.8f);
                            
                            StartCoroutine(tileFleshScript2.Pulse(attackDelay / 1000));
                        }
                        else
                        {
                            //Play card defend sounds
                            AudioManager.singleton.PlaySound("attackBlocked", true, 0.8f);
                        }
                    }

                    if (attackBlocked)
                    {
                        if (cardDied)
                        {
                            tc.TileRecievePopup(0, 3);
                        }
                        else
                        {
                            tc.TileRecievePopup(workingDamage, 0);
                        }
                        
                    }
                    else
                    {
                        tc.TileRecievePopup(workingDamage, 2);
                    }
                    
                    
                    
                    await Task.Delay(attackDelay);
                }
                
            }

            RemoveDamageInstancesForPlayer(Player.PlayerId.Player1);
            RemoveDefenseInstancesForPlayer(Player.PlayerId.Player2);
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
        await Task.Delay(attackDelay);
        attackInProgress = false;
    }

    public void PruneUnitVisuals()
    {
        for (int i = 0; i < unitsCount; i++)
        {
            Unit unit = unitsList[i];
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

                cardDied.Invoke();
            }
        }

         
    }

    public void PruneUnitList()
    {
        for (int i = unitsCount - 1; i >= 0; i--)
        {
            if (unitsList[i].Health <= 0)
            {
                RemoveUnitAt(i);
            }
        }
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

        for (int i = 0; i < unitsCount; i++)
        {
            Unit unit = unitsList[i];
            if (unit.ID == id)
            {
                totalCards++;
            }
        }

        return totalCards;
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
