using UnityEngine;

public enum cardColor {purple,green,blue,yellow}
public class Card : MonoBehaviour
{
    public cardColor[] colors;
    public Sprite[] cardSprites; //0 purple,1 green, 2 blue, yellow
    SpriteRenderer spriteRenderer;

    public void Init(cardColor sideOne, cardColor sideTwo, Sprite[] cardSprites)
    {
        colors[0] = sideOne;
        colors[1] = sideTwo;

        this.cardSprites = cardSprites;
    }

    public void changeSprites(Sprite[] cardSprites)
    {
        this.cardSprites = cardSprites;
    }

}

