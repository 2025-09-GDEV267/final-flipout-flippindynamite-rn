using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
public class SkinSwitcher : MonoBehaviour
{
    List<string> optionsToAdd = new List<string>();

    public TextAsset textJSON;
    private string filePath = "CardSets\\";
    private string skinFolder = "Default";
    private string filePathBlue = "\\CardPNGS\\CardBlue";
    private string filePathGreen = "\\CardPNGS\\CardGreen";
    private string filePathPurple = "\\CardPNGS\\CardPurple";
    private string filePathYellow = "\\CardPNGS\\CardYellow";

    [SerializeField]
    Sprite[] currentSprites;

    public TMP_Dropdown dropdown;
    [System.Serializable]
    public class CardSkins
    {
        public int id;
        public string skinName;
        public string folderName;
    }
    [System.Serializable]
    public class SkinsList
    {
        public CardSkins[] cardSkins;
    }

    public SkinsList mySkinsList;

    private void Awake()
    {
        mySkinsList = JsonUtility.FromJson<SkinsList>(textJSON.text);

        foreach (var cardSkins in mySkinsList.cardSkins)
        {
            optionsToAdd.Add(cardSkins.skinName);
        }

        dropdown.ClearOptions();

        dropdown.AddOptions(optionsToAdd);

        onDropDownUpdate(0);
    }
    /*void initTestCard()
    {
        Sprite blueCard = changeSprite(filePathBlue);
        Sprite greenCard = changeSprite(filePathGreen);
        Sprite yellowCard = changeSprite(filePathYellow);
        Sprite purpleCard = changeSprite(filePathPurple);

        Sprite[] sprites = { purpleCard, greenCard, blueCard, yellowCard };

        Array allPossibleColors = Enum.GetValues(typeof(cardColor));

        int randomOne = UnityEngine.Random.Range(0, allPossibleColors.Length);

        int randomTwo = UnityEngine.Random.Range(0, allPossibleColors.Length);

        GameObject newCard = GameObject.Instantiate(cardPrefab,new Vector3(0,0,0),Quaternion.identity);

        newCard.GetComponent<Card>().Init((cardColor)allPossibleColors.GetValue(randomOne), (cardColor)allPossibleColors.GetValue(randomTwo), sprites);

        gameDeck.Add(newCard.GetComponent<Card>());
    }*/

    public void onDropDownUpdate(int selection)
    {
        bool success = false;

        foreach (var cardSkins in mySkinsList.cardSkins)
        {
            if(selection == cardSkins.id)
            {
                skinFolder = cardSkins.folderName;
                success = true;
            }
        }

        if (success)
        {
            Sprite blueCard = changeSprite(filePathBlue);
            Sprite greenCard = changeSprite(filePathGreen);
            Sprite yellowCard = changeSprite(filePathYellow);
            Sprite purpleCard = changeSprite(filePathPurple);

            currentSprites = new Sprite[]{ purpleCard, greenCard, blueCard, yellowCard};
        }

    }

    private Sprite changeSprite(string dir)
    {
        string newFilePath = filePath + skinFolder + dir;
        Debug.Log(newFilePath);
        Debug.Log(Resources.Load<Sprite>(newFilePath));
        return Resources.Load<Sprite>(newFilePath);
    }

    [SerializeField]
    public Sprite[] getSkin()
    {
        return currentSprites;
    }
}
