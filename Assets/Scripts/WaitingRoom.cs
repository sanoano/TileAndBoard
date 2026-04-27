using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tweens;

public class WaitingRoom : NetworkBehaviour
{

    private Lobby lobby;

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private GameObject waitingText;
    [SerializeField] private GameObject startGameButtonObject;
    private Button startGameButton;
    [SerializeField] private Button leaveGameButton;
    [SerializeField] private Image lensCap;
    [SerializeField] private GameObject otherIsland;

    [Header("Parameters")] 
    [SerializeField] private float fadeDuration;
    

    private void Start()
    {
        startGameButton = startGameButtonObject.GetComponent<Button>();

        lobby = NetworkManager.Singleton.gameObject.GetComponent<Lobby>();
        startGameButton.onClick.AddListener(StartGame);
        leaveGameButton.onClick.AddListener(LeaveGame);
        lobby.m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;

        otherIsland.SetActive(false);
    }

    private void Update()
    {
        if (NetworkManager.Singleton.LocalClient.IsSessionOwner && NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            startGameButtonObject.SetActive(true);
            waitingText.SetActive(false);
            otherIsland.SetActive(true);
        }
        else
        {
            startGameButtonObject.SetActive(false);
            waitingText.SetActive(true);
        }
        
        UpdatePlayerList();
        
        sessionNameText.text = lobby._session.Name;
        joinCodeText.text = $"Join Code: {lobby._session.Code}";
    }

    private void UpdatePlayerList()
    {
        playerListText.text = "";
        
        foreach (var player in lobby._session.Players)
        {
            var name = player.GetPlayerName() ?? "Unknown";
            string trimmedName = name.Substring(0, name.Length - 5); // Removes the username suffix ie. #XXXX

            playerListText.text += $"{trimmedName}\n";
        }
    }

    private void OnClientConnectedCallback(ulong id)
    {
        UpdatePlayerList();
    }

    private void StartGame()
    {
        foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
        {
            StartGameHandlerRpc(RpcTarget.Single(clientIds, RpcTargetUse.Temp));
        }

        startGameButton.interactable = false;
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    private void StartGameHandlerRpc(RpcParams rpcParams = default)
    {
        StartCoroutine(StartGameRoutine());
    }


    private IEnumerator StartGameRoutine()
    {
        
        lensCap.gameObject.SetActive(true);
        
        Color opaque = new Color(0, 0, 0, 255);
        
        var backgroundTween = new ColorTween {
            from = lensCap.color,
            to = opaque,
            duration = fadeDuration,
            easeType = EaseType.ExpoInOut,
            onUpdate = (_, value) => lensCap.color = value,
        };

        var instance = lensCap.gameObject.AddTween(backgroundTween);

        yield return instance.AwaitDecommission();

        if (NetworkManager.Singleton.LocalClient.IsSessionOwner)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("BattleArena", LoadSceneMode.Single);
        }
        

        yield return null;

    }

    private void LeaveGame()
    {
        lobby.LeaveSessionAsync();
    }
}
