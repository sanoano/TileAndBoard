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
        m_NetworkManager.OnConnectionEvent += OnClientDisconnect;
        m_NetworkManager.OnTransportFailure += OnTransportFailure;
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
        string cleanedValue = value.Replace(" ", String.Empty);
        _profileName = cleanedValue;
    }
    
    private void onSessionNameSet(string value)
    {
        string cleanedValue = value.Replace(" ", String.Empty);
        _sessionName = cleanedValue;
    }
    
    private async void OnClientDisconnect(NetworkManager manager,ConnectionEventData connectionEventData )
    {
        print("player disconnect");
        if (connectionEventData.EventType == ConnectionEvent.PeerDisconnected && connectionEventData.ClientId != NetworkManager.Singleton.LocalClientId)
        {
            await LeaveSessionAsync();
            
        }
        
    }
    
    public async Task LeaveSessionAsync()
    {
      
        if (_session != null)
        {
            await _session.LeaveAsync();
            _session = null;
        }

        if (m_NetworkManager != null && m_NetworkManager.ShutdownInProgress == false)
        {
            m_NetworkManager.Shutdown();
            
           
            await WaitForShutdown();
        }
        
        AuthenticationService.Instance.SignOut();
        
        ClearSessionState();
        SceneManager.LoadScene("Lobby");

        Debug.Log("Session left successfully");
    }

    private async Task WaitForShutdown()
    {
        
        int maxWait = 5000;
        int waited = 0;
        
        while (m_NetworkManager.ShutdownInProgress)
        {
            await Task.Delay(100);
            waited += 100;
            
            if (waited >= maxWait)
            {
                Debug.LogWarning("Network shutdown timeout");
                break;
            }
        }
    }

    private void ClearSessionState()
    {
        
        System.GC.Collect();
        
        
        
        Debug.Log("State cleared");
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
        
        if (m_NetworkManager.ConnectedClientsList.Count == 2 && m_NetworkManager.LocalClient.IsSessionOwner)
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
            await AuthenticationService.Instance.UpdatePlayerNameAsync(_profileName);
            
           _state = ConnectionState.Connected;
           statusText.text = "Session created! Waiting for player...";
           

       }
       catch (Exception e)
       {
           _state = ConnectionState.Disconnected;
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           AuthenticationService.Instance.SignOut();
           statusText.text = "Failed to connect. Error: " + e;
           username.gameObject.SetActive(true);
           sessionName.gameObject.SetActive(true);
           startButton.gameObject.SetActive(true);
       }
   }

   void OnTransportFailure()
   {
       NetworkManager.Singleton.Shutdown();
       AuthenticationService.Instance.SignOut();
       
       username.gameObject.SetActive(true);
       sessionName.gameObject.SetActive(true);
       startButton.gameObject.SetActive(true);

       statusText.text = "Transport failure! Please try again. If problem persists, please restart game.";

   }
}

