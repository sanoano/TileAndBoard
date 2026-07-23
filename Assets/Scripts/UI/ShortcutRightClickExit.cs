using UnityEngine;
using UnityEngine.UI;

public class ShortcutRightClickExit : MonoBehaviour
{//I'm sick of clicking on back buttons so ESC or right click toggles the button this script is attached to. Rn only applied to back button prefab.

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonUp(1))
            GetComponent<Button>().onClick.Invoke();
    }
}
