using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{

    public static GameManager instance;

    public Player.PlayerId playerId;

    
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

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

    }

    private void OnClientDisconnect(ulong clientId)
    {
        NetworkManager.Singleton.gameObject.GetComponent<ConnectionManager>()._session.LeaveAsync();
        SceneManager.LoadScene("Lobby");
    }

    //In this instance, because this scene is loaded using the scene manager, Start is actually called before OnNetworkSpawn,
    //keep this in mind when considering execution order.
    public override void OnNetworkSpawn()
    {
        
       
    }

    private void Start()
    {
        

    }

    private void Update()
    {
        
    }

    
    
}
