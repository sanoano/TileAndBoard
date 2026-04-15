using UnityEngine;

public class UIDialogueSlide : MonoBehaviour
{//Makes dialogues slide out as required, instead of just appearing/disappearing

    [SerializeField] private float travelLength;
    [SerializeField] private int directionState;//0 right->left, 1 left->right, 2 up->down, 3 down->up

    private Vector3 startPos, endPos;
    void Start()
    {
        startPos = transform.position;
        
        if(directionState == 0)
            endPos = new Vector3(startPos.x + travelLength, startPos.y, startPos.z);
        else if (directionState == 1)
            endPos = new Vector3(startPos.x - travelLength, startPos.y, startPos.z);
        else if (directionState == 2)
            endPos = new Vector3(startPos.x, startPos.y - travelLength, startPos.z);
        else if (directionState == 3)
            endPos = new Vector3(startPos.x, startPos.y + travelLength, startPos.z);
    }


    public void SlideIn()
    {
        //tween movement between the start to end pos
    }

    public void SlideOut()
    {
        //tween movement between the end to start pos
    }
}
