using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField]
    Card[] deck;
    public void updateCards(Sprite purple, Sprite green, Sprite blue, Sprite yellow)
    {
        Sprite[] cards = { purple, green, blue, yellow };
        foreach (var card in deck)
        {
            card.changeSprites(cards);
        }
    }
}
