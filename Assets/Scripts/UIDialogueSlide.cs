using UnityEngine;
using Tweens;

public class UIDialogueSlide : MonoBehaviour
{//Makes dialogues slide out as required, instead of just appearing/disappearing

    [SerializeField] private float travelLength;
    [SerializeField] private int directionState;//0 right->left, 1 left->right, 2 up->down, 3 down->up
    [SerializeField] private float tweenDuration;

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
        var tween = new AnchoredPositionYTween()
        {
            from = startPos,
            to = endPos,
            duration = tweenDuration,
            easeType = EaseType.SineOut
        };

        gameObject.AddTween(tween);
    }

    public void SlideOut()
    {
        var tween = new AnchoredPositionYTween()
        {
            from = endPos,
            to = startPos,
            duration = tweenDuration,
            easeType = EaseType.SineOut
        };

        gameObject.AddTween(tween);
    }
}
