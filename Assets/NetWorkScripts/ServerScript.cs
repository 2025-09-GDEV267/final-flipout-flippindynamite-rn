using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System;


public class ServerScript : NetworkBehaviour
{
    public NetworkList<Card> NetworkDeck;
    public NetworkList<player> PlayerList;

    // Establishes a Singleton
    public static ServerScript instance;
    
    // Could we do this in the OnNetworkSpawn()
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        NetworkDeck = new NetworkList<Card>();
        PlayerList = new NetworkList<player>();
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //Makes a new NetworkVarible of type Card (see def) called randomValues and then sets the read perms to everyone and the writing perms to only the server
    //See this link for what can be NetworkVariables https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.7/manual/serialization.html
    public NetworkVariable<Card> cardValue = new NetworkVariable<Card>(
        new Card
        {
            //sets default values
            colorOne = 0,
            colorTwo = 1,
            ColorVisibleToOwner = 0,
            
        },
        //sets var perms
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    //CardDeck Initialization
    //creates a new Card struct that extends INetworkSerializable
    
    //We make a struct instead of a Class because a class is a reference type i ~think~ either way if we're storing a bunch of values this is how
    public struct Card : INetworkSerializable, IEquatable<Card>
    {
        //0 Blue,1 Purple,2 Green, 3 Yellow, 4 Red
        public int colorOne;
        public int colorTwo;
        //Between 0-1 (0 for side One, and 1 for side Two)
        public int ColorVisibleToOwner;
        public int cardId;
        
        //idk why we do this but just make sure you do this whenever you make a struct (we can figure out why later)
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref colorOne);
            serializer.SerializeValue(ref colorTwo);
            serializer.SerializeValue(ref ColorVisibleToOwner);
            serializer.SerializeValue(ref cardId);
        }
        public bool Equals(Card other)
        {
            return colorOne == other.colorOne &&
                   colorTwo == other.colorTwo &&
                   ColorVisibleToOwner == other.ColorVisibleToOwner &&
                   cardId == other.cardId;
        }
    }
    //0 Blue,1 Purple,2 Green, 3 Yellow, 4 Red
    public Card[] deckCardRange = new Card[15]
    {
        new() {colorOne = 4, colorTwo = 4, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 4, colorTwo = 2, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 4, colorTwo = 0, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 4, colorTwo = 1, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 4, colorTwo = 3, ColorVisibleToOwner = 0, cardId = 0},

        new() {colorOne = 2, colorTwo = 2, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 2, colorTwo = 0, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 2, colorTwo = 1, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 2, colorTwo = 3, ColorVisibleToOwner = 0, cardId = 0},

        new() {colorOne = 0, colorTwo = 0, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 0, colorTwo = 1, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 0, colorTwo = 3, ColorVisibleToOwner = 0, cardId = 0},

        new() {colorOne = 1, colorTwo = 1, ColorVisibleToOwner = 0, cardId = 0},
        new() {colorOne = 1, colorTwo = 3, ColorVisibleToOwner = 0, cardId = 0},

        new() {colorOne = 3, colorTwo = 3, ColorVisibleToOwner = 0, cardId = 0}
    };

    public struct player : INetworkSerializable, IEquatable<player>
    {
        public ulong id;
        public int PlayerCount;
        public int cardOneIDs;
        public int cardTwoIDs;
        public int cardThreeIDs;
        public int cardFourIDs;
        public int cardFiveIDs;
        public int cardSixIDs;


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref PlayerCount);
            serializer.SerializeValue(ref cardOneIDs);
            serializer.SerializeValue(ref cardTwoIDs);
            serializer.SerializeValue(ref cardThreeIDs);
            serializer.SerializeValue(ref cardFourIDs);
            serializer.SerializeValue(ref cardFiveIDs);
            serializer.SerializeValue(ref cardSixIDs);
        }
        public bool Equals(player other)
        {
            return id == other.id &&
                PlayerCount == other.PlayerCount &&
                   cardOneIDs == other.cardOneIDs &&
                   cardTwoIDs == other.cardTwoIDs &&
                   cardThreeIDs == other.cardThreeIDs &&
                   cardFourIDs == other.cardFourIDs &&
                   cardFiveIDs == other.cardFiveIDs &&
                   cardSixIDs == other.cardSixIDs;
        }
    }

    //Overrides the OnNetworkSpawn and adds debug log changes to the OnValueChanged method
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            creatDeck();
        }
        //uses a Lamda operator to add further methods to the OnValueChanged method
        //(I tried to edit it to prevent values from being changed outside their limits, edited them out cause i couldnt figure it out)
        cardValue.OnValueChanged += (Card previousValue, Card newValue) =>
        {
           /*
            * I tried placing a clamping method in this code so the server has stricter control on the changing of
            * values. shelved for now for sake of simplicity
            * 
            * if (newValue.colorOne > 3 || newValue.colorOne < 0)
            {
                newValue.colorOne = previousValue.colorOne;
            }
            if (newValue.colorTwo > 3 || newValue.colorTwo < 0)
            {
                newValue.colorTwo = previousValue.colorTwo;
            }
            if (newValue.ColorVisibleToOwner > 1 || newValue.ColorVisibleToOwner < 0)
            {
                newValue.ColorVisibleToOwner = previousValue.ColorVisibleToOwner;
            }
           */
            Debug.Log(OwnerClientId + "; " + newValue.colorOne + " ; " + newValue.colorTwo + "; " + newValue.ColorVisibleToOwner);
        };
    }
    //create this bool to control the rate at which invoke is called 
    public bool latch = false;

    // Update is called once per frame
    void Update()
    {
        // Only the server can do anything in Update else return;
        if (!IsServer) return;
        
        if (!latch)
        {
            Invoke("updateValues",2);
            latch = true;
        }
        
    }
    //Updates the NetWorkValue randomValues 
    private void updateValues()
    {
        cardValue.Value = NetworkDeck[UnityEngine.Random.Range(0,NetworkDeck.Count)];
        latch = false;
    }
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        player newPlayer = new player
        {
            id = clientId,
            PlayerCount = PlayerList.Count + 1,
            cardOneIDs = NetworkDeck[UnityEngine.Random.Range(0,NetworkDeck.Count)].cardId,
            cardTwoIDs = NetworkDeck[UnityEngine.Random.Range(0, NetworkDeck.Count)].cardId,
            cardThreeIDs = NetworkDeck[UnityEngine.Random.Range(0, NetworkDeck.Count)].cardId,
            cardFourIDs = NetworkDeck[UnityEngine.Random.Range(0, NetworkDeck.Count)].cardId,
            cardFiveIDs = NetworkDeck[UnityEngine.Random.Range(0, NetworkDeck.Count)].cardId,
            cardSixIDs = NetworkDeck[UnityEngine.Random.Range(0, NetworkDeck.Count)].cardId
        };

        PlayerList.Add(newPlayer);
    }
    public player getMyPlayer(ulong id)
    {
        foreach (player play in PlayerList)
        {
            if (play.id.Equals(id))
            {
                return play;
            }
        }
        return new player { };
    }
    public void creatDeck()
    {
        if (!IsServer) return;

        int index = 0;

        Card[] deck = new Card[90]; 

        for (int i = 0; i < deckCardRange.Length; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                deck[i * 6 + j] = deckCardRange[index];
                index++;
                if (index == deckCardRange.Length) index = 0;
            }
        }

        for (int i = 0; i < deck.Length; i++)
        {
            deck[i].cardId = i;
            deck[i].ColorVisibleToOwner = 0;
            NetworkDeck.Add(deck[i]);
        }
    }
}
