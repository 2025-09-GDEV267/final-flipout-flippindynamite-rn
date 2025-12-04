using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    // Network variables
    public NetworkVariable<ulong> whosTurn = new NetworkVariable<ulong>(
        ulong.MaxValue,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> currentNumber = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    [SerializeField]
    private List<ulong> playerIds = new List<ulong>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
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

        playerIds.Add(clientId);

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

        playerIds.Remove(clientId);

        // If the disconnected player was the current turn holder
        if (whosTurn.Value == clientId && playerIds.Count > 0)
        {
            int nextIndex = 0; // Or implement your turn order logic
            whosTurn.Value = playerIds[nextIndex];
        }

        // Update player list
        UpdatePlayerListClientRpc(playerIds.ToArray());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PassTurnServerRpc(int newNumber, ServerRpcParams rpcParams = default)
    {
        // Validate it's the player's turn
        if (rpcParams.Receive.SenderClientId != whosTurn.Value)
        {
            Debug.LogWarning($"Player {rpcParams.Receive.SenderClientId} tried to pass turn out of turn!");
            return;
        }

        // Update the number
        currentNumber.Value = newNumber;

        // Move to next player
        if (playerIds.Count > 0)
        {
            int currentIndex = playerIds.IndexOf(whosTurn.Value);
            int nextIndex = (currentIndex + 1) % playerIds.Count;
            whosTurn.Value = playerIds[nextIndex];

            Debug.Log($"Turn passed to player {whosTurn.Value}");
        }
    }

    [ClientRpc]
    private void UpdatePlayerListClientRpc(ulong[] playerIdsArray)
    {
        // This syncs the player list to all clients
        playerIds.Clear();
        playerIds.AddRange(playerIdsArray);
    }
}