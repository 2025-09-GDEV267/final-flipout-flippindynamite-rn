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
        
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {

            transform.position = new Vector3(0, -3.25f, 0);
            return;
        }

        transform.position = new Vector3(0, 3.25f, 0);
    }

    private void Update()
    {
        if (gameManager == null) return;
        if (gameManager.Hands == null || gameManager.Hands.Count == 0) return;

        if (IsOwner)
        {
            myHand.Value = gameManager.GetPlayerNumber(OwnerClientId);
            
            for (int i = 0; i < cardSpriteRenderers.Length; i++)
            {
                cardSpriteRenderers[i].sprite = cardSprites[gameManager.getCard(true, myHand.Value, i)];
            }
            return;
        }

        for (int i = 0; i < cardSpriteRenderers.Length; i++)
        {
            cardSpriteRenderers[i].sprite = cardSprites[gameManager.getCard(false, myHand.Value, i)];
        }


    }
}
