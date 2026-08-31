using TMPro;
using UnityEngine;

public class TextLoading : MonoBehaviour
{//simple script that just gives the loading message some motion

    private TextMeshProUGUI loadingTMP;
    private float timer = 0.0f;

    private void Start()
    {
        loadingTMP = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {

        timer += Time.deltaTime * 2;

        if (timer % 4 > 0 && timer % 4 < 1)
            loadingTMP.text = "Loading";
        else if (timer % 4 > 1 && timer % 4 < 2)
            loadingTMP.text = "Loading.";
        else if (timer % 4 > 2 && timer % 4 < 3)
            loadingTMP.text = "Loading..";
        else
            loadingTMP.text = "Loading...";
    }
}
