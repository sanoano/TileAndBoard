using UnityEngine;
using UnityEngine.UI;

public class ShortcutScroll : MonoBehaviour
{//Made for the help menu, but could probably be applied to other things. Who knows

    [SerializeField] private bool scrollUp;

    void Update()
    {//If scrollUp is toggled, it detects positive change on Input.mouseScrollDelta.y. Otherwise it only detects negative change

        float scrollPos = 0;

        if (scrollUp)
        {
            if (Input.mouseScrollDelta.y > scrollPos)
            {
                GetComponent<Button>().onClick.Invoke();
                scrollPos = Input.mouseScrollDelta.y;
            }
        }
        else
        {
            if (Input.mouseScrollDelta.y < scrollPos)
            {
                GetComponent<Button>().onClick.Invoke();
                scrollPos = Input.mouseScrollDelta.y;
            }
        }
    }
}
