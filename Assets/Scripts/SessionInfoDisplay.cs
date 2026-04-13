using TMPro;
//using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UI;

public class SessionInfoDisplay : MonoBehaviour
{

    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI sessionName;

    public void SetSessionName(string name)
    {
        sessionName.text = name;
    }

    public void SetJoinButton(string sessionID, Lobby manager) 
    {
        joinButton.onClick.AddListener(delegate {manager.JoinSessionAsync(sessionID) ;});
    }
    

}
