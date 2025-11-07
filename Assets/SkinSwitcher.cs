using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
public class SkinSwitcher : MonoBehaviour
{
    List<string> optionsToAdd = new List<string>();

    public TextAsset textJSON;
    private string filePath = "Resources\\CardSets\\";
    private string skinFolder = "Default";
    private string filePathBlue = "\\CardPNGS\\CardBlue";
    private string filePathGreen = "\\CardPNGS\\CardGreen";
    private string filePathPurple = "\\CardPNGS\\CardPurple";
    private string filePathYellow = "\\CardPNGS\\CardYellow";

    public Deck gameDeck;

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

    private void Start()
    {
        mySkinsList = JsonUtility.FromJson<SkinsList>(textJSON.text);

        foreach (var cardSkins in mySkinsList.cardSkins)
        {
            optionsToAdd.Add(cardSkins.skinName);
        }

        dropdown.ClearOptions();

        dropdown.AddOptions(optionsToAdd);

        onDropDownUpdate(1);
    }

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

            gameDeck.updateCards(purpleCard,greenCard,blueCard,yellowCard);
        }
    }

    private Sprite changeSprite(string dir)
    {
        string newFilePath = filePath + skinFolder + dir;
       
        return Resources.Load<Sprite>(newFilePath); ;
    }


}
