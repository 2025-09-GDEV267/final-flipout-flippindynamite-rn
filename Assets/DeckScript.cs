using UnityEngine;
using Unity.Netcode;
using static ServerScript;
using System.Diagnostics;

public class DeckScript : NetworkBehaviour 
{
    [SerializeField]
    SpriteRenderer[] cardSpriteRenderers;

    [SerializeField]
    Sprite[] cardSprite;

    NetworkList<Card> cards;

    [SerializeField]
    ServerScript serverScript;

    private void Awake()
    {
        cards = new NetworkList<Card>(new Card[6]);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer && !IsHost) return;

        if (IsOwner)
        {
            transform.position = new Vector3(0, -3, 0);
        }
        else
        {
            transform.position = new Vector3(0, 3, 0);
        }
        
       

        serverScript = FindFirstObjectByType<ServerScript>();

        readHandFromServer();
    }

    private void Update()
    {
    }

    private void readHandFromServer()
    {

        if (IsServer) return;

        if (IsOwner)
        {
            player myPlayer = serverScript.getMyPlayer(OwnerClientId);

            cards[0] = serverScript.NetworkDeck[myPlayer.cardOneIDs];
            cards[1] = serverScript.NetworkDeck[myPlayer.cardTwoIDs];
            cards[2] = serverScript.NetworkDeck[myPlayer.cardThreeIDs];
            cards[3] = serverScript.NetworkDeck[myPlayer.cardFourIDs];
            cards[4] = serverScript.NetworkDeck[myPlayer.cardFiveIDs];
            cards[5] = serverScript.NetworkDeck[myPlayer.cardSixIDs];
        }

        for (int i = 0; i < cardSpriteRenderers.Length; i++)
        {
            /*
            cardReaders[i].SideOne = cards[i].colorOne;
            cardReaders[i].SideTwo = cards[i].colorTwo;
            cardReaders[i].ColorVisibleToOwner = cards[i].ColorVisibleToOwner;
            */

            if (IsOwner)
            {
                switch (cards[i].ColorVisibleToOwner)
                {
                    case 0:
                        cardSpriteRenderers[i].sprite = cardSprite[cards[i].colorOne];
                        break;
                    case 1:
                        cardSpriteRenderers[i].sprite = cardSprite[cards[i].colorTwo];
                        break;
                }
            }
            else if (!IsOwner)
            {
                switch (cards[i].ColorVisibleToOwner)
                {
                    case 0:
                        cardSpriteRenderers[i].sprite = cardSprite[cards[i].colorTwo];
                        break;
                    case 1:
                        cardSpriteRenderers[i].sprite = cardSprite[cards[i].colorOne];
                        break;
                }
            }
        }
    }

}
