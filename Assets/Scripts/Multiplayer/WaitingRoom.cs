using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Tweens;

public class WaitingRoom : NetworkBehaviour
{
    private const string ReadyPropertyKey = "ready";

    private Lobby lobby;
    private bool isUpdatingReadyState;
    private bool isStartingGame;

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private GameObject waitingText;
    private TextMeshProUGUI waitingStatusText;
    [SerializeField] private GameObject startGameButtonObject;
    private Button startGameButton;
    [SerializeField] private Button readyGameButton;
    private TextMeshProUGUI readyGameButtonText;
    [SerializeField] private Button leaveGameButton;
    [SerializeField] private Image lensCap;
    [SerializeField] private GameObject otherIsland;

    [Header("Parameters")] 
    [SerializeField] private float fadeDuration;
    

    private async void Start()
    {
        startGameButton = startGameButtonObject.GetComponent<Button>();
        readyGameButtonText = readyGameButton.GetComponentInChildren<TextMeshProUGUI>();
        waitingStatusText = waitingText.GetComponent<TextMeshProUGUI>();

        lobby = NetworkManager.Singleton.gameObject.GetComponent<Lobby>();
        startGameButton.onClick.AddListener(StartGame);
        readyGameButton.onClick.AddListener(ToggleReady);
        leaveGameButton.onClick.AddListener(LeaveGame);
        lobby.m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        lobby.m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;

        otherIsland.SetActive(false);
        await SetReadyStateAsync(false);
    }

    private void Update()
    {
        bool isSessionOwner = NetworkManager.Singleton.LocalClient.IsSessionOwner;
        bool canStartGame = CanStartGame();

        startGameButtonObject.SetActive(isSessionOwner);
        startGameButton.interactable = !isStartingGame && canStartGame;
        waitingText.SetActive(!isSessionOwner);
        otherIsland.SetActive(NetworkManager.Singleton.ConnectedClientsIds.Count == 2);

        UpdatePlayerList();
        UpdateReadyButton();
        UpdateWaitingText();
        
        sessionNameText.text = lobby._session.Name;
        joinCodeText.text = $"Join Code: {lobby._session.Code}";
    }

    private void UpdatePlayerList()
    {
        playerListText.text = "";
        
        foreach (var player in lobby._session.Players)
        {
            var name = player.GetPlayerName() ?? "Unknown";
            int suffixIndex = name.LastIndexOf('#');
            string trimmedName = suffixIndex > 0 ? name.Substring(0, suffixIndex) : name;
            string readyStatus = IsPlayerReady(player) ? "READY" : "NOT READY";

            playerListText.text += $"{trimmedName} [{readyStatus}]\n";
        }
    }

    private void OnClientConnectedCallback(ulong id)
    {
        UpdatePlayerList();
    }

    private void OnClientDisconnectCallback(ulong id)
    {
        UpdatePlayerList();
    }

    private async void ToggleReady()
    {
        await SetReadyStateAsync(!IsLocalPlayerReady());
    }

    private async Task SetReadyStateAsync(bool isReady)
    {
        if (isStartingGame || isUpdatingReadyState || lobby?._session?.CurrentPlayer == null)
        {
            return;
        }

        bool previousReadyState = IsLocalPlayerReady();
        isUpdatingReadyState = true;
        readyGameButton.interactable = false;

        try
        {
            lobby._session.CurrentPlayer.SetProperty(
                ReadyPropertyKey,
                new PlayerProperty(isReady.ToString(), VisibilityPropertyOptions.Public));
            await lobby._session.SaveCurrentPlayerDataAsync();
        }
        catch (Exception exception)
        {
            lobby._session.CurrentPlayer.SetProperty(
                ReadyPropertyKey,
                new PlayerProperty(previousReadyState.ToString(), VisibilityPropertyOptions.Public));
            Debug.LogException(exception);
        }
        finally
        {
            isUpdatingReadyState = false;
            UpdateReadyButton();
        }
    }

    private void UpdateReadyButton()
    {
        readyGameButton.interactable = !isStartingGame && !isUpdatingReadyState;
        readyGameButtonText.text = IsLocalPlayerReady() ? "Unready" : "Ready Up";
    }

    private void UpdateWaitingText()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2)
        {
            waitingStatusText.text = "Waiting for other player...";
        }
        else if (AreAllPlayersReady())
        {
            waitingStatusText.text = "Waiting for host to start...";
        }
        else
        {
            waitingStatusText.text = "Waiting for both players to ready up...";
        }
    }

    private bool CanStartGame()
    {
        return NetworkManager.Singleton.LocalClient.IsSessionOwner && AreAllPlayersReady();
    }

    private bool AreAllPlayersReady()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count != 2 ||
            lobby._session.Players.Count != 2)
        {
            return false;
        }

        foreach (var player in lobby._session.Players)
        {
            if (!IsPlayerReady(player))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsLocalPlayerReady()
    {
        return lobby?._session?.CurrentPlayer != null && IsPlayerReady(lobby._session.CurrentPlayer);
    }

    private static bool IsPlayerReady(IReadOnlyPlayer player)
    {
        return player.Properties != null &&
               player.Properties.TryGetValue(ReadyPropertyKey, out var readyProperty) &&
               bool.TryParse(readyProperty.Value, out bool isReady) &&
               isReady;
    }

    private void StartGame()
    {
        if (isStartingGame || !CanStartGame())
        {
            return;
        }

        isStartingGame = true;

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

    private async void LeaveGame()
    {
        await lobby.LeaveSessionAsync();
    }

    public override void OnDestroy()
    {
        if (startGameButton != null)
        {
            startGameButton.onClick.RemoveListener(StartGame);
        }

        if (readyGameButton != null)
        {
            readyGameButton.onClick.RemoveListener(ToggleReady);
        }

        if (leaveGameButton != null)
        {
            leaveGameButton.onClick.RemoveListener(LeaveGame);
        }

        if (lobby?.m_NetworkManager != null)
        {
            lobby.m_NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            lobby.m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        base.OnDestroy();
    }
}
