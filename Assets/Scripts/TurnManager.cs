using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
        
        
    }

    public override void OnNetworkSpawn()
    {
        turnButton.onClick.AddListener(ChangeTurn);
        currentTurn = TurnState.Player1Turn;
        UpdateTurnText(currentTurn);

        if (GameManager.instance.playerId == Player.PlayerId.Player2)
        {
            turnButton.gameObject.SetActive(false);
            isYourTurn = false;
        }

        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            isYourTurn = true;
        }
        
    }

    public enum TurnState : byte
    {
        Player1Turn,
        Player2Turn,
    }

    public void UpdateTurnText(TurnState turnState)
    {

        if (turnState == TurnState.Player1Turn)
        {
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player1 ? "Your Turn" : "Waiting...";
        }
        else
        {
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player2 ? "Your Turn" : "Waiting...";
        }
        
    }
    
    public void OnTurnChanged(TurnState current)
    {
        Debug.Log("Turn changed to " + current);

        switch (current)
        {
            case TurnState.Player1Turn:
                
                TacticsManager.instance.AddTacticsPoints(TacticsManager.instance.tacticsPointsPerTurn);
                TacticsManager.instance.currentActions = TacticsManager.instance.actionsPerTurn;
                
                if (GameManager.instance.playerId == Player.PlayerId.Player1)
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
                }
                
                BoardManager.Instance.EvaluateDamage(Player.PlayerId.Player2);

                break;
            
            
            case TurnState.Player2Turn:
                
                if (GameManager.instance.playerId == Player.PlayerId.Player2)
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
                }
                
                BoardManager.Instance.EvaluateDamage(Player.PlayerId.Player1);

                break;
        }
        
    }

    public void ChangeTurn()
    {

        if (UIManager.Instance.interactionState != UIManager.InteractionState.None) return;
        
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
        AudioManager.singleton.PlaySound(Resources.Load<AudioClip>("Audio/SFX/peg falling"));
        isYourTurn = !isYourTurn;
    }
    
}
