using System;
using System.Collections;
using System.Linq;
using TMPro;
using Tweens;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Tweens;
using Tweens.Core;

public class Hourglass : MonoBehaviour
{
    [SerializeField] private GameObject half1Empty;//This item starts at defaultScale then shrinks to 0
    [SerializeField] private GameObject half1Fill;//This item starts at empty then creeps up to defaultScale
    [SerializeField] private GameObject half2Empty;
    [SerializeField] private GameObject half2Fill;

    private bool flippedUp = false;
    private float defaultScaleValue = 0.9f;
    private Vector3 defaultScale;

    //This is for all the stuff in switching from left to right
    private Transform position1, position2;
    private Transform player1Position, player2Position;
    private bool positionsAssigned = false;

    void Start()
    {
        defaultScale = new Vector3(defaultScaleValue, defaultScaleValue, defaultScaleValue);

        StartCoroutine(InitiateTimer(10.0f, true));
    }

    void Update()
    {

    }

    public void AssignPositions(bool firstPlayerLeft)
    {
        if (firstPlayerLeft)
        {
            player1Position = position1;
            player2Position = position2;
        }
        else
        {
            player1Position = position2;
            player2Position = position1;
        }
    }

    private IEnumerator InitiateTimer(float timerLength, bool p1)
    {
        float elapsed = 0f;

        if (flippedUp)
        {
            var tweenX = new EulerAnglesXTween()
            {
                from = 0,
                to = 180.0f,
                duration = 0.01f,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenX);
        }
        else
        {
            var tweenX = new EulerAnglesXTween()
            {
                from = 180.0f,
                to = 0.0f,
                duration = 0.01f,
                easeType = EaseType.SineOut
            };

            gameObject.AddTween(tweenX);
        }


        if (p1)
        {
            half1Empty.transform.localScale = defaultScale;
            half2Fill.transform.localScale = Vector3.zero;
        }
        else
        {
            half2Empty.transform.localScale = defaultScale;
            half1Fill.transform.localScale = Vector3.zero;
        }

        while (elapsed < timerLength)
        {
            float t = elapsed / timerLength;

            if (p1)
            {
                half1Empty.transform.localScale = Vector3.Lerp(defaultScale, Vector3.zero, t);
                half2Fill.transform.localScale = Vector3.Lerp(Vector3.zero, defaultScale, t);
            }
            else
            {
                half2Empty.transform.localScale = Vector3.Lerp(defaultScale, Vector3.zero, t);
                half1Fill.transform.localScale = Vector3.Lerp(Vector3.zero, defaultScale, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (p1)
        {
            half1Empty.transform.localScale = Vector3.zero;
            half2Fill.transform.localScale = defaultScale;
        }
        else
        {
            half2Empty.transform.localScale = Vector3.zero;
            half1Fill.transform.localScale = defaultScale;
        }

        if (!flippedUp)
            flippedUp = true;
    }
}
