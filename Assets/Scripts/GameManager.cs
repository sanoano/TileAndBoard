using System;
using TMPro;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{

    [SerializeField] private GameObject playerHead;
    private GameObject headInstance;
    private NetworkObject headInstanceNO;

    public static GameManager instance;

    public Player.PlayerId playerId;

    private string playerName;

    
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
        
        playerId = Player.AssignPlayerID();
        
        Application.targetFrameRate = 60;

    }

    

    //In this instance, because this scene is loaded using the scene manager, Start is actually called before OnNetworkSpawn,
    //keep this in mind when considering execution order.
    public override void OnNetworkSpawn()
    {

        headInstance = Instantiate(playerHead);

        headInstanceNO = headInstance.GetComponent<NetworkObject>();
        
        headInstanceNO.Spawn(false);
        
        GetPlayerName();
        
    }
    
    async void GetPlayerName()
    {
        try
        {
            playerName = await AuthenticationService.Instance.GetPlayerNameAsync();

            if (playerId == Player.PlayerId.Player1)
            {
                foreach (var clientIds in NetworkManager.ConnectedClientsIds)
                {
                    SetPlayer1NameRpc(playerName, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                foreach (var clientIds in NetworkManager.ConnectedClientsIds)
                {
                    SetPlayer2NameRpc(playerName, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
       
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void SetPlayer1NameRpc(string name, RpcParams rpcParams = default)
    {
        string trimmedName = name.Substring(0, name.Length - 5); // Removes the username suffix ie. #XXXX
        UIManager.Instance.player1Name.text = trimmedName;
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void SetPlayer2NameRpc(string name, RpcParams rpcParams = default)
    {
        string trimmedName = name.Substring(0, name.Length - 5); 
        UIManager.Instance.player2Name.text = trimmedName;
    }

    public async void DisconnectUser()
    {
        await NetworkManager.Singleton.gameObject.GetComponent<Lobby>().LeaveSessionAsync();

    }

    private void Start()
    {
        

    }

    private void Update()
    {
        
    }

    
    
}
