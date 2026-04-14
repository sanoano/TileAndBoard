using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Tweens;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class CardManager : NetworkBehaviour
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
    public int maxCards;
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
        
        
        if (!TacticsManager.instance.CanAfford(1))
        {
            TextDialogue.instance.DialogueRecieveStatus(1);
            return;
        }

        if (!TurnManager.instance.isYourTurn)
        {
            TextDialogue.instance.DialogueRecieveStatus(5);
            return;
        }

        if (playerHand.Count >= maxCards)
        {
            TextDialogue.instance.DialogueRecieveStatus(4);
            return;
        }

        if (BoardManager.Instance.attackInProgress)
        {
            return;
        }
        
        TacticsManager.instance.RemoveTacticsPoints(1);
        StartCoroutine(DrawCard(amount));
        
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
            
            AudioManager.singleton.PlaySound("cardDeal", true);

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

    public void RecallCard(GameObject cardVisual, BoardManager.Unit unit)
    {
        
        TacticsManager.instance.RemoveTacticsPoints(1);
        
        UIManager.Instance.DestroyCurrentInfoInstance();
        
        BoardManager.Instance.NullSelection();

        BoardManager.Instance.localBoard.Visuals[unit.Position.x, unit.Position.y] = null;
        
        CardDeck cardList = Resources.Load<CardDeck>("Data/MasterList");

        CardDeck.CardData cardData = new CardDeck.CardData();

        foreach (CardDeck.CardData card in cardList.Cards)
        {
            if (card.Name == unit.Name)
            {
                cardData = card;
            }
        }

        cardData.Health = unit.Health;
        
        playerHand.Add(cardData);
        playerHandVisuals.Add(cardVisual);

        
        
        if (playerHand.Count != 1)
        {

            var pos = new Vector3(0 - (cardDisplayOffset * playerHand.Count - cardDisplayOffset),
                0,
                0);

            var tween = new LocalPositionTween
            {
                to = pos,
                duration = cardLayoutTime,
                easeType = EaseType.ElasticOut
            };
            
            var rotTween = new LocalRotationTween
            {
                to = Quaternion.identity,
                duration = cardLayoutTime,
                easeType = EaseType.ElasticOut
            };

            var scaleTween = new LocalScaleTween
            {
                to = new Vector3(4, 4, 4),
                duration = cardLayoutTime,
                easeType = EaseType.ElasticOut
            };

            cardVisual.AddTween(tween);
            cardVisual.AddTween(rotTween);
            cardVisual.AddTween(scaleTween);
            
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
                
          
        }
        
        
        cardVisual.GetComponent<CardDrag>().isPlaced = false;

        if (NetworkManager.Singleton)
        {
            foreach (ulong clientIds in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (clientIds == NetworkManager.Singleton.LocalClientId) continue;
                RecallCardRpc(BoardManager.Instance.unitsList.IndexOf(unit),RpcTarget.Single(clientIds, RpcTargetUse.Temp));
            }
        }
        

        BoardManager.Instance.unitsList.Remove(unit);

    }
    [Rpc(SendTo.SpecifiedInParams)]
    private void RecallCardRpc(int unitIndex, RpcParams rpcParams = default)
    {

        BoardManager.Unit unit = BoardManager.Instance.unitsList[unitIndex];
        
        Destroy(BoardManager.Instance.enemyBoard.Visuals[unit.Position.x, unit.Position.y]);

        BoardManager.Instance.unitsList.Remove(unit);


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
        var cardSprites = cardVisual.GetComponentsInChildren<SpriteRenderer>(true);
        
        textFields[0].text = cardData.Name;
        textFields[1].text = cardData.Health.ToString();

        if (cardData.Speed > 0)
        {
            textFields[2].text = cardData.Speed.ToString();
            cardSprites[4].enabled = true;

        }
        else
        {
            textFields[2].text = "";
            cardSprites[4].enabled = false;
        }

        if (cardData.Damage > 0)
        {
            textFields[3].text = cardData.Damage.ToString();
            cardSprites[5].enabled = true;
        }
        else
        {
            textFields[3].text = "";
            cardSprites[5].enabled = false;
        }

        if (cardData.Defence > 0)
        {
            textFields[4].text = cardData.Defence.ToString();
            cardSprites[6].enabled = true;
        }
        else
        {
            textFields[4].text = "";
            cardSprites[6].enabled = false;
        }

        //I'm not arsed to do something smart rn...will fix this later...maybe...
        //Just wakes up the right squares. Make sure they're all inactive in the prefab before running the game...



        foreach (Vector2Int coord in cardData.Range)
        {// Simple logic tree to find out which squares should show up...inelegant but robust enough...
            int x = coord.x;
            int y = coord.y;

            if (y == 0)
            {
                if (x == 0)
                    cardSprites[8].gameObject.SetActive(true);
                else if (x == 1)
                    cardSprites[9].gameObject.SetActive(true);
                else if (x == 2)
                    cardSprites[10].gameObject.SetActive(true);
            }
            else if (y == 1)
            {
                if (x == 0)
                    cardSprites[11].gameObject.SetActive(true);
                else if (x == 1)
                    cardSprites[12].gameObject.SetActive(true);
                else if (x == 2)
                    cardSprites[13].gameObject.SetActive(true);
            }
            else if (y == 2)
            {
                if (x == 0)
                    cardSprites[14].gameObject.SetActive(true);
                else if (x == 1)
                    cardSprites[15].gameObject.SetActive(true);
                else if (x == 2)
                    cardSprites[16].gameObject.SetActive(true);
            }
        }

        return cardVisual;
    }
    
}
