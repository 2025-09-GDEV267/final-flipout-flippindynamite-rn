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
            myHand.Value = gameManager.GetPlayerNumber(OwnerClientId);
        }
    }

    public void Flip(SpriteRenderer cardSprite)
    {
        for (int i = 0; i < cardSpriteRenderers.Length; i++)
        {
                if (cardSprite.Equals(cardSpriteRenderers[i]))
                {
                    gameManager.flipServerRpc(myHand.Value, i);
                }
        }
    }


    private void Update()
    {
        if (gameManager == null) return;
        if (gameManager.Hands == null || gameManager.Hands.Count == 0) return;

        if (IsOwner)
        {
            myHand.Value = gameManager.GetPlayerNumber(OwnerClientId);

            transform.position = new Vector3(0, -3.25f, 0);

            for (int i = 0; i < cardSpriteRenderers.Length; i++)
            {
                cardSpriteRenderers[i].sprite = cardSprites[gameManager.getCard(true, myHand.Value, i)];
            }
        }

        if (!IsOwner)
        {

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != gameObject) setPos(i, players[i].transform);
            }

            transform.localScale = new Vector3(0.75f, 0.75f, 1);
            
            for (int i = 0; i < cardSpriteRenderers.Length; i++)
            {
                cardSpriteRenderers[i].sprite = cardSprites[gameManager.getCard(false, myHand.Value, i)];
            }
        }

    }

    public void setPos(int mySpot, Transform gameOBJ)
    {
        switch (mySpot)
        {
            case 1:
                gameOBJ.position = new Vector3(5.25f, 0.75f, 0f);
                break;
            case 2:
                gameOBJ.position = new Vector3(0, 3.25f, 0);
                break;
            case 3:
                gameOBJ.position = new Vector3(-5.25f, 0.75f, 0);
                break;
        };
    }
}
