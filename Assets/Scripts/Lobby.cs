using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ConnectionManager : MonoBehaviour
{
   private string _profileName;
   private string _sessionName;
   private int _maxPlayers = 2;
   private ConnectionState _state = ConnectionState.Disconnected;
   public ISession _session;
   private NetworkManager m_NetworkManager;

   [SerializeField] private TMP_InputField username;
   [SerializeField] private TMP_InputField sessionName;
   [SerializeField] private Button startButton;
   [SerializeField] private TextMeshProUGUI statusText;

   private enum ConnectionState
   {
       Disconnected,
       Connecting,
       Connected,
   }

    private async void Awake()
    {
        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        m_NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        await UnityServices.InitializeAsync();
        
        username.onValueChanged.AddListener(onUsernameSet);
        sessionName.onValueChanged.AddListener(onSessionNameSet);
        startButton.onClick.AddListener(StartOrJoin);
        statusText.text = "";

    }

    private void StartOrJoin()
    {
        username.gameObject.SetActive(false);
        sessionName.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        CreateOrJoinSessionAsync();
        statusText.text = "Connecting to/Creating session...";
    }

    private void onUsernameSet(string value)
    {
        _profileName = value;
    }
    
    private void onSessionNameSet(string value)
    {
        _sessionName = value;
    }
    
    private async void OnClientDisconnect(ulong clientId)
    {
        if (clientId != m_NetworkManager.LocalClientId && SceneManager.GetActiveScene().name == "Battle")
        {
            // await _session.LeaveAsync();
            // AuthenticationService.Instance.SignOut();
            // SceneManager.LoadScene("Lobby");
        }
        
    }

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (m_NetworkManager.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{m_NetworkManager.LocalClientId} is the session owner!");
            
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (m_NetworkManager.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
            
            
        }
        
        if (m_NetworkManager.ConnectedClientsList.Count == 2)
        {
            m_NetworkManager.SceneManager.LoadScene("Battle2", LoadSceneMode.Single);
            
        }
    }
    

   private void OnDestroy()
   {
       _session?.LeaveAsync();
   }

   private async Task CreateOrJoinSessionAsync()
   {
       
       _state = ConnectionState.Connecting;
       
       
       try
       {
           AuthenticationService.Instance.SwitchProfile(_profileName);
           await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var options = new SessionOptions() {
                Name = _sessionName,
                MaxPlayers = _maxPlayers
            }.WithDistributedAuthorityNetwork();

            _session = await MultiplayerService.Instance.CreateOrJoinSessionAsync(_sessionName, options);

           _state = ConnectionState.Connected;
           statusText.text = "Session created! Waiting for player...";

       }
       catch (Exception e)
       {
           _state = ConnectionState.Disconnected;
           Debug.LogException(e);
           AuthenticationService.Instance.SignOut();
           statusText.text = "Failed to connect. Error: " + e;
           username.gameObject.SetActive(true);
           sessionName.gameObject.SetActive(true);
           startButton.gameObject.SetActive(true);
       }
   }
}

