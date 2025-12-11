using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class UItoNetwork : MonoBehaviour
{
    [SerializeField]
    NetworkManager networkManager;

    [SerializeField]
    GameObject dropDown;

    NetworkTransport transport;

    public void host()
    {
       
        networkManager.StartHost();
        dropDown.SetActive(false);
        gameObject.SetActive(false);
    }

    public void client()
    {
        networkManager.StartClient();
        dropDown.SetActive(false);
        gameObject.SetActive(false);
    }
}
