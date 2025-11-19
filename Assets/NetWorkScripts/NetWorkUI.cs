using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using UnityEditor;

public class NetWorkUI : NetworkBehaviour
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
        
        networkManager.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        networkManager.StartHost();
        if(!IsClient) networkManager.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }
    // Sets IP to inputField and starts client
    public void startClient()
    {
        networkManager.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        networkManager.StartClient();
    }
}
