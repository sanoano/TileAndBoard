using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class TurnManager : NetworkBehaviour
{

    public static TurnManager instance;

    public TurnState currentTurn;

    public bool isYourTurn;

    [Header("UI References")] 
    [SerializeField]
    private TextMeshProUGUI turnText;
    [SerializeField]
    private Button turnButton;

    [Header("Master Deck")] 
    [SerializeField] private CardDeck cardList;

    [Header("Turn Count")]
    public int turnCount = 1;

    [Header("Turn Timer")] 
    public float maxTimePerTurn = 180f;
    public float currentTime;

    [Header("Hourglass Stuff")]
    [SerializeField] private GameObject hourglass;
    private Hourglass hourglassScript;
    [HideInInspector] public bool isSide1 = true;
    [SerializeField] private Transform[] side1Transforms;//0 is on the left, 1 on the right
    [SerializeField] private Transform[] side2Transforms;
    
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        
        turnButton.onClick.AddListener(ChangeTurn);

    }    

    public override void OnNetworkSpawn()
    {
        
        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            int rand;

            rand = Random.Range(0, 2);

            if (rand == 0)
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    SetFirstTurnRpc(TurnState.Player1Turn, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    SetFirstTurnRpc(TurnState.Player2Turn, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
        }
        
        
        
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    private void SetFirstTurnRpc(TurnState turn, RpcParams rpcParams = default)
    {
        currentTurn = turn;
        
        UpdateTurnText(currentTurn);

        if (GameManager.instance.playerId == Player.PlayerId.Player2 && turn == TurnState.Player2Turn)
        {
            isYourTurn = true;
        }

        if (GameManager.instance.playerId == Player.PlayerId.Player1 && turn == TurnState.Player1Turn)
        {
            isYourTurn = true;
        }

        if (!isYourTurn)
        {
            turnButton.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        currentTime = maxTimePerTurn;
        
        if (!NetworkManager.Singleton)
        {
            currentTurn = TurnState.Player1Turn;
            isYourTurn = true;
            UpdateTurnText(currentTurn);
        }

        hourglassScript = hourglass.GetComponent<Hourglass>();

        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            hourglassScript.AssignPositions(side1Transforms[0], side1Transforms[1]);
            //Debug.Log(side1Transforms[0]);
            //Debug.Log(side1Transforms[1]);
        }
        else
        {
            hourglassScript.AssignPositions(side2Transforms[0], side2Transforms[1]);
            //Debug.Log(side2Transforms[0]);
            //Debug.Log(side2Transforms[1]);
        }

        //hourglassScript.FlipHourglass(maxTimePerTurn);
    }

    public enum TurnState : byte
    {
        Player1Turn,
        Player2Turn,
    }

    public void Update()
    {
        if (isYourTurn && !BoardManager.Instance.attackInProgress)
        {
            currentTime -= Time.deltaTime;
        }

        if (currentTime < 0)
        {
            ForceEndTurn();
        }
    }

    public void ForceEndTurn()
    {
        UIManager.Instance.interactionState = UIManager.InteractionState.None;
        OrbitCamera orbitCamera = Camera.main.GetComponent<OrbitCamera>();
        if (orbitCamera.cameraState == OrbitCamera.CameraState.Free)
        {
            orbitCamera.SwapCameraMode();
        }
        
        UIManager.Instance.EnableControlsText();
        
        BoardManager.Instance.ClearTiles();
        BoardManager.Instance.UpdateTileVisuals();
        BoardManager.Instance.NullSelection();
            
        ChangeTurn();
    }

    public void UpdateTurnText(TurnState turnState)
    {

        if (turnState == TurnState.Player1Turn)
        {
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player1 ? "Your Turn" : "Opponent's Turn";
        }
        else
        {
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player2 ? "Your Turn" : "Opponent's Turn";
        }
        
    }
    
    public async void OnTurnChanged(TurnState current)
    {
        Debug.Log("Turn changed to " + current);

        switch (current)
        {
            case TurnState.Player1Turn:
                
                turnCount += 1;
                await BoardManager.Instance.EvaluateDamage(Player.PlayerId.Player2);

                if (GameManager.instance.playerId == Player.PlayerId.Player1 && turnCount != 2)
                {
                    foreach (var unit in BoardManager.Instance.unitsList)
                    {
                        if (unit.ID == Player.PlayerId.Player1)
                        {
                            unit.HasActed = false;
                            CardDeck.CardData data =  cardList.Cards.Find(card => card.Name == unit.Name);
                            unit.Movement = data.Speed;
                        }
                    }
                    var manaToGain = 9 - BoardManager.Instance.GetCardAmount(Player.PlayerId.Player1);
                    ManaManager.instance.AddManaPoints(manaToGain);
                    if (manaToGain > 0)
                    {
                        AudioManager.singleton.PlaySound("manaGain", false);
                    }
                    foreach (var tile in BoardManager.Instance.localBoard.TileTransforms)
                    {
                        var coords = BoardManager.Instance.CoordinatesOf<GameObject>(BoardManager.Instance.localBoard.TileTransforms, tile);
                        bool cardFound = false;
                        foreach(BoardManager.Unit unit in BoardManager.Instance.unitsList)
                        {
                           if (Equals(unit.Position, coords) && unit.ID == Player.PlayerId.Player1)
                            {
                                cardFound = true;
                            }
                        }
                        if (!cardFound)
                        {
                            tile.GetComponent<tileColour>().TileRecievePopup(1, 1);
                        } 
                        
                    }
                }
                

                

                break;
            
            
            case TurnState.Player2Turn:

                turnCount += 1;
                await BoardManager.Instance.EvaluateDamage(Player.PlayerId.Player1);
                
                if (GameManager.instance.playerId == Player.PlayerId.Player2 && turnCount != 2)
                {
                    foreach (var unit in BoardManager.Instance.unitsList)
                    {
                        if (unit.ID == Player.PlayerId.Player2)
                        {
                            unit.HasActed = false;
                            CardDeck.CardData data =  cardList.Cards.Find(card => card.Name == unit.Name);
                            unit.Movement = data.Speed;
                        }
                    }
                    
                    var manaToGain = 9 - BoardManager.Instance.GetCardAmount(Player.PlayerId.Player2);
                    ManaManager.instance.AddManaPoints(manaToGain);
                    if (manaToGain > 0)
                    {
                        AudioManager.singleton.PlaySound("manaGain", false);
                    }
                    foreach (var tile in BoardManager.Instance.localBoard.TileTransforms)
                    {
                        var coords = BoardManager.Instance.CoordinatesOf<GameObject>(BoardManager.Instance.localBoard.TileTransforms, tile);
                        bool cardFound = false;
                        foreach(BoardManager.Unit unit in BoardManager.Instance.unitsList)
                        {
                           if (Equals(unit.Position, coords) && unit.ID == Player.PlayerId.Player2)
                            {
                                cardFound = true;
                            }
                        }
                        if (!cardFound)
                        {
                            tile.GetComponent<tileColour>().TileRecievePopup(1, 1);
                        } 
                        
                    }
                }
                

                

                break;
        }
        
    }

    public void ChangeTurn()
    {

        currentTime = maxTimePerTurn;


        if (UIManager.Instance.interactionState != UIManager.InteractionState.None)
        {
            TextDialogue.instance.DialogueRecieveStatus(8);
            return; 
        } 
        if (BoardManager.Instance.attackInProgress) return;
        
        UIManager.Instance.DestroyCurrentInfoInstance();

        if (NetworkManager.Singleton)
        {
            if (currentTurn == TurnState.Player1Turn && GameManager.instance.playerId == Player.PlayerId.Player1)
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    ChangeTurnRpc(TurnState.Player2Turn, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    ChangeTurnRpc(TurnState.Player1Turn, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
        }
        else
        {
            print("meep");
            ChangeTurnLocal(TurnState.Player2Turn);
            ChangeTurnLocal(TurnState.Player1Turn);
        }
        
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    private void ChangeTurnRpc(TurnState turn, RpcParams rpcParams = default)
    {
        if (turnButton.IsActive() == false)
        {
            turnButton.gameObject.SetActive(true);
        }
        else
        {
            turnButton.gameObject.SetActive(false);
        }
        currentTurn = turn;
        UpdateTurnText(currentTurn);
        OnTurnChanged(currentTurn);
        //AudioManager.singleton.PlaySound("roundChange", false);
        hourglassScript.FlipHourglass(maxTimePerTurn);
        isYourTurn = !isYourTurn;
    }
    
    private void ChangeTurnLocal(TurnState turn)
    {
        if (turnButton.IsActive() == false)
        {
            turnButton.gameObject.SetActive(true);
        }
        else
        {
            turnButton.gameObject.SetActive(false);
        }
        currentTurn = turn;
        UpdateTurnText(currentTurn);
        OnTurnChanged(currentTurn);
        //AudioManager.singleton.PlaySound("roundChange", false);
        hourglassScript.FlipHourglass(maxTimePerTurn);
        isYourTurn = !isYourTurn;
    }
    
}
