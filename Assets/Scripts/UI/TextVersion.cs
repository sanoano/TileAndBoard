using TMPro;
using UnityEngine;

public class TextVersion : MonoBehaviour
{//Lets players know what version they're running when they're in the lobby. Just remember to update it in the build player settings.

    private TextMeshProUGUI versionText;
    [SerializeField] private int verType = 0; //just adds a wee α (0) or β (1) to the version. 2 means full version, so no letter prefix.

    void Start()
    {
        versionText = GetComponent<TextMeshProUGUI>();

        if (verType == 0)
            versionText.text = "Version: " + "α " + Application.version;
        else if (verType == 1)
            versionText.text = "Version: " + "β " + Application.version;
        else
            versionText.text = "Version: " + Application.version;
    }
}
