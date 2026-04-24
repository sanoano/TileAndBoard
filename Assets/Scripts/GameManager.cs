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
    private static readonly int Rotation = Shader.PropertyToID("_Rotation");

    public Player.PlayerId playerId;

    private string playerName;

    [SerializeField] private Material skybox;

    [Header("Parameters")] 
    [SerializeField] private float rotationSpeed;

    
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
        
        if (playerId == Player.PlayerId.Player2)
        {
            Vector3 pos = new Vector3(-39.7999992f, 36.6399994f, -1.20000005f);
            Quaternion rot = Quaternion.Euler(45, 90, 0);

            Camera.main.transform.position = pos;
            Camera.main.transform.rotation = rot;
        }
        
        //Application.targetFrameRate = 60;

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
        if (playerId == Player.PlayerId.Player1)
        {
            UIManager.Instance.player1Name.text = trimmedName;
        }
        else
        {
            UIManager.Instance.player2Name.text = trimmedName;

        }
        
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void SetPlayer2NameRpc(string name, RpcParams rpcParams = default)
    {
        string trimmedName = name.Substring(0, name.Length - 5); 
        if (playerId == Player.PlayerId.Player1)
        {
            UIManager.Instance.player2Name.text = trimmedName;
        }
        else
        {
            UIManager.Instance.player1Name.text = trimmedName;

        }
    }

    public async void DisconnectUser()
    {
        if (NetworkManager.Singleton)
        {
            await NetworkManager.Singleton.gameObject.GetComponent<Lobby>().LeaveSessionAsync();
        }
        else
        {
            print("Game Ended");
        }
        StopAllCoroutines();
    }

    private void Start()
    {
        

    }

    private void FixedUpdate()
    {
        skybox.SetFloat(Rotation, Time.time * rotationSpeed);
    }

    
    
}
