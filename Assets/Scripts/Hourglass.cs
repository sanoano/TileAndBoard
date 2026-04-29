using System;
using System.Collections;
using System.Linq;
using TMPro;
using Tweens;
using Tweens.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class Hourglass : MonoBehaviour
{
    [SerializeField] private GameObject half1Empty;//This item starts at defaultScale then shrinks to 0
    [SerializeField] private GameObject half1Fill;//This item starts at empty then creeps up to defaultScale
    [SerializeField] private GameObject half2Empty;
    [SerializeField] private GameObject half2Fill;

    private bool flippedUp = false;
    private float defaultScaleValue = 0.9f;
    private Vector3 defaultScale;
    [SerializeField] private float turnSpeed = 50.0f;
    [SerializeField] private float moveSpeed = 5.0f;

    //This is for all the stuff in switching from left to right
    [SerializeField] private Transform position1, position2;
    private Transform player1Position, player2Position;
    private bool positionsAssigned = false;

    void Start()
    {
        if (!positionsAssigned)
            AssignPositions(true);
    }

    private void Update()
    {
        //if (Input.GetKeyDown("r"))
        //    FlipHourglass(5f);
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

    public void FlipHourglass(float timerLength)
    {
        Debug.Log(flippedUp);

        Quaternion originalRot = Quaternion.identity;
        Quaternion flippedRot = new Quaternion(originalRot.x + 180.0f, originalRot.y, originalRot.z, 1);

        var tweenX = new RotationTween()
        {
            from = originalRot,
            to = flippedRot,
            duration = turnSpeed,
            easeType = EaseType.Linear,
        };

        gameObject.AddTween(tweenX);

        StopAllCoroutines();

        if (flippedUp)
        {
            half2Empty.SetActive(true);
            half1Empty.SetActive(false);

            half2Empty.transform.localScale = defaultScale;
            half1Fill.transform.localScale = Vector3.zero;
            half2Fill.transform.localScale = Vector3.zero;

            StartCoroutine(InitiateTimer(timerLength));

            var tweenPos1 = new PositionTween()
            {
                from = player1Position.position,
                to = player2Position.position,
                duration = moveSpeed,
                easeType = EaseType.ExpoOut
            };

            gameObject.AddTween(tweenPos1);

            flippedUp = false;
        }
        else
        {
            half2Empty.SetActive(true);
            half1Empty.SetActive(false);

            half2Empty.transform.localScale = defaultScale;
            half1Fill.transform.localScale = Vector3.zero;
            half2Fill.transform.localScale = Vector3.zero;

            StartCoroutine(InitiateTimer(timerLength));

            var tweenPos2 = new PositionTween()
            {
                from = player2Position.position,
                to = player1Position.position,
                duration = moveSpeed,
                easeType = EaseType.ExpoOut
            };

            gameObject.AddTween(tweenPos2);

            flippedUp = true;
        }
    }

    private IEnumerator InitiateTimer(float timerLength)
    {
        float elapsed = 0f;
        defaultScale = new Vector3(defaultScaleValue, defaultScaleValue, defaultScaleValue);

        while (elapsed < timerLength)
        {
            float t = elapsed / timerLength;

            half2Empty.transform.localScale = Vector3.Lerp(defaultScale, Vector3.zero, t);
            half1Fill.transform.localScale = Vector3.Lerp(Vector3.zero, defaultScale, t);

            elapsed += Time.deltaTime;

            yield return null;
        }

        half2Empty.transform.localScale = Vector3.zero;
        half1Fill.transform.localScale = defaultScale;
    }
}
