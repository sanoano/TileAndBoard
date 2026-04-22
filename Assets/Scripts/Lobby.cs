using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
   private string _sessionName;
   private string _sessionJoinCode;
   private int _maxPlayers = 2;
   private bool isPrivate;
   public ISession _session;
   [HideInInspector] public NetworkManager m_NetworkManager;

    [Header("UI References")]
    [SerializeField] private UIManagerMainMenu UIManagerScript;
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
   //[SerializeField] private Button joinDirectButton;
   [SerializeField] private Button joinGameDirectButton;
   [SerializeField] private Button backButtonJoin;
   [SerializeField] private Toggle privateToggle;
   [SerializeField] private Button reconnectButton;

   [Header("Session Prefab")] 
   [SerializeField] private GameObject sessionInfoPrefab;

   [Header("Parameters")] 
   [SerializeField] private float checkDisconnectTime;

   private static Lobby instance;
   

    private async void Awake()
    {

        if (instance != null)
        {
            DestroyImmediate(instance.gameObject);
            Destroy(instance);
        }
        
        instance = this;


        m_NetworkManager = GetComponent<NetworkManager>();
        
        m_NetworkManager.SetSingleton();
        // m_NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        m_NetworkManager.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        m_NetworkManager.OnConnectionEvent += OnClientDisconnect;
        m_NetworkManager.OnTransportFailure += OnTransportFailure;

        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                print("Signed In");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            statusText.text = "Multiplayer Services failed to initialize.";
        }
        
        
        username.onValueChanged.AddListener(onUsernameSet);
        sessionName.onValueChanged.AddListener(onSessionNameSet);
        joinCodeInput.onValueChanged.AddListener(onJoinCodeSet);
        createGameButton.onClick.AddListener(StartSession);
        joinGameDirectButton.onClick.AddListener(JoinGameByJoinCode);
        privateToggle.onValueChanged.AddListener(onPrivateSet);
        reconnectButton.onClick.AddListener(Reconnect);
        joinButton.onClick.AddListener(delegate { QuerySessions();});
        refreshButton.onClick.AddListener(delegate { QuerySessions();});
        
        // statusText.text = "";

        if (AuthenticationService.Instance.PlayerName != null)
        {
            username.text = AuthenticationService.Instance.PlayerName;
        }
        else
        {
            var result = await AuthenticationService.Instance.GetPlayerNameAsync(true);
            username.text = result;
        }
        
        InvokeRepeating(nameof(CheckReconnect), checkDisconnectTime, checkDisconnectTime);

    }
    

    private void CheckReconnect()
    {
        if (!AuthenticationService.Instance.IsSignedIn ||
            UnityServices.State != ServicesInitializationState.Initialized)
        {
            if (SceneManager.GetActiveScene().name != "MainMenu") return;
            statusText.text = "Disconnected from Multiplayer Services. Press button to try reconnect.";
            reconnectButton.gameObject.SetActive(true);

        }
    }

    private async void Reconnect()
    {
        
        reconnectButton.gameObject.SetActive(false);
        statusText.text = "Reconnecting...";
        
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                print("Signed In");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            statusText.text = "Failed to reconnect.";
            reconnectButton.gameObject.SetActive(true);
            return;
        }

        statusText.text = "Connected successfully.";
        reconnectButton.gameObject.SetActive(false);
    }

    private void StartSession()
    {
        UIManagerScript.SetMenuScreen(8);
        
        if (_sessionName == String.Empty)
        {
            UIManagerScript.SetMenuScreen(3);
            statusText.text = "You must set a session name before creating a session.";
            return;
        }
        CreateSessionAsync();
        statusText.text = "Creating session...";
    }
    
    private void JoinGameByJoinCode()
    {
        UIManagerScript.SetMenuScreen(8);
        
        if (_sessionJoinCode == String.Empty)
        {
            UIManagerScript.SetMenuScreen(5);
            statusText.text = "You must provide a join code.";
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
            results = await MultiplayerService.Instance.QuerySessionsAsync(new QuerySessionsOptions());
        }
        catch (AuthenticationException e)
        {
            Debug.Log(e);
            statusText.text = "Not authorised.";
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
        
    }

    private void onUsernameSet(string value)
    {
        if (value == String.Empty) return;
        if (AuthenticationService.Instance.PlayerName == value) return;
        
        string cleanedValue = value.Replace(" ", String.Empty);
        AuthenticationService.Instance.UpdatePlayerNameAsync(cleanedValue);
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

    private void onPrivateSet(bool value)
    {
        isPrivate = value;
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
        
        // AuthenticationService.Instance.SignOut();
        
        ClearSessionState();
        SceneManager.LoadScene("MainMenu");

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

    // private void OnClientConnectedCallback(ulong clientId)
    // {
    //     if (m_NetworkManager.LocalClientId == clientId)
    //     {
    //         Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
    //         
    //         
    //     }
    //     
    //     if (m_NetworkManager.ConnectedClientsList.Count == 2 && m_NetworkManager.LocalClient.IsSessionOwner)
    //     {
    //         m_NetworkManager.SceneManager.LoadScene("Battle2", LoadSceneMode.Single);
    //         
    //     }
    // }
    

   private void OnDestroy()
   {
       _session?.LeaveAsync();
       AuthenticationService.Instance.SignOut();
   }

   public async Task JoinSessionAsync(string id)
   {

        UIManagerScript.SetMenuScreen(8);

        statusText.text = "Joining session...";
       
       try
       {

           _session = await MultiplayerService.Instance.JoinSessionByIdAsync(id, new JoinSessionOptions().
               WithPlayerName(VisibilityPropertyOptions.Public));
           
          
       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           statusText.text = "Failed to connect. Please try again.";
            UIManagerScript.SetMenuScreen(5);
       }

      
   }
   
   public async Task JoinSessionByJoinCodeAsync(string joinCode)
   {
       
       try
       {

           _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode, new JoinSessionOptions().
               WithPlayerName(VisibilityPropertyOptions.Public));
           
           
       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           statusText.text = "Failed to connect. Check join code and try again.";
           UIManagerScript.SetMenuScreen(5);
        }
       
       
   }

   private async Task CreateSessionAsync()
   {
       try
       {

           SessionOptions options;
           
           if (isPrivate)
           {
               options = new SessionOptions() {
                   Name = _sessionName,
                   MaxPlayers = _maxPlayers,
                   IsPrivate = true
               }.WithDistributedAuthorityNetwork().WithPlayerName(VisibilityPropertyOptions.Public);
           }
           else
           {
               options = new SessionOptions() {
                   Name = _sessionName,
                   MaxPlayers = _maxPlayers
               }.WithDistributedAuthorityNetwork().WithPlayerName(VisibilityPropertyOptions.Public);
           }
           

           _session = await MultiplayerService.Instance.CreateSessionAsync(options);
            
            
           statusText.text = "Session created! Waiting for player...";
           
           NetworkManager.Singleton.SceneManager.LoadScene("PlayerLobby", LoadSceneMode.Single);
       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           statusText.text = "Failed to create session. Please try again.";

           UIManagerScript.SetMenuScreen(0);
       }
       
   }

   void OnTransportFailure()
   {
       NetworkManager.Singleton.Shutdown();

        UIManagerScript.SetMenuScreen(5);

        statusText.text = "Transport failure! Please try again. If problem persists, please restart game.";

   }
}

