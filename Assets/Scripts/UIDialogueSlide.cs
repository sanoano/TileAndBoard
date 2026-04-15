using Tweens;
using Tweens.Core;
using UnityEngine;

public class UIDialogueSlide : MonoBehaviour
{//Makes dialogues slide out as required, instead of just appearing/disappearing

    [SerializeField] private float travelLength;
    [SerializeField] private int directionState;//0 right->left, 1 left->right, 2 up->down, 3 down->up
    [SerializeField] private float tweenDuration;

    private RectTransform rect;
    private Vector2 startPos, endPos;
    void Start()
    {
        rect = GetComponent<RectTransform>();

        startPos = rect.anchoredPosition;
        endPos = startPos;
        
        if(directionState == 0)
            endPos += Vector2.left * travelLength;
        else if (directionState == 1)
            endPos += Vector2.right * travelLength;
        else if (directionState == 2)
            endPos += Vector2.down * travelLength;
        else if (directionState == 3)
            endPos += Vector2.up * travelLength;
    }


    public void SlideIn()
    {
        if (directionState == 0 || directionState == 1)
        {
            var tweenX = new AnchoredPositionXTween()
            {
                from = startPos.x,
                to = endPos.x,
                duration = tweenDuration,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenX);
        }
        else
        {
            var tweenY = new AnchoredPositionYTween()
            {
                from = startPos.y,
                to = endPos.y,
                duration = tweenDuration,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenY);
        }
    }

    public void SlideOut()
    {
        if (directionState == 0 || directionState == 1)
        {
            var tweenX = new AnchoredPositionXTween()
            {
                from = endPos.x,
                to = startPos.x,
                duration = tweenDuration,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenX);
        }
        else
        {
            var tweenY = new AnchoredPositionYTween()
            {
                from = endPos.y,
                to = startPos.y,
                duration = tweenDuration,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenY);
        }
    }
}
