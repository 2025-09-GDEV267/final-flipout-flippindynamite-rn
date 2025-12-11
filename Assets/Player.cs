using UnityEngine;
using Unity.Netcode;
using static ServerScript;
using System.Diagnostics;

public class Player : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPlayer.Value == myValue)
        {
            Debug.log("It is my turn.");
        }
    }
}
