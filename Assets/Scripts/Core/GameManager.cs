using System;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    private const string GamesWonKey = "GamesWon";

    [SerializeField] private GameObject playerHead;
    private GameObject headInstance;
    private NetworkObject headInstanceNO;

    public static GameManager instance;

    public Player.PlayerId playerId;

    private string playerName;

    
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
        
        if (playerId == Player.PlayerId.Player2)
        {
            Vector3 pos = new Vector3(-39.7999992f, 36.6399994f, -1.20000005f);
            Quaternion rot = Quaternion.Euler(45, 90, 0);

            Camera.main.transform.position = pos;
            Camera.main.transform.rotation = rot;

            TurnManager.instance.isSide1 = false;
        }
        
        Application.targetFrameRate = 60;

    }

    

    //In this instance, because this scene is loaded using the scene manager, Start is actually called before OnNetworkSpawn,
    //keep this in mind when considering execution order.
    public override void OnNetworkSpawn()
    {

        headInstance = Instantiate(playerHead);

        headInstanceNO = headInstance.GetComponent<NetworkObject>();
        
        headInstanceNO.Spawn(destroyWithScene: true);
        
        GetPlayerName();

        
        
    }
    
    async void GetPlayerName()
    {
        try
        {
            Lobby lobby = NetworkManager.Singleton.GetComponent<Lobby>();
            playerName = lobby.IsLanSession
                ? lobby.PlayerDisplayName
                : await AuthenticationService.Instance.GetPlayerNameAsync();

            if (playerId == Player.PlayerId.Player1)
            {
                foreach (var clientIds in NetworkManager.ConnectedClientsIds)
                {
                    SetPlayer1NameRpc(playerName, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
            else
            {
                foreach (var clientIds in NetworkManager.ConnectedClientsIds)
                {
                    SetPlayer2NameRpc(playerName, RpcTarget.Single(clientIds, RpcTargetUse.Temp));
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
       
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void SetPlayer1NameRpc(string name, RpcParams rpcParams = default)
    {
        string trimmedName = TrimPlayerName(name);
        if (playerId == Player.PlayerId.Player1)
        {
            UIManager.Instance.player1Name.text = trimmedName;
        }
        else
        {
            UIManager.Instance.player2Name.text = trimmedName;

        }
        
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    void SetPlayer2NameRpc(string name, RpcParams rpcParams = default)
    {
        string trimmedName = TrimPlayerName(name);
        if (playerId == Player.PlayerId.Player1)
        {
            UIManager.Instance.player2Name.text = trimmedName;
        }
        else
        {
            UIManager.Instance.player1Name.text = trimmedName;

        }
    }

    private static string TrimPlayerName(string name)
    {
        int suffixIndex = name.LastIndexOf('#');
        return suffixIndex > 0 ? name.Substring(0, suffixIndex) : name;
    }

    public async void DisconnectUser()
    {
        try
        {
            if (NetworkManager.Singleton)
            {
                await NetworkManager.Singleton.gameObject.GetComponent<Lobby>().LeaveSessionAsync();
            }
            else
            {
                print("Game Ended");
            }
            StopAllCoroutines();
        }
        catch(Exception)
        {
            StopAllCoroutines();
            SceneManager.LoadScene("MainMenu");
        }
        
    }

    public void ReturnToLobby()
    {
        if (NetworkManager.Singleton)
        {
            StopAllCoroutines();
            if (playerId == Player.PlayerId.Player1)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("PlayerLobby", LoadSceneMode.Single);
            }
        }
    }

    public void SaveGameWin(Player.PlayerId winner)
    {
        if (playerId != winner)
            return;

        int gamesWon = PlayerPrefs.GetInt(GamesWonKey, 0);
        PlayerPrefs.SetInt(GamesWonKey, gamesWon + 1);
        PlayerPrefs.Save();
    }

}
