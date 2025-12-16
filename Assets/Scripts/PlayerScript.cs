using NUnit.Framework.Interfaces;
using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    [SerializeField]
    NetworkVariable<int> myHand = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [SerializeField]
    GameManager gameManager;

    [SerializeField]
    SpriteRenderer[] cardSpriteRenderers;

    //0 Blue,1 Purple,2 Green, 3 Yellow, 4 Red
    [SerializeField]
    Sprite[] cardSprites;
    private void Awake()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        myHand.Value = new int();
    }
    private void OnNetworkInstantiate()
    {
        if (IsOwner)
        {
            myHand.Value = gameManager.GetPlayerNumber(OwnerClientId);
        }
    }

    private void Update()
    {
        if (gameManager != null)
        {
            for (int i = 0; i < cardSpriteRenderers.Length; i++)
            {
                cardSpriteRenderers[i].sprite = cardSprites[gameManager.getCard(IsOwner, myHand.Value, i)];
            }
        }
    }
}
