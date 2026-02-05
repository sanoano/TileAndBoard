using System;
using TMPro;
using Unity.Entities;
using Unity.Netcode;
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

        

    }

    

    //In this instance, because this scene is loaded using the scene manager, Start is actually called before OnNetworkSpawn,
    //keep this in mind when considering execution order.
    public override void OnNetworkSpawn()
    {

        headInstance = Instantiate(playerHead);

        headInstanceNO = headInstance.GetComponent<NetworkObject>();
        
        headInstanceNO.Spawn(false);

    }

    private void Start()
    {
        

    }

    private void Update()
    {
        
    }

    
    
}
