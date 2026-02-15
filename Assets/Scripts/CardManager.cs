using System;
using System.Collections;
using System.Collections.Generic;
using Tweens;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{

    public static CardManager instance;

    [Header("References")]
    [SerializeField] private CardDeck playerDeckSO;
    [SerializeField] private GameObject cardVisualPrefab;
    public GameObject cardHoldPosition;

    public List<CardDeck.CardData> playerDeck;
    public List<CardDeck.CardData> playerHand;
    public List<GameObject> playerHandVisuals;
    
    

    [Header("Properties")] 
    [SerializeField] private int initialDrawAmount;
    [SerializeField] private float cardDisplayOffset;
    [SerializeField] private float cardLayoutTime;
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        
        
    }

    private void Start()
    {
        playerDeck = new List<CardDeck.CardData>(playerDeckSO.Cards);

        playerHandVisuals = new List<GameObject>();

        StartCoroutine(DrawCard(initialDrawAmount));

    }

    public IEnumerator DrawCard(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (playerDeck.Count == 0) break;

            var randInt = Random.Range(0, playerDeck.Count);

            playerHand.Add(playerDeck[randInt]);

            
            //Need to replace with card builder method later on.
            var cardVisual = Instantiate(cardVisualPrefab,
                cardHoldPosition.transform.position,
                cardHoldPosition.transform.rotation,
                cardHoldPosition.transform);

            playerHandVisuals.Add(cardVisual);

            cardVisual.name = playerHand[playerHand.Count - 1].Name;

            playerDeck.RemoveAt(randInt);
            

            if (playerHand.Count != 1)
            {

                var pos = new Vector3(cardVisual.transform.localPosition.x - (cardDisplayOffset * playerHand.Count - cardDisplayOffset),
                    cardVisual.transform.localPosition.y,
                    cardVisual.transform.localPosition.z);

                var tween = new LocalPositionTween
                {
                    to = pos,
                    duration = cardLayoutTime,
                    easeType = EaseType.ElasticOut
                };

                var instance = cardVisual.AddTween(tween);

                

                foreach (GameObject card in playerHandVisuals)
                {
                    if (card == cardVisual) continue;

                    var pos2 = new Vector3(card.transform.localPosition.x + cardDisplayOffset,
                        card.transform.localPosition.y,
                        card.transform.localPosition.z);

                    var tween2 = new LocalPositionTween()
                    {
                        to = pos2,
                        duration = cardLayoutTime,
                        easeType = EaseType.ElasticOut
                    };

                    card.AddTween(tween2);
                }
                
                yield return instance.AwaitDecommission();
            }

            yield return null;
        }
    }




}
