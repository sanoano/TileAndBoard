using System;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoom : NetworkBehaviour
{

    private Lobby lobby;

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button leaveGameButton;

    private void Awake()
    {
        
    }

    private void Start()
    {
        lobby = NetworkManager.Singleton.gameObject.GetComponent<Lobby>();
        startGameButton.onClick.AddListener(StartGame);
        leaveGameButton.onClick.AddListener(LeaveGame);
        lobby.m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        
        
    }

    private void Update()
    {
        if (NetworkManager.Singleton.LocalClient.IsSessionOwner && NetworkManager.Singleton.ConnectedClientsIds.Count == 2)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
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
            playerListText.text += $"{name}\n";
        }
    }

    private void OnClientConnectedCallback(ulong id)
    {
        UpdatePlayerList();
    }

    private void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene("Battle2", LoadSceneMode.Single);
    }

    private void LeaveGame()
    {
        lobby.LeaveSessionAsync();
    }
}
