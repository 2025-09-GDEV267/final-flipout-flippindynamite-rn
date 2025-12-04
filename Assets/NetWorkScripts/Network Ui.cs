using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using UnityEditor;

public class NetWorkUi : NetworkBehaviour
{
    [SerializeField]
    TMP_InputField inputField;

    [SerializeField]
    NetworkManager networkManager;

    [SerializeField]
    string gameScene;
    /*
     * Sets the IP address equal to the inputfield's text. Before opening a new scene
     */
    public void startHost()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        networkManager.StartHost();
        if (IsServer) networkManager.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }
    // Sets IP to inputField and starts client
    public void startClient()
    {
        networkManager = NetworkManager.Singleton;
        networkManager.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        networkManager.StartClient();
    }
}
