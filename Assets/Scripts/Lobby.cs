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

public class Lobby : MonoBehaviour
{
   private string _profileName;
   private string _sessionName;
   private string _sessionJoinCode;
   private int _maxPlayers = 2;
   private ConnectionState _state = ConnectionState.Disconnected;
   public ISession _session;
   private NetworkManager m_NetworkManager;

   [Header("UI References")]
   [SerializeField] private TMP_InputField username;
   [SerializeField] private TMP_InputField sessionName;
   [SerializeField] private TMP_InputField joinCodeInput;
   [SerializeField] private Button createGameButton;
   [SerializeField] private Button createButton;
   [SerializeField] private Button joinButton;
   [SerializeField] private TextMeshProUGUI statusText;
   [SerializeField] private GameObject sessionListContent;
   [SerializeField] private GameObject sessionList;
   [SerializeField] private Button backButton;
   [SerializeField] private Button refreshButton;
   [SerializeField] private Button joinDirectButton;
   [SerializeField] private Button joinGameDirectButton;
   [SerializeField] private Button backButtonJoin;

   [Header("Session Prefab")] 
   [SerializeField] private GameObject sessionInfoPrefab;

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

        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.Log(e);
            statusText.text = "Unity Services failed to initialize. Please restart game.";
        }
        
        
        username.onValueChanged.AddListener(onUsernameSet);
        sessionName.onValueChanged.AddListener(onSessionNameSet);
        joinCodeInput.onValueChanged.AddListener(onJoinCodeSet);
        createGameButton.onClick.AddListener(StartSession);
        joinGameDirectButton.onClick.AddListener(JoinGameByJoinCode);
        joinButton.onClick.AddListener(delegate { QuerySessions();});
        refreshButton.onClick.AddListener(delegate { QuerySessions();});
        statusText.text = "";

        _profileName = String.Empty;
    }

    private void Update()
    {
        try
        {
            if (_profileName == String.Empty)
            {
                joinButton.interactable = false;
                createButton.interactable = false;
                joinDirectButton.interactable = false;
            }
            else
            {
                joinButton.interactable = true;
                createButton.interactable = true;
                joinDirectButton.interactable = true;
            }
        }
        catch(Exception e)
        {
            
        }
        
    }

    private void StartSession()
    {
        username.gameObject.SetActive(false);
        sessionName.gameObject.SetActive(false);
        createGameButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        
        if (_profileName == String.Empty)
        {
            username.gameObject.SetActive(true);
            sessionName.gameObject.SetActive(true);
            createGameButton.gameObject.SetActive(true);
            backButton.gameObject.SetActive(true);
            statusText.text = "You must set a username before creating a session!";
            return;
        }
        CreateSessionAsync();
        statusText.text = "Creating session...";
    }
    
    private void JoinGameByJoinCode()
    {
        joinCodeInput.gameObject.SetActive(false);
        joinGameDirectButton.gameObject.SetActive(false);
        backButtonJoin.gameObject.SetActive(false);
        username.gameObject.SetActive(false);
        
        if (_profileName == String.Empty)
        {
            joinCodeInput.gameObject.SetActive(true);
            joinGameDirectButton.gameObject.SetActive(true);
            backButtonJoin.gameObject.SetActive(true);
            username.gameObject.SetActive(true);
            statusText.text = "You must set a username before joining a session!";
            return;
        }

        JoinSessionByJoinCodeAsync(_sessionJoinCode);
        statusText.text = "Connecting to session...";
    }

    public async Task QuerySessions()
    {
        statusText.text = "";

        foreach (var child in sessionListContent.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject != sessionListContent)
            {
                Destroy(child.gameObject);
            }
        }
        
        
        QuerySessionsResults results;

        try
        {
            AuthenticationService.Instance.SwitchProfile(_profileName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            results = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
            statusText.text = "You must set a username before searching for sessions.";
            return;
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
            statusText.text = "Failed to query sessions. Please try again.";
            return;
        }
        

        if (results.Sessions.Count > 0)
        {
            foreach (var session in results.Sessions)
            {
                var instance = Instantiate(sessionInfoPrefab, sessionListContent.transform);
                var infoDisplayInstance = instance.GetComponent<SessionInfoDisplay>();
                infoDisplayInstance.SetSessionName(session.Name);
                infoDisplayInstance.SetJoinButton(session.Id, this);

            }
        }
        else
        {
            statusText.text = "No sessions found.";
        }
        
        AuthenticationService.Instance.SignOut();
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

    private void onJoinCodeSet(string value)
    {
        _sessionJoinCode = value;
    }
    
    private async void OnClientDisconnect(NetworkManager manager,ConnectionEventData connectionEventData )
    {
        
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

   public async Task JoinSessionAsync(string id)
   {
       
       sessionList.SetActive(false);
       username.gameObject.SetActive(false);

       statusText.text = "Joining session...";
       
       try
       {
           AuthenticationService.Instance.SwitchProfile(_profileName);
           await AuthenticationService.Instance.SignInAnonymouslyAsync();

           _session = await MultiplayerService.Instance.JoinSessionByIdAsync(id, new JoinSessionOptions());
           await AuthenticationService.Instance.UpdatePlayerNameAsync(_profileName);
       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           AuthenticationService.Instance.SignOut();
           statusText.text = "Failed to connect. Please try again.";
           username.gameObject.SetActive(true);
           joinButton.gameObject.SetActive(true);
           createButton.gameObject.SetActive(true);
       }
   }
   
   public async Task JoinSessionByJoinCodeAsync(string joinCode)
   {
       
       try
       {
           AuthenticationService.Instance.SwitchProfile(_profileName);
           await AuthenticationService.Instance.SignInAnonymouslyAsync();

           _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, new JoinSessionOptions());
           await AuthenticationService.Instance.UpdatePlayerNameAsync(_profileName);
       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           AuthenticationService.Instance.SignOut();
           statusText.text = "Failed to connect. Check join code and try again.";
           joinCodeInput.gameObject.SetActive(true);
           joinGameDirectButton.gameObject.SetActive(true);
           backButtonJoin.gameObject.SetActive(true);
           username.gameObject.SetActive(true);
       }
   }

   private async Task CreateSessionAsync()
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

            _session = await MultiplayerService.Instance.CreateSessionAsync(options);
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
           statusText.text = "Failed to create session. Please try again.";
           username.gameObject.SetActive(true);
           sessionName.gameObject.SetActive(true);
           createGameButton.gameObject.SetActive(true);
           backButton.gameObject.SetActive(true);
       }
   }

   void OnTransportFailure()
   {
       NetworkManager.Singleton.Shutdown();
       AuthenticationService.Instance.SignOut();
       
       username.gameObject.SetActive(true);
       sessionName.gameObject.SetActive(true);
       createGameButton.gameObject.SetActive(true);

       statusText.text = "Transport failure! Please try again. If problem persists, please restart game.";

   }
}

