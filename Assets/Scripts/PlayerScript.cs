using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    NetworkVariable<int> myHand = new NetworkVariable<int>(0,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

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
    private void OnNetworkInstantiate()
    {
        if (IsOwner)
        {
            myHand.Value = gameManager.GetPlayerNumber(OwnerClientId);
        }
    }

    private void Update()
    {
        
    }
}
