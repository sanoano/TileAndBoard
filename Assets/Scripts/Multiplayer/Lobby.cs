using System;
using System.Collections.Generic;
using System.IO;
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
using static Unity.Entities.EntitiesJournaling;

public class Lobby : MonoBehaviour
{
   private const string TurnTimePropertyKey = "turnTimeSeconds";
   private const string StartingHealthPropertyKey = "startingPlayerHealth";
   private const string HostNamePropertyKey = "hostName";
   private const string MatchPreferencesFileName = "create-game-preferences.json";
   
   public const int DefaultTurnTimeSeconds = 60;
   public const int DefaultStartingPlayerHealth = 50;

   private string _sessionName;
    private string PreferredSessionName;
   private string _sessionJoinCode;
   private int _maxPlayers = 2;
   private bool isPrivate;
   private int selectedTurnTimeSeconds = DefaultTurnTimeSeconds;
   private int selectedStartingPlayerHealth = DefaultStartingPlayerHealth;
   public ISession _session;
   [HideInInspector] public NetworkManager m_NetworkManager;

   [Header("UI References")]
   [SerializeField] private UIManagerMainMenu UIManagerScript;
   [SerializeField] private TMP_InputField username;
   [SerializeField] private TextMeshProUGUI statusText;
   [SerializeField] private Button reconnectButton;

    [Header("Create Game UI")]
    [SerializeField] private Button createGameButton;
    [SerializeField] private Toggle privateToggle;
    //[SerializeField] private Toggle timerToggle; Need code to deal with no timer in games
    [SerializeField] private TMP_InputField sessionName;
    [SerializeField] private TMP_InputField timerInput;
    [SerializeField] private TMP_InputField lpInput;
     /*[SerializeField] private TMP_Dropdown timerSelect;
      [SerializeField] private TMP_Dropdown healthSelect;*/

    [Header("Join Game UI")] 
   [SerializeField] private GameObject sessionInfoPrefab;
   [SerializeField] private Button joinGameDirectButton;
   [SerializeField] private Button refreshButton; 
   [SerializeField] private TMP_InputField joinCodeInput;
   [SerializeField] private GameObject sessionListContent;
   [SerializeField] private GameObject sessionList;
   [SerializeField] private Button joinButton;

   [Header("Parameters")] 
   [SerializeField] private float checkDisconnectTime;

   private static Lobby instance;

   [Serializable]
   private class MatchPreferences
   {
       public int turnTimeSeconds;
       public int startingPlayerHealth;
       public string gameName;
   }

   public int TurnTimeSeconds => GetPositiveSessionSetting(
       TurnTimePropertyKey,
       selectedTurnTimeSeconds);

   public int StartingPlayerHealth => GetPositiveSessionSetting(
       StartingHealthPropertyKey,
       selectedStartingPlayerHealth);
   
   
    private async void Awake()
    {

        if (instance != null)
        {
            DestroyImmediate(instance.gameObject);
            Destroy(instance);
        }
        
        instance = this;


        m_NetworkManager = GetComponent<NetworkManager>();
        m_NetworkManager.NetworkConfig.ProtocolVersion = GetProtocolVersion();
        
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
        //timerToggle.onValueChanged.AddListener(onTimerSet);
        reconnectButton.onClick.AddListener(Reconnect);
        joinButton.onClick.AddListener(QuerySessionsFromButton);
        refreshButton.onClick.AddListener(QuerySessionsFromButton);
        //timerSelect.onValueChanged.AddListener(SetTurnTimePerTurn);
        //healthSelect.onValueChanged.AddListener(SetStartingPlayerHealth);

        LoadPreferredMatchSettings();
        
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

    private async void StartSession()
    {
        UIManagerScript.SetMenuScreen(8);
        UIManagerScript.SetMenuLevel(1);

        if (_sessionName == String.Empty)
        {
            UIManagerScript.SetMenuScreen(3);
            UIManagerScript.SetMenuLevel(3);
            statusText.text = "You must set a session name before creating a session.";
            return;
        }
        await CreateSessionAsync();
        statusText.text = "Creating session...";
    }
    
    private async void JoinGameByJoinCode()
    {
        UIManagerScript.SetMenuScreen(8);
        
        if (_sessionJoinCode == String.Empty)
        {
            UIManagerScript.SetMenuScreen(5);
            statusText.text = "You must provide a join code.";
            return;
        }

        statusText.text = "Connecting to session...";
        await JoinSessionByJoinCodeAsync(_sessionJoinCode);
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
                infoDisplayInstance.SetSessionName(session.Name + " |");
                infoDisplayInstance.SetJoinButton(session.Id, this);
                infoDisplayInstance.SetMaxTimeText($"Time: {session.Properties["turnTimeSeconds"].Value}");
                infoDisplayInstance.SetMaxLPText($"Life Points: {session.Properties["startingPlayerHealth"].Value}");
                infoDisplayInstance.SetHostName(session.Properties["hostName"].Value);
            }
        }
        else
        {
            statusText.text = "No sessions found.";
        }
        
    }

    private async void QuerySessionsFromButton()
    {
        await QuerySessions();
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

    public void SaveSessionNamePreference()
    {
        PreferredSessionName = sessionName.text;
    }


    public void SetTurnTimePerTurn(bool defaults)
    {//This function is fired when you stop editing one of the input fields

        //(int optionIndex)
        /*selectedTurnTimeSeconds = GetDropdownSetting(
            timerSelect,
            optionIndex,
            DefaultTurnTimeSeconds);*/

        if (!defaults)
        {
            if (int.Parse(timerInput.text) < 30)
            {
                selectedTurnTimeSeconds = 30;
                timerInput.text = "30";
            }
            else if (int.Parse(timerInput.text) >= 999)
            {
                selectedTurnTimeSeconds = 999;
                timerInput.text = "999";
            }
            else
                selectedTurnTimeSeconds = int.Parse(timerInput.text);
        }
        else
            selectedTurnTimeSeconds = DefaultTurnTimeSeconds;
    }

    public void SetStartingPlayerHealth(bool defaults)
    {//This function is fired when you stop editing one of the input fields

        //(int optionIndex)
        /*selectedStartingPlayerHealth = GetDropdownSetting( 
            healthSelect,
            optionIndex,
            DefaultStartingPlayerHealth);*/

        if (!defaults)
        {
            if (int.Parse(lpInput.text) < 10)
            {
                selectedStartingPlayerHealth = 10;
                lpInput.text = "10";
            }
            else if (int.Parse(lpInput.text) >= 99)
            {
                selectedStartingPlayerHealth = 99;
                lpInput.text = "99";
            }
            else
                selectedStartingPlayerHealth = int.Parse(lpInput.text);
        }
        else
            selectedStartingPlayerHealth = DefaultStartingPlayerHealth;
    }

    public void ResetMatchSettings()
    {
        //SetDropdownToSetting(timerSelect, DefaultTurnTimeSeconds);
        //SetDropdownToSetting(healthSelect, DefaultStartingPlayerHealth);

        //SetTurnTimePerTurn(timerSelect.value);
        //SetStartingPlayerHealth(healthSelect.value);
    }

    public void LoadPreferredMatchSettings()
    {
        ResetMatchSettings();

        string preferencesPath = GetMatchPreferencesPath();
        if (!File.Exists(preferencesPath))
        {
            lpInput.text = DefaultStartingPlayerHealth.ToString();
            timerInput.text = DefaultTurnTimeSeconds.ToString();

            SetTurnTimePerTurn(true);
            SetStartingPlayerHealth(true);
            return;
        }

        try
        {
            string json = File.ReadAllText(preferencesPath);
            MatchPreferences preferences = JsonUtility.FromJson<MatchPreferences>(json);
            if (preferences == null)
            {
                lpInput.text = DefaultStartingPlayerHealth.ToString();
                timerInput.text = DefaultTurnTimeSeconds.ToString();

                SetTurnTimePerTurn(true);
                SetStartingPlayerHealth(true);
                return;
            }

            //SetDropdownToSetting(timerSelect, preferences.turnTimeSeconds);
            //SetDropdownToSetting(healthSelect, preferences.startingPlayerHealth);

            lpInput.text = preferences.startingPlayerHealth.ToString();
            timerInput.text = preferences.turnTimeSeconds.ToString();
            sessionName.text = preferences.gameName;

            SetTurnTimePerTurn(false);
            SetStartingPlayerHealth(false);
            
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load create game preferences from '{preferencesPath}'. {exception.Message}");
        }
    }

    public void SavePreferredMatchSettings()
    {
        /*SetTurnTimePerTurn(timerSelect.value);
        SetStartingPlayerHealth(healthSelect.value);*/

        MatchPreferences preferences = new MatchPreferences
        {
            turnTimeSeconds = selectedTurnTimeSeconds,
            startingPlayerHealth = selectedStartingPlayerHealth,
            gameName = PreferredSessionName
        };

        string preferencesPath = GetMatchPreferencesPath();

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(preferencesPath, JsonUtility.ToJson(preferences, true));
            Debug.Log($"Saved create game preferences to '{preferencesPath}'.");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not save create game preferences to '{preferencesPath}'. {exception.Message}");
        }
    }

    private static string GetMatchPreferencesPath()
    {
        return Path.Combine(Application.persistentDataPath, MatchPreferencesFileName);
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
       _ = _session?.LeaveAsync();
       AuthenticationService.Instance.SignOut();
   }

   public async Task JoinSessionAsync(string id)
   {

        UIManagerScript.SetMenuScreen(8);

        statusText.text = "Joining session...";
       
       try
       {
           _session = await MultiplayerService.Instance.JoinSessionByIdAsync(
               id,
               new JoinSessionOptions().WithPlayerName(VisibilityPropertyOptions.Public));

       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           statusText.text = "Failed to connect. Verify host game version.";
            UIManagerScript.SetMenuScreen(5);
       }

      
   }
   
   public async Task JoinSessionByJoinCodeAsync(string joinCode)
   {
       
       try
       {

           _session = await MultiplayerService.Instance.JoinSessionByCodeAsync(
               joinCode,
               new JoinSessionOptions().WithPlayerName(VisibilityPropertyOptions.Public));

       }
       catch (Exception e)
       {
           Debug.LogException(e);
           NetworkManager.Singleton.Shutdown();
           statusText.text = "Failed to connect. Check the join code and make sure you are using the same game version as the host.";
           UIManagerScript.SetMenuScreen(5);
        }
       
       
   }

   private async Task CreateSessionAsync()
   {
       try
       {
           SessionOptions options = new SessionOptions()
           {
               Name = _sessionName,
               MaxPlayers = _maxPlayers,
               IsPrivate = isPrivate,
               SessionProperties = new Dictionary<string, SessionProperty>
               {
                   [TurnTimePropertyKey] = new SessionProperty(selectedTurnTimeSeconds.ToString()),
                   [StartingHealthPropertyKey] = new SessionProperty(selectedStartingPlayerHealth.ToString()),
                   [HostNamePropertyKey] = new SessionProperty(
                       AuthenticationService.Instance.PlayerName,
                       VisibilityPropertyOptions.Public)
               }
           }.WithDistributedAuthorityNetwork().WithPlayerName(VisibilityPropertyOptions.Public);
           

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

   private static ushort GetProtocolVersion()
   {
       if (!Version.TryParse(Application.version, out Version version)) return 0;

       return (ushort)(version.Major * 10000 + version.Minor * 100 + version.Build);
   }

   private int GetPositiveSessionSetting(string key, int fallback)
   {
       if (_session != null &&
           _session.Properties.TryGetValue(key, out SessionProperty property))
       {
           return ParsePositiveSetting(property.Value, fallback);
       }

       return fallback;
   }

   private static int ParsePositiveSetting(string value, int fallback)
   {
       return int.TryParse(value, out int parsedValue) && parsedValue > 0
           ? parsedValue
           : fallback;
   }

   private static int GetDropdownSetting(TMP_Dropdown dropdown, int optionIndex, int fallback)
   {
       if (dropdown == null || optionIndex < 0 || optionIndex >= dropdown.options.Count)
       {
           return fallback;
       }

       return ParsePositiveSetting(dropdown.options[optionIndex].text, fallback);
   }

   private static void SetDropdownToSetting(TMP_Dropdown dropdown, int setting)
   {
       if (dropdown == null) return;

       for (int i = 0; i < dropdown.options.Count; i++)
       {
           if (ParsePositiveSetting(dropdown.options[i].text, -1) != setting) continue;

           dropdown.SetValueWithoutNotify(i);
           dropdown.RefreshShownValue();
           return;
       }
   }

   void OnTransportFailure()
   {
       NetworkManager.Singleton.Shutdown();

        UIManagerScript.SetMenuScreen(5);

        statusText.text = "Transport failure! Please try again. If problem persists, please restart game.";

   }
}
