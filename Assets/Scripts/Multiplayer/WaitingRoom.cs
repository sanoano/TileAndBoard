//using Mono.Cecil.Cil;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using Tweens;
using Unity.Collections;
using Unity.Entities.UniversalDelegates;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingRoom : NetworkBehaviour
{
    public static WaitingRoom Instance;

    private const string ReadyPropertyKey = "ready";

    private Lobby lobby;
    private bool isUpdatingReadyState;
    private bool isStartingGame;
    private readonly NetworkVariable<FixedString64Bytes> lanHostName = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<FixedString64Bytes> lanClientName = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> lanHostReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<bool> lanClientReady = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [Header("UI References")] 
    [SerializeField] private Button readyGameButton;
    

    private async void Start()
    {     
        lobby = NetworkManager.Singleton.gameObject.GetComponent<Lobby>();
        
        lobby.m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        lobby.m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnectCallback;

        
        if (!lobby.IsLanSession)
        {
            await SetReadyStateAsync(false);
        }

        UIManagerLobby.Instance.UpdateReadyButton(0);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        lobby = NetworkManager.Singleton.gameObject.GetComponent<Lobby>();
        if (!lobby.IsLanSession) return;

        RegisterLanPlayerRpc(new FixedString64Bytes(lobby.PlayerDisplayName));
    }

    private void Update()
    {
        //bool canStartGame = CanStartGame();
        bool playersReady = AreAllPlayersReady();

        //startGameButtonObject.SetActive(isSessionOwner);
        //startGameButton.interactable = !isStartingGame && canStartGame;
        //waitingText.SetActive(!isSessionOwner);

        UIManagerLobby.Instance.EnableIsland(NetworkManager.Singleton.ConnectedClientsIds.Count == 2);

        UpdatePlayerName();
        UpdatePlayerStatus();
        
        UpdateReadyButton();
        UpdateWaitingText();

        if (lobby.IsLanSession)
        {
            UIManagerLobby.Instance.UpdateSessionInfo(lobby.LanSessionName, "Nearby game");
        }
        else
        {
            UIManagerLobby.Instance.UpdateSessionInfo(lobby._session.Name, $"Join Code: {lobby._session.Code}");
        }

        if (playersReady)
        {
            StartCoroutine(StartCountdown());
        }
            

    }

    private void UpdatePlayerName()
    {
        if (lobby.IsLanSession)
        {
            UIManagerLobby.Instance.UpdatePlayerName(true, lanHostName.Value.ToString());
            UIManagerLobby.Instance.UpdatePlayerName(false, lanClientName.Value.ToString());
            return;
        }

        for (int i = 0; i < lobby._session.Players.Count; i++)
        {
            var player = lobby._session.Players[i];

            var name = player.GetPlayerName() ?? "Unknown";
            int suffixIndex = name.LastIndexOf('#');
            string trimmedName = suffixIndex > 0 ? name.Substring(0, suffixIndex) : name;

            UIManagerLobby.Instance.UpdatePlayerName(i == 0, trimmedName);
        }
    }

    private void UpdatePlayerStatus()
    {
        if (lobby.IsLanSession)
        {
            UIManagerLobby.Instance.UpdatePlayerStatus(true, lanHostReady.Value ? "READY" : "NOT READY");
            UIManagerLobby.Instance.UpdatePlayerStatus(
                false,
                string.IsNullOrEmpty(lanClientName.Value.ToString())
                    ? ""
                    : lanClientReady.Value ? "READY" : "NOT READY");
            return;
        }

        for (int i = 0; i < lobby._session.Players.Count; i++)
        {
            var player = lobby._session.Players[i];
            string readyStatus = IsPlayerReady(player) ? "READY" : "NOT READY";

            UIManagerLobby.Instance.UpdatePlayerStatus(i == 0, readyStatus);
        }
    }

    private void OnClientConnectedCallback(ulong id)
    {
        UpdatePlayerName();
    }

    private void OnClientDisconnectCallback(ulong id)
    {
        if (lobby.IsLanSession && IsServer && id != NetworkManager.ServerClientId)
        {
            lanClientName.Value = default;
            lanClientReady.Value = false;
        }

        UpdatePlayerName();
    }

    public async void ToggleReady()
    {
        await SetReadyStateAsync(!IsLocalPlayerReady());
    }

    private async Task SetReadyStateAsync(bool isReady)
    {
        if (isStartingGame || isUpdatingReadyState)
        {
            return;
        }

        if (lobby != null && lobby.IsLanSession)
        {
            SetLanReadyRpc(isReady);
            return;
        }

        if (lobby?._session?.CurrentPlayer == null)
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
        if (!isStartingGame && !isUpdatingReadyState && !IsLocalPlayerReady())
            UIManagerLobby.Instance.UpdateReadyButton(0);
        else if (!isStartingGame && !isUpdatingReadyState && IsLocalPlayerReady())
            UIManagerLobby.Instance.UpdateReadyButton(1);
        else if (isStartingGame)
            UIManagerLobby.Instance.UpdateReadyButton(2);
    }

    private void UpdateWaitingText()
    {
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2)
        {
            UIManagerLobby.Instance.UpdateWaitingText(0);
        }
        else if (!AreAllPlayersReady())
        {
            UIManagerLobby.Instance.UpdateWaitingText(1);
        }
        else
        {
            UIManagerLobby.Instance.UpdateWaitingText(2);
        }
    }

    private bool CanStartGame()
    {
        bool controlsSession = lobby.IsLanSession
            ? NetworkManager.Singleton.IsHost
            : NetworkManager.Singleton.LocalClient.IsSessionOwner;

        return controlsSession && AreAllPlayersReady();
    }

    private bool AreAllPlayersReady()
    {
        if (lobby.IsLanSession)
        {
            return NetworkManager.Singleton.ConnectedClientsIds.Count == 2 &&
                   lanHostReady.Value &&
                   lanClientReady.Value;
        }

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
        if (lobby != null && lobby.IsLanSession)
        {
            return NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId
                ? lanHostReady.Value
                : lanClientReady.Value;
        }

        return lobby?._session?.CurrentPlayer != null && IsPlayerReady(lobby._session.CurrentPlayer);
    }

    [Rpc(SendTo.Server)]
    private void RegisterLanPlayerRpc(FixedString64Bytes playerName, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId)
        {
            lanHostName.Value = playerName;
        }
        else
        {
            lanClientName.Value = playerName;
        }
    }

    [Rpc(SendTo.Server)]
    private void SetLanReadyRpc(bool isReady, RpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId == NetworkManager.ServerClientId)
        {
            lanHostReady.Value = isReady;
        }
        else
        {
            lanClientReady.Value = isReady;
        }
    }

    private static bool IsPlayerReady(IReadOnlyPlayer player)
    {
        return player.Properties != null &&
               player.Properties.TryGetValue(ReadyPropertyKey, out var readyProperty) &&
               bool.TryParse(readyProperty.Value, out bool isReady) &&
               isReady;
    }

    public IEnumerator StartCountdown()
    {//3 seconds are given after players are readied up and before starting the game

        float timer = 0.0f;
        float duration = 3.0f;

        while (timer < duration)
        {
            if (!CanStartGame())
            {
                UIManagerLobby.Instance.CountdownUI("");
                yield break;
            }                

            //Graphical indicator for timer
            if (timer > 0 && timer < 1)
                UIManagerLobby.Instance.CountdownUI("3");
            else if (timer > 1 && timer < 2)
                UIManagerLobby.Instance.CountdownUI("2");
            else if (timer > 2  && timer < 3)
                UIManagerLobby.Instance.CountdownUI("1");
            else
                UIManagerLobby.Instance.CountdownUI("0");

            timer += Time.deltaTime;
            yield return null;
        }

        StartGame();   
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
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    private void StartGameHandlerRpc(RpcParams rpcParams = default)
    {
        StartCoroutine(UIManagerLobby.Instance.StartGameRoutine());
    }


    public async void LeaveGame()
    {
        await lobby.LeaveSessionAsync();
    }

    

    public override void OnDestroy()
    {

        if (lobby?.m_NetworkManager != null)
        {
            lobby.m_NetworkManager.OnClientConnectedCallback -= OnClientConnectedCallback;
            lobby.m_NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
        }

        base.OnDestroy();
    }
}
