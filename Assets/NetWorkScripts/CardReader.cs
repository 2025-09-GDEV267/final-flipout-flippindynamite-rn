using System.Diagnostics;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CardReader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    SpriteRenderer spriteRenderer;
    //0 Blue, 1 Purple, 2 Green, 3 Yellow, 4 Red, 5 Error


    ServerScript serverScript;

    public int SideOne;
    public int SideTwo;
    public int ColorVisibleToOwner;
}
