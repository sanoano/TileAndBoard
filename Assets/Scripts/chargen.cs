using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using ColorPicker;

public class chargen : MonoBehaviour
{//Includes UI and data loading

    public static chargen instance;


    [Header("Data Files")]
    CosmeticStatus cosmeticStatus;
    string defaultCode = "ADD8E68B00005C2B2400000009";//blue skin (6), red eyes (6), auburn hair (6), first outfit (3), first head (3), ninth sprite (2)
    string code;
    string filePath;
    const string fileName = "Cosmetics.json";

    [HideInInspector] public int torsoIndex, headIndex, spriteIndex;
    [HideInInspector] public Color skinColour, eyeColour, hairColour;

    [Header("UI")]
    /*[SerializeField] private Image swatch0;
    [SerializeField] private Image swatch1;
    [SerializeField] private Image swatch2;*/
    [SerializeField] private Button[] buttons;

    //[SerializeField] private GameObject[] colourPickers;
    //private ColorPicker.ColorPicker activePicker;
    private int oldPicker = 3;

    public struct CosmeticStatus
    {
        public string characterCode;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        filePath = Application.persistentDataPath;
        cosmeticStatus = new CosmeticStatus();
        Debug.Log(filePath);
        LoadCosmeticData(false);

        //Button assignment
        buttons[0].onClick.AddListener(delegate { UIManagerMainMenu.instance.navigateCosmetics(true, 2); });
        buttons[1].onClick.AddListener(delegate { UIManagerMainMenu.instance.navigateCosmetics(false, 2); });
    }

    private void Update()
    {
        /*if (oldPicker < 3)
        {
            chOutfits.instance.ChangeColour(activePicker.type, activePicker.CurrentSelectedColor);

            if (oldPicker == 0)
                testImage0.color = skinColour;
            else if (oldPicker == 1)
                testImage1.color = eyeColour;
            else if (oldPicker == 2)
                testImage2.color = hairColour;
        }*/
    }

    public void LoadCosmeticData(bool loadDefaults)
    {//If loading defaults, it saves them at the same time

        string fullPath = Path.Combine(filePath, fileName);

        if (loadDefaults)
        {
            code = defaultCode;

            cosmeticStatus.characterCode = code;

            string cosmeticStatusJson = JsonUtility.ToJson(cosmeticStatus);
            File.WriteAllText(fullPath, cosmeticStatusJson);

            Debug.Log("Defaults loaded & previous preferences overwritten");
        }
        else if (File.Exists(fullPath))
        {
            string loadedJson = File.ReadAllText(fullPath);

            cosmeticStatus = JsonUtility.FromJson<CosmeticStatus>(loadedJson);

            code = cosmeticStatus.characterCode;

            if (string.IsNullOrEmpty(code) || code.Length < defaultCode.Length)
            {
                code = defaultCode;
                cosmeticStatus.characterCode = code;
                Debug.Log("Cosmetic code invalid. Loading default values");
            }
            else
                Debug.Log("Cosmetic info found, loading " + code);
        }
        else
        {
            code = defaultCode;
            cosmeticStatus.characterCode = code;
            Debug.Log("Cosmetic info not found. Loading default values");
        }


        //Turns whatever was loaded from the above function into variables for the script
        string skinColourSubstring = code.Substring(0, 6);
        ColorUtility.TryParseHtmlString("#" + skinColourSubstring, out skinColour);

        string eyeColourSubstring = code.Substring(6, 6);
        ColorUtility.TryParseHtmlString("#" + eyeColourSubstring, out eyeColour);

        string hairColourSubstring = code.Substring(12, 6);
        ColorUtility.TryParseHtmlString("#" + hairColourSubstring, out hairColour);

        string torsoIndexSubstring = code.Substring(18, 3);
        torsoIndex = Int32.Parse(torsoIndexSubstring);

        string headIndexSubstring = code.Substring(21, 3);
        headIndex = Int32.Parse(headIndexSubstring);

        string spriteIndexSubstring = code.Substring(24, 2);
        spriteIndex = Int32.Parse(spriteIndexSubstring);

        /*testImage0.color = skinColour;
        //Debug.Log(skinColour + " " + skinColourSubstring);
        testImage1.color = eyeColour;
        //Debug.Log(eyeColour + " " + eyeColourSubstring);
        testImage2.color = hairColour;
        //Debug.Log(hairColour + " " + hairColourSubstring);

        chOutfits.instance.loadTorso(torsoIndex);
        chOutfits.instance.loadHead(headIndex);*/

        UIManagerMainMenu.instance.outfitsManager.loadSprite(spriteIndex);
    }

    public void SaveCosmeticStatus()
    {
        //Turns the edited ints into substrings to be reabsorbed back into the Big String
        string skinColourSubstring = ColorUtility.ToHtmlStringRGB(skinColour);

        string eyeColourSubstring = ColorUtility.ToHtmlStringRGB(eyeColour);

        string hairColourSubstring = ColorUtility.ToHtmlStringRGB(hairColour);

        string torsoIndexSubstring = torsoIndex.ToString();
        if (torsoIndexSubstring.Length == 1)
            torsoIndexSubstring = "00" + torsoIndexSubstring;
        else if (torsoIndexSubstring.Length == 2)
            torsoIndexSubstring = "0" + torsoIndexSubstring;

        string headIndexSubstring = headIndex.ToString();
        if (headIndexSubstring.Length == 1)
            headIndexSubstring = "00" + headIndexSubstring;
        else if (headIndexSubstring.Length == 2)
            headIndexSubstring = "0" + headIndexSubstring;

        string spriteIndexSubstring = spriteIndex.ToString();
        if (spriteIndexSubstring.Length == 1)
            spriteIndexSubstring = "0" + spriteIndexSubstring;

        cosmeticStatus.characterCode = skinColourSubstring + eyeColourSubstring + hairColourSubstring + torsoIndexSubstring + headIndexSubstring + spriteIndexSubstring;
        Debug.Log(cosmeticStatus.characterCode);

        string cosmeticStatusJson = JsonUtility.ToJson(cosmeticStatus);
        File.WriteAllText(filePath + "/" + fileName, cosmeticStatusJson);
        Debug.Log("Cosmetics saved");
    }

    //UI functions, should probably move this to a dedicated 
    /*public void ToggleColourPicker(int type)//0 for skin, 1 for eyes, 2 for hair
    {
        if (oldPicker == type)
        {
            colourPickers[type].SetActive(false);
            activePicker = null;
            oldPicker = 3;
        }

        else
        {
            colourPickers[type].SetActive(true);
            activePicker = colourPickers[type].GetComponent<ColorPicker.ColorPicker>();
            oldPicker = type;
        }


        if (type == 0)
        {
            colourPickers[1].SetActive(false);
            colourPickers[2].SetActive(false);
        }
        else if (type == 1)
        {
            colourPickers[0].SetActive(false);
            colourPickers[2].SetActive(false);
        }
        else if (type == 2)
        {
            colourPickers[0].SetActive(false);
            colourPickers[1].SetActive(false);
        }
        else
        {
            colourPickers[0].SetActive(false);
            colourPickers[1].SetActive(false);
            colourPickers[2].SetActive(false);
        }
    }*/
}
