using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{

    public NetworkVariable<ulong> whosTurn;

    List<Player> playerIds = new List<Player>();

    public NetworkList<Card> NetworkDeck;

    public NetworkList<Card> NetworkDiscard;

    public NetworkList<Card> Hands = new NetworkList<Card>(new Card[24],NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    const int CardsPerPlayer = 6;


    // Establishes a Singleton
    public static GameManager instance;

    // Could we do this in the OnNetworkSpawn()
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        NetworkDeck = new NetworkList<Card>();
        NetworkDiscard = new NetworkList<Card>();
       

        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        Player newPlayer;

        newPlayer.Id = clientId;

        newPlayer.hand = playerIds.Count;

        playerIds.Add(newPlayer);

        if (whosTurn.Value == ulong.MaxValue)
        {
            whosTurn.Value = clientId;
        }

        // Update all clients with the player list
        UpdatePlayerListClientRpc(playerIds.ToArray());
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        Player player = new Player();

        foreach (Player p in playerIds)
        {
            if (p.Id == clientId)
            {
                player = p;
            }
        }

        playerIds.Remove(player);

        // If the disconnected player was the current turn holder
        if (whosTurn.Value == clientId && playerIds.Count > 0)
        {
            int nextIndex = 0; // Or implement your turn order logic
            whosTurn.Value = playerIds[nextIndex].Id;
        }

        // Update player list
        UpdatePlayerListClientRpc(playerIds.ToArray());
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
    public struct Player : INetworkSerializable, IEquatable<Player>
    {
        public ulong Id;
        public int hand;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref hand);
        }
        public bool Equals(Player other)
        {
            return Id == other.Id && hand == other.hand;
        }
    }
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
           createDeck();
           dealOut();
            
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
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
        if (IsServer && !IsHost) return;
    }
    //Updates the NetWorkValue randomValues 
    private void updateValues()
    {

    }

    public void dealOut()
    {
        if (!IsServer) return;

        for (int player = 0; player < 4; player++)
        {
            for (int card = 0; card < CardsPerPlayer; card++)
            {
                int rand = UnityEngine.Random.Range(0, NetworkDeck.Count);
                Hands[Index(player, card)] = NetworkDeck[rand];
                NetworkDiscard.Add(NetworkDeck[rand]);
                NetworkDeck.RemoveAt(rand);
            }
        }


    }

    [ServerRpc(RequireOwnership = false)]
    public void flipServerRpc(int playerNumber, int cardNumber)
    {
        if (true /* I'll add some logic like turn order or whatever here*/)
        {
            var c = Hands[Index(playerNumber, cardNumber)];

            if (c.ColorVisibleToOwner == 0)
            {
                c.ColorVisibleToOwner = 1;
            }
            else
            {
                c.ColorVisibleToOwner = 0;
            }

            Hands[Index(playerNumber, cardNumber)] = c;

            updateValues();
        }
    }
    public void createDeck()
    {
        if (!IsServer || !IsHost) return;

        for (int i = 0; i < 90; i++)
        {
            NetworkDeck.Add(deckCardRange[i % deckCardRange.Length]);
        }
    }

    [ClientRpc]
    public void UpdatePlayerListClientRpc(Player[] newPlayerids)
    {
        playerIds.Clear();
        playerIds.AddRange(newPlayerids);
    }

    public int getCard(bool isCardOwner, int playerNumber, int cardNumber)
    {
        var c = Hands[Index(playerNumber, cardNumber)];

        if (IsOwner)
        {
            if (c.ColorVisibleToOwner == 0)
            {
                return c.colorOne;
            }
            else
            {
                return c.colorTwo;
            }
        }
        else
        {
            if (c.ColorVisibleToOwner == 0)
            {
                return c.colorTwo;
            }
            else
            {
                return c.colorOne;
            }
        }
    }

    int Index(int player, int card)
    {
        return player * CardsPerPlayer + card;
    }

    public int GetPlayerNumber(ulong playerId)
    {
        foreach (var players in playerIds)
        {
            if (players.Id == playerId)
            {
                return players.hand;
            }
        }

        return 0;
    }

}
