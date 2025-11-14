using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum cardColor {purple,green,blue,yellow}
public class Card : NetworkBehaviour
{
    SpriteRenderer spriteRenderer;

    public NetworkVariable<cardColor> colorOne = new NetworkVariable<cardColor>();
    public NetworkVariable<cardColor> colorTwo = new NetworkVariable<cardColor>();

    [ClientRpc]
    public void SetClientRpc(cardColor sideOne, cardColor sideTwo)
    {
        //if (IsServer)
        //{
            colorOne.Value = sideOne;
            colorTwo.Value = sideTwo;
       // }

        updateGraphics();
    }
   private void OnColorOneChanged(cardColor oldValue, cardColor newValue)
    {
        updateGraphics();
    }

    private void OnColorTwoChanged(cardColor oldValue, cardColor newValue)
    {
        updateGraphics();
    }
    public void updateGraphics()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        Sprite[] cardSprites = GameObject.FindGameObjectWithTag("CosMan").GetComponent<SkinSwitcher>().getSkin();

        Debug.Log(cardSprites.IsUnityNull());

        switch (colorOne.Value)
        {
            case (cardColor.purple):
                spriteRenderer.sprite = cardSprites[0];
                break;
            case (cardColor.green):
                spriteRenderer.sprite = cardSprites[1];
                break;
            case (cardColor.blue):
                spriteRenderer.sprite = cardSprites[2];
                break;
            case (cardColor.yellow):
                spriteRenderer.sprite = cardSprites[3];
                break;
        }
    }
}

