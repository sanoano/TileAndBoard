using Tweens;
using Tweens.Core;
using UnityEngine;

public class UIDialogueSlide : MonoBehaviour
{//Makes dialogues slide out as required, instead of just appearing/disappearing

    [SerializeField] private float travelLength;
    [SerializeField] private int directionState;//0 right->left, 1 left->right, 2 up->down, 3 down->up
    [SerializeField] private float tweenDuration;
    [SerializeField] private bool hasMouseSlide;

    private RectTransform rect;
    private Vector2 startPos, endPos, mouseSlidePos;
    private float travelLengthMouse, tweenDurationMouse;
    void Start()
    {
        rect = GetComponent<RectTransform>();

        startPos = rect.anchoredPosition;
        endPos = startPos;
        travelLengthMouse = travelLength / 8;
        tweenDurationMouse = tweenDuration / 4;

        if (directionState == 0)
            endPos += Vector2.left * travelLength;
        else if (directionState == 1)
            endPos += Vector2.right * travelLength;
        else if (directionState == 2)
            endPos += Vector2.down * travelLength;
        else if (directionState == 3)
            endPos += Vector2.up * travelLength;

        if (hasMouseSlide)
        {
            if (directionState == 0)
                mouseSlidePos += Vector2.left * travelLengthMouse;
            else if (directionState == 1)
                mouseSlidePos += Vector2.right * travelLengthMouse;
            else if (directionState == 2)
                mouseSlidePos += Vector2.down * travelLengthMouse;
            else if (directionState == 3)
                mouseSlidePos += Vector2.up * travelLengthMouse;
        }
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

    //The following functions are called by Event Triggers that detect On Pointer Enter and Exit events
    public void SlideInMouse()
    {
        if (hasMouseSlide)
        {
            if (directionState == 0 || directionState == 1)
            {
                var tweenX = new AnchoredPositionXTween()
                {
                    from = endPos.x,
                    to = mouseSlidePos.x,
                    duration = tweenDurationMouse,
                    easeType = EaseType.SineOut
                };

                gameObject.AddTween(tweenX);
            }
            else
            {
                var tweenY = new AnchoredPositionYTween()
                {
                    from = endPos.y,
                    to = mouseSlidePos.y,
                    duration = tweenDurationMouse,
                    easeType = EaseType.SineOut
                };

                gameObject.AddTween(tweenY);
            }
        }
    }

    public void SlideOutMouse()
    {
        if (hasMouseSlide)
        {
            if (directionState == 0 || directionState == 1)
            {
                var tweenX = new AnchoredPositionXTween()
                {
                    from = mouseSlidePos.x,
                    to = endPos.x,
                    duration = tweenDurationMouse,
                    easeType = EaseType.SineOut
                };

                gameObject.AddTween(tweenX);
            }
            else
            {
                var tweenY = new AnchoredPositionYTween()
                {
                    from = mouseSlidePos.y,
                    to = endPos.y,
                    duration = tweenDurationMouse,
                    easeType = EaseType.SineOut
                };

                gameObject.AddTween(tweenY);
            }
        }
    }
}
