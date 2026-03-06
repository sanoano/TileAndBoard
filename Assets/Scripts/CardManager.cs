using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tweens;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{

    public static CardManager instance;

    [Header("References")]
    [SerializeField] private CardDeck playerDeckSO;
    public GameObject cardVisualPrefab;
    public GameObject cardHoldPosition;

    public List<CardDeck.CardData> playerDeck;
    public List<CardDeck.CardData> playerHand;
    public List<GameObject> playerHandVisuals;



    [Header("Properties")] 
    [SerializeField] private int maxCards;
    [SerializeField] private int initialDrawAmount;
    [SerializeField] private float cardDisplayOffset;
    [SerializeField] private float cardLayoutTime;
    public bool cardDrawInProgress;
    
    
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

    public void DrawCardHandler(int amount)
    {
        if (TacticsManager.instance.CanAfford(1) && TurnManager.instance.isYourTurn)
        {
            TacticsManager.instance.RemoveTacticsPoints(1);
            StartCoroutine(DrawCard(amount));
        }
        
    }

    private IEnumerator DrawCard(int amount)
    {

        cardDrawInProgress = true;
        
        for (int i = 0; i < amount; i++)
        {
            if (playerDeck.Count == 0) break;
            if (playerHand.Count >= maxCards) break;

            var randInt = Random.Range(0, playerDeck.Count);

            playerHand.Add(playerDeck[randInt]);
            
            var cardVisual = BuildCard(playerDeck[randInt]);

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

            else
            {
                yield return new WaitForSeconds(cardLayoutTime);
            }
            
            
        }

        cardDrawInProgress = false;
        yield return null;
        
    }

    public void RemoveCard(GameObject cardVisual)
    {
        var index = playerHandVisuals.IndexOf(cardVisual);

        if (playerHandVisuals.Count == 0) return;
        foreach (GameObject card in playerHandVisuals)
        {
            if (playerHandVisuals.IndexOf(card) > index)
            {
                var tween = new LocalPositionTween()
                {
                    to = new Vector3(card.transform.localPosition.x + cardDisplayOffset,
                        card.transform.localPosition.y,
                        card.transform.localPosition.z),
                    duration = cardLayoutTime,
                    easeType = EaseType.ElasticOut
                };

                card.AddTween(tween);
            }
            else if (playerHandVisuals.IndexOf(card) < index)
            {
                var tween = new LocalPositionTween()
                {
                    to = new Vector3(card.transform.localPosition.x - cardDisplayOffset,
                        card.transform.localPosition.y,
                        card.transform.localPosition.z),
                    duration = cardLayoutTime,
                    easeType = EaseType.ElasticOut
                };

                card.AddTween(tween);
            }
            
        }
        
    }
    
    public GameObject BuildCard(CardDeck.CardData cardData)
    {// Probably a nicer way to do this... Oh well...
        
        
        var cardVisual = Instantiate(cardVisualPrefab,
            cardHoldPosition.transform.position,
            cardHoldPosition.transform.rotation,
            cardHoldPosition.transform);

        var textFields = cardVisual.GetComponentsInChildren<TextMeshProUGUI>();
        var gridSquares = cardVisual.GetComponentsInChildren<SpriteRenderer>(true);
        
        textFields[0].text = cardData.Name;
        textFields[1].text = cardData.Health.ToString();
        textFields[2].text = cardData.Speed.ToString();
        textFields[3].text = cardData.Damage.ToString();
        textFields[4].text = cardData.Defence.ToString();

        //I'm not arsed to do something smart rn...will fix this later...maybe...
        //Just wakes up the right squares. Make sure they're all inactive in the prefab before running the game...
        
        foreach (Vector2Int coord in cardData.Range)
        {// Simple logic tree to find out which squares should show up...inelegant but robust enough...
            int x = coord.x;
            int y = coord.y;

            if (y == 0)
            {
                if (x == 0)
                    gridSquares[1].gameObject.SetActive(true);
                else if (x == 1)
                    gridSquares[2].gameObject.SetActive(true);
                else if (x == 2)
                    gridSquares[3].gameObject.SetActive(true);
            }
            else if (y == 1)
            {
                if (x == 0)
                    gridSquares[4].gameObject.SetActive(true);
                else if (x == 1)
                    gridSquares[5].gameObject.SetActive(true);
                else if (x == 2)
                    gridSquares[6].gameObject.SetActive(true);
            }
            else if (y == 2)
            {
                if (x == 0)
                    gridSquares[7].gameObject.SetActive(true);
                else if (x == 1)
                    gridSquares[8].gameObject.SetActive(true);
                else if (x == 2)
                    gridSquares[9].gameObject.SetActive(true);
            }
        }

        return cardVisual;
    }
    
}
