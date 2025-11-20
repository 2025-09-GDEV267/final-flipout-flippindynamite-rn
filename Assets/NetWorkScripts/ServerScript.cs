using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System;


public class ServerScript : NetworkBehaviour
{
    public NetworkList<Card> NetworkDeck;

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
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public NetworkVariable<string> textLog = new NetworkVariable<string>("Hewwo Wowd", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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
        
        //idk why we do this but just make sure you do this whenever you make a struct (we can figure out why later)
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref colorOne);
            serializer.SerializeValue(ref colorTwo);
            serializer.SerializeValue(ref ColorVisibleToOwner);
        }
        public bool Equals(Card other)
        {
            return colorOne == other.colorOne &&
                   colorTwo == other.colorTwo &&
                   ColorVisibleToOwner == other.ColorVisibleToOwner;
        }
    }
    //0 Blue,1 Purple,2 Green, 3 Yellow, 4 Red
    public Card[] deckCardRange = new Card[15]
    {
        new() {colorOne = 4, colorTwo = 4, ColorVisibleToOwner = 0},
        new() {colorOne = 4, colorTwo = 2, ColorVisibleToOwner = 0},
        new() {colorOne = 4, colorTwo = 0, ColorVisibleToOwner = 0},
        new() {colorOne = 4, colorTwo = 1, ColorVisibleToOwner = 0},
        new() {colorOne = 4, colorTwo = 3, ColorVisibleToOwner = 0},

        new() {colorOne = 2, colorTwo = 2, ColorVisibleToOwner = 0},
        new() {colorOne = 2, colorTwo = 0, ColorVisibleToOwner = 0},
        new() {colorOne = 2, colorTwo = 1, ColorVisibleToOwner = 0},
        new() {colorOne = 2, colorTwo = 3, ColorVisibleToOwner = 0},

        new() {colorOne = 0, colorTwo = 0, ColorVisibleToOwner = 0},
        new() {colorOne = 0, colorTwo = 1, ColorVisibleToOwner = 0},
        new() {colorOne = 0, colorTwo = 3, ColorVisibleToOwner = 0},

        new() {colorOne = 1, colorTwo = 1, ColorVisibleToOwner = 0},
        new() {colorOne = 1, colorTwo = 3, ColorVisibleToOwner = 0},

        new() {colorOne = 3, colorTwo = 3, ColorVisibleToOwner = 0}
    };


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
            NetworkDeck.Add(deck[i]);
        }
    }
}
