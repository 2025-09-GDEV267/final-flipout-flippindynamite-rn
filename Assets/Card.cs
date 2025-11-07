using UnityEngine;

public enum cardColor {purple,green,blue,yellow}
public class Card : MonoBehaviour
{
    public cardColor[] colors;
    public Sprite[] cardSprites; //0 purple,1 green, 2 blue, yellow
    SpriteRenderer spriteRenderer;

    public void Init(cardColor sideOne, cardColor sideTwo, Sprite[] newCardSprites)
    {
        colors[0] = sideOne;
        colors[1] = sideTwo;

        cardSprites = newCardSprites;

        spriteRenderer = GetComponent<SpriteRenderer>();

        updateGraphics();
    }

    private void updateGraphics()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        cardSprites = GameObject.FindGameObjectWithTag("CosMan").GetComponent<SkinSwitcher>().getSkin();

        switch (colors[0])
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
                spriteRenderer.sprite= cardSprites[3];
                break;
        }
    }

}

