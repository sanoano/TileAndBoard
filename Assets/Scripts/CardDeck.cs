using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json; 

[CreateAssetMenu(fileName = "CardDeck", menuName = "Scriptable Objects/CardDeck")]
public class CardDeck : ScriptableObject
{
    public List<int> cardIds = new List<int>();
    
    [Serializable]
    public class CardData  
    {
        public int ID;
        public string Name;
        public int Health;
        public int Cost;
        public int Speed;
        public int Defence;
        public List<int[]> Range;
        public int Damage;
    }
    
    public List<CardData> Cards;

    [InspectorButton("FromJsonToDeck")]
    public bool populateDeck;
    
    [InspectorButton("ClearDeck")]
    public bool clearDeck;

    public void FromJsonToDeck()
    {
     
        TextAsset jsonAsset = Resources.Load<TextAsset>("Data/CardData");
        if (jsonAsset == null)
        {
            Debug.LogError("Failed to load 'Data/CardData' from Resources folder.");
            return;
        }
        string json = jsonAsset.text;


        List<CardData> tempCards;
        tempCards = JsonConvert.DeserializeObject<List<CardData>>(json);

        foreach (CardData card in tempCards)
        {
            if (cardIds.Contains(card.ID))
            {
                Cards.Add(card);
                for (int i = 0; i < card.Range.Count; i++)
                {
                    Debug.Log($"Card {card.Name}: {card.Range[i][0]}" + $", {card.Range[i][1]}");
                    
                }
            }
        }



    }

    public void ClearDeck()
    {
        Cards.Clear();
    }
}