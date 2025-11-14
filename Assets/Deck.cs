using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Deck : NetworkBehaviour
{
    [SerializeField]
    GameObject cardPrefab;

    [SerializeField]
    List<Card> cards;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnAllCards();
        }

        if (IsOwner)
        {
            transform.position = new Vector3(0, -3, 0);
        }
        else
        {
            transform.position = new Vector3(0, 3, 0);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            foreach (Card cards in cards)
            {
                Destroy(cards.gameObject);
            }
            Destroy(gameObject);
        }
    }
    private void SpawnAllCards()
    {
        float startX = -7.5f;
        Array allPossibleColors = Enum.GetValues(typeof(cardColor));

        for (int i = 0; i < 6; i++)
        {
            GameObject newCard = Instantiate(cardPrefab, gameObject.transform);
            
            newCard.transform.localPosition = new Vector3(startX + (3 * i), 0, 0);

            NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
          
            cardNetworkObject.Spawn();
            
            cardNetworkObject.TrySetParent(gameObject,false);

            int randomOne = UnityEngine.Random.Range(0, allPossibleColors.Length);

            int randomTwo = UnityEngine.Random.Range(0, allPossibleColors.Length);

            cardColor color1 = (cardColor)allPossibleColors.GetValue(randomOne);

            cardColor color2 = (cardColor)allPossibleColors.GetValue(randomTwo);

            Card cardComponent = newCard.GetComponent<Card>();

            cardComponent.SetClientRpc(color1, color2);

            cards.Add(cardComponent);
        }
    }
}
