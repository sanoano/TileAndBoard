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

    public void SetSessionName(string name)
    {
        sessionName.text = name;
    }

    public void SetJoinButton(string sessionID, Lobby manager) 
    {
        joinButton.onClick.AddListener(async () => await manager.JoinSessionAsync(sessionID));
    }

    public void SetMaxTimeText(string time)
    {
        maxTimeText.text = time;
    }

    public void SetMaxLPText(string life)
    {
        maxLPText.text = life;
    }
    

}
