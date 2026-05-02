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

    [SerializeField] private GameObject cloud;

    private bool flippedUp = false;
    private float defaultScaleValue = 0.9f;
    private Vector3 defaultScale;
    [SerializeField] private float turnSpeed = 50.0f;
    [SerializeField] private float moveSpeed = 5.0f;

    //This is for all the stuff in switching from left to right
    //[SerializeField] private Transform position1, position2;
    private Transform player1Position, player2Position;
    private Vector3 player1PositionCloud, player2PositionCloud;
    private bool positionsAssigned = false;

    void Start()
    {
        //if (!positionsAssigned)
        //    AssignPositions(true);
    }

    private void Update()
    {
        //if (Input.GetKeyDown("r"))
        //    FlipHourglass(5f);
    }

    public void AssignPositions(Transform leftPos, Transform rightPos)
    {
        player1Position = leftPos;
        player2Position = rightPos;

        player1PositionCloud = new Vector3(player1Position.position.x, player1Position.position.y - 5, player1Position.position.z);
        player2PositionCloud = new Vector3(player2Position.position.x, player2Position.position.y - 5, player2Position.position.z);

        if (GameManager.instance.playerId == Player.PlayerId.Player1)
        {
            if (TurnManager.instance.currentTurn == TurnManager.TurnState.Player1Turn)
            {
                gameObject.transform.position = player1Position.position;
                cloud.transform.position = player1PositionCloud;
                flippedUp = true;
            }
            else
            {
                gameObject.transform.position = player2Position.position;
                cloud.transform.position = player2PositionCloud;
            }
        }
        else
        {
            if (TurnManager.instance.currentTurn == TurnManager.TurnState.Player1Turn)
            {
                gameObject.transform.position = player1Position.position;
                cloud.transform.position = player1PositionCloud;
                flippedUp = true;
            }
            else
            {
                gameObject.transform.position = player2Position.position;
                cloud.transform.position = player2PositionCloud;
            }
        }
        
        
    }

    public void FlipHourglass(float timerLength)
    {
        //Debug.Log(flippedUp);

        Quaternion originalRot = Quaternion.identity;
        Quaternion flippedRot = Quaternion.Euler(originalRot.x + 180f, originalRot.y, originalRot.z);

        var tweenX = new RotationTween()
        {
            from = originalRot,
            to = flippedRot,
            duration = moveSpeed,
            easeType = EaseType.ExpoOut,
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

            var tweenCloudPos1 = new PositionTween()
            {
                from = player1PositionCloud,
                to = player2PositionCloud,
                duration = moveSpeed / 4,
                easeType = EaseType.ExpoOut
            };

            gameObject.AddTween(tweenPos1);
            cloud.AddTween(tweenCloudPos1);

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

            var tweenCloudPos2 = new PositionTween()
            {
                from = player2PositionCloud,
                to = player1PositionCloud,
                duration = moveSpeed / 4,
                easeType = EaseType.ExpoOut
            };

            gameObject.AddTween(tweenPos2);
            cloud.AddTween(tweenCloudPos2);

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
