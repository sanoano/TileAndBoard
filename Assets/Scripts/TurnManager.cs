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
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player1 ? "Make Your Move!" : "Waiting...";
        }
        else
        {
            turnText.text = GameManager.instance.playerId == Player.PlayerId.Player2 ? "Make Your Move!" : "Waiting...";
        }
        
    }
    
    public void OnTurnChanged(TurnState current)
    {
        Debug.Log("Turn changed to " + current);

        if (GameManager.instance.playerId == Player.PlayerId.Player1 && current == TurnState.Player1Turn)
        {
            foreach (var unit in BoardManager.Instance.unitsList)
            {
                if (unit.ID == Player.PlayerId.Player1)
                {
                    unit.HasActed = false;
                }
            }
            
            TacticsManager.instance.AddTacticsPoints(1);
        }
        
        if (GameManager.instance.playerId == Player.PlayerId.Player2 && current == TurnState.Player2Turn)
        {
            foreach (var unit in BoardManager.Instance.unitsList)
            {
                if (unit.ID == Player.PlayerId.Player2)
                {
                    unit.HasActed = false;
                }
            }
            
            TacticsManager.instance.AddTacticsPoints(1);
        }
        
    }

    public void ChangeTurn()
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
