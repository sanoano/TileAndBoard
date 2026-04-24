using TMPro;
using UnityEngine;

public class DEBUG_FpsDisplay : MonoBehaviour
{
    public TMP_Text fpscounter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        fpscounter.text = "FPS: " + Mathf.Round( 1f / Time.deltaTime);
    }
}
