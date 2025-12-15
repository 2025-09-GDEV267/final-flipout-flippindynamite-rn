using UnityEngine;
using Unity.Networking;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIJOINSCRIPT : NetworkBehaviour
{

    public TMP_InputField inputField;

    

    public void JoinHost()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        NetworkManager.Singleton.StartHost();

        if (IsServer || IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("CoreGame", LoadSceneMode.Single);
        }
    }
    public void StartClient()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address = inputField.text;
        NetworkManager.Singleton.StartClient();
    }
}
