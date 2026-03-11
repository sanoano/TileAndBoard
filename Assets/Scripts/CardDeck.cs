using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;  


// You guys are going to have to excuse the AI gen code in here. I would literally only use it as a last resort
// because honestly getting this shit to serialize properly was an odyssey. Unity's support for serialization
// from JSON and honestly serialization in general is just terrible and not even documented that well. Pray God this is the
// only bit of LLM code we have in here. Really annoying how it leaves superfluous comments everywhere too.



// Custom converter for Vector2Int to handle [int, int] arrays from JSON
public class Vector2IntConverter : JsonConverter<Vector2Int>
{
    public override Vector2Int ReadJson(JsonReader reader, Type objectType, Vector2Int existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Read as int array [x, y]
        int[] values = serializer.Deserialize<int[]>(reader);
        return (values.Length >= 2) ? new Vector2Int(values[0], values[1]) : Vector2Int.zero;
    }

    public override void WriteJson(JsonWriter writer, Vector2Int value, JsonSerializer serializer)
    {
        writer.WriteStartArray();
        writer.WriteValue(value.x);
        writer.WriteValue(value.y);
        writer.WriteEndArray();
    }
}

[CreateAssetMenu(fileName = "CardDeck", menuName = "Scriptable Objects/CardDeck")]
public class CardDeck : ScriptableObject
{
    public List<int> cardIds = new List<int>();  // User-defined, left unchanged
    
    [Serializable]
    public struct CardData  
    {
        public int ID;
        public string Name;
        public int Health;
        public int Cost;
        public int Speed;
        public int Defence;
        public List<Vector2Int> Range;  
        public int Damage;
    }
    
    public List<CardData> Cards = new List<CardData>();

    [InspectorButton("FromJsonToDeck")]
    public bool populateDeck;
    
    [InspectorButton("ClearDeck")]
    public bool clearDeck;

    [InspectorButton("FromJSONToMaster")] 
    public bool addAll;

    public void FromJsonToDeck()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("Data/CardData");
        if (jsonAsset == null)
        {
            Debug.LogError("Failed to load 'Data/CardData' from Resources folder.");
            return;
        }
        string json = jsonAsset.text;

        try
        {
            // Settings with converter for Vector2Int and error handling
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { args.ErrorContext.Handled = true; },  // Ignore conversion errors, use defaults
                Converters = { new Vector2IntConverter() }  // Apply converter for List<Vector2Int>
            };

            List<CardData> tempCards = JsonConvert.DeserializeObject<List<CardData>>(json, settings) ?? new List<CardData>();

            foreach (CardData card in tempCards)
            {
                if (cardIds.Contains(card.ID))  // As per your logic: only add if ID is in user-defined cardIds list
                {
                    Cards.Add(card);
                    // Debug Range (fixed: iterate through all elements, use .x/.y instead of Item1/Item2)
                    if (card.Range != null)
                    {
                        for (int i = 0; i < card.Range.Count; i++)
                        {
                            Debug.Log($"Card {card.Name}: Range[{i}] = ({card.Range[i].x}, {card.Range[i].y})");
                        }
                    }
                    else
                    {
                        Debug.Log($"Card {card.Name}: Range is null");
                    }
                }
            }
            
            // Mark ScriptableObject as dirty (essential for range values to persist at runtime)
            //UnityEditor.EditorUtility.SetDirty(this);
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON Deserialization failed: {e.Message}");
        }
    }

    public void FromJSONToMaster()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>("Data/CardData");
        if (jsonAsset == null)
        {
            Debug.LogError("Failed to load 'Data/CardData' from Resources folder.");
            return;
        }
        string json = jsonAsset.text;

        try
        {
            // Settings with converter for Vector2Int and error handling
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Error = (sender, args) => { args.ErrorContext.Handled = true; },  // Ignore conversion errors, use defaults
                Converters = { new Vector2IntConverter() }  // Apply converter for List<Vector2Int>
            };

            List<CardData> tempCards = JsonConvert.DeserializeObject<List<CardData>>(json, settings) ?? new List<CardData>();

            foreach (CardData card in tempCards)
            {
               
                Cards.Add(card);
                // Debug Range (fixed: iterate through all elements, use .x/.y instead of Item1/Item2)
                if (card.Range != null)
                {
                    for (int i = 0; i < card.Range.Count; i++)
                    {
                            Debug.Log($"Card {card.Name}: Range[{i}] = ({card.Range[i].x}, {card.Range[i].y})");
                    }
                }
                else 
                {
                    Debug.Log($"Card {card.Name}: Range is null");
                }
                
            }
            
            // Mark ScriptableObject as dirty (essential for range values to persist at runtime)
            //UnityEditor.EditorUtility.SetDirty(this);
        }
        catch (Exception e)
        {
            Debug.LogError($"JSON Deserialization failed: {e.Message}");
        }
    }

    public void ClearDeck()
    {
        Cards.Clear();
    }
}