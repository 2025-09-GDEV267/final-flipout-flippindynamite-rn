using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

public class CardReader : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    SpriteRenderer spriteRenderer;
    //0 Blue, 1 Purple, 2 Green, 3 Yellow, 4 Red, 5 Error
    [SerializeField]
    Sprite[] cardSprite;

    ServerScript serverScript;

    //when a card is created on the network find their instance of the serverScript and spriteRenderer
    //gets how many other player objects (in this case single cards) exists and moves accordingly
    public override void OnNetworkSpawn()
    {
        serverScript = FindFirstObjectByType<ServerScript>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        int playerCount = GameObject.FindObjectsByType<CardReader>(FindObjectsSortMode.None).Length;

        transform.position = new Vector3(-10 + (3 * playerCount),0,0);
    }
    //Checks if the card belongs to a player if it does show one side, if it doesn't show the other
    //if the server tries to access the method, escape it
    void Update()
    {
        //checks if the side running this method is the host, if it is continue onto the script
        //if it's not check if it's the server, if it's the server escape the method, otherwise continue
        if (!IsHost)
        {
            if (IsServer) return;
        }

        //checks if the card belongs to the client
        if (IsOwner)
        {
            //if the card belongs to this client set the sides equal to ColorVisibleToOwner
            switch (serverScript.cardValue.Value.ColorVisibleToOwner)
            {
                case (0):
                    spriteRenderer.sprite = cardSprite[serverScript.cardValue.Value.colorOne];
                    break;
                case (1):
                    spriteRenderer.sprite = cardSprite[serverScript.cardValue.Value.colorTwo];
                    break;
                default:
                    spriteRenderer.sprite = cardSprite[5];
                    break;
            }
        }
        else if (!IsOwner)
        {
            //if the client running this code isn't the owner, display the opposite side of the card
            switch (serverScript.cardValue.Value.ColorVisibleToOwner)
            {
                case (0):
                    spriteRenderer.sprite = cardSprite[serverScript.cardValue.Value.colorTwo];
                    break;
                case (1):
                    spriteRenderer.sprite = cardSprite[serverScript.cardValue.Value.colorOne];
                    break;
                default:
                    spriteRenderer.sprite = cardSprite[5];
                    break;
            }
        }
    }
}
