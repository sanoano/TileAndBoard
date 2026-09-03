using TMPro;
using Unity.VisualScripting;

//using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.UI;

public class SessionInfoDisplay : MonoBehaviour
{

    [SerializeField] private Button joinButton;
    [SerializeField] private TextMeshProUGUI sessionName;
    [SerializeField] private TextMeshProUGUI maxTimeText;
    [SerializeField] private TextMeshProUGUI maxLPText;
    [SerializeField] private TextMeshProUGUI hostName; 

    public void SetSessionName(string name)
    {
        sessionName.text = name;
    }

    public void SetJoinButton(string sessionID, Lobby manager) 
    {
        joinButton.onClick.AddListener(async () => await manager.JoinSessionAsync(sessionID));
    }

    public void SetLanJoinButton(LanSessionInfo session, Lobby manager)
    {
        joinButton.onClick.AddListener(() => manager.JoinLanSession(session));
    }

    public void SetMaxTimeText(string time)
    {
        maxTimeText.text = time;
    }

    public void SetMaxLPText(string life)
    {
        maxLPText.text = life;
    }

    public void SetHostName(string name)
    {
        hostName.text = name;
    }
    

}
