using UnityEngine;
using Unity.Netcode;
using static ServerScript;
using System.Diagnostics;

public class GameLoop : NetworkBehaviour
{
    private NetworkVariable<int> currentPlayer = new NetworkVariable<int>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPlayer.Value = 0;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
