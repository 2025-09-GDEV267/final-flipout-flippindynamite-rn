using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerLogic : NetworkBehaviour
{
    [SerializeField]
    TMP_Text number;
    [SerializeField]
    Button button;
    [SerializeField]
    GameObject canvas;
    [SerializeField]
    GameObject toggleables;

    [SerializeField]
    GameManager gameManager;

    int numValue;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        button.onClick.AddListener(() =>
        {
            gameManager.PassTurnServerRpc(numValue);
        });

    }

    private void OnNetworkInstantiate()
    {
        if (!IsOwner)
        {
            canvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (gameManager.whosTurn.Value == OwnerClientId)
        {
            toggleables.SetActive(true);

            if (Input.GetKeyDown(KeyCode.LeftArrow) && numValue > 0)
            {
                numValue--;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) && numValue < 9)
            {
                numValue++;
            }

            number.text = numValue.ToString();
        }
        else
        {
            numValue = gameManager.currentNumber.Value;
            toggleables.SetActive(false);
        }
    }
}